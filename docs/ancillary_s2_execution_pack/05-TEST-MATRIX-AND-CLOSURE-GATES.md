# S2 — Exhaustive acceptance / anti-regression / closure matrix

**اصل:** تمام سناریوهای این سند عیناً از §12 و §13 سند مادر گرفته شده‌اند تا دو مرجع متناقض درباره تست تولید نشود. هر مورد باید در گزارش Trace `{scenarioId,variant?,source Offer/Order,surface?,test name,expected,actual,source fixture,owner,status}` داشته باشد. `[EXTERNAL_PENDING]` یک نتیجه مشروع است، اما به معنای Positive real integration نیست.

## 1. Mandatory implementation gates — پیش از گزارش Done

| Gate | معیار سنجش | Evidence مورد نیاز |
|---|---|---|
| G01 | HEAD و Authority با diff واقعی match باشند | SHA + `CLAUDE.md` + one S2 ADR |
| G02 | Offer List/Details/legacy Single برای ۳ Surface regress نشوند | ۹ Golden semantic comparison, old paths preserved |
| G03 | Order-based List/Details/Evaluate با همان Engine | tests using trusted Order Context Fake and real adapter binding if available |
| G04 | ۲۰ Candidate، بدون فراخوانی خودکار ۲۰ Details | request count / Instrumentation test |
| G05 | A01..A24 positive or honest blocked-by-source & negative | ۲۴× traceability, cases below |
| G06 | Price/Quantity/Tax/Fee/FX/Shared Fee accurate | `PRICE-01..24` including 9 decimal ROE, 0 vs null |
| G07 | Batch mixed Seat/Baggage/Pax/Order-scope canonical | per-line result and basket ready/conflicts |
| G08 | Existing 3 SeatMap routes / DTO frozen | Exact JSON snapshot structure, price semantics allowed only audited fixes |
| G09 | SeatMap original ID → Evaluate new priced token → Resolve gives same seat/pax/FlightCapacity/price | `S2-SEAT-01..16`, `SEAT-17..25` and documented trace |
| G10 | Token key rotation, expiry, tamper, cross-Order/Agency, oversized payload, S2S auth | `SEC-01..25`, no bearer bypass |
| G11 | OrderVersion/Coupon changes handled, no silent reprice/partial Add | `FLOW-01..16`, `POST-01..14`, concurrency tests |
| G12 | No Quote DB/Aggregate/Changes in sibling repos | git diff, migration scan |
| G13 | Build/test truly executed | target SHA, assemblies, discovered/executed/passed/failed/skipped, commands, CI if any |
| G14 | Full Ordering/FlightFlow real integration not falsely claimed | Explicit `EXT-*` statuses + consumer handoff |

## 2. Unit/Integration-test structure

1. **Domain Shopping contract**: Definition/Provision/Pricing invariant and Rule matching with real fake Source facts, 24 variant cases; no new S1 entity when not needed.
2. **Source adapters**: Offer mapper + OrderCommercialContext converter, trusted sales and ownership, Coupon state/ExistingService completeness, no phantom free or authorization.
3. **Presentation/API**: ۳ Surface old Golden fixtures, new source/choice schema, inline option budget/Details gating, serialization response stability, Auth per route.
4. **Exact Monetary**: effective quantity rules, PerKilogram, TaxTreatment, fee per Item/Ticket/Order, currency+FX and rounding, incomplete basket behavior.
5. **Seat**: ۳ old route WIRE Goldens; matching 43-char SHA, negative occupancy, aircraft swap, free only with proof, multi-Pax conflicts, Order source.
6. **Security**: protected token+keyring+auth+expiry/version+foreign agency, bytes tampering, oversized, no PII/secrets in logs.
7. **Internal Resolve**: trusted Ordering reader, all statuses, per-line source checks, fully typed OrderService DTO, allValid check, no Hold side effects.
8. **S3 boundary**: current `Service-Holds` request remains compatible and separate; no fake EMD/Payment success.

## 3. Test runner report schema (MANDATORY)

```text
Repo/branch/HEAD: ...
SDK: ...
Commands actually run: [exact commands]
Build: [pass/fail/not run] + exit code
Assembly: [name]
Discovered: X; Executed: Y; Passed: P; Failed: F; Skipped: S; Aborted: A
Every required Scenario ID: covered test(s) or honest EXTERNAL_PENDING/UNSUPPORTED with reason
Old Golden fixtures: changed/unchanged; diffs and rationale
SeatMap Shape Diff: NONE (field names/types/status values)
Source unresolved: [source, owner, evidence, fail-closed behavior]
No commit/push performed unless explicitly ordered
Verdict: INCOMPLETE | S2_INTERNAL_CONTRACT_READY; owner closes stage later
```

Never report an assembly green when tests aborted or `discovered != executed` without explicit accounting of skipped/discovery conditions. Never replace actual test output with an invented count.

---

## 3.1 Frozen SeatMap → Evaluate → Resolve tests (verbatim from Master §09.6)

### 09.6 تست Seat/Order trace الزام‌آور

