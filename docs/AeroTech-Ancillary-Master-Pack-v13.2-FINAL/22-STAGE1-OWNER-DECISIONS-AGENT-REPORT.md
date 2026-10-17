# 22 — هشت تصمیم قطعی درباره گزارش Agent در آغاز Stage 1

**وضعیت:** تصمیم اجرایی v13.2؛ «سؤال باز» نیست. مبنا: `CLAUDE.md` روی `AeroTech.Ancillary/k8s-stg@9aca1cd` و سورس Seat Provision و Ch04/05/13/21. **فقط Ancillary Repo، Stage1؛ مرحله ۲ و ۳ را باز نکن.**

## تصمیم ۱ — CancellationTerms و Rule2 CLAUDE
**رد پیشنهاد استثنا.** `CLAUDE.md` قانون ۲ نوشته: «One write path per aggregate. No endpoint/use case per child entity/rule group.» پس `GET/PUT /Backoffice/v1/AncillaryProvisions/{id}/CancellationTerms` که v13.1 درخواست کرده بود **لغو و ممنوع** است.

`CancellationRules` یک لیست child داخل `AncillaryProvision` و در قرارداد **nested `Settlement.CancellationRules?`** است. `ProvisionSettlementInput` امروز ۴ member دارد: `ReissueRefund`, `FormOfRefund`, `Commissionable`, `InterlineSettlement`. تنها یک member اختیاری trailing `CancellationRules: IReadOnlyList<ProvisionCancellationRuleInput>? = null` به مدل مشترک اضافه شود. در top-level `SeatProvisionRequest` هیچ member جدید اجباری یا route/field حذف یا جابه‌جایی نده. از همان `DefineAncillaryProvisionService` و `ChangeAncillaryProvisionService` استفاده شود؛ child CRUD مستقل و dual write ممنوع. `GET /.../Provisions/{pid}/Form` و generic `GET /AncillaryProvisions/{pid}/Form` read-back همان full PUT body را در `Settlement` منعکس کنند.

**قانون backward compatibilityِ صریح:** Create `null/missing/[] => no filed rules`. Change `null/missing => preserve current rules`; `[] => explicitly clear current rules`؛ non-empty => replace entire list. این قرارداد افزایشی به Seat قدیمی اجازه می‌دهد body قبلی را بفرستد و هیچ Rule جدیدی به‌طور ضمنی پاک نشود. هنگام Read اگر فهرست خالی است، فیلد را از JSON Seat حذف کنید؛ اگر nonempty است قابل round-trip باشد. فقط Draft قابل Change است؛ Published Rule فقط از مسیر Revision فعلی Provision قابل تغییر خواهد بود. Rule در relational child table (نه JSON/`nvarchar(max)`) ذخیره می‌شود. Aggregate در همان Define/Change Validate می‌کند. از nested `Settlement` استفاده کن، نه Aggregate دوم.

### نمونه افزایشیِ سازگار با Seat (فقط بخش مشترک `settlement`)
```json
{
  "settlement": {
    "reissueRefund": 1,
    "formOfRefund": 1,
    "commissionable": false,
    "interlineSettlement": false,
    "cancellationRules": [
      {
        "ruleRef": "BAG-VOL-24H",
        "reasonKind": 1,
        "beforeStartMinutesFrom": 1440,
        "beforeStartMinutesTo": null,
        "penaltyKind": 2,
        "penaltyFixedAmount": 10.00,
        "penaltyPercentage": null,
        "penaltyCurrencyId": 978,
        "percentageBase": null,
        "waiverPermitted": true,
        "sourceReference": "CARRIER-FILED-TERMS-2026"
      }
    ]
  }
}
```
- `settlement` **دقیقاً همان شیء موجود** در تمام Provision requestهاست؛ این fragment جایگزین کل request نیست. برای Seat فعلی body بدون `cancellationRules` همچنان معتبر است. `Change` بدون این member یعنی preserve، با `[]` یعنی clear؛ GET فرم وقتی list موجود است آن را برمی‌گرداند.
- مبلغ/ارز/کد و SourceReference بالا **فقط نمونه قرارداد هستند، نه Rule واقعی Carrier**؛ در تولید واقعی بدون منبع معتبر Filed Rule ایجاد نکنید.

