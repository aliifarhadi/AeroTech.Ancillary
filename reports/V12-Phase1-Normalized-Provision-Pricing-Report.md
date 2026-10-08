# گزارش پایان فاز ۱ — Pack نسخهٔ v12.0 (Provision نرمال‌شده + Aggregate قیمت‌گذاری)

تاریخ: 2026-10-08 — مرجع: `docs/AeroTech-Ancillary-FINAL-v12.0/` (اسناد 00 تا 10)

```
PHASE_1_V12_READY_FOR_OWNER_AUDIT
Repository: AeroTech.Ancillary (branch k8s-stg)
Base SHA: 6b0a80ff708815ce3a6a957fbef3380b83a3f4cd
Implementation SHA: ندارد — همهٔ کار روی working tree و commit نشده است (commit/push فقط با تأیید Owner)
Changed files by Module: بخش ۳ (317 فایل جدید، 81 تغییریافته، 16 حذف‌شده)
Migration names: 20261008103942_V12Phase1NormalizedProvisionAndPricing (Command) ، 20261008103947_V12Phase1NormalizedProvisionAndPricingQuery (Query)
Legacy->new mapping counts & exceptions: بخش ۴ (بدون مغایرت؛ 53 کنترل یکپارچگی همه صفر؛ PricingUnit هر 31 تعریف با تأیید Owner ثبت شد)
Scenario tests: 184 passed / 0 failed / 0 skipped (نقشهٔ متدها در بخش ۵)
Build: dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental → exit 0 ، 0 Error ، 13 Warning (همه قدیمی، در Contracts/Framework)
CommandDb / QueryDb pending migration/model changes: هیچ («No changes have been made to the model since the last migration» برای هر دو context)
No Reservation/Hold/Get/Confirm changes: 36 فایل منجمد با sha256 یکسان؛ git diff روی مسیرهای Reservation خالی (بخش ۶)
No other repo changes: AirAvail / Ordering / FlightFlow / AirPrice / JetPay بدون هیچ فایل تغییرکرده از 2026-10-08 (بخش ۶)
Remaining gap register: بخش ۹
GO_WITH_CORRECTIONS
```

دلیل `GO_WITH_CORRECTIONS`: کد و مهاجرت و تست‌ها کامل و سبز است؛ نگاشت `PricingUnit` برای 31 تعریف موجود در همان روز با تأیید Owner اعمال شد (بخش ۱۰-الف). تنها مورد باز که با تصمیم Owner بسته می‌شود: شکستن قرارداد خواندن SQL در کپی commit نشدهٔ AirAvail.

---

## ۱. چه چیزی ساخته شد

مشکل گزارش‌شده: `AncillaryProvision` تخت بود و مثلاً 1000 تاریخ فروش باید در یک ستون ذخیره می‌شد. در v12:

| Aggregate | نقش | تغییر |
|---|---|---|
| `AncillaryServiceDefinition` | هویت تجاری سرویس | فیلد جدید `PricingUnit` (PerPassenger=1، PerRoom=2، PerItem=3، PerVehicle=4، PerSeat=5، PerPiece=6، PerKilogram=7). بعد از اولین انتشار برای همان `(OwnerAirlineId, ServiceDefinitionRef)` قابل تغییر نیست. |
| `AncillaryProvision` (N به ازای هر سرویس) | قانون واجد شرایط بودن و نتیجه | 25 مجموعهٔ فرزند typed؛ هر مقدار یک ردیف با Id واقعی. بدون JSON/CSV/EAV/matcher. Fee و PriceLine از آن خارج شد. Fulfillment دست‌نخورده. |
| `AncillaryPricing` (N به ازای هر Provision) | نسخهٔ قیمت | `Version, CurrencyId, FeeApplicationUnit?, Status(Draft/Active/Suspended/Retired)` + `AncillaryPricingLine(PTC?, AgeFromInclusive?, AgeToExclusive?, Category, Code?, Name?, CountryId?, StationAirportId?, Amount decimal(18,2))`. |
| `Supplier` | بدون تغییر | — |

فرزندان Provision: PassengerType، PointOfSale، Customer، CustomerType، Origin/Destination/Via Airport، RoutePair، Marketing/Operating Airline، FlightNumber، Flight، Aircraft، AirFare، AirFareType، FareFamily، FareBasis، CabinClass، Rbd، SeatNumber، SeatCharacteristic و چهار بُعد زمانی مشابه ATPCO:

- `ProvisionTravelDate` (تاریخ صریح مجاز — اثبات 1000 ردیف)
- `ProvisionSeasonalPeriod` (Cat 3)
- `ProvisionBlackoutPeriod` (Cat 11)
- `ProvisionDayTimeRestriction(DayOfWeek, StartTime?, EndTime?, Effect Allow|Deny)` (Cat 2)

قواعد قیمت: دقیقاً یک قیمت Active برای هر Provision (ایندکس یکتای فیلترشدهٔ SQL + `SwitchActivePricing` اتمیک)؛ Provision فعالِ Paid دقیقاً یک قیمت فعال دارد و Free/NotAvailable هیچ؛ برای هر کلید نرخ `(PTC, AgeFrom, AgeTo)` دقیقاً یک خط پایهٔ مثبت؛ بازه‌های سنی نیم‌باز مثل `[0,65)` و `[65,+∞)`؛ واحدهای غیرمسافری PTC/سن نمی‌پذیرند؛ جمع فقط «به ازای هر نرخ» محاسبه می‌شود.

API جدید Backoffice (همه زیر `Backoffice/v1`):

- `AncillaryPricings`: POST، PUT `{id}`، POST `{id}/Activate|Suspend|Reactivate|Retire|Revise`، GET `Paginated`، GET `{id}`
- `AncillaryProvisions/{provisionId}/TravelDates|SeasonalPeriods|BlackoutPeriods|DayTimeRestrictions` با POST و PUT/DELETE `/{rowId}`
- `AncillaryProvisions/{provisionId}/Publish` (انتشار اتمیک Provision + قیمت) و `/SwitchActivePricing`
- `AncillaryServiceDefinitions/{id}/AssignPricingUnit` (نگاشت صریح ردیف‌های قدیمی)

جمع مسیرها در سورس: 49 (25 مسیر قبلی بدون تغییر + 24 مسیر جدید). هیچ مسیر Preview/Simulate/Bulk/Evaluation وجود ندارد.

## ۲. کدهای خطای جدید (بازهٔ 16000–16999)

16005 تغییر همزمان (409) · 16209 تعارض PricingUnit (409) · 16210 PricingUnit تعیین نشده (409) · 16211 PricingUnit قبلاً تعیین شده (409) · 16307 سرویس فعال نیست (409) · 16308 ردیف شرط پیدا نشد (404) · 16309 ردیف تکراری (409) · 16310 قیمت فعال لازم است (409) · 16311 قیمت فعال مجاز نیست (409) · 16501 قیمت پیدا نشد (404) · 16502 قیمت نامعتبر (422) · 16503 تغییر وضعیت قیمت مجاز نیست (409) · 16504 Provision قیمت پیدا نشد (422) · 16505 Provision قیمت‌پذیر نیست (409) · 16506 عدم تطابق واحد (409) · 16507 قیمت فعال دیگری هست (409) · 16508 تعارض نرخ‌ها (409) · 16509 قیمت موردنیاز Provision فعال (409) · 16510 انتظار قیمت قبلی برقرار نیست (409).

## ۳. فایل‌های تغییرکرده به تفکیک ماژول

| ماژول | جدید | تغییر | حذف |
|---|---|---|---|
| Contracts/AeroTech.Messages/Ancillary/Enums | 3 | 0 | 0 |
| AeroTech.Ancillary.Domain | 34 | 9 | 8 |
| AeroTech.Ancillary.Application | 163 | 27 | 0 |
| AeroTech.Ancillary.Persistence | 29 | 7 | 1 |
| AeroTech.Ancillary.Query | 67 | 14 | 2 |
| AeroTech.Ancillary.Synchronizer | 1 | 4 | 0 |
| AeroTech.Ancillary.RestApi | 9 | 6 | 0 |
| tests/Domain.ConformanceTests | 3 | 6 | 1 |
| tests/Application.AcceptanceTests | 8 | 8 | 4 |
| **جمع** | **317** | **81** | **16** |

