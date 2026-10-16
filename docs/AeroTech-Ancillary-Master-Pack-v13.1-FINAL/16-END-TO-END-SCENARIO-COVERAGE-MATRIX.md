# 16 — سناریوهای E2E و ماتریس تصمیم دامنه

این فهرست **رفتارهای قابل مشاهده و قراردادی** است، نه دستور نوشتن تست یا انتخاب دیتابیس. هر مورد باید نتیجه روشن (Selectable/Blocked/Pending/Confirmed/Cancelled/Refund) و Owner مشخص داشته باشد. ۲۴ Variant در فصل 06 جدا پوشش داده شده‌اند؛ این جدول چالش‌های بین‌سرویسی و چرخه عمر را اضافه می‌کند.

## 16.1 Authoring/Commercial (S1)
| # | سناریو | رفتار الزامی |
|---|---|---|
| C01 | ایجاد Definition هر یک از ۹ Profile | فقط spec همان Profile؛ Owner/Supplier معتبر |
| C02 | ارسال BaggageRule به Meal form | schema reject؛ no hidden columns |
| C03 | ویرایش Draft Seat موجود | Backend همان DTO/Route؛ front working unchanged |
| C04 | انتشار Provision بدون active pricing (Filed) | reject/pending publish، no sellable offer |
| C05 | Free Service با PriceOrigin.Free معتبر | Zero with evidence، not zero fallback |
| C06 | Price revise after Publish | old snapshot price stale؛ new active revision |
| C07 | POS null/empty vs nonempty | null/empty unrestricted sale rule؛ nonempty trusted POS only |
| C08 | Date range/blackout/seasonality/daytime | blocked out-of-period؛ timezone-aware |
| C09 | Country/station/TaxCode fee rules | monetary components correct and isolated |
| C10 | nonSeat Local Count/Weight/Slot policy | only verified configured stock consumable |
| C11 | No Seat InventoryPolicy | Seat Definition/Provision/Pricing work independently |
| C12 | A09 ExtraSeat special DocumentRouting | no regular seatmap cell substitution |

## 16.2 Shopping / Options / Pricing (S2)
| # | سناریو | رفتار الزامی |
|---|---|---|
| S01 | ۲ Pax ADT/CHD × ۲ Bound | per Pax/Flight/Portion offers and prices correctly |
| S02 | Included bag only one coupon | upsell extra only where allowed; no duplicate allowance |
| S03 | A01 Qty1 vs Qty2 | same CandidateRef permissible; different pricedSelectionRef and exact totals |
| S04 | A02 5kg vs 10kg package | real optionRef/weight package; different price and selection refs |
| S05 | A03 measured weight absent | total unknown; no pricedSelectionRef |
| S06 | A04 oversize + A03 overweight | charge combination follows filed rule, not arbitrary double fee |
| S07 | A11 SSR Free vs A12 Paid Menu | separate Definition/PriceOrigin, no inferred free paid meal |
| S08 | A12 MenuChoice source outage | source unknown/not verified; no fake option |
| S09 | Pet cabin vs hold, weight, docs | typed constraints + possible provider pending |
| S10 | Wheelchair/UMNR/medical | required PTC/SSR/guardian or medical evidence, no fake approval |
| S11 | CIP Lounge facility/time guests | valid appointment/guestCount, source capacity if required |
| S12 | A24 plan/device cap | planRef real, no arbitrary plan code accepted |
| S13 | A10 dynamic cabin Upgrade | NeedsQuote until exact ProviderQuote expiry verified |
| S14 | Orders with Order-scoped service | TravellerId null valid only with true Order-scope and one total |
| S15 | Fare benefit makes seat free | explicit validated zero Price, not missing-Candidate fallback |
| S16 | no Seat Candidate with physically free seat | SeatMaps same DTO; blocked, price/id null |
| S17 | OccupiedSeats partial return for F1,F2 | fail closed; missing F2 != empty seats |
| S18 | aircraft swap, layout mismatch | stale map not shown as sellable |
| S19 | Exit row child, disabled eligibility | blocked per rules, no fake source |
| S20 | 2 POS with different Seat rates | no price cross contamination, no random first POS |
| S21 | Foreign Agency uses another `offerItemRef` | reject/not leak existence |
| S22 | Expired/pricing-revised pricedSelectionRef | stale/expired; new Evaluate required |
| S23 | Currency/ROE precision & charges | unit vs Order fee correct; tax code/country/station kept |
| S24 | Post-ticketed Shopping | trusted Order context, only eligible future services |
| S25 | Coupon used/flown/refunded | post-order applicability correct; no offer for consumed service |
| S26 | Two same Definition different selection instances | distinct purchase refs, no over-dedup |
| S27 | No Option enum in source | typed constraints, `optionSourceState=Unknown`; not empty-discount |