`S2-SEAT-01`: SeatMaps id=43char، status AVAILABLE، Price صالح؛ `S2-SEAT-02`: همان ID → Evaluate seat → token مثبت؛ `S2-SEAT-03`: Token → Resolve with OrderId → same Seat/Traveller/Flight/Amount and original SeatID؛ `S2-SEAT-04`: passenger swapped ID => reject؛ `S2-SEAT-05`: same seat two Pax in one basket => conflict؛ `S2-SEAT-06`: same Pax two seats same Segment => conflict؛ `S2-SEAT-07`: occupied since List => Stale؛ `S2-SEAT-08`: missing FlightFlow flight result => unavailable؛ `S2-SEAT-09`: aircraft swap/version changed => Stale؛ `S2-SEAT-10`: unknown Price/Commercial candidate => BLOCKED/null/null؛ `S2-SEAT-11`: verified free => Price 0/id valid؛ `S2-SEAT-12`: same seat different POS price refs never conflated؛ `S2-SEAT-13`: fake 43-character id fail； `S2-SEAT-14`: post-ticket order seat map requires valid original OfferId; `S2-SEAT-15`: old Offer ID can be matched only when origin Order linkage/current terms verified؛ `S2-SEAT-16`: existing ۳ route و تمام nested JSON property names/statuses unchanged.

---

## 4. Source-exact A01–A24 and full executable scenario suite

**متن زیر بدون خلاصه‌سازی از سند مرجع کپی شده تا هیچ فیلد/سناریویی از قلم نیفتد.**

## 12. ماتریس الزام‌آور تمام ۲۴ Variant — منبع، Input، Pricing، Coverage، Outcome و Handoff

**قانون کلی:** این ۲۴ سناریو «فهرست فعال‌کردن خودکار همه خدمات» نیستند. هر Variant تنها اگر Definition/Provision/Pricing/Supplier capability/Fact evidence معتبر دارد Candidate می‌سازد؛ **نبود منبع ≠ صفر/رایگان/قابل خرید.** کدها/نام‌ها و PricingUnitهای مجاز دقیقاً با `AncillaryVariant.cs` و `CustomerSelectionContract.cs` در SHA فوق تطبیق داده شده‌اند. هر سناریو باید حداقل یک تست مثبت *اگر منبع متناسب وجود دارد* و یک یا چند تست Fail-closed داشته باشد. اگر سناریوی مثبت مستلزم منبع خارجیِ موجودنبودنی است، به‌صورت `EXTERNAL_PENDING` با دقیق‌ترین contract gap ثبت و شبیه‌سازی فریبنده ممنوع است.

### 12.1 خلاصه ۲۴ ردیف اجرایی

| Code | Definition / pricing allowed | Beneficiary/coverage | Required customer input | Option source | Outcome مطلوب |
|---|---|---|---|---|---|
| A01 | Extra checked bag / `PerPiece` | Pax×Portion | `quantity` | typed qty | unit × qty؛ inventory/allowance verified |
| A02 | Extra weight package / `PerItem` | Pax×Portion | `packageProductRef`, `quantity` | **same DefinitionRef** | each 5kg/10kg own SKU/rate |
| A03 | Overweight / `PerPiece|PerKilogram` | Pax×Portion | `bagRef`,`measuredWeightKg` | typed measured weight + source chargeable basis | no exact price without chargeable basis |
| A04 | Oversize / `PerPiece` | Pax×Portion | `dimensions` | typed dimensions | linear-size / overlap rules |
| A05 | Cabin bag / `PerPiece` | Pax×Portion | `quantity` | typed qty | cabin allowance/capacity source |
| A06 | Sports equipment / `PerItem|PerPiece` | Pax×Portion | `equipmentKind`, dimensions/weight conditional | authored kind | special supplier approval |
| A07 | Standard seat / `PerSeat` | Pax×Sector | **SeatMap id only** | SeatMaps actual Seat | verified seat price, FlightFlow later holds |
| A08 | Preferred seat / `PerSeat` | Pax×Sector | **SeatMap id only** | SeatMaps preferred characteristic | no exit-row for ineligible pax |
| A09 | Extra seat / `PerSeat` | Pax×Sector | `seatNumber`, `purpose`, terms conditional | generic offers, not SeatMaps generic A07/A08 | coordinated physical+ticket later |
| A10 | Cabin upgrade / `PerPassenger|PerSeat` | Pax×Flight | `targetCabinId`, dynamic `quoteRef` | authored target + supplier quoted | false CouponUpgradable=>reject; true insufficient |
| A11 | Free special meal / `PerPassenger|PerItem` | Pax×Flight | `mealCode` | authored meal code | explicit Free, SSR/lead-time approval |
| A12 | Paid meal / `PerItem|PerPassenger` | Pax×Flight | `menuItemRef`, `quantity` | authored real menu only | subtotal/multiple, no fake free |
| A13 | Pet in cabin / `PerItem` | Pax×Portion | animal,weight,dimensions,docs conditional | authored list | pet/cabin quota, provider approval |
| A14 | Pet in hold / `PerItem` | Pax×Portion | A13 + `sizeBracket` if authored | authored size bracket | hold capability+connection rules |
| A15 | Wheelchair / `PerPassenger` | Pax×Flight | `assistanceSsrCode` | allowed SSR codes | no automated confirmation without source |
| A16 | Disability assistance / `PerPassenger` | Pax×Flight | `assistanceSsrCode` | allowed SSR codes | eligibility/approval |
| A17 | Medical equipment / `PerPassenger|PerItem` | Pax×Flight | `equipmentCode`, evidence,oxygenUnits conditional | authored equipment | medical approval/oxygen capacity |
| A18 | Bassinet / `PerPassenger|PerItem` | Infant + Guardian / Flight | `infantRef`,`guardianRef` | actual travellers | age/association/space verified |
| A19 | UMNR / `PerPassenger` | Child×Bound | `childRef`, verified handoff/pickup contacts | Ordering contact records | no boolean-as-proof; connection/country constraints |
| A20 | Lounge / `PerPassenger` | Pax×Airport | `airportId`, facility/time/guests conditional | authored facility, supplier slot if available | time-window and guest pricing |
| A21 | Fast Track / `PerPassenger` | Pax×Airport | `airportId`,facility/time conditional | authored airport/terminal | cutoff/availability source |
| A22 | CIP / `PerPassenger|PerItem` | Pax or Order if truly allowed | `airportId`, facility, appointment/guests conditional | authored package/facility | grouped components one charge; provider |
| A23 | Priority boarding/checkin / `PerPassenger` | Pax×Flight | `optIn:true` | typed boolean | no duplicate verified fare benefit |
| A24 | Onboard Wi-Fi / `PerItem|PerPassenger` | Pax or Order if allowed | `planCode`,deviceCount conditional | **same DefinitionRef** | provider activation later, one charge for Order scope |

