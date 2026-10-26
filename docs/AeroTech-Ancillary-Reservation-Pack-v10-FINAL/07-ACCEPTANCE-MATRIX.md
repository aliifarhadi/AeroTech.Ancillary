# 07 — Reservation v10 — ماتریس پذیرش قطعی

**مفهوم PASS:** هر ردیف فقط با تست اتوماتیک نام‌دار (Domain/SQL/API) و در موارد مشخص با HTTP واقعی PASS می‌شود؛ صرف سبز بودن Build یا گزارش شفاهی کافی نیست. تست‌های پیشین `M1Reservation*`, `SeatReservation*`, `V9*` و Pinهای قدیمی باید پس از تصمیم جدید درست update شوند، نه حذف شوند.

## A — Contract و Mapping
| ID | Scenario | Expected |
|---|---|---|
| A01 | Hold Request بدون `orderId` | 400، zero write |
| A02 | Header expiry ندارد/null است | 400/422، no infinite new hold |
| A03 | RequestedExpiresAt <= now | reject، zero allocation |
| A04 | IdempotencyKey خالی/بیش از 128 | reject |
| A05 | Reference نامعتبر | reject |
| A06 | Services خالی/Null | reject |
| A07 | دو Unit با OrderServiceId برابر در یک Create | reject |
| A08 | Definition ناشناخته | reject، zero write |
| A09 | Provision متعلق به Definition دیگر | reject، zero write |
| A10 | Provision quantity خارج Min/Max | reject |
| A11 | Coverage Sector با 0 یا 2 Flight | reject |
| A12 | TravellerId در Scope Order غیرnull | reject |
| A13 | Portion دو Flight بدون از دست‌رفتن Coverage | یک Unit + دو allocation per source لازم |
| A14 | دو Provision متفاوت یک Definition و Scope | مجاز اگر OrderServiceId متفاوت و conflicts واقعی ندارند |
| A15 | Hold body بدون price/currency/EMD | accepted shape، no money in Domain reservation |
| A16 | DTO unknown inventory fact در pattern نامرتبط | reject، no silent ignored field |

## B — Local Inventory & concurrency (SQL Server real)
| ID | Scenario | Expected |
|---|---|---|
| I01 | Unlimited active | Held، Allocations=[] |
| I02 | Missing Inventory Policy | fail closed، not Unlimited |
| I03 | Draft/Suspended/Retired Policy | fail closed |
| I04 | External Supplier policy | unsupported explicit error، no Held |
| I05 | Unsupported DailyCount/RoomNight/AssignedAsset | explicit reject |
| I06 | FlightCount Qty2 x CountPerUnit3 | allocation Amount=6 در همان source |
| I07 | Two products share source | shared capacity counted once from same ledger |
| I08 | Existing held unexpired counted | new hold may shortage |
| I09 | Existing confirmed counted | new hold may shortage |
| I10 | Existing held logically expired before poller | not counted، stock immediately available |
| I11 | FlightWeight fixedKg x quantity | accurate Kg total |
| I12 | Policy FlightWeight با `AcceptedWeightKg` | explicit Unsupported reject **before** any Allocation |
| I13 | Payload carrying an accepted-weight field | rejected as unknown field؛ no column and no Allocation written |
| I14 | FlightCountPlusWeight count enough / weight shortage | **neither** allocation committed |
| I15 | FlightCountPlusWeight both enough | both allocated atomic |
| I16 | Portion Flight801+802, one source missing | zero allocations from entire Unit / Create |
| I17 | AirportSlot 2 slices contiguous | two allocations with same persons demand |
| I18 | AirportSlot gap in timeslots | reject |
| I19 | AirportSlot ClosedForSale | reject |
| I20 | GuestCount included in persons | `PeoplePerAcceptedUnit*(Quantity+GuestCount)` |
| I21 | Two different orders concurrent last 1 count unit | exactly one success، no oversell SQL |
| I22 | Two different orders concurrent last 1kg | exactly one success، no oversell SQL |
| I23 | Two different orders concurrent same slot | exactly one success، no oversell SQL |
| I24 | Admin AdjustTo below confirmed/held usage | reject in same resource lock |
| I25 | Hold does not change TotalCapacity/CapacityKg/CapacityPersons | unchanged before/after Hold/Confirm |
| I26 | Failed Save rollback | no partially saved Reservation/Allocations |
| I27 | Count/Slot Amount fractional or invalid precision | reject Domain/Persistence |
| I28 | Resource OwnerAirlineId or source kind mismatch | reject |
| I29 | Legacy Held با ExpiresAt null در جریان migration | تا reconciliation باعث آزادشدن ناخواسته ظرفیت/oversell نمی‌شود |
| I30 | Policy با PassengerUsageLimit و Usage Evidence غیرقابل اثبات | fail-closed یا اجرای کنترل اثبات‌شده؛ بدون bypass خاموش |
| I31 | Unlimited با SubjectToConfirmation و بدون Provider واقعی | fail-closed، نه Fake Held |
| I32 | Legacy live Hold/Confirmed بدون Allocation و stock mapping قابل اثبات | Local Hold جدید برای stock مشکوک fail-closed تا reconciliation، نه zero-consumption assumption |