بدون تغییر: `Framework/`، ماژول ReferenceData، سایر پوشه‌های Contracts، `docs/`، و هر مسیر `AncillaryReservationAggregate`.
بهداشت 398 فایل `.cs`: 0 خط LF تنها، 0 کامنت، 0 ناسازگاری سبک namespace، 0 تب.

## ۴. مهاجرت و نگاشت داده

راهبرد: فقط افزایشی. ستون‌ها و جدول‌های v11 (ستون‌های لیستی JSON، `FeeCurrencyId`، `FeeApplicationUnit`، `TravelFrom/To`، `TimeFrom/To`، `ProvisionPriceLines`، `ReadModel.AncillaryProvisionPriceLines`) **حذف نشده‌اند** و فقط از مدل EF خارج شده‌اند.

هر migration: 1 `AddColumn` (PricingUnit، nullable)، 26 `CreateTable`، 24 دستور `INSERT INTO … SELECT` برای backfill. تنها عملیات غیرافزایشی: `DropIndex IX_ProvisionRoutePairs_AncillaryProvisionId` در Command که با ایندکس یکتای `(AncillaryProvisionId, Origin, Destination)` جایگزین شد (فقط ایندکس، بدون داده).

قاعدهٔ نگاشت شناسه‌ها (log نگاشت قدیم→جدید): `Pricing.Id = Provision.Id` · `PricingLine.Id = ProvisionPriceLine.Id` · `RoutePair.Id` بدون تغییر · شناسهٔ ردیف‌های فرزند = `ROW_NUMBER` به ترتیب `(Provision.Id, ترتیب در آرایه)` در هر جدول (قطعی و یکسان در Command و ReadModel). وضعیت قیمت: Draft→Draft ، Retired→Retired ، بقیه→Active.

شمارش روی دیتابیس dev (`DotAirAncillary`) — قبل = بعد، Command = ReadModel:

| قدیم (v11) | جدید (v12) | تعداد |
|---|---|---|
| PassengerTypeCodes | ProvisionPassengerTypes | 43 |
| PointOfSaleIds | ProvisionPointsOfSale | 6 |
| CustomerTypes | ProvisionCustomerTypes | 1 |
| OriginAirportIds / DestinationAirportIds | ProvisionOriginAirports / …Destination… | 29 / 3 |
| MarketingAirlineIds / OperatingAirlineIds | …MarketingAirlines / …OperatingAirlines | 3 / 8 |
| FlightNumbers | ProvisionFlightNumbers | 7 |
| AircraftIds | ProvisionAircraft | 11 |
| FareFamilyIds / CabinClassIds / RbdIds | …FareFamilies / …CabinClasses / …Rbds | 12 / 19 / 3 |
| SeatNumbers / SeatCharacteristicCodes | ProvisionSeatNumbers / …SeatCharacteristics | 2 / 19 |
| TravelFrom+TravelTo | ProvisionSeasonalPeriods | 6 |
| DaysOfWeek + TimeFrom/To | ProvisionDayTimeRestrictions (Allow) | 3 |
| ProvisionRoutePairs | همان جدول، همان Id | 63 |
| Provision پولی | AncillaryPricings (همه Active) | 69 |
| ProvisionPriceLines | AncillaryPricingLines | 97 |
| Customer، ViaAirport، Flight، AirFare، AirFareType، FareBasis | — | 0 (در دادهٔ dev وجود نداشت) |

مبالغ دقیقاً حفظ شد: IRR پایه 445,700,000.00 (28 خط) و مالیات 44,570,000.00 (28 خط)؛ USD پایه 2,114.00 (41 خط). وضعیت Provisionها: 69 Paid، 21 Free، 9 NotAvailable (همه Active). جدول‌های Reservation: بدون تغییر.

استثناها (طبق سند 06 نگاشت نشده و فقط گزارش می‌شوند):

| استثنا | تعداد در dev | رفتار |
|---|---|---|
| تعریف بدون `PricingUnit` | 31 از 31 بلافاصله بعد از مهاجرت؛ **0 بعد از نگاشت تأییدشده** | با `AssignPricingUnit` ثبت شد (بخش ۱۰-الف) |
| بازهٔ سفر یک‌طرفه (فقط From یا فقط To) | 0 | تبدیل نمی‌شود؛ ستون قدیمی حفظ است |
| پنجرهٔ زمانی عبور از نیمه‌شب (From > To) | 0 | تبدیل نمی‌شود؛ ستون قدیمی حفظ است |
| خط Tax/Fee بدون Code | 0 | عیناً کپی می‌شود ولی Revise آن 16502 می‌دهد (v12 کد را الزامی کرده)؛ راه اصلاح: قیمت جدید + Switch |