### 12.2 تست مثبت/منفی و داده Source برای هر Variant

#### A01 — Extra checked bag

- **Input:** `{quantity:2}` برای `offerItemRef` خاص `ADT-1,B1`. `quantity=1` همان Candidate ولی Token متفاوت؛ `ADT-2` Token دیگر.
- **Source:** checked baggage allowance همان Coupon؛ filed increment limit؛ active rate PerPiece/ROE/Tax؛ remaining cumulative count اگر policy می‌خواهد.
- **List:** Inline، `min/max`, `unitPrice`, `coverage`, `selectedCount=0` پیش از انتخاب؛ `canSelect=true`, `canPurchase=false` تا Evaluate.
- **Negative:** max violation؛ duplicate same counting-family؛ Allowance unknown؛ `A01` Order-scope؛ ticket Used postIssue. **Handoff:** `quantity:2, travellerId, coverage=Portion, flightIds, serviceSubCode, exact tax-fee lines`.

#### A02 — Fixed weight package

- **Input:** `{packageProductRef:"BAG_WEIGHT_10",quantity:1}` فقط اگر همان **active Definition.ServiceDefinitionRef**؛ با وجود دو Package، دو OfferItemRef مستقل؛ تغییر SKU از 5 به 10kg نیازمند Candidate دیگر.
- **Source:** `BaggageSpecification.PackageWeightKg` برای هر Definition؛ published A02 Rate `PerItem`؛ exclude double charge/use included kilos.
- **List:** بسته ۵ و ۱۰kg دو ردیف یا grouped list با ۲ distinct candidate IDs در Presentation؛ **در Domain یک Definition چند option invented نسازید**.
- **Negative:** مقدار نامعتبر `<PACKAGE-DIFFERENT>` یا `PlanKind` به‌جای SKU، یک Package متعلق به Definition دیگر، یا Quantity بالاتر از سقف => `SelectedOptionNotOffered`/Invalid. **Handoff:** selected packageRef، وزن تعریف‌شده و quantity.

#### A03 — Overweight

- **Input:** `{bagRef:"<actual-bag-ref>",measuredWeightKg:27.5}`. `MeasuredWeightKg` به تنهایی کافی نیست؛ `chargeableWeightKg` باید تحت Rule معتبر محاسبه شود (مثل فقط وزن مازاد یا کل وزن نرخ‌گذاری‌شده). انتخاب یکی از این دو بدون Source Rule غلط است.
- **Pricing:** PerPiece = یک واحد overweight طبق Rule؛ PerKilogram = `verifiedChargeableKg × verifiedRatePerKg` با rounding/step واقعی Supplier.
- **Negative:** bagRef نامعتبر، وزن نامعتبر، allowance unknown، chargeable basis absent => `NeedsVerification`/no token. **Handoff:** measured weight/chargeable weight و evidence، نه فقط Quantity:int.

#### A04 — Oversize

- **Input:** `{dimensions:{lengthCm:...,widthCm:...,heightCm:...}}`؛ هر عدد >۰ و constraints واقعی.
- **Pricing:** PerPiece؛ اگر A03 هم‌زمان برای همان BagRef باشد، `ChargeCombination` باید از Rule واقعی معلوم باشد.
- **Negative:** size missing/oversize excluded، overlap double fee without verified rule. **Handoff:** Dimensions و coverage، approval Provider بعداً.