## C — Domain lifecycle / partial operations
| ID | Scenario | Expected |
|---|---|---|
| L01 | Initial multi-service hold | one Root, N Units, each unique UnitId |
| L02 | Read Hold GET | exact persisted Units/Status/Expiry |
| L03 | Confirm Held root | active Held→Confirmed؛ allocations stay consumed |
| L04 | Confirm already Confirmed | no-op exact same UnitRefs |
| L05 | Confirm Released/Expired | reject |
| L06 | Release whole Held | only active Held→Released؛ availability restored |
| L07 | Release already Released | no-op without double increment |
| L08 | `DELETE …?unitIds=` one Unit out of three | one Released؛ other two Held؛ stock only that Unit available |
| L09 | `DELETE …?unitIds=` naming a Confirmed Unit | reject 409، nothing changed؛ must Cancel |
| L10 | Confirm after one Unit Released | Released stays Released؛ other Held→Confirmed |
| L11 | Cancel all Confirmed with reasonCode (no `unitIds`) | remaining Confirmed→Cancelled؛ Released and earlier Cancelled untouched |
| L12 | `Cancellations {unitIds:[one]}` of two Confirmed | one Cancelled، other Confirmed، remaining capacity counted |
| L13 | Selective Cancel retry same reason | no-op، reason preserved |
| L14 | Selective Cancel retry different reason | conflict، audit unchanged |
| L15 | `Cancellations {unitIds}` naming a Held Unit | reject 409؛ requires Release |
| L16 | Expire Held past deadline | effective Expired immediately + poller persists |
| L17 | Expire job twice | unchanged twice، no double release |
| L18 | Confirm at exactly ExpiresAt | reject |
| L19 | Two requests same create key same body | identical HoldId and UnitIds؛ no new allocations |
| L20 | Same create key different `timeWithOffset`/`guestCount`/`quantity` or business ids | 409 conflict |
| L21 | Another hold for same live OrderServiceId | reject |
| L22 | Status enum numeric values unchanged | 1..5 unchanged in DB/JSON |
| L23 | RequestedExpiresAt stays immutable on append | no extension، equal parent |

