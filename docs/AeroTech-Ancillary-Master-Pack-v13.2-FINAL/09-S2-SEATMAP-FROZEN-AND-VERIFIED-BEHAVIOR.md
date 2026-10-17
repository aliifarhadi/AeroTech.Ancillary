# 09 — قرارداد SeatMap ثابت (FROZEN)؛ تصحیح رفتاری بدون Breaking Change

## 9.1 مسیرها و نوع داده تغییر نکنند
`POST /Backoffice/v1/SeatMaps`  
`POST /Api/v1/SeatMaps`  
`POST /OtaPanel/v1/SeatMaps`

**سورس Contract:** `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs` در SHA `9aca1cd...`.

**ساختار رسمی و دقیق پاسخ جاری:**
```
SeatMapsDto
  Meta { Count:int }
  Data[] {
    Type:string, FlightOfferId:string, SegmentId:string,
    Departure { IataCode?, Terminal?, At:DateTimeOffset },
    Arrival { IataCode?, Terminal?, At:DateTimeOffset },
    CarrierCode?, Number?, Operating { CarrierCode? },
    AircraftId:int, Class?, CabinClassId?,
    Decks[] {
      DeckType?, DeckConfiguration { Width,Length,StartSeatRow,EndSeatRow,
        StartWingsX,EndWingsX,StartWingsRow,EndWingsRow,ExitRowsX[] },
      Cabins[] { CabinClass?,CabinClassId?,CabinStartSeatRow,CabinEndSeatRow,
        Items[] { Number?,Code?,Column,Row,Title?,CharacteristicsCodes[],
          Conditions[],Coordinates {X,Y,Length}, TravelerPricing[] {
            Id?, TravelerId:string, SeatAvailabilityStatus:string,
            Price? {Currency:string,Base:string,Total:string,
              Taxes[]{Amount:string,Code?}, Fees[]{Amount:string,Type?}} }
        }}
    }, AvailableSeatsCounters[] { TravelerId,Value }
  }
  Dictionaries { Facilities:map, SeatCharacteristics:map }
```
**هیچ نام، نوع، nullability، collection shape، nesting، status string یا نام route تغییر نکند.** حتی اصلاح Price/Id فقط از طریق مقادیر مجاز همین DTO انجام شود. Consumer Backoffice SeatMap نمی‌شکند.

## 9.2 رفتار صحیح Status/Price/ID
| وضع واقعی | `SeatAvailabilityStatus` | `Price` | `Id` | توضیح |
|---|---|---|---|---|
| Occupied معتبر | `OCCUPIED` | null | null | Physical owner |
| blocked by Cabin/Exit/Eligibility | `BLOCKED` | null | null | حتی وقتی Physical Empty |
| Physical Empty + verified published Paid price + commercial eligible | `AVAILABLE` | valid money lines | opaque ID | ما تنها Offer می‌دهیم؛ Hold هنوز لازم است |
| Physical Empty + **verified** Free entitlement/published `Free` | `AVAILABLE` | صفر معتبر همان Currency | opaque ID | Zero با Evidence، نه از Null |
| Physical Empty + no commercial product/unknown quote/source | `BLOCKED` | null | null | UI نمی‌تواند آن را قطعی خرید تلقی کند |
| missing occupancy for requested flight / source outage | **typed unavailable response** با رفتار خطای فعلی | — | — | response موفقِ خالی/قیمت صفر ساختگی نده |

این اصلاحات **رفتار دامنه** هستند، نه migration ظاهری DTO. `AvailableSeatsCounters` فقط واقعا AVAILABLE را می‌شمارد. `Price.Total` تمام components قابل اعمال را بازتاب دهد؛ اگر charge/fee قابل تعیین نیست Seat نباید FullyPriced یا رزروپذیر نشان داده شود.

## 9.3 Ownership
Aircraft physical layout از مرجع Aircraft/Flight معتبر، occupied/hold/seat assignment از FlightFlow. Ancillary خود `SeatSpecification`/`SeatApplication`/`FareApplication`/pricing را دارد و برای هر Pax/Flight/Seat می‌سنجد. **نقشه در Ancillary موجود است به‌عنوان Read Composition Facade**؛ حذف Adapter یا تغییر Route صرفاً بر اساس ADR قدیمی ممنوع. `AncillaryInventoryPolicy` برای Seat تعریف نمی‌شود.

## 9.4 Source guards
- همه flightIdهای مورد درخواست دقیقاً در OccupiedSeats response presence داشته باشند؛ missing flight ≠ empty occupied list؛ duplicate response یا invalid seat Number => fail closed.
- layout از همان Aircraft/FlightVersion واقعی باشد؛ mismatch، seat coordinates stale، تغییر هواپیما یا کد ویژگی نامشخص => blocked/unavailable.
- POS باید یکتا و authorized برای همان price quote باشد؛ چند POS نامشخص ⇒ error existing؛ first random POS ممنوع.
- `travelerPricing[].id` 43-char opaque موجود حفظ؛ Hash بدون Candidate معتبر حق خرید نیست. Resolve/Order چک Scope/Commercial Price و Physical Hold جدا را انجام می‌دهند.
- `A09 ExtraSeat` فقط به‌عنوان Option واقعی with special adjacent seat/ticket requirements فروخته می‌شود؛ در این Stage مسیر Generic AncillaryOffers باقی می‌ماند، نه seat cell با `Price=0`.

## 9.5 FlightFlow gating
در Repo FlightFlow SHA `962f4188` Endpoint `POST Service/v1/Flights/OccupiedSeats` مشاهده شد. **این صرفاً حقیقت منبع، نه تأیید محیط استقرار است.** Owner می‌گوید تغییرات FlightFlow برای این پروژه Gate نباشد. در closure S2، External integration را جداگانه با عنوان `[EXTERNAL_PENDING]` ثبت کن؛ هیچ‌گاه عدم دسترسی به FlightFlow را به «هیچ صندلی اشغال نیست» تبدیل نکن.
