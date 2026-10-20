# AeroTech Ancillary S2 — Public/API/Seat/Resolve Contract Freeze

**مرجع فیلدها:** `01-S2-OWNER-DESIGN-MASTER-v1.0.md` §04–11 و §14. این فایل یک Contract Gate اجرایی است: **هر فیلد فرعی در Master الزام‌آور است و حذف‌شدنش به علت خلاصه‌نویسی اینجا مجاز نیست**.

## 1. Endpoint matrix و وضعیت واقعی

`S=Backoffice|Api|OtaPanel`. همه Surfaceهای عمومی Auth جدا و Engine مشترک دارند.

| # | Verb/Path | وضعیت در 7efee95 | Request | Response | توضیح |
|---|---|---|---|---|---|
| E1 | `POST /{S}/v1/AncillaryOffers` | CURRENT | `offerId:string`, `options?` فقط Backoffice | Surface List DTO | Additive inline choice/fields |
| E2 | `POST /{S}/v1/Orders/{orderId}/AncillaryOffers` | TARGET | `orderId` از URL, `options?` فقط Backoffice | Same surface List envelope, `source.type=order` additive | no N+1 details |
| E3 | `POST /{S}/v1/AncillaryOffers/Details` | CURRENT+TARGET | `offerId XOR orderId`, `offerItemRef`, BO `options?` | Existing Detail DTO + canonical options/terms | On demand; both contexts |
| E4 | `POST /{S}/v1/AncillaryOffers/Selections/Evaluate` | CURRENT Legacy | `offerId,offerItemRef,selection`, BO `options?` | Existing Single DTO (all old fields) | backward compatible |
| E5 | SAME E4 path, batch shape | TARGET ADDITIVE | `offerId XOR orderId`, `selections[]` | Batch response | distinguish via validated request shape, do not ambiguous-dispatch |
| E6 | `POST /{S}/v1/SeatMaps` | CURRENT/FROZEN | `offerId`, BO `options?` | **Exact existing `SeatMapsDto`** | NO wire change |
| E7 | `POST /{S}/v1/Orders/{orderId}/SeatMaps` | TARGET conditional | `orderId`, BO `options?` | **Same existing `SeatMapsDto`** | require real source `originPricedOfferId`; otherwise explicit unavailable |
| E8 | `POST /Service/v1/Ancillaries/Selections/Resolve` | TARGET S2S | `orderId,expectedOrderCommercialVersion,pricedSelectionRefs[],correlationId` | `ResolvedBatch` | Order-only, true internal auth |
| E9 | `POST /Service/v1/Orders/AncillaryShoppingContext` | EXTERNAL TARGET, NOT LIVE | `orderId` | trusted OrderCommercialContext | written contract only, no fake claim |
| E10 | `POST /{S}/v1/Orders/{orderId}/Ancillaries` | EXTERNAL TARGET, NOT LIVE | `pricedSelectionRefs[],expectedOrderCommercialVersion,idempotencyKey` | OrderChange/OrderService | Ordering owns writing |
| E11 | `POST /Service/v1/Ancillaries/Service-Holds` | CURRENT S3 | real Hold DTO | Hold result | DO NOT change in S2 |

### 1.1 Wire-level invariants

- `offerId` and `orderId` XOR for Details/Batch; both/none => 422. On E2/E7 `orderId` path owns value. Trusted source may retain `originalPricedOfferId`, but cannot use stale Offer as fallback when Order source fails.
- `options` only BO uses current `BackofficeAncillaryOfferOptions` **actual declared keys**; Api/Ota caller cannot supply POS/customer/agency/airline/channel/office/currency override. Do not silently accept unknown JSON fields for purchasing/security contexts.
- `orderId > 0`, correct int64 wire format as configured in service; `clientSelectionId` 1..64 and unique per batch; 1..max configured lines, default cap 50 (not a benchmark value).
- Every choice is scoped to original Offer/Order, Variant, Passenger(s), Flight(s), active Provision/Pricing, source revision and trusted SalesScope. `optionRef` is a one-way ref (not proof price or purchasability by itself).
- `source` public projection tells Offer/Order identity, but never internal foreign account, supplier credentials or hidden PCC/Office.
- Rounding/currency/precision: explicit source CurrencyId and money type; public API textual decimal amounts (invariant) where already used; no floats and no multiplication of per-Order fee per line.

