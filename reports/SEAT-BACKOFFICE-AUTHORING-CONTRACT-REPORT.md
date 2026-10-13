# Seat Backoffice provision authoring - implementation report

Baseline and HEAD: `d8dbeef303e990449a89a2c643588a62fd5c7647 Remove UnUsed Things!` on `k8s-stg`. Nothing is committed or pushed; the work is in the working tree.

## 1. Verdict

- The four Seat routes are implemented as a typed facade over the existing Define / Change provision use cases. No aggregate, table or migration was added.
- Airline ownership check (R1 decision 2): closed by the Owner on 2026-10-13. No check is added while the platform runs for one airline; nothing was changed for it.
- Not run: a 403 control with an agency-panel token (no such token was supplied).

## 2. Routes

| Method | Path | Request | Answer |
|---|---|---|---|
| GET | `/Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/ProvisionForm` | - | `BackofficeSeatProvisionCreateFormDtoApiResult` |
| POST | `/Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/Provisions` | `SeatProvisionRequest` | `ProvisionResultApiResult` |
| PUT | `/Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/Provisions/{provisionId}` | `SeatProvisionRequest` | `ProvisionResultApiResult` |
| GET | `/Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/Provisions/{provisionId}/Form` | - | `BackofficeSeatProvisionFormDtoApiResult` |

All four are in `RestApi/V1/AncillaryProvisionAggregate/Controllers/SeatBackofficeController.cs`, tagged `Backoffice Seat`, under `[Authorize(SurfaceAuthorization.Backoffice)]`. No existing route was renamed or removed. The published document of the side host holds 129 operations (117 of the REST API, 12 reference sync routes).

## 3. Contract as published and as answered by the running host

`SeatProvisionRequest` (POST and PUT body) is published with `additionalProperties: false` and these members: `sequence`, `coverageScope`, `purchaseStage`, `priceOrigin`, `quoteProviderKey`, `quantity`, `outcome`, `settlement`, `availability`, `fulfillment`, `passengerEligibility`, `salesRestrictions`, `geography`, `flightApplication`, `fareApplication`, `travelDate`, `dayTimeApplication`, `advancePurchase`, `seatApplication`.

It has no `serviceDefinitionId`, `applicationType`, `baggageApplication`, `petRule`, `assistedTravelRule`, `airportServiceRule`, price or stock member. Unknown members are refused at the top level of the body; members nested inside the shared rule inputs follow the existing behaviour of those types.

Create descriptor, as answered for the standard seat product (`GET .../ProvisionForm`):

```json
{
  "serviceDefinitionId": "1559299218793824256",
  "variantCode": "A07",
  "profile": {
    "value": 2,
    "name": "Seat",
    "title": "Seat"
  },
  "definitionStatus": {
    "value": 2,
    "name": "Active",
    "title": "Active"
  },
  "pricingUnit": {
    "value": 5,
    "name": "PerSeat",
    "title": "Per Seat"
  },
  "serviceDateBasis": {
    "value": 1,
    "name": "FlightDeparture",
    "title": "Flight Departure"
  },
  "applicationType": {
    "value": 3,
    "name": "Seat",
    "title": "Seat"
  },
  "allowedRuleSections": [
    "PassengerEligibility",
    "SalesRestrictions",
    "Geography",
    "TravelDate",
    "DayTimeApplication",
    "AdvancePurchase",
    "FlightApplication",
    "FareApplication",
    "SeatApplication"
  ],
  "constraints": {
    "quantityUnit": {
      "value": 1,
      "name": "Each",
      "title": "Each"
    },
    "maxQuantity": 1,
    "requiresSeatNumberOrCharacteristic": true,
    "seatNumberFilterRequiresAircraft": true,
    "publishedPointOfSaleCount": 1,
    "requiresPreferredCharacteristic": false
  }
}
```

Edit form of a draft, as answered (`GET .../Provisions/{provisionId}/Form`); its `form` was sent back unchanged as the PUT body:

```json
{
  "id": "1559563506242551808",
  "serviceDefinitionId": "1559299218793824256",
  "status": {
    "value": 1,
    "name": "Draft",
    "title": "Draft"
  },
  "profile": {
    "value": 2,
    "name": "Seat",
    "title": "Seat"
  },
  "variantCode": "A07",
  "editable": true,
  "allowedRuleSections": [
    "PassengerEligibility",
    "SalesRestrictions",
    "Geography",
    "TravelDate",
    "DayTimeApplication",
    "AdvancePurchase",
    "FlightApplication",
    "FareApplication",
    "SeatApplication"
  ],
  "constraints": {
    "quantityUnit": {
      "value": 1,
      "name": "Each",
      "title": "Each"
    },
    "maxQuantity": 1,
    "requiresSeatNumberOrCharacteristic": true,
    "seatNumberFilterRequiresAircraft": true,
    "publishedPointOfSaleCount": 1,
    "requiresPreferredCharacteristic": false
  },
  "createdAt": "2026-10-13T13:48:35.1773235+00:00",
  "activatedAt": null,
  "suspendedAt": null,
  "retiredAt": null,
  "form": {
    "sequence": 990,
    "coverageScope": "Sector",
    "purchaseStage": "Both",
    "priceOrigin": "Filed",
    "quoteProviderKey": null,
    "quantity": {
      "unit": "Each",
      "minQuantity": 1,
      "maxQuantity": 1
    },
    "outcome": {
      "disposition": "Paid",
      "documentRequired": true,
      "bookingRequired": true
    },
    "settlement": {
      "reissueRefund": "NonRefundableReusable",
      "formOfRefund": null,
      "commissionable": false,
      "interlineSettlement": false
    },
    "availability": {
      "mustCheckAvailability": true
    },
    "fulfillment": {
      "fulfillmentProviderKey": "Ancillary"
    },
    "passengerEligibility": {
      "allowedPassengerTypes": [
        "ADT",
        "CHD"
      ],
      "allowedAgeBands": []
    },
    "salesRestrictions": {
      "salesEffectiveFrom": null,
      "salesDiscontinueAt": null,
      "allowedPointOfSaleIds": [
        "1"
      ],
      "allowedCustomerIds": [],
      "allowedCustomerTypes": []
    },
    "geography": {
      "allowedOriginAirportIds": [],
      "allowedDestinationAirportIds": [],
      "allowedViaAirportIds": [],
      "allowedRoutePairs": [
        {
          "originAirportId": 1,
          "destinationAirportId": 3,
          "direction": "BothDirections"
        },
        {
          "originAirportId": 1,
          "destinationAirportId": 5,
          "direction": "BothDirections"
        },
        {
          "originAirportId": 1,
          "destinationAirportId": 10,
          "direction": "BothDirections"
        },
        {
          "originAirportId": 1,
          "destinationAirportId": 13,
          "direction": "BothDirections"
        },
        {
          "originAirportId": 3,
          "destinationAirportId": 10,
          "direction": "BothDirections"
        }
      ],
      "serviceLocations": [],
      "coverageCountryIds": []
    },
    "flightApplication": null,
    "fareApplication": {
      "allowedAirFareIds": [],
      "allowedAirFareTypes": [],
      "allowedFareFamilyIds": [
        "1",
        "2"
      ],
      "allowedFareBasisCodes": [],
      "allowedCabinClassIds": [],
      "allowedRbdIds": []
    },
    "travelDate": {
      "permittedPeriods": [
        {
          "startDate": "2026-10-01",
          "endDate": "2027-12-31"
        }
      ],
      "blackoutPeriods": []
    },
    "dayTimeApplication": null,
    "advancePurchase": {
      "minimumPeriod": 3,
      "unit": "Hours",
      "sameTimeAsTicketed": false,
      "maximumPeriod": null
    },
    "seatApplication": {
      "seatNumbers": [],
      "seatCharacteristicCodes": [
        "W",
        "A",
        "9"
      ]
    }
  }
}
```

## 4. Decisions taken in the implementation