همین مهاجرت روی کپی `DotAirAncillary_V12Clone` (بازیابی از backup قبل از مهاجرت) هم بدون مغایرت اجرا شده بود.

## ۵. تست‌ها — نقشهٔ سناریو به متد

نتیجهٔ اجرا: Domain.ConformanceTests = 91 کشف / 91 موفق؛ Application.AcceptanceTests (روی SQL Server واقعی با migration هر سه context) = 93 کشف / 93 موفق؛ 0 ناموفق، 0 skip، بدون abort. (baseline روی `6b0a80f`: 92 + 76.)

پوشهٔ تست‌های acceptance: `tests/AeroTech.Ancillary.Application.AcceptanceTests/`

| سناریو | فایل.متد |
|---|---|
| FAM01 | Families/V12FamilyAcceptanceTests.`V12_FAM01_extra_baggage_is_filed_per_piece_and_per_kilogram_as_separate_services_with_every_outcome` |
| FAM02 | …`V12_FAM02_sports_equipment_has_its_own_identity_with_permitted_dates_an_allowance_and_a_price` |
| FAM03 | …`V12_FAM03_wheelchair_assistance_uses_ssr_metadata_is_free_or_not_available_and_never_fabricates_an_industry_code` |
| FAM04 | …`V12_FAM04_a_meal_is_filed_for_adults_and_children_by_flight_and_cabin_with_its_booking_metadata` |
| FAM05 | …`V12_FAM05_travel_insurance_is_filed_per_passenger_with_distinct_rates_below_and_from_sixty_five` |
| FAM06 | …`V12_FAM06_a_paid_seat_is_filed_per_seat_by_aircraft_seat_number_and_seat_characteristic` |
| FAM07 | …`V12_FAM07_an_airport_lounge_uses_the_verified_industry_code_and_files_passenger_rates_with_tax_and_fee` |
| FAM08 | …`V12_FAM08_priority_boarding_is_filed_per_passenger_with_day_and_time_windows_and_every_outcome` |
| FAM09 | …`V12_FAM09_fast_track_is_filed_by_point_of_sale_seasons_and_blackout_dates` |
| FAM10 | …`V12_FAM10_wifi_is_filed_per_item_by_aircraft_flight_cabin_and_fare_family_without_an_activation_workflow` |
| FAM11 | …`V12_FAM11_a_pet_service_carries_booking_metadata_a_route_and_flight_scope_and_a_fixed_price` |
| FAM12 | …`V12_FAM12_meet_and_assist_is_filed_by_point_of_sale_airport_and_time_per_passenger_or_per_item` |
| FAM13 | …`V12_FAM13_unaccompanied_minor_handling_is_filed_for_child_passengers_on_several_explicit_dates` |
| V12_N01 | Provisions/V12ProvisionAcceptanceTests.`V12_N01_one_thousand_travel_dates_are_separate_rows_and_one_date_is_edited_or_deleted_alone` + Domain `V12_N01_*` (2) |
| V12_N02 | …`V12_N02_seasons_and_blackouts_are_authored_row_by_row_and_survive_activation` + Domain `V12_N02_*` |
| V12_N03 | …`V12_N03_day_time_rows_allow_or_deny_a_local_weekday_window` + Domain `V12_N03_*` |
| V12_N04 | …`V12_N04_every_selector_survives_the_command_store_the_read_model_the_backoffice_and_a_draft_edit` + Domain `V12_N04_*` (4) |
| V12_N05 | …`V12_N05_an_unconditioned_provision_is_unrestricted_and_every_condition_table_is_typed` + Boundary `V12_C04_N05_*` + Domain `V12_N05_*` (2) |
| V12_N06 | …`V12_N06_an_active_provision_is_immutable_and_a_new_draft_supersedes_it_at_the_same_sequence` ، `V12_N06_two_concurrent_edits_of_one_draft_do_not_overwrite_each_other_silently` + Domain `V12_N06_*` (2) |
| V12_P01 | Pricings/V12PricingAcceptanceTests.`V12_P01_the_pricing_unit_is_fixed_for_a_service_identity_once_a_version_is_published` ، `V12_P01_a_pricing_takes_the_unit_of_its_service_and_a_wrong_unit_is_never_published` + Domain `V12_P01_*` (4) |
| V12_P02 | …`V12_P02_a_provision_owns_many_price_versions_and_only_one_is_active_even_under_a_race` (دو اتصال؛ بازنده 16507 از ایندکس فیلترشده) |
| V12_P03 | …`V12_P03_an_active_paid_provision_has_exactly_one_active_price_and_a_free_or_unavailable_one_has_none` |
| V12_P04 / P07 | …`V12_P04_P07_per_passenger_rates_are_filed_by_passenger_type_and_age_band_and_totalled_per_rate_only` + Domain `V12_P04_*` ، `V12_P07_*` |
| V12_P05 | …`V12_P05_a_non_passenger_unit_files_one_rate_in_one_currency_and_refuses_passenger_selectors` (6 واحد) + Domain `V12_P05_*` |
| V12_P06 | …`V12_P06_tax_and_fee_components_round_trip_with_category_code_name_country_and_station` + Domain `V12_P06_*` |
| V12_P08 | …`V12_P08_the_active_price_is_switched_atomically_and_a_stale_expectation_conflicts` (هر دو ترتیب Id + رقابت دو اتصال) + Domain `V12_P08_*` |
| V12_C01 | Suppliers/M1SupplierAcceptanceTests (2) ، P1SupplierAcceptanceTests (3) ، ServiceDefinitions/M1… (4) ، P1ServiceDefinitionAcceptanceTests (7) ، Backoffice/M1PaginatedAcceptanceTests (3) — بدون تضعیف |
| V12_C02 | Provisions/V12ProvisionLifecycleAcceptanceTests (6 متد `V12_C02_*`) ، Boundary `V12_C02_C04_the_rest_api_exposes_exactly_…` ، `V12_C02_every_command_service_…` ، Families/V12PriceStressAcceptanceTests (8 متد؛ جایگزین قوی‌تر PS1–PS6 و REQ در v11) |
| V12_C03 | Migration/V12MigrationAcceptanceTests.`V12_C03_a_v11_database_is_migrated_with_every_selector_price_line_status_and_row_identity_preserved` ، `V12_C02_C03_legacy_rows_stay_unmapped_until_an_explicit_pricing_unit_is_assigned_and_then_work_as_v12_rows` ، Boundary `V12_C03_the_v12_migrations_only_add_objects_…` |
| V12_C04 | Boundary/V12BoundaryAcceptanceTests (4 متد `V12_C04_*`؛ شامل قفل hash 36 فایل منجمد) + Holds/M1ReservationAcceptanceTests (9، دست‌نخورده) |
| V12_C05 | build تمیز + اجرای کامل هر دو پروژه + migration تازه (TestDatabase) + migration دیتابیس موجود (تست C03 و دیتابیس dev) + pending model changes |
| V12_C06 | Provisions/V12ProvisionAcceptanceTests.`V12_C06_two_drafts_racing_for_the_same_active_sequence_end_with_one_active_and_a_conflict` |