## 2. Canonical List item — FULL field requirements

| Field (projection name) | Type | Origin | Mandatory state / rule |
|---|---|---|---|
| `offerItemRef` | opaque string | existing OfferItemRef | required stable within source version/scope, not decodable |
| `serviceDefinitionRef` | string | Definition | SKU for A02/A24 |
| `profile`, `variantCode` | enum/code strings | Registry | 9 profiles / A01–A24 verified |
| `commercialName`, `description?` | string | Definition | customer-safe label |
| `serviceTypeCode`, `serviceSubCode` | string | Definition | no invented ATPCO subcodes |
| `beneficiaries` | travellerRefs[], mapped IDs when available | Source | required if not true Order scope |
| `coverage` | scope+portionRefs+flightRefs | Source+Provision | exact flight/portion/traveller applicability |
| `pricing` | origin,status,pricingUnit,currencyId/base/unit/preview, tax+fee summary,FX evidence | price engine | null or Incomplete when unknown; 0 only verified Free |
| `quantity` | unit,min,max,step?,selectionRequired | Quantity assessment | not confused with EffectiveQuantity |
| `selection` | kind,fields[{name,type,required,constraints?}] | CustomerSelectionContract | enough for simple inline products |
| `optionGroups[]` | fieldName,sourceState,choices[],isComplete,requiresDetails | OptionCatalog/verified provider | fixed bounded inline choices; overflow on demand; no fake choices |
| `interaction` | `Inline|DetailsOnDemand|DedicatedSeatMap` / equivalent projection | derived actual candidate | NOT unconditional mapping by Variant alone |
| `availability` | checkState,guaranteed,requiresCheckAtReserve | source | unknown != no stock; no guarantees invented |
| `readiness`, `canSelect`, `canPurchase` | statuses/booleans | assessed commercial state | `canSelect` != quoteReady; `canPurchase` implies real exact selection+price, not List pre-purchase |
| `commercialTerms` | purchaseStage, disposition,refund/reissue,confirmation,booking,document, mandatory requirements | Provision/Definition | public summary; sensitive internal fields masked |
| `reasonCodes[]`, `missingFacts[]` | string arrays | engine | stable, safe error reasons, no secrets |
| `validUntilUtc?`, `sourceVersion?` | typed | provenance | no publisher guarantee attributed to local TTL |

**Specific trade-off:** Inline option payload budget (initial suggestion 12 choices / 4096 bytes, configurable) controls whether `choices` are complete. Truncated `choices` MUST carry `isComplete=false` + `requiresDetails=true`. Details is called on click, not automatically for every item. Rich multi-step/provider/appointment options are Details-on-demand. A07/A08 use E6/E7, not generic List.

## 3. Preserve each Surface, improve only additively

| Surface | Keep existing wire | Target additions |
|---|---|---|
| Backoffice | `offerId,evaluatedAtUtc,currency,flightOffer,scope,journey,travellers,categories,offers,blockedOffers,diagnostics,ratesOfExchange,completeness` | each item inline form/choices/price provenance; internal IDs only for authorized operator |
| Api (Partner) | `{meta,data,included}`, `included.bags`, `included.other-services`, `data.travelers`,`data.segments`, string prices | readable service, applicability/constraints/conditions, selection, stable code/message, option state, Source; no internal Provider/POS/PCC |
| OtaPanel | `offerId,currencyId,currencyCode,travellerGroups,blockedCount,ratesOfExchange,orderScopedServices` | UI-ready choice, form, price state, condition summary while preserving group keys |

**Do not replace** one surface DTO with an identical global DTO; canonical engine output is common; projection is separate. Compare 9 S2-01 Goldens by actual values for unchanged fields; old API `data/included` remains.

## 4. Detail E3 fields