#### A05 — Cabin bag

- **Input:** `{quantity:1}` با cabin allowance per Pax/Coupon و operating aircraft restrictions.
- **Pricing:** PerPiece؛ ExistingServices و ticket stage برای تعداد مجاز مؤثر است.
- **Negative:** unknown cabin allowance، no capacity، count violation، Order scope. **Handoff:** cab-baggage kind distinct from checked bag.

#### A06 — Sports/special equipment

- **Input:** `{equipmentKind:"<authored-kind>",weightKg:12,dimensions:{...}}` فقط فیلدهای required فعلی Definition.
- **Pricing:** PerItem/PerPiece؛ نوع و محدودیت‌های اندازه/وزن از Authoring واقعی.
- **Negative:** arbitrary kind، embargo، oversized/overweight، supplier approval pending. **Handoff:** Specialized SSR/provider requirements.

#### A07 — Standard seat

- **Input:** `{kind:"seatMap",seatMapSelectionId:"<43-char-id>"}`؛ Source regenerated SeatMaps، نه decode SHA.
- **Source:** FlightFlow occupancy کامل، AirInfo layout matching، paid/free seat Product با نرخ معتبر.
- **Negative:** occupied، forged id، aircraft swap، missing commercial candidate/price، same Pax two seats. **Handoff:** SeatNumber, OrderTravellerId, FlightCapacityId, exact money, SeatHold later.

#### A08 — Preferred/exit-row seat

- **Input:** همان A07؛ برای exit row `exitRowTermsAccepted:true` در **فیلد افزایشی Evaluate Seat line**، نه در SeatMapsDto.
- **Source:** preferred characteristics و eligibility verified، cabin/child/infant/adult restrictions.
- **Negative:** child exit-row، false consent، cabin mismatch، source unknown. **Handoff:** seat category/accepted terms evidence.

#### A09 — ExtraSeat (EXST)

- **Input:** `{seatNumber:"12B",purpose:"<authored-purpose>"}` در generic offers؛ A07/A08-style generic SeatMap ID برای EXST فرض نکنید.
- **Source:** adjacent seat/capacity rules، BookingMethod، Special Ticket treatment.
- **Negative:** unavailable adjacency، false capacity، insufficient provider support. **Handoff:** specialized EXST physical/ticket obligation، not regular seat.

#### A10 — Cabin upgrade

- **Input:** `{targetCabinId:2,quoteRef:"<valid-provider-quote>"}` فقط در Quote-required Source؛ Fixed tariff مطابق authored rule.
- **Source:** `CouponUpgradable=false` => reject؛ true تنها شرط لازم، FareFamily/RBD/target cabin/exchange policy.
- **Negative:** wrong target، expired supplier quote، coupon partly unupgradeable، postissue exchange not supported. **Handoff:** target/coupon/ticket exchange routing، نه EMD arbitrary.

#### A11 — Free special meal

- **Input:** `{mealCode:"<authored-SSR-code>"}`، نه quantity؛ Free فقط با explicit filed PriceOrigin.Free یا verified fare rule.
- **Source:** Passenger/Meal restrictions، lead time، SSR.
- **Negative:** wrong meal code، catering cutoff، paid meal inferred free، same meal family. **Handoff:** zero-money evidence و confirmation requirements.

#### A12 — Paid preorder meal

- **Input:** `{menuItemRef:"MENU_PASTA_01",quantity:2}` تنها وقتی actual authored MenuRef.
- **Source:** real menu item option، catering stock/cutoff/price.
- **Negative:** arbitrary MenuItemRef، Qty too large، no trusted menu option، duplicate meal family. **Handoff:** item/qty/Pax/flight.

#### A13 — Pet in cabin

- **Input:** `{animalType:"<authored>",combinedWeightKg:7.1,carrierDimensions:{...},documentAcknowledgements:[...]}` طبق Definition.
- **Source:** allowed type, age, weight, operating carrier PETC constraints, quota and approval.
- **Negative:** young/unallowed animal، missing docs evidence، source capacity unknown. **Handoff:** specific Pet service, not baggage piece.

#### A14 — Pet in hold

- **Input:** A13 plus `{sizeBracket:"<authored-bracket>"}` conditional.
- **Source:** cargo hold aircraft/connection restrictions، bracket policy.
- **Negative:** size bracket mismatch، no hold provider، unsupported connection. **Handoff:** hold-type pet BookingMethod and Provider.

#### A15 — Wheelchair

- **Input:** `{assistanceSsrCode:"WCHR"}` فقط وقتی WCHR در AllowedSsrCodes است.
- **Source:** passenger/airport lead time and supplier ack.
- **Negative:** invalid SSR، airport cutoff، pending approval treated as confirmed. **Handoff:** SSR + not-yet-confirmed semantics.

#### A16 — Disability assistance

- **Input:** `{assistanceSsrCode:"<authored-code>"}`.
- **Source:** approved SSR and Airport/Carrier conditions؛ no guessed disability/medical facts.
- **Negative:** unknown SSR، outside window، unsupported Provider. **Handoff:** assistance booking requirements.