تست‌های v11 که جایگزین شدند (هر کدام با معادل قوی‌تر): `P1FamilyAcceptanceTests` → `V12FamilyAcceptanceTests`؛ `P1PriceStressAcceptanceTests` → `V12PriceStressAcceptanceTests`؛ `P1ProvisionAcceptanceTests` → `V12ProvisionAcceptanceTests` + `V12ProvisionLifecycleAcceptanceTests`؛ `P1BoundaryAcceptanceTests` → `V12BoundaryAcceptanceTests`؛ `P1ProvisionCriteriaConformanceTests` → `V12ProvisionConformanceTests`.

نکته دربارهٔ تست‌های منجمد Hold: این تست‌ها یک Provision لانژ را از fixture مشترک `M1Commands.LoungeProvision` فعال می‌کنند. چون در v12 فعال‌سازی Provision پولی بدون قیمت فعال ممنوع است، پیش‌فرض این fixture را Free کردم (کنترل‌های Hold به disposition/قیمت نگاه نمی‌کنند). فایل‌های تست منجمد بایت‌به‌بایت بدون تغییرند.

## ۶. شواهد

- **Build**: `dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental` → 0 Error، 13 Warning (همان 13 هشدار baseline).
- **Pending model**: `dotnet ef migrations has-pending-model-changes` برای `AncillaryDbContext` و `AncillaryQueryDbContext` → «No changes».
- **دیتابیس dev**: `database update` برای هر سه context (Persistence، Query، ReferenceData) → Done؛ reconcile قبل/بعد بدون مغایرت.
- **Reservation**: `sha256sum -c frozen-hashes.txt` → 36 OK / 0 ناموفق؛ `git diff --name-only 6b0a80f` و `git status` هیچ مسیر Reservation/Holds ندارند؛ جدول‌های Reservation در migration لمس نشده‌اند.
- **مخزن‌های دیگر**: AirAvail (318f7be)، AeroTech.Ordering (7508b53)، AeroTech.Ordering.Final (cd50a2a)، FlightFlow (d2180b2e)، AirPrice (1b41f08)، JetPay/AeroTech.JetPay: 0 فایل تغییرکرده از 2026-10-08. (فقط فایل‌های cache نادیده‌گرفته‌شدهٔ dev-server در دو مخزن فرانت شما تغییر کرده؛ `git status` آن‌ها تمیز است.)
- **Smoke زنده روی HTTP واقعی با توکن Backoffice** (host جانبی، پورت 5299، بعد از اجرا متوقف شد):
  - روی کپی مهاجرت‌شده: 44 از 44 — خواندن 31 تعریف / 99 Provision / 69 قیمت فعال / 97 خط؛ جریان کامل نوشتن (تعریف سرویس با واحد → Provision با تاریخ/فصل/blackout/روز-ساعت → POST/PUT/DELETE ردیف‌ها → قیمت → Publish → Revise → Switch → AssignPricingUnit) و خطاهای 401/404/409/422 مورد انتظار.
  - روی دیتابیس dev: فقط خواندن، 7 از 7 (هیچ دادهٔ آزمایشی به کاتالوگ dev اضافه نشد).
  - OpenAPI: 59 عملیات = 48 authoring/hold + Ping + 10 مسیر Syncer قبلی ReferenceData؛ 0 مسیر ممنوع.