Request `{offerId?:string,orderId?:long,offerItemRef:string,options?:BOOnly}`. Response preserves all old fields and adds `selectionContract:{kind,fields[{name,type,required,constraints?}]}`, `quantity:{unit,min,max,step?,selectionRequired}`, `optionGroups:[{fieldName,sourceState,choices:[{optionRef,value,label,availability,applicability,pricePreview?,sourceVersion?,validUntilUtc?}]}]`, `optionSourceState`, `priceCompleteness:{isComplete,reasonCodes,missingInputs}`, `commercialTerms:{purchaseStage,disposition,reissueRefund,formOfRefund?,confirmationRequirement,bookingMethod,documentRouting,documentType,rfic?,rfisc?,cancellationRules[]}`. **Existing field and nested names not removed.** External option provenance required; typed free text/measurements are not fake enumerations.

## 5. Evaluate E4/E5 typed inputs

Batch exact envelope:

```json
{
  "orderId": 12345,
  "selections": [
    {"clientSelectionId":"L1","kind":"ancillary","offerItemRef":"<opaque>","selection":{"quantity":2},"selectedOptionRefs":{}},
    {"clientSelectionId":"L2","kind":"seatMap","seatMapSelectionId":"<original-43-char-hash>"}
  ]
}
```

Selection model is **not** a generic unvalidated dictionary. Require variant-specific allowed/required subsets of these existing fields and the narrow A19 correction in Master §07.3:

`quantity:int?`, `packageProductRef:string?`, `bagRef:string?`, `measuredWeightKg:decimal?`, `dimensions:{lengthCm,widthCm,heightCm}?`, `equipmentKind:string?`, `weightKg:decimal?`, `seatNumber:string?`, `exitRowTermsAccepted:bool?`, `purpose:ExtraSeatPurpose?`, `targetCabinId:int?`, `quoteRef:string?`, `mealCode:string?`, `menuItemRef:string?`, `animalType:PetAnimalType?`, `otherAnimalCode:string?`, `animalAgeWeeks:int?`, `combinedWeightKg:decimal?`, `carrierDimensions:{lengthCm,widthCm,heightCm}?`, `documentAcknowledgements:string[]?`, `sizeBracket:string?`, `assistanceSsrCode:string?`, `equipmentCode:string?`, `evidenceDocumentRefs:string[]?`, `oxygenUnits:decimal?`, `infantRef:string?`, `guardianRef:string?`, `childRef:string?`, **legacy** `guardianHandoffContactProvided:bool`, `guardianPickupContactProvided:bool`, **target source verified** `guardianHandoffContactRef?:string`, `guardianPickupContactRef?:string`, `airportId:int?`, `facilityRef:long?`, `timeWithOffset:DateTimeOffset?`, `guestCount:int?`, `optIn:bool?`, `planCode:string?`, `deviceCount:int?`.

Never accept `Guardian...Provided=true` as proof. For A02 `packageProductRef` equals chosen `ServiceDefinitionRef`; A24 `planCode` equals one authored `ServiceDefinitionRef` plan; A12 menu only with authored or genuinely verified live supplier option; A10 `isUpgradable=true` alone not external quote proof. For Seat, pass only `seatMapSelectionId`; never trust attacker-supplied `seatNumber`/Pax/price from body. Inputs per Variant and conditions in Master §12.

Batch response FULL keys per Master §07.4: `source{type,id,commercialVersion?},evaluatedAtUtc,currencyId,status,allReady,items:[{clientSelectionId,kind,status,readiness,offerItemRef?,seatMapSelectionId?,pricedSelectionRef?,validUntilUtc?,service,beneficiary,coverage,selectedOptions,exactPrice,requiresCheckAtReserve,reasonCodes,fieldIssues}],basketPricing{lineTotal?,sharedFees[],sharedTaxes[],total?,currencyId,isComplete,reasonCodes},basketConflictCodes[]`. Do not silently omit an input line; order by clientSelectionId input sequence. `allReady=false` means no basket Add, irrespective of individual status. Legacy single response contains its old properties intact.

## 6. EXACT `SeatMapsDto` freeze

From current `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs` (§09 Master):