## تصمیم ۲ — CancellationRule Field/Enum/Window — هیچ چیز حدسی نماند
فیلدها و نوع‌ها عین Ch13.2 هستند: `RuleRef:string required`, `ReasonKind:CancellationReasonKind`, `BeforeStartMinutesFrom:int? >=0` inclusive, `BeforeStartMinutesTo:int? >0` exclusive, `PenaltyKind:PenaltyKind`, `PenaltyFixedAmount:decimal?`, `PenaltyPercentage:decimal?`, `PenaltyCurrencyId:int?`, `PercentageBase:CancellationPercentageBase?`, `WaiverPermitted:bool`, `SourceReference:string?`.

`CancellationPercentageBase.ServiceBase=1`, `ServiceTotal=2`; فقط برای Percentage واجب؛ اگر در سورس همین enum با همان semantics موجود بود duplicate نساز. `PenaltyKind.None=1`, `FixedAmount=2`, `Percentage=3`, `ProviderQuoted=4`, `ManualReview=5`. `CancellationReasonKind.Voluntary=1`, `Involuntary=2`, `SupplierFailure=3`, `NoShow=4` اگر کد نوع معادل ندارد. `NoShowApplicability` حذف (ReasonKind.NoShow معادل است). `AppliesFromUtc`/`AppliesUntilUtc` absolute حذف (Rule reusable است). NoShow فقط با evidence وضعیت واقعی استفاده می‌شود. اگر زمان مرجع flight/service معتبر نیست AutoQuote ممنوع. Rules با Reason+Time-window همپوشانی نداشته باشند؛ Domain guard هنگام authoring.

## تصمیم ۳ — Publish بدون CancellationRules
**بله، مجاز است.** عدم وجود Policy جریمه، مانع انتشار/فروش نیست و **هرگز Penalty=0 یا FullRefund نتیجه نمی‌شود**. در Stage3: ProviderQuoted معتبر اگر Supplier quotes دارد؛ در غیر این صورت `NeedsManualReview`، مگر waiver/zero penalty صریح و قابل اثبات وجود داشته باشد. `ReissueRefund=NonRefundable` ممکن است Base غیرقابل استرداد باشد اما Tax/EMD/مصرف واقعی همچنان بررسی Ordering/Ticketing لازم دارد.

## تصمیم ۴ — هشت Profile با حداقل duplication
**۵ Request Shape برای ۸ Profile:**
1. `BaggageProvisionRequest = common + BaggageApplication`
2. `PetProvisionRequest = common + PetRule`
3. `AssistedTravelProvisionRequest = common + AssistedTravelRule`
4. `AirportServiceProvisionRequest = common + AirportServiceRule`
5. `StandardProvisionRequest = common`، در Upgrade/Meal/Priority/Connectivity مشترک.

هر Profile Route مخصوص `GET ProvisionForm`, `POST Provisions`, `GET Provisions/{pid}/Form`, `PUT Provisions/{pid}` دارد؛ Commandهای typed facade یا wrapper حداقلی اند و بدون business logic مستقیماً سرویس Common موجود را صدا می‌کنند. `Profile` از route و `ApplicationType` از `AncillaryVariant.Registry` قطعی است (`Baggage=Baggage`, `Seat=Seat`, بقیه `Standard`). **Request shape مشترک، Profile identity مشترک نیست**؛ هر route باید Profile واقعی را Guard کند. `JsonUnmappedMemberHandling(Disallow)` روی DTOهای تخصصی برای foreign field ⇒ 400؛ Domain foreign rule ⇒ 409/16321. چهار route Seat و Controller آن Frozen.