## ۷. تغییرات شکنندهٔ قرارداد (برای تأیید Owner)

مصرف‌کنندهٔ فرانت برای مسیرهای Ancillary در مخزن‌های فرانت پیدا نشد؛ با این حال:

1. `POST/PUT AncillaryServiceDefinitions`: فیلد اجباری جدید `pricingUnit`.
2. `POST/PUT AncillaryProvisions`: بلوک `fee` حذف شد (به `AncillaryPricings` رفت)؛ در `travel` فیلدهای `travelFrom/travelTo/daysOfWeek/timeFrom/timeTo` حذف و `travelDates/seasonalPeriods/blackoutPeriods/dayTimeRestrictions` اضافه شد.
3. `GET AncillaryProvisions/{id}`: هر لیست حالا ردیف `{id, value}` است و نام‌ها عوض شده (`passenger.passengerTypes`، `sales.pointsOfSale`، `travel.originAirports`، …)؛ `priceLines` و `feeCurrencyId` حذف شد.
4. Grid لیست Provision: ستون‌های Amount/Currency حذف و شمارش Dates/Seasons/Blackouts/Day-Time اضافه شد.
5. فعال‌سازی Provision پولی بدون قیمت فعال حالا 16310 است (قبلاً قیمت داخل Provision بود).
6. **AirAvail**: کپی commit نشدهٔ M1 در AirAvail با SQL مستقیم `ReadModel.AncillaryProvisions.FeeCurrencyId/FeeApplicationUnit` و `ReadModel.AncillaryProvisionPriceLines` را می‌خواند. این ستون‌ها باقی‌اند ولی برای ردیف‌های جدید/ویرایش‌شده دیگر پر نمی‌شوند. AirAvail را دست نزدم.

## ۸. تصمیم‌ها و انحراف‌های آگاهانه