```text
meta:{count}
data[]:{type,flightOfferId,segmentId,departure{iataCode,terminal,at},arrival{iataCode,terminal,at},carrierCode,number,operating{carrierCode},aircraftId,class,cabinClassId,
 decks[]:{deckType,deckConfiguration{width,length,startSeatRow,endSeatRow,startWingsX,endWingsX,startWingsRow,endWingsRow,exitRowsX[]},
  cabins[]:{cabinClass,cabinClassId,cabinStartSeatRow,cabinEndSeatRow,items[]:{number,code,column,row,title,characteristicsCodes[],conditions[],coordinates{x,y,length},travelerPricing[]:{id,travelerId,seatAvailabilityStatus,price?{currency,base,total,taxes[]{amount,code},fees[]{amount,type}}}}}},
 availableSeatsCounters[]:{travelerId,value}}
dictionaries:{facilities,seatCharacteristics}
```

Status strings exactly `AVAILABLE|BLOCKED|OCCUPIED`; id existing 43-char URL-safe SHA, **cannot decode**. No schema/routes change. Valid AVAILABLE iff physical empty, commercial eligible and **complete priced positive or explicitly free verified**; otherwise BLOCKED with null price and id. Occupancy incomplete/outage cannot mean empty. Regenerate ID/candidate and quote in Evaluate and again in Resolve; map exact pax-flight-seat-Source-Scope, original offer→order mapping if verified; seat actual Hold is FlightFlow later.

## 7. Resolve E8; never infer Order commit from quote

Request JSON:

```json
{"orderId":12345,"expectedOrderCommercialVersion":"V17","pricedSelectionRefs":["<token1>","<token2>"],"correlationId":"e7934519-8e56-47f4-a532-d7f077899e7b"}
```

Response: `{orderId,orderCommercialVersion,allValid,evaluatedAtUtc,items[],basketPricing}` with each `items[]` having `status,reasonCodes,selectionKind,sourceSelectionRefDigest,offerItemRef?,seatMapSelectionId?,service{serviceDefinitionRef,definitionVersionId,provisionId,pricingRevisionId?,pricingRateId?,variantCode,profile,serviceTypeCode,serviceSubCode,bookingMethod,ssrCode?,documentRouting,documentType,rfic?,rfisc?,fulfillmentProviderKey,confirmationRequirement},beneficiaries[{orderTravellerId,sourceTravellerRef}],coverage{scope,portionRefs,flightRefs,flightIds,flightCapacityIds},selection,price{currencyId,quantity?,chargeableWeightKg?,pricingUnit,baseAmount,unitTotal,lineTotal,includedTaxes,addedTaxes,unitFees,sharedFees,filedCurrencyId,fxEvidence,isTotalComplete},availability{requiresCheckAtReserve,isGuaranteed:false unless verified},validUntilUtc,sourceVersion,orderCommercialVersion`; `currentPrice?`/`changedFields[]?` advisory only on stale. `Valid` never guarantees supplier Hold. No incomplete or missing price in `Valid`. Status enum projection per Master §11.3; preserve Order input order and never double charge shared fee.

**Auth:** Explicit service policy + verified Ordering identity; `pss.service`, `context_type=Service`, `principal_type=internal_service`, allowlist; missing any => 401/403/fail closed. Validate cryptographic payload BEFORE trusting any claim; still re-read Order and price. No public access to provider keys, PII, token payload. External Ordering Add is not an Ancillary endpoint.

## 8. Stateless reference details

`pricedSelectionRef`: AES-GCM authenticated encryption or framework Data Protection (existing sibling preferred), v1 schema, kid/issuer/audience/nonce/iat/exp/source identity/OrderVersion/scope/Definition+Provision+Pricing+Rate revisions, normalized selected input, option evidence, beneficiary+coverage, amount/currency/tax/fee/FX and seat identity where applicable; short configured TTL, protected persistent key ring, replay guarded **at Ordering** via Idempotency + optimistic version. No aggregate, no Quote persistence, no signed-price trust without recompute; no PII or secrets in payload; max token size; deterministic canonical change fingerprint.

## 9. HTTP/error contract

Structured, stable error code and safe description. Validation 400/422, unauthorized 401, forbidden 403, unknown cross-account 404 where appropriate, commercial stale/conflict 409, missing upstream 503 or typed unavailable according to framework pattern. Batch per-line commercial rejection is HTTP 200 with `allReady=false`/`allValid=false` when transport/request valid; structural malformed batch 422. No Supplier secret in error. Follow existing `ExceptionFactory` and REST middleware conventions; no new parallel error framework. Full mapping in Master §15.
