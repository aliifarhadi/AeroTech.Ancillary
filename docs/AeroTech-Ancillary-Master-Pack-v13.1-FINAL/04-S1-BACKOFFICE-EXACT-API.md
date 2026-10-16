# 04 — Stage 1: API دقیق Backoffice و Typed Authoring

**هدف:** هر Variant بدون فیلد نامربوط در فرم خودش تعریف، ویرایش، منتشر و قیمت‌گذاری شود؛ APIهای موجود Seat و Definition بدون Breaking Change بمانند.

## 4.1 Definition — همه مسیرها [CURRENT]
Prefix: `Backoffice/v1/AncillaryServiceDefinitions`

| عملیات | Method / Path | دامنه رفتار |
|---|---|---|
| لیست همه | `GET /Paginated?profile={Profile?}&...` | به تفکیک Owner/Status، Pagination مشترک |
| Metadata واقعی Variantها | `GET /Variants` | A01..A24، Profile، فیلدهای انتخاب و allowable Pricing Units |
| تعریف اختصاصی | `POST /{Profile}` | فقط `TProfileSpecification` |
| خواندن فرم اختصاصی | `GET /{Profile}/{serviceDefinitionId}` | `Form` با همان فیلدهای PUT |
| ویرایش اختصاصی | `PUT /{Profile}/{serviceDefinitionId}` | فقط Draft؛ هویت Owner/StableRef غیرقابل تغییر |
| خواندن عمومی | `GET /{serviceDefinitionId}` | اطلاعات نمایشی/مدیریتی |
| چرخه عمر | `POST /{id}/Activate|Suspend|Reactivate|Retire|Revise` | چرخه مشترک موجود |
| تغییر مجاز Unit/Date Basis | `POST /{id}/AssignPricingUnit`, `POST /{id}/AssignServiceDateBasis` | فقط قواعد مجاز Variant، طبق مدل جاری |

`{Profile}`: **`Baggage`, `Seat`, `Upgrade`, `Meal`, `Pet`, `AssistedTravel`, `AirportService`, `Priority`, `Connectivity`**. اسم‌ها مطابق کد هستند؛ `/Ancillaries/Seat` یا ۲۴ Controller مستقل نسازید. `Backoffice/v1/AncillaryServiceDefinitions/Seat` و DTOهای آن **فریز هستند**.

## 4.2 Provision — الگوی یکسان Typed Facade، نه منطق جدا
Prefix: `Backoffice/v1/AncillaryServiceDefinitions/{Profile}/{serviceDefinitionId}`

| Method / Path | موجود/جدید | خروجی |
|---|---|---|
| `GET /ProvisionForm` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | metadata form مجاز/constraints/variant و field visibility |
| `POST /Provisions` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | Provision draft، Id/Status/DefinitionId |
| `GET /Provisions/{provisionId}/Form` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | مقدار قابل ویرایش با همان بدنه PUT |
| `PUT /Provisions/{provisionId}` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | Update Draft با guard مالکیت |

**فهرست و Lifecycle مشترک فعلی** زیر `Backoffice/v1/AncillaryProvisions` می‌مانند:
`GET /Paginated?serviceDefinitionId=...`, `GET /{id}`, `GET /{id}/Form`, `GET /{id}/Pricings`, `POST /{id}/Activate`, `POST /{id}/Publish`, `POST /{id}/SwitchActivePricing`, `POST /{id}/Suspend`, `POST /{id}/Reactivate`, `POST /{id}/Retire`.

Definition ownership: Wrong Profile Id = **409/16219**؛ Provision متعلق به Definition دیگر = **404/16301** (همان کد موجود). فیلد غیرمجاز برای Profile به‌صورت typed rejected؛ هیچ کد جدیدی بدون ضرورت اختراع نشود.

## 4.3 Pricing — صددرصد مشترک [CURRENT]
Prefix: `Backoffice/v1/AncillaryPricings`

`POST /`, `PUT /{pricingId}`, `GET /{pricingId}`, `GET /Paginated`, `POST /{pricingId}/Activate`, `POST /{pricingId}/Suspend`, `POST /{pricingId}/Reactivate`, `POST /{pricingId}/Retire`, `POST /{pricingId}/Revise`.