| موضوع | تصمیم | دلیل |
|---|---|---|
| ایندکس یک-قیمت-فعال | `(AncillaryProvisionId, Status)` یکتا با فیلتر `[Status] = 2` | معنای یکسان با سند؛ وجود Status باعث می‌شود EF دو UPDATE عملیات Switch را به ترتیب درست بفرستد (در هر دو ترتیب Id تست شده) |
| `PricingUnit` nullable | فقط برای ردیف‌های قدیمی نگاشت‌نشده | سند 06: «safely nullable»؛ ردیف جدید همیشه مقدار دارد و Activate/Revise بدون آن 16210 می‌دهد |
| مسیرهای اضافه بر فهرست سند 05 | `Publish` و `AssignPricingUnit` | اولی همان «coordinated catalog publishing» سند 05 است؛ دومی سازوکار نگاشت تأییدشدهٔ سند 06 |
| اعتبارسنجی قیمت | هنگام نوشتن و دوباره هنگام فعال‌سازی | سخت‌گیرانه‌تر از «رد هنگام فعال‌سازی»؛ Draft نامعتبر ذخیره نمی‌شود |
| طول FareBasis | 64 (سند: 10 «verify») | مطابق سورس Ordering |
| کد مشخصهٔ صندلی | uppercase ذخیره می‌شود | یکسان با شمارهٔ صندلی/پرواز |
| RoutePair تکراری | همان جفت مرتب، یا معکوس وقتی یکی BothDirections است | جلوگیری از ابهام |
| خطاهای یکتایی/همزمانی | در `AncillaryUnitOfWork` به 16005/16507/16306/16204/16309 ترجمه می‌شوند | سند 05: خطای دامنه به‌جای نشت خطای DB |

Benchmark AirPrice (`k8s-stg@1b41f08`، فقط خواندن): در `AeroTech.AirPrice.Domain/AirFareAggregate`: `ValueObjects/Blackouts/{BlackoutsRule, TravelBlackoutDateRange}`، `ValueObjects/DayTime/{DayTimePermissionRule, DayTimePermissionTimeRange}`، `ValueObjects/Seasonality/{SeasonalityRule, PermittedTravelDateRange}`، `Arguments/{Blackouts, DayTimePermission, Seasonality}`، `ChainHandlers/Reservation/{BlackoutChecker, DayTimeChecker, SeasonalityChecker}`؛ `AeroTech.AirPrice.Persistence/AirFareAggregate/AirFareEntityTypeConfiguration.cs`؛ `AeroTech.AirPrice.Domain/AirChargeAggregate/{AirCharge, Entities/AirChargeCondition}`.
گرفته شد: قانون typed و جدول فرزند نرمال با Id. گرفته **نشد**: chain checkerهای زمان اجرا (evaluator در فاز ۱ ممنوع است)، validatorهای ناقص، و مقایسهٔ `==` بین Entityها (باگ بازگشتی `Entity.Equals` ↔ `operator ==` در `Framework/AeroTech.Framework.Core/Domain/Entities/Entity.cs` این مخزن هنوز هست؛ ردیف‌های v12 با `SameAs` و Id مقایسه می‌شوند).

## ۹. Gap register

1. نگاشت `PricingUnit` برای 31 تعریف موجود dev — **بسته شد** (2026-10-08، با تأیید Owner؛ بخش ۱۰-الف).
2. قرارداد خواندن AirAvail (بند ۶ بخش ۷) — تصمیم Owner.
3. ویرایش همزمان **یک ردیف فرزند** از دو درخواست: last-write-wins (فقط RowVersion ریشه و ایندکس‌های یکتا محافظت می‌کنند).
4. بازنشسته/جایگزین شدن Provision، قیمت فعال آن را خودکار Retire نمی‌کند (قیمت به‌عنوان snapshot می‌ماند و دستی قابل Retire است).
5. بازهٔ سفر یک‌طرفه و پنجرهٔ زمانی عبور از نیمه‌شب v11 تبدیل نمی‌شوند (0 مورد در dev؛ در v12 هم `StartTime ≤ EndTime` الزامی است، یعنی پنجرهٔ شبانه باید دو ردیف باشد).
6. خط Tax/Fee بدون Code از v11 قابل Revise نیست (0 مورد در dev).
7. برابری Id ردیف‌های backfill بین Command و ReadModel به همگام بودن دو store قبل از مهاجرت متکی است (در dev و clone برقرار بود و reconcile آن را کنترل می‌کند).
8. شناسه‌های مرجع (ارز، فرودگاه، POS، مشتری، …) در فاز ۱ فقط از نظر شکل کنترل می‌شوند، نه وجود در ReferenceData (مثل v11).
9. ستون‌ها/جدول‌های قدیمی v11 هنوز در DB هستند؛ حذف‌شان باید فاز جدا با تأیید Owner باشد.
10. هیچ evaluator/matcher، stock، adapter، FX یا تغییر Reservation ساخته نشده (فاز ۲ و ۳).