#### A17 — Medical equipment/Oxygen

- **Input:** `{equipmentCode:"<authored-code>",evidenceDocumentRefs:["<authorized-ref>"],oxygenUnits:1}` if oxygen.
- **Source:** verified approval/documents, oxygen volume/provider stock.
- **Negative:** missing document، expired approval، units overmax، source outage. **Handoff:** evidenceRefs only, no personal health content in token.

#### A18 — Bassinet

- **Input:** `{infantRef:"INF-1",guardianRef:"ADT-1"}`.
- **Source:** infant and adult association, age, cabin/layout eligibility, bassinet availability.
- **Negative:** forged guardian، infant too old، ineligible seat/cabin، source capacity absent. **Handoff:** linked infant/adult refs.

#### A19 — UMNR

- **Input (TARGET):** `{childRef:"CHD-1",guardianHandoffContactRef:"<verified-order-contact>",guardianPickupContactRef:"<verified-order-contact>"}`.
- **Source:** real Order contact records/permission, passenger age, connection/minor policy.
- **Negative:** only booleans true without contacts، invalid contacts، connection forbidden، no supplier confirmation. **Handoff:** verified contactRefs only; Stage1-contract compatibility task required.

#### A20 — Airport Lounge

- **Input:** `{airportId:...,facilityRef:...,timeWithOffset:"...",guestCount:0}` as required by spec.
- **Source:** actual station/terminal/timezone, provider appointment availability, guests pricing.
- **Negative:** wrong airport، invalid DST، guests fee unknown، source slot absent. **Handoff:** appointment/facility/scope.

#### A21 — Fast Track

- **Input:** `{airportId:...,facilityRef:...? ,timeWithOffset:...?}` conditional.
- **Source:** departure/arrival airport rule, time window/terminal, provider.
- **Negative:** invalid terminal، past window، no provider. **Handoff:** station/time/service method.

#### A22 — CIP Meet & Assist

- **Input:** `{airportId:...,facilityRef:...,timeWithOffset:"...",guestCount:...}` conditional.
- **Coverage:** Pax default؛ Order فقط با verified Definition/Provision/Supplier and one-fee rule.
- **Source:** bundled components, appointment, guest fee and provider support.
- **Negative:** Order-scope with per-pax cost without eligible rule، duplicate component charge، no slot. **Handoff:** one order-scoped service / no dummy Pax.

#### A23 — Priority boarding/check-in

- **Input:** `{optIn:true}` per Pax×Flight.
- **Source:** validated fare benefits and current existing services.
- **Negative:** false optIn، benefit already included، fare-benefits source missing => NeedsVerification. **Handoff:** SSR/priority as approved.

#### A24 — Wi-Fi

- **Input:** `{planCode:"WIFI_FULL_FLIGHT",deviceCount:1}`; PlanCode EXACT active DefinitionRef; `PlanKind` only descriptive.
- **Coverage:** Pax or true Order-scoped Plan; charge exactly once for Order unit.
- **Source:** actual aircraft eligibility, device cap, provider activation path.
- **Negative:** caller sends PlanKind instead of DefinitionRef، devices over max، provider unsupported، wrong Order coverage. **Handoff:** internal plan SKU, device count, actual Provider key internally.

### 12.3 منشأ Option و نحوه نمایش

| Variant | Verified option source / typed only | Default UI classification |
|---|---|---|
| A01,A05 | quantity typed bounded | Inline |
| A02 | DefinitionRef + PackageWeightKg، ۱ Choice per Definition | Inline |
| A03,A04 | bagRef/weight/dimensions typed، chargeable basis external | DetailsOnDemand یا NeedsVerification |
| A06 | authored equipment kind singleton، typed dimensions/weight | DetailsOnDemand |
| A07,A08 | SeatMap pricing by Pax/Flight | SeatMap |
| A09 | authored purpose + seat typed | DetailsOnDemand |
| A10 | authored target cabin + verified dynamic quote | RequiresExternalQuote |
| A11 | authored MealCode | Inline when facts complete |
| A12 | authored MenuItemRef singleton + qty | Inline or Details |
| A13,A14 | authored AnimalType/SizeBracket، typed dimensions | DetailsOnDemand |
| A15,A16 | authored SSR code list | Inline or Details according to approval |
| A17 | authored MedicalEquipment, verified docs | DetailsOnDemand |
| A18,A19 | verified TravellerRef/OrderContactRef | DetailsOnDemand |
| A20,A21,A22 | authored Airport/Facility, time typed, provider slot verified separately | DetailsOnDemand/NeedsQuote |
| A23 | optIn typed, fare benefit verified | Inline if evidence |
| A24 | PlanCode=DefinitionRef، device count | Inline if source provider supports |

منبع Unknown → صفر Choice معلوم، `sourceState=Unknown` و no purchasable ref؛ List نباید گزینه‌های فرضی FullFlight/VG/5kg ایجاد کند. بزرگ بودن گزینه‌ها → `isComplete=false` و `requiresDetails=true` نه «گزینه‌ای وجود ندارد».

## 13. ماتریس سناریوهای Acceptance و Anti-Regression — قبل از پیاده‌سازی نوشته شود

