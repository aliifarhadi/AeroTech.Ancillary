# Owner approval sheet - sign off before coding

## Proposed decisions (approve/amend)
- [ ] A. Exactly three Phase-1 commercial roots: `AncillaryServiceDefinition`, `AncillaryProvision`, `AncillaryPricing`; preserve existing Supplier.
- [ ] B. Provision owns 10 typed Rule-group child Entities and 27 typed child-row Entities; no general-purpose RuleEngine/EAV or independent Rule aggregate.
- [ ] C. Replace separate exact-date list + seasonal list with ONE `ProvisionTravelDateRule` containing `ProvisionPermittedTravelPeriod` (allow) and `ProvisionBlackoutPeriod` (deny). No annual recurrence yet.
- [ ] D. Day/Time is weekly `ProvisionDayTimeApplicationRule` with day mask, start/end local-time and Allow/Deny; Deny wins.
- [ ] E. Passenger, Sale, Geography, Flight and Fare selectors are positive allow-lists; exclusions via more specific `NotAvailable` Provision with lower Sequence.
- [ ] F. Add `ServiceDateBasis` to ServiceDefinition so flight, hotel, transfer, insurance, SIM are not interpreted using a fictional common travel date.
- [ ] G. Keep one PricingUnit per product identity, preserve Pricing aggregate/lines, add a QuantityUnit compatibility matrix and explicit age/PTC selector rules.
- [ ] H. Preserve historic v12 record meaning; migration intersection of old date lists, not union; no phase-1 production cleanup of historical data.
- [ ] I. Commit to the P1 scenario matrix and NO-GO gate; Phase-2 and Phase-3 remain locked.

## Deliverables after approval
1. Build a **new implementable agent prompt** (not currently released) against an explicitly verified commit SHA.
2. Require failing coverage tests showing the current v12 defects and the corrected accepted truth tables.
3. Implement only approved Phase-1 domain changes, EF command/query migration, read API and backoffice changes; preserve 3-phase plan.
4. Run local build + full test suite + migration clone + regression; owner audits push before closing Phase-1.

## Summary diff vs v12
| v12 | proposed v12.1 |
|---|---|
| `ProvisionTravelDate(DateOnly)` | `ProvisionPermittedTravelPeriod(StartDate,EndDate)` |
| independent `ProvisionSeasonalPeriod` | REMOVED as second positive dimension; seasonality business label applied to permitted periods |
| `ProvisionBlackoutPeriod` peer | retained, nested under TravelDateRule, semantics: deny |
| `ProvisionDayTimeRestriction(DayOfWeek,Start,End,Effect)` | `ProvisionDayTimeWindow(DaysOfWeekMask,StartLocalTime?,EndLocalTime?,Effect)` |
| 25 direct peer criterion collections | 10 typed rule Entities owning typed child rows |
| no separate nonflight local occurrence basis | new immutable `ServiceDateBasis` |
| product unit vs quantity unit mismatch possible | fixed compatibility matrix and explicit publication guard |
| 1000 consecutive days = 1000 rows | 1000 consecutive days = one period row |
| no comprehensive negative-veto truth table | explicit NotAvailable precedence and Deny tests |

**STATE: AWAITING OWNER APPROVAL. DO NOT IMPLEMENT.**
