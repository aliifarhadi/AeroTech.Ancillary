# 01 — بنچمارک مستند و تطبیق با سورس

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## الف: ATPCO Optional Services (مرجع تجاری، نه Inventory API)

`Create and Update Provisions` مستند می‌کند که S7 شامل `Free/Not Available`, `Fee Applies Per`, `Day/Time`, `Advance Purchase` و `Check Availability` است. `Check Availability` علامت نیاز به استعلام موجودی برای pricing است؛ جدول ATPCO نمی‌گوید quota برای Flight/Room/Kg را کدام Microservice مدیریت کند. مثال Free/Not Available حتی امکان نمایش سرویس روی وب ولی عدم دسترس‌پذیری در برخی پروازها را دارد.
- https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Provisions.htm
- https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/what_are_optional_services.htm

**نتیجه دامنه:** عدم واجد شرایط بودن، `NotAvailable` و Sold Out متفاوت‌اند؛ UI باید Reason و Authority را متمایز دریافت کند.

## ب: Amadeus Seatmap و FlightFlow

در Seatmap Display وضعیت فیزیکی صندلی به `AVAILABLE`, `BLOCKED`, `OCCUPIED` تفکیک می‌شود؛ BLOCKED می‌تواند از نوع مسافر یا Fare نتیجه شود نه لزوماً اشغال صندلی.
- https://admin.developers.amadeus.com/self-service/apis-docs/guides/developer-guides/faq/
- https://github.com/amadeus4dev/developer-guides/blob/master/docs/resources/flights.md

سورس داخلی `FlightFlow` واقعاً ظرفیت RBD/flight/allotment و hold/confirm دارد:
- `aliifarhadi/Aerotech.FlightFlow/k8s-stg`, `AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.Domain/FlightAggregate/Entities/Capacities/FlightCapacity.cs`
- `.../Entities/Capacities/FlightSeatHold.cs`

**نتیجه:** برای فروش Seat Ancillary از موجودی صندلی FlightFlow و شرایط seating استفاده کنید. ساخت `AncillarySeatCounter` یا گرفتن کپی `AvailableCap` در Ancillary به‌عنوان Source of Truth ممنوع. برای Fee/Eligibility پرواز، Provision همچنان صاحب تصمیم تجاری است.

## پ: Finnair PETC / محدودیت هم‌زمان مسافر و Flight

Finnair صریح می‌گوید یک carrier برای هر مسافر و معمولاً دو مسافر دارای حیوان در کابین برای هر پرواز، با محدودیت‌های اضافی نوع هواپیما/آب‌وهوا/شرایط دیگر. این فقط **نمونه بنچمارک** است، نه سهمیه جهانی یا قانون ایرلاین ما.
- https://www.finnair.com/ch-en/pets-on-finnair-flights

**نتیجه:** یک FlightCount quota مشترک + PassengerUsageLimit (مثلاً 1 درخواست برای هر مسافر در هر پرواز) بدون مدل «Stock مختص مسافر» می‌تواند هر دو محدودیت را پوشش دهد. برخی درخواست‌ها نیاز به تأیید عملیاتی supplier دارند.

## ت: Meal / Advance Purchase

British Airways برای وعده‌های خاص پنجره‌های درخواست وابسته به فرودگاه/نوع غذا و ضرب‌الاجل 24/36/48 ساعت را توضیح می‌دهد. این پنجره مجاز بودن فروش است و از ظرفیت آشپزخانه جداست.
- https://www.britishairways.com/content/en/de/information/food-and-drink/special-meals

**نتیجه:** Provision.AdvancePurchase/DayTime را به جای Inventory بازاستفاده کنید؛ برای محدودیت شمار غذا `FlightCountInventory` نیاز است، نه واحد StockDate عجیب.

## ث: Airport Lounge / Facility Slots

Priority Pass از Pre-book در loungeهای منتخب، دوره‌های عدم موجودی ظرفیت و وابستگی پذیرش به شرایط زمانی خبر می‌دهد.
- https://www.prioritypass.com/de-DE/lounges-prebook
- https://memberhelp.prioritypass.com/en-GB/support/lounge-pre-book?url_locale=en

**نتیجه:** Facility + local timezone + slot/time occupancy model، نه فقط AirportId. ظرفیت سالن با Fast Track مستقل است مگر دو محصول واقعاً یک Resource مشترک مصرف کنند.

## ج: Booking.com hotel inventory

Booking.com `roomstosell` را به Room Type + Date متصل می‌کند و صریحاً می‌گوید موجودی اتاق در همه RatePlanها مشترک است. Calendar dates برای ظرفیت اتاق باید bucket روزانه داشته باشند؛ بازه‌های چندروزه در Backoffice یک فرمان bulk authoring هستند، نه یک Counter مشترک برای همه روزها. تعطیلی فروش نیز separate از مقدار موجودی است.
- https://developers.booking.com/connectivity/docs/b_xml-availability
- https://developers.booking.com/connectivity/docs/b_xml-roomrateavailability
- https://developers.booking.com/connectivity/docs/ari

**نتیجه:** `RoomNightInventory` به DateOnly هر شب نیاز دارد؛ با `ProvisionPermittedTravelPeriod` که یک بازه Rule تجاری است **تفاوت ماهوی** دارد. در Booking.com همه موجودی‌ها لزوماً متعلق به ما نیستند؛ برای external property/Booking/Hotel PMS باید Delegated/SupplierManaged باشد.

## چ: سوابق کد واقعی Ancillary

- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/ValueObjects/AvailabilityDefinition.cs`: صرفاً `bool MustCheckAvailability`.
- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/ValueObjects/QuantityRule.cs`: `Unit`, `MinQuantity`, `MaxQuantity` (محدودیت درخواست، نه CurrentStock).
- `src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/Entities/AncillaryReservationUnit.cs`: `StockPoolId?`, `ProviderUnitRef?`, `Quantity`.
- `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/HoldAncillaryServicesService.cs`: فاقد Stock debit/lease/availability.
- Pack مصوب v12.1: `/mnt/data/AeroTech-Ancillary-v12.1-APPROVED-PHASE1-COMPLETE.md`؛ `ServiceDateBasis` را برای تاریخ سفر/خدمت مشخص کرده، اما مدل فیزیکی Stock را هنوز تصمیم‌گیری نکرده است.

## مواردی که بنچمارک به تنهایی اثبات نمی‌کند

- سقف واقعی وعده غذا، PETC، UMNR، wheelchair یا بار در ایرلاین/تأمین‌کننده ما.
- اینکه کدام Supplier اجازه Hold موقت می‌دهد و کدام صرفاً Confirm-on-booking دارد.
- اینکه مراکز فرودگاهی سهمیه را برای بازه ورود می‌گیرند یا برای هم‌زمانی اشغال در مدت خدمت.
- اینکه API هتل/تأمین‌کننده ما Allotment تضمین‌شده می‌دهد یا فقط موجودی زنده.

این‌ها باید به‌عنوان **Provider capability** احراز شوند، نه اینکه در Aggregate حدس زده شوند.