## D — SeatMap commercial only
| ID | Scenario | Expected |
|---|---|---|
| S01 | Seat-only `SeatMaps/Holds` with holdId absent | create 1 Root + seat Unit |
| S02 | Mixed: NonSeat Hold then SeatMap append | **same HoldId**، Unit added to same Root |
| S03 | Seat append with wrong orderId or Reference | reject، existing Root untouched |
| S04 | Seat append to expired or confirmed Root | reject |
| S05 | Same append key/body repeated | same Units/IDs، no duplicates |
| S06 | Same append key different SeatMapSelectionId | conflict |
| S07 | Seat Number/Flight repeated within active same Hold | reject commercial duplicate |
| S08 | Seat Unit has Quantity1 / Sector / 1Flight | invariant |
| S09 | Seat Unit Allocations | **empty**, even if physical seat exists in FlightFlow |
| S10 | Ancillary outgoing Seat Reservation calls to FlightFlow | **zero** (`Seat-Holds`,`OccupiedSeats`,`Confirm`,`Cancel`) |
| S11 | Release/Cancel commercial Seat Unit | changes only Ancillary Unit، not FlightFlow seat |
| S12 | SeatMap Pricing/Resolver unchanged | existing tests & source hashes equal except deliberate facade append |
| S13 | Two different orders same physical seat attempt | Ancillary doesn't arbitrate; ownership documented in Ordering/FlightFlow |
| S14 | Ticketed Seat selection coupon flag missing | existing SeatMap Validator rejection preserved |
| S15 | Pre-order Seat selection no physical occupancy proof field | accepted if commercial verifier passes |

## E — Failure / Retry / Handoff
| ID | Scenario | Expected |
|---|---|---|
| E01 | Price/Resolve accepted data mapped into Hold | all IDs/Sector/Portion/Quantity match Order accepted facts |
| E02 | Offer travellerRef mapped to wrong OrderTravellerId | Ordering adapter rejects before Hold |
| E03 | Portion refs mapped to wrong flight ids | Ordering adapter rejects before Hold |
| E04 | Price Snapshot expires after Order stored | retry of Hold from accepted Order facts not broken by snapshot TTL |
| E05 | Unknown SQL commit result then same key retry | read existing state؛ no second stock allocation |
| E06 | Seat physical hold fails after Ancillary nonseat Hold | Ordering Release command compensates؛ Ancillary no FlightFlow call |
| E07 | Seat append fails | previous nonseat Hold remains Held؛ no accidental Confirm |
| E08 | Mixed Hold + partial release (`unitIds`) + confirm + partial cancel (`unitIds`) | state transitions legal، exact balances |
| E09 | HTTP 401 for unauthorized Service consumer if existing project policy requires | no write؛ no public reservation endpoint |
| E10 | Expiry background job unavailable | availability computed from logical expiry immediately |
| E11 | No Supplier adapter present | explicit error، not fake ProviderUnitRef |
| E12 | Booking ref/EMD not generated by Ancillary | Reserve only، no money/ticket/document writing |

## F — Regressions / Closure
| ID | Scenario | Expected |
|---|---|---|
| R01 | Existing `V9PriceRevalidationTests` | pass unchanged |
| R02 | `V9GroupingTests`, `V9SelectedOffersTests`, `V9BatchTests` | pass unchanged |
| R03 | Existing SeatMap shopping/Resolve/Verifier tests | pass unchanged |
| R04 | Existing Domain inventory/Policy conformance tests | pass یا update pin فقط به‌دلیل scope تاییدشده |
| R05 | Migration `has-pending-model-changes` | no pending |
| R06 | SQL real concurrency & rollback tests run with no mocks | PASS evidence/log |
| R07 | (a) Live HTTP Hold/Read/partial Release/Confirm/partial Cancel/Expire for Unlimited + Seat؛ (b) real SQL for Local (concurrency Count/Weight/Slot، rollback، Admin Adjust) | (a) PASS with actual DB؛ (b) PASS on SQL؛ Local live HTTP = GATED until a real reference source exists، never counted as PASS |
| R08 | Live HTTP SeatOnly + Mixed Seat Append | PASS; no FlightFlow calls |
| R09 | Diff limited to Ancillary Reservation/Inventory + tests/docs/migrations | no unrelated changes |
| R10 | No old v12/v13 conflicting authority used | report clean authority chain |

## Evidence table required at closure
Agent report must have for **each** ID or grouped test: `ID | test method name | execution command | PASS/FAIL/GATED | evidence/log | commit`. GATED cannot be counted PASS. Partial operations are tested only through the group routes with `unitIds` (no per-unit route exists). For consumer mappings E01-E03 which require Ordering repo not available to this agent: create contract tests in Ancillary for input validity and report external integration **NOT VERIFIED**, not fake E2E green.