## 16.3 Hold/Confirm/Supplier (S3)
| # | سناریو | رفتار الزامی |
|---|---|---|
| R01 | Group A01 Pax1 + A12 Pax2 + A22 Order | one HoldId, three Units, exact coverage |
| R02 | Request replay same idempotency key | same refs and result; no double hold |
| R03 | Same key different payload | explicit conflict; no side effect |
| R04 | Two simultaneous last-slot/local count | at most one Unit validly Held |
| R05 | ExternalHoldConfirm success | ProviderUnitRef saved, confirm after evidence |
| R06 | Supplier timeout after physical hold | PendingReconciliation, retries same op id, no duplicate booking |
| R07 | Manual provider awaiting call | Unit Pending until approved outcome, not fake Confirmed |
| R08 | Supplier DirectConfirm (no hold) | pending/commit after payment gate; compensatable if fails |
| R09 | Partial provider success group | per-Unit truth and `Partial` outcome |
| R10 | Confirm only subset | successful units Confirmed, other Units unchanged/pending |
| R11 | Expired Hold / extend unsupported | no unauthorized confirm/extension |
| R12 | Release group and partial release | only Held units released; stock returned once |
| R13 | Supplier cancel fails asynchronously | Pending status; no unconditional Cancelled |
| R14 | Seat paid physical plus commercial | FlightFlow seat HoldId/SeatHoldReference alongside Ancillary Unit; not conflated |
| R15 | Seat EXST (2 occupied seats) | multiple physical refs & ticket action coordinated |
| R16 | Post-order add-on to ticketed Order | new OrderService/Unit and correct EMD/payment |
| R17 | Duplication same OrderServiceId | reject double reservation; recovery idempotent |
| R18 | Order-scoped Unit with null Pax | valid if actual Supplier/Rule allows, no dummy Traveller |

## 16.4 Cancel / Penalty / Document / Financial
| # | سناریو | رفتار الزامی |
|---|---|---|
| F01 | cancel before Confirm | Release, not CancelConfirmed; document/refund separate |
| F02 | voluntary after Confirm pre-EMD | ServiceCancellationQuote then Cancel; proper payment handling |
| F03 | paid bag EMD-A partially refunded | coupon-linked document action, once-only amount |
| F04 | Lounge EMD-S standalone | correct standalone document handling |
| F05 | A10 ticket exchange | not emitted as arbitrary EMD |
| F06 | nonrefundable yet cancellable | CanCancel independent of CanRefund; net refund may 0 |
| F07 | refundable policy with no penalty rule | NeedsManualReview/ProviderQuote, not auto 0 penalty |
| F08 | fixed penalty, tax partially refundable | amount with currency and tax components, correct net |
| F09 | no-show after cutoff | provider rule/clock timezone, not normal voluntary refund |
| F10 | airline disruption + waiver evidence | involuntary reason, waiver only if proof |
| F11 | used/consumed service | cannot auto refund without filed waiver |
| F12 | partial cancellation one of three Units | unrelated services and documents untouched |
| F13 | expired/changed cancellation quote | new preview/approval, no silent extra penalty |
| F14 | Payment wallet + card split | refund by original funding liability, Payment owner |
| F15 | agency credit/cash airport office | net settlement obligation, not forced online card |
| F16 | voucher instead of refund | follow FormOfRefund/issuer document policy |
| F17 | supplier cancel timed out | PendingReconciliation, do not double refund or claim success |
| F18 | duplicate refund request | same payment refs; no double payment |
| F19 | cancel Seat + refund Seat fee | FlightFlow physical cancel, Ancillary commercial cancel, Ticketing/Payment financial actions separately |
| F20 | flight cancellation with 3 suppliers | selected partial compensation, PNR/coupon/ancillary history preserved |
| F21 | interline/codeshare operating carrier restriction | quote/provider rule not invented; blocked/manual if unsupported |
| F22 | fail mid doc void or processor refund | OrderChange pending and recoverable; Ancillary may already Cancelled truthfully |

## 16.5 Coverage acceptance rule
برای هر سناریو در گزارش Agent تنها outcome `Supported/RequiresExternal/NotSupportedByCurrentSupplier`، **Owner of fact**، Route involved و دلیل لازم است. **Pack هیچ ابزار/سبک/تعداد تست اجباری نمی‌کند.** `NotSupportedByCurrentSupplier` می‌تواند نتیجه درست باشد، ولی UI نباید قابلیت unsupported را قابل فروش/تضمین‌شده نشان دهد.