**اصل:** یک قیمت فعال/نسخه revision مطابق rule موجود؛ نرخ/fee/tax/FX مستقل از مشخصات Profile. در پایان، `Provision.Publish` به Pricing معتبر لینک می‌شود؛ SwitchActivePricing تغییر قیمت آتی است و quote قبلی باید زمان Hold دوباره اعتبارسنجی شود.

## 4.4 CancellationTerms مشترک و اختیاری [TARGET]
`GET /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` و `PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` برای ثبت Ruleهای filed penalty/waiver در **Draft Provision**، بدون تغییر DTO فرم Seat؛ `Settlement.ReissueRefund` به‌تنهایی مبلغ جریمه نیست. Ruleها زیر ValueObject Provision می‌مانند، نه Aggregate جدید. در P3 فقط مصرف/Quote می‌شوند؛ جزئیات فصل 13 و 21.

## 4.5 Inventory — فقط در موارد دارای Inventory واقعی [CURRENT]
Prefix: `Backoffice/v1/AncillaryInventoryPolicies`; Define/Change/Activate/Suspend/Retire/ByServiceIdentity/Paginated. خانواده `FlightCountInventory`, `FlightWeightInventory`, `AirportSlotInventory` خودشان endpoint مدیریتی فعلی دارند. **هیچ Seat InventoryPolicy Tab یا الزام Policy برای Seat ایجاد نکنید.**

## 4.6 تفاوت Requestهای تخصصی — فقط بخش مناسب
| Profile | فیلدهای اختصاصی Provision که فرم می‌بیند | فیلدهای نامربوط که نباید بپذیرد |
|---|---|---|
| Baggage | `BaggageApplication` + Shared eligibility | SeatApplication/PetRule |
| Seat | `SeatApplication` + Shared؛ **بدون تغییر نسبت به فرم موجود** | BaggageApplication، InventoryPolicy |
| Upgrade | Shared Fare/Flight + Definition.Upgrade | Seat/Pet/Assisted Rules |
| Meal | Shared AdvancePurchase/Passenger/Fare + Definition.Meal | Pet/Seat/Baggage Rule |
| Pet | `PetRule` + Shared | Seat/Assisted/Airport Rule |
| AssistedTravel | `AssistedTravelRule` + Shared | Pet/Seat/Baggage Rule |
| AirportService | `AirportServiceRule` + Shared | Seat/Pet/Baggage Rule |
| Priority | Shared Flight/Fare/Passenger | تخصصی تهی و بی‌دلیل |
| Connectivity | Shared Flight/Fare/Date | تخصصی تهی و بی‌دلیل |

Profile-specific specialization در `Specification` و در صورت وجود rule-specific section است؛ از اضافه‌کردن Rule دسته‌ای فقط برای زیبایی فرم خودداری کنید. API عمومی Generic Provision تا زمانی که کد داخلی مصرف دارد باقی می‌ماند ولی UI Backoffice از typed استفاده می‌کند؛ حذف آن تغییر جداگانه می‌خواهد.

## 4.7 نمونه سلسله‌مراتب یک محصول
1. `POST .../AncillaryServiceDefinitions/Meal`: ایجاد Paid Meal A12، تعریف `MenuItemRef`, lead time، Supplier.
2. `GET .../Meal/{id}/ProvisionForm`: فقط Field/Ruleهای قابل اعمال A12.
3. `POST .../Meal/{id}/Provisions`: Fare/Passenger/POS/Flight/date/Qty rule.
4. `POST /Backoffice/v1/AncillaryPricings`: Rate/Tax/Fee.
5. `POST /Backoffice/v1/AncillaryProvisions/{provisionId}/Publish`: فعال‌شدن فروش طبق Lifecycle.
6. `GET /.../Paginated`, `/.../Pricings`, `/.../Form`: نمایش دقیق اطلاعات بدون فرم General All-In-One.

## خروجی Stage 1
Definition همه ۹ Profile و ۲۴ Variant، Provision تایپ‌شده برای هر Profile، Pricing مشترک، `CancellationTerms` اختیاری و publish/lifecycle کامل؛ قرارداد Backoffice Seat فعلی بدون کوچک‌ترین تغییر و بدون الزام اتصال FlightFlow. **S1 در این Pack طراحی‌شده، نه لزوماً در کد بسته‌شده**.