## ۱۰. موارد نیازمند تصمیم Owner

**الف) نگاشت `PricingUnit` — تأیید Owner در 2026-10-08 و اعمال‌شده روی دیتابیس dev:**

| ServiceDefinitionRef | واحد ثبت‌شده | مبنا |
|---|---|---|
| XBAG_PIECE_23KG، SPORT_BIKE، SPORT_DIVING، SPORT_SKI | PerPiece | واحد مقدار Piece |
| XBAG_WEIGHT_5KG، XBAG_WEIGHT_10KG، XBAG_WEIGHT_20KG | PerItem (انتخاب صریح Owner) | بستهٔ وزنی ثابت (min = max) با قیمت کل بسته؛ قیمت‌های فعلی بدون تغییر درست می‌مانند |
| SEAT_SELECTION | PerSeat | — |
| WIFI_FULL_FLIGHT، WIFI_MESSAGING | PerItem | یک pass |
| PET_IN_CABIN، PET_IN_HOLD | PerItem | به ازای هر حیوان/قفس |
| LOUNGE_CIP_IKA، LOUNGE_DOTAIR_IKA، LOUNGE_DOTAIR_THR | PerPassenger | — |
| MEAL_CHML، MEAL_PREORDER_HOT، MEAL_VGML | PerPassenger | نرخ بر اساس PTC |
| INS_DOMESTIC، INS_TRAVEL_BASIC، INS_TRAVEL_PLUS | PerPassenger | نرخ سنی |
| FASTTRACK_IKA، PRIORITY_BOARDING، UMNR_SERVICE | PerPassenger | — |
| ASSIST_STRETCHER، ASSIST_WCHC، ASSIST_WCHR، ASSIST_WCHS | PerPassenger | — |
| CIP_ARRIVAL_IKA، CIP_DEPARTURE_IKA، MAAS_IKA | PerPassenger (پیشنهاد پذیرفته‌شده) | سند 09: «PerPassenger or PerItem as approved» |

اعمال: 31 فراخوانی `POST Backoffice/v1/AncillaryServiceDefinitions/{id}/AssignPricingUnit` با توکن Backoffice Owner روی host جانبی (پورت 5299، بعد از اجرا متوقف شد) — 31 موفق، 0 ناموفق.

نتیجهٔ کنترل‌شده در دیتابیس dev (Command = ReadModel): تعریف‌ها 19 PerPassenger، 7 PerItem، 1 PerSeat، 4 PerPiece، 0 بدون واحد؛ قیمت‌ها 35 PerPassenger، 14 PerItem، 12 PerSeat، 8 PerPiece (جمع 69، همه Active)؛ 0 قیمت با واحدی غیر از واحد سرویس خودش؛ مبالغ بدون تغییر (IRR پایه 445,700,000.00 و مالیات 44,570,000.00؛ USD پایه 2,114.00؛ 97 خط). واحد ثبت‌شده برای این هویت‌های منتشرشده دیگر از API قابل تغییر نیست (16209/16211).

**ب)** تصمیم دربارهٔ قرارداد خواندن AirAvail (بند ۶ بخش ۷).

**ج)** تأیید commit و push (هیچ‌کدام انجام نشده).

**د)** دیتابیس `DotAirAncillary_V12Clone` و فایل backup `…\MSSQL\Backup\DotAirAncillary_v11_before_v12.bak` روی SQL Express باقی است (نقطهٔ بازگشت v11)؛ هر وقت بگویید حذف می‌کنم.

فاز ۲ (stock) و فاز ۳ (reservation) شروع نشده و منتظر مجوز صریح Owner است.
