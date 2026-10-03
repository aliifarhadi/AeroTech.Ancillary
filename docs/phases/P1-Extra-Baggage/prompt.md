# Prompt — Phase 1 (record)

Phase 1 is done (`11e5941`, `467aa55`). It was built in two sessions, kept here as a record. Their paths are the ones the repository had then; today the files are `docs/phases/P1-Extra-Baggage/phase.md` and `proof.http`.

1. **Step A — build and prove.** Read CLAUDE.md, the Master, `phase.md`, the OpenAPI file and `proof.http`; build Phase 1 in this order: enums, `ServiceSubCode` with the industry reference (`0CC`), `AncillaryProduct`, `AncillaryPriceRule`, the quote; migrations; run `proof.http`; report. No tests in this step.
2. **Closure — layout repair and conformance tests.** Put the documents in their places; build and prove again; write one test per row of `phase.md` §4 (70 rows, `P1_<rowId>_…`) as domain, application, persistence and contract tests; the only allowed project references: AcceptanceTests → Query and Framework.Presentation; report.