**معیار تست:** هر مورد ورودی، منبع حقیقت، انتظار HTTP/Readiness، اثر State و اثبات عدم فروش نامعتبر دارد. برای Code-only Closure، همه مواردی که Source/Ordering واقعی هنوز آماده نیستند باید با **Contract Fake خوانا** و تست Host/Mapper معتبر شوند؛ در گزارش با `[EXTERNAL_PENDING]` از اجرای Integration واقعی جدا شوند. نتایج CI/`dotnet test` بدون command، count و SHA ثبت‌شده معتبر نیستند.

### 13.1 Scenario suite تجارت دو جریان

| ID | Given / When | Expected (exact observable invariant) | Owner |
|---|---|---|---|
| FLOW-01 | Valid priced Offer, no Order, List | works under `SourceKind.Offer`, no invented OrderId | Ancillary/AirAvail |
| FLOW-02 | Same Offer, client creates Order, List with OrderId | SourceKind.Order, `PreOrder`, facts originate from Ordering | Ordering adapter |
| FLOW-03 | Order Issued, Order List | SourceKind.Order, `PostTicketed`, coupon/service facts read | Ordering adapter |
| FLOW-04 | Order from Offer changed afterwards | original Offer-based pricing not blindly accepted | Ordering adapter |
| FLOW-05 | 2 adults × 2 bounds × 2 flights | stable Pax×Scope candidates, no product cross-assignment | engine |
| FLOW-06 | Infant without seat entitlement | no paid/occupied seat invented for lap infant | seat engine |
| FLOW-07 | Order scope A22/A24 | single offer item/price; `travellerRefs=[]`, no fake Traveller | engine/Ordering contract |
| FLOW-08 | Order scope A01/A12/A13/A23 | no Candidate or hard NotEligible; forgery Evaluation rejected | engine |
| FLOW-09 | Order change to Coupon Used | corresponding post-ticketed candidate removed or NotEligible | adapter+engine |
| FLOW-10 | One Pax coupon Open, other Used | allowed only for Open Pax/Flight unit | adapter+engine |
| FLOW-11 | Order Refunded/Cancelled | no new sellable quote for affected services | adapter |
| FLOW-12 | Original Offer expired but Order valid | OrderShopping still works from Order facts; no stale AirOffer fallback | adapter |
| FLOW-13 | Order source API outage | explicit unavailable/error, no fabricated blank order | adapter |
| FLOW-14 | OrderCommercialVersion changes mid checkout | Resolve Stale, no Add success | Ordering/Resolve |
| FLOW-15 | Same `OfferId` and `OrderId` body | 422, no Source selected | public API |
| FLOW-16 | API caller for foreign orderId | 403/404, no order details leak | Auth |

### 13.2 List/Details/UI/Surface suite

| ID | Given / When | Expected | Owner |
|---|---|---|---|
| VIEW-01 | ۲۰ configured candidate rows | ۱ List response، no implicit ۲۰ Details | Query |
| VIEW-02 | A01 simple fixed quantity | List includes SelectionField/Qty/price preview; user can choose inline | mapper |
| VIEW-03 | A02 5kg/10kg independent Definitions | ۲ candidates/SKUs، selectable without fabricated options | mapper |
| VIEW-04 | A24 `ServiceDefinitionRef` plan | exact valid choice from authored spec | mapper |
| VIEW-05 | A13 with many forms | `DetailsOnDemand`, List not gigantic | mapper |
| VIEW-06 | Dynamic supplier menu offline | `optionSourceState=Unknown`, no fake VG/CH menu | Query |
| VIEW-07 | ۳۰ valid choices > inline budget | `isComplete=false,requiresDetails=true`, Details full list | mapper |
| VIEW-08 | One Details clicked | only candidate selected retrieved; no ۱۹ sibling Calls | host/front contract |
| VIEW-09 | Existing Offer-based Backoffice golden | all previously published wire paths/types kept | Backoffice mapper |
| VIEW-10 | Existing Offer-based API golden | `meta,data,included.bags,other-services` all intact | Api mapper |
| VIEW-11 | Existing OtaPanel golden | `travellerGroups`/`orderScopedServices` intact | Ota mapper |
| VIEW-12 | Agency API response | no `pointOfSaleId`,`internal SupplierId`,`quoteProviderKey`,`airlineOfficeId`,`PCC` | security |
| VIEW-13 | Public API 24 types | human-readable names/types/constraints/error codes per contract | Api mapper |
| VIEW-14 | Backoffice diagnostics | operator can see rule/Revision details under role | Backoffice mapper |
| VIEW-15 | Reusing cached Details after sourceVersion changes | re-fetch/invalidated; optionRefs stale | client/Query |
| VIEW-16 | ۲ eligible POS price differently | explicit provenance/no arbitrary first POS | Query |

### 13.3 Exact selection, Price, Basket, Money