- The route profile and the parent definition are checked inside the existing services (Owner's choice): `IDefineAncillaryProvisionCommand.Profile` and `IChangeAncillaryProvisionCommand.ServiceDefinitionId` / `Profile` are optional members; the generic commands return null, the Seat commands return the route values.
- Errors reuse existing codes only: 16219 (definition of another profile), 16301 (absent provision or another parent), 16321 (stored rule of another profile when the form is read), 16201 (unknown definition).
- `editable` is absent from the create descriptor and equals `status == Draft` on the edit form.
- The descriptor limits are fixed values in `SeatProvisionFormMapper.Constraints` and are pinned against the domain by test SEAT_06 (A07 / A08 quantity above one refused at publication, A09 accepted).
- A provision with a missing parent definition answers the existing 16304 on POST, as the generic route does.

## 5. Evidence

Authenticated HTTP on a side host built from this working tree (port 5299, dev database, Backoffice token of the Owner): **36 of 36 steps and checks passed**.

| Result | Call | HTTP | Code | What |
|---|---|---|---|---|
| PASS | GET `/AncillaryServiceDefinitions/Paginated` | 200 | - | seat definitions, paginated |
| PASS | GET `/AncillaryServiceDefinitions/Paginated` | 200 | - | baggage definitions, paginated |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299218793824256/ProvisionForm` | 200 | - | SEAT_STANDARD create descriptor |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299254248275968/ProvisionForm` | 200 | - | SEAT_PREFERRED create descriptor |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299602094489600/ProvisionForm` | 200 | - | SEAT_EXTRA_COMFORT create descriptor |
| PASS | GET `/AncillaryProvisions/Paginated` | 200 | - | provisions of the standard seat definition |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions/1559299223835377664/Form` | 200 | - | seat form of a published provision |
| PASS | POST `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions` | 200 | - | define a seat provision from the seat body |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions/1559563506242551808/Form` | 200 | - | seat form of the new draft |
| PASS | PUT `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions/1559563506242551808` | 200 | - | save the seat form back unchanged |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions/1559563506242551808/Form` | 200 | - | seat form after the save |
| PASS | GET `/AncillaryProvisions/1559563506242551808/Form` | 200 | - | generic form of the same provision |
| PASS | POST `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions` | 400 | - | a body with applicationType is refused |
| PASS | POST `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions` | 400 | - | a body with petRule is refused |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299254248275968/Provisions/1559563506242551808/Form` | 404 | 16301 | a provision under another seat definition is not found |
| PASS | PUT `/AncillaryServiceDefinitions/Seat/1559299254248275968/Provisions/1559563506242551808` | 404 | 16301 | a change through another seat definition is not found |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299037247569920/ProvisionForm` | 409 | 16219 | a baggage definition on the seat route |
| PASS | POST `/AncillaryServiceDefinitions/Seat/1559299037247569920/Provisions` | 409 | 16219 | a seat body posted to a baggage definition |
| PASS | GET `/AncillaryServiceDefinitions/Seat/987654321987/ProvisionForm` | 404 | 16201 | an unknown definition |
| PASS | PUT `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions/1559299223835377664` | 409 | 16303 | a change of the published provision is refused |
| PASS | GET `/AncillaryServiceDefinitions/Seat/1559299218793824256/ProvisionForm` | 401 | - | without a token |
| PASS | POST `/AncillaryServiceDefinitions/Seat/1559299218793824256/Provisions` | 400 | - | malformed body |
| PASS | GET `/AncillaryPricings/Paginated` | 200 | - | pricings of the published provision |
| PASS | GET `/AncillaryPricings/1559299227618639872` | 200 | - | pricing by id |
| PASS | POST `/AncillaryProvisions/1559563506242551808/Retire` | 200 | - | retire the draft made by this run |

Checks on the answers: the list holds only seat variants; SEAT_STANDARD descriptor names the variant, the fixed type and no provision; SEAT_PREFERRED descriptor names the variant, the fixed type and no provision; SEAT_EXTRA_COMFORT descriptor names the variant, the fixed type and no provision; the child list holds only rows of that definition; a published provision is not editable; the draft is editable and its form equals the body sent; the form has only the seat members; nothing changed after the save; the generic form holds the same values and the fixed seat type; the pricing belongs to that provision.

The run created one draft provision (sequence 990) on the standard seat product and retired it at the end; an earlier interrupted run left a second retired draft of the same kind.

Tests: `SeatProvisionFacadeAcceptanceTests` 12 of 12 (descriptor per variant, define / read / save back per variant, refused foreign members, 16219 / 16301 / 16321, domain seat rules, paid publication through the existing pricing and publish use cases, published OpenAPI). Suites after the slow tests were removed on the Owner's order: Domain 237 of 237, Shopping 182 of 182; Acceptance 292 of 295 in the full run, then the three failing pins (they hash the frozen reservation test file, which was restored) and the reservation tests passed 15 of 15 on their own. The full acceptance suite was not run again after that restore.

## 6. Airline ownership

Closed by the Owner on 2026-10-13: no ownership check is required. No guard was added and no route was changed for it.

## 7. Changed paths of this increment

```
 M src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/ChangeAncillaryProvision/Backoffice/BackofficeChangeAncillaryProvisionCommand.cs
 M src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/ChangeAncillaryProvision/ChangeAncillaryProvisionService.cs
 M src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/ChangeAncillaryProvision/IChangeAncillaryProvisionCommand.cs
 M src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/DefineAncillaryProvision/Backoffice/BackofficeDefineAncillaryProvisionCommand.cs
 M src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/DefineAncillaryProvision/DefineAncillaryProvisionService.cs
 M src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/DefineAncillaryProvision/IDefineAncillaryProvisionCommand.cs
 M src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetAncillaryProvisionById/AncillaryProvisionMapper.cs
 M src/AeroTech.Ancillary.Query/DependencyInjection.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Backoffice/P32COpenApiConformanceAcceptanceTests.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Backoffice/P32CSeatContractAcceptanceTests.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Boundary/V12BoundaryAcceptanceTests.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Fixtures/ControllerDesk.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Fixtures/M1Commands.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Fixtures/OpenApi/definition-contracts.json
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Fixtures/P1Commands.cs
?? docs/AeroTech-Ancillary-Seat-Backoffice-Agent-Pack-v1.0.zip
?? src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/ChangeAncillaryProvision/Backoffice/BackofficeChangeSeatProvisionCommand.cs
?? src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/ChangeAncillaryProvision/Backoffice/BackofficeChangeSeatProvisionCommandHandler.cs
?? src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/ChangeAncillaryProvision/Backoffice/BackofficeChangeSeatProvisionCommandValidator.cs
?? src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/DefineAncillaryProvision/Backoffice/BackofficeDefineSeatProvisionCommand.cs
?? src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/DefineAncillaryProvision/Backoffice/BackofficeDefineSeatProvisionCommandHandler.cs
?? src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/DefineAncillaryProvision/Backoffice/BackofficeDefineSeatProvisionCommandValidator.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Dto/BackofficeSeatProvisionCreateFormDto.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Dto/BackofficeSeatProvisionFormDto.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionCreateForm/Backoffice/BackofficeGetSeatProvisionCreateFormQuery.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionCreateForm/Backoffice/BackofficeGetSeatProvisionCreateFormQueryHandler.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionCreateForm/GetSeatProvisionCreateFormService.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionCreateForm/IGetSeatProvisionCreateFormService.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionForm/Backoffice/BackofficeGetSeatProvisionFormQuery.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionForm/Backoffice/BackofficeGetSeatProvisionFormQueryHandler.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionForm/GetSeatProvisionFormService.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionForm/IGetSeatProvisionFormService.cs
?? src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Queries/GetSeatProvisionForm/SeatProvisionFormMapper.cs
?? src/AeroTech.Ancillary.RestApi/V1/AncillaryProvisionAggregate/Controllers/SeatBackofficeController.cs
?? src/AeroTech.Ancillary.RestApi/V1/AncillaryProvisionAggregate/Requests/SeatProvisionRequest.cs
?? tests/AeroTech.Ancillary.Application.AcceptanceTests/Backoffice/SeatProvisionFacadeAcceptanceTests.cs
```

Other changes in the same working tree: the Owner-ordered removal of slow tests (the `Migration` test folder and 55 test methods) and the new root `CLAUDE.md`.