## تصمیم ۵ — Constraints غیر Seat
**یک DTO مشترک `ProvisionConstraintsDto`** با Fieldهای `QuantityUnit`, `MaxQuantity?`, `AllowedDispositions[]`, `AllowedPriceOrigins[]`, `AllowedPurchaseStages[]` فقط اگر Domain/Variant فعلی مجاز بودن آنها را واقعاً enforce می‌کند؛ در غیراین‌صورت API نباید Restriction ساختگی گزارش کند. `AllowedRuleSections` موجود metadata اصلی حضور Sections است؛ `Flight`/`Fare` فقط وقتی `ServiceDateBasis=FlightDeparture`. `SeatProvisionConstraintsDto` unchanged. سورس `AncillaryProvision.Publication.cs` قطعی است: **A11/A15 فقط Paid را رد می‌کنند**؛ در وضعیت current `Free` و `NotAvailable` مجازند، پس AllowedDispositions آن دو `[Free,NotAvailable]` است (نه صرفاً Free). در Upgrade اگر `Outcome=Paid` و نوع FixedAncillary باشد `Filed` لازم و برای بقیه نوع‌ها `ExternalQuote` لازم است. Connectivity با `DeliveryStage=OnBoard` دقیقاً `PurchaseStage=OnBoard` دارد و بقیه چنین Stageی را نمی‌پذیرند. A07/A08 MaxQuantity=1 (Seat Constraints دست‌نخورده). این قواعد currently در `EnsureProfileConformance`/`EnsurePublishable` دیده می‌شوند؛ CLAUDE Rule5 الزام می‌کند **Invariant در Define/Change هم برقرار باشد**، نه فقط Publish. Metadata باید با این business guards برابر باشد.

## تصمیم ۶ — AirportService Flight/Fare
Fieldهای optional FlightApplication/FareApplication برای پایداری شکل common Request در DTO بمانند. اگر Definition `ServiceDateBasis=ServiceStart` دارد، Form آن بخش‌ها را نشان ندهد و Domain هنگام Define/Change non-null آنها را رد کند؛ `FlightDeparture` اجازه قواعد Flight/Fare مرتبط را می‌دهد. هیچ الگوی Request ثالثی صرفاً برای ServiceStart نساز.

## تصمیم ۷ — Generic Provision Routes
`POST/PUT /Backoffice/v1/AncillaryProvisions` و read/list/lifecycle موجود فعلاً باقی بمانند. Typed Facadeها همان Use Case را فراخوانی می‌کنند و دیتای authoritative دوم نمی‌سازند. UI Backoffice خانواده تخصصی را مصرف می‌کند. حذف Generic API صرفاً یک تصمیم مستقل آینده است؛ Stage1 حق شکستن مصرف‌کننده فعلی را ندارد.

## تصمیم ۸ — روش Acceptance
در Master Pack نه تعداد فایل تست، نه framework، نه دیتابیس تست و نه تعداد acceptance خاص دیکته نمی‌شود. ایجنت باید به شیوه استاندارد Repo اثبات کند: roundtrip برای هر ۵ Request Shape / تمام ۸ Profile، قبول/رد foreign field، 409/16219 و 404/16301 و 409/16321، `Settlement.CancellationRules` create/read/change/preserve/clear، validation/effective rules و Publish بدون Rules، Seat unchanged، Pricing/Lifecycle intact. گزارش concise در `reports/V13.2-S1-OWNER-REVIEW.md`، بدون commit/push مگر مجوز صریح Owner.

## تصمیم فرادستی — دستورهای داخلی مخزن
`CLAUDE.md` قواعد ۱–۹ را خود Owner نوشته؛ در تمام مراحل روی Pack مقدم است. شبیه‌سازی ادعایی AirPrice/Ordering بدون سورس ممنوع. اگر Agent تناقض واقعی جدیدی دید، طبق Rule6 با evidence گزارش می‌کند، نه اینکه Exception خودسرانه به ممنوعیت‌های Owner بسازد. این هشت موضوع **بسته** هستند و مجدداً به‌صورت open question ارجاع نشوند.