| ID | Given / When | Expected | Owner |
|---|---|---|---|
| PRICE-01 | A01 Qty1 vs Qty2 same item | ۲ priced tokens different and totals 1× vs 2× | Pricing |
| PRICE-02 | same A01 for T1 and T2 | per Pax quote refs and OrderTravellerId distinct | Pricing |
| PRICE-03 | A24 `PerItem` with deviceCount=3 | price not accidentally tripled unless authored per-device | Pricing |
| PRICE-04 | A15 `PerPassenger`, no quantity field | effective quantity 1, exact line total | Pricing |
| PRICE-05 | A10 fixed upgrade `PerPassenger`, Qty absent | full quote if all rules valid (not null just for missing Quantity) | Pricing |
| PRICE-06 | A03 `PerKilogram`, chargeable basis source absent | Incomplete + no token, reason `ChargeableWeightNotDetermined` | Pricing |
| PRICE-07 | A03 source knows only measured weight | no invented excess kg by subtracting allowance without rule | Pricing |
| PRICE-08 | A03 trusted chargeable basis supplied | exact decimal weight×rate, negative/step reject | Pricing external contract |
| PRICE-09 | Tax Added vs Included | exact arithmetic once each, Type/Country/Station trace | Pricing |
| PRICE-10 | Fee per Order and ۳ selection lines | one shared Fee in basket only, no double charge | Pricing |
| PRICE-11 | PerTicket shared fee ۲ lines same ticket | once by real fee key and ticket ref | Pricing |
| PRICE-12 | Shared fee authority/amount unknown | basket total incomplete, no purchasable token for final charge | Pricing |
| PRICE-13 | Multi-currency with verified ROE | precision and rounding per published conversion | Pricing |
| PRICE-14 | ROE 9 decimal places | no 18,2 truncation or binary float | Pricing |
| PRICE-15 | zero price with explicit Free | price zero **and** isComplete true, valid currency evidence | Pricing |
| PRICE-16 | Null Price and unknown benefit | never `0.00` or `canPurchase=true` | Pricing |
| PRICE-17 | A10 CouponUpgradable=false | NotEligible + `CouponNotUpgradable` | Domain engine |
| PRICE-18 | CouponUpgradable=true, provider quote unavailable | NeedsQuote, no ref | Domain engine |
| PRICE-19 | One Line Rejected in ۳-Line basket | allReady=false, whole basket not committed | Evaluate/Ordering |
| PRICE-20 | duplicate exact candidate same Basket | 422 `DuplicateSelectionScope` (chosen rule) | Evaluate |
| PRICE-21 | same SeatID twice | reject duplicate | Evaluate |
| PRICE-22 | Qty=0 explicit | omit/remove line in UI, do not generate zero purchase | Evaluate |
| PRICE-23 | exact Price changes 1 minor unit | Resolve Stale and returns new price advisory | Resolve |
| PRICE-24 | Multiple same family services exceeds cumulative max | Basket conflict; no extra charge | engine |

### 13.4 Seat regression suite (complete)

باید تمام ۱۶ تست `S2-SEAT-01..16` فصل ۰۹ اجرا شوند. **افزون بر آنها:**

| ID | Given / When | Expected |
|---|---|---|
| SEAT-17 | Old seatMap OfferId available before Order; create Order | Source→Order mapping validated; seat id accepted only if same valid snapshot |
| SEAT-18 | Order-based SeatMap with missing `originPricedOfferId` | explicit incompatibility; no fabricated `flightOfferId` |
| SEAT-19 | Two seats same flight, same passenger; different Cabin | conflict before Add |
| SEAT-20 | Seat per flight in connection with two legs | distinct `FlightRef`/capacity; BoundId not mistaken for a physical Flight |
| SEAT-21 | Missing one FlightFlow FlightId in OccupiedSeats response | entire affected map/unpriced seat unavailable, not `AVAILABLE` |
| SEAT-22 | Seatmap and Ancillary generic joint Batch | mixed seat and baggage exact totals, two provider owners, Order handoff correct |
| SEAT-23 | Seat status AVAILABLE but candidate.Price null | fails after internal fix, null Id+Price and BLOCKED |
| SEAT-24 | Seat free with verified explicit entitlement | Price.Total `0` true and Id valid; not fake zero |
| SEAT-25 | Same 43-char id from foreign agency | 403/404 even with shared POS |

### 13.5 Token/Resolve/Auth/Concurrency suite

| ID | Given / When | Expected |
|---|---|---|
| SEC-01 | Alter a byte of encrypted Token | integrity fail; no price exposed |
| SEC-02 | Unknown `keyId`, key revoked | no acceptance; typed reason |
| SEC-03 | Expired Token | `Expired` not auto renew |
| SEC-04 | Token `nbf` future, clock skew | reject according to configured safety window |
| SEC-05 | Token Source=Offer used in Add Order | reject; must re-Evaluate order-scoped |
| SEC-06 | Token orderId=11 used on OrderId=12 | NotAuthorized/NotFound (safe), no price leak |
| SEC-07 | AgencyA token used by AgencyB same POS | reject |
| SEC-08 | Same agency, other Office without selling permission | reject |
| SEC-09 | Valid Token with OrderVersion old | Stale even before TTL |
| SEC-10 | Token PriceRevision old | Stale, no silent reprice |
| SEC-11 | Option removed after Evaluate | Stale, no arbitrary fallback |
| SEC-12 | Different quantity same OfferItemRef | different token/evaluation, exact price |
| SEC-13 | Different A12 MenuItem same Definition not authored | 422 |
| SEC-14 | Order-scoped A24 with Pax pseudo-ref | reject; genuine empty beneficiaries only |
| SEC-15 | Resolve endpoint without JWT | HTTP 401 |
| SEC-16 | JWT has user/API scope but not internal service scope | HTTP 403 |
| SEC-17 | JWT internal_service but non-allowlisted client | HTTP 403 |
| SEC-18 | Missing Allowlist config at startup | fail-closed access, no public route |
| SEC-19 | Same Ordering Add `idempotencyKey` + same payload | same result, exactly one OrderChange |
| SEC-20 | Same key + different payload | conflict, no partial duplicate |
| SEC-21 | Concurrent Order Change between Resolve and Persist | Optimistic Concurrency conflict; no stale OrderService |
| SEC-22 | Client sends supplierKey/currencyOverride | rejected/ignored under explicit schema, no injection |
| SEC-23 | Client sends raw selected price | trusted server ignores; no underpayment |
| SEC-24 | Very large Token/JSON duplicate fields | constrained validation, no DoS/ambiguous parse |
| SEC-25 | One invalid token in many | Resolve returns all statuses, `allValid=false`, Ordering no silent subset |

### 13.6 PostTicketed and S3 handoff

| ID | Given / When | Expected |
|---|---|---|
| POST-01 | Ticketed Order with fresh valid flight coupon | Eligible service appears only if PurchaseStage permits |
| POST-02 | Used/Flown segment | no add-on on consumed flight unless explicit supplier exception |
| POST-03 | Already issued A01 bag EMD | duplicate counting family blocked |
| POST-04 | Refund service but coupon Open | policy/waiver/source decides re-purchase; never assume yes |
| POST-05 | Priority already included fare | no paid duplicate quote |
| POST-06 | A10 ticket exchange needed | needs verified exchange/reshop support; no fake EMD route |
| POST-07 | CIP guest count fee unknown | no false basket total |
| POST-08 | Supplier hold fails after valid S2 Resolve | Stage3 status pending/failed truthfully; S2 Token not a Hold guarantee |
| POST-09 | Partial provider confirm | Order per-unit status and compensation belong Stage3/Ordering |
| POST-10 | EMD-A Bag, EMD-S standalone Lounge | metadata routed correctly, only Ordering/Ticketing issues actual doc |
| POST-11 | Wallet/Card/Agency credit/cash settlement | Pricing Shopping unaffected; Payment ownership in Ordering/JetPay |
| POST-12 | Ticketing/Payment call times out | no claim of issued/charged state from S2 |
| POST-13 | Order scoped A24 in Ordering TravellerId long NOT nullable | explicit `EXTERNAL_PENDING` until Consumer supports cardinality; no dummy ID |
| POST-14 | Only Order context endpoint unavailable | do not claim real PostOrder integration complete |

### 13.7 Coverage × Surface × Source combinatorial gate

- **For every A01–A24:** at least ۱ positive/eligible or fully documented external-block fixture، ۱ missing-field، ۱ wrong-option، ۱ source-unknown case where relevant. All ۲۴ have a traceable `VariantCode` and `CustomerSelectionContract` validation.
- **For each public surface** Backoffice/Api/OtaPanel: test Offer-based List, Order-based List, Details only-needed, Single Evaluate legacy, Batch Evaluate, SeatMap Offer legacy, SeatMap Order (if required context), Auth fail. PII/Provider secrets not leaked in Api/Ota.
- **All source outcomes** Accepted, Incomplete, Rejected, NeedsQuote, NeedsVerification, Stale, Expired, NotAuthorized, Unavailable, ConflictingOrderVersion. HTTP body status and HTTP code mapping must be snapshot-tested.
- **Golden comparison:** ۹ old S2-01 fixtures *semantic/value compare* for existing members؛ not merely path subset (current `S2GoldenContractTests` only checks property paths). Values intentionally fixed by Price/Seat Bugfix get an explicit expected change review.
- **Build and full discovered tests:** list assemblies, per-assembly discovered count/executed pass/fail/skipped, no abort، same SHA, CI link. A log stating "all tests pass" without counts/command/source commit not valid.

---


---

## 5. Required reports to write inside repo

- `reports/S2-PREFLIGHT-SOURCE-GAPS.md`: source-specific gaps, sibling patterns, proposed source adapters and auth proof.
- `reports/S2-IMPLEMENTATION-CONFORMANCE.md`: exact routes, all variant and test IDs, successful Seat ID trace, money+token+security proof, tests run with counts, external pending and final verdict.
- `docs/integration/S2-Ordering-Consumer-Contract.md`: consumer context/Add/Resolve and S3 handoff with CURRENT/TARGET/EXTERNAL labels.
- `docs/S2-Execution-Authority-2026-10-20.md`: explicit precedence vs Pack §07/08/09 and narrow original authority.
