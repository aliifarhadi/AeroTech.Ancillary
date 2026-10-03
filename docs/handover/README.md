# Handover to other services

AirOffer and Ordering each build their side from a spec and a prompt kept here. Ancillary owns these documents; the other repositories receive copies.

## What to copy where

Create `docs/ancillary/` in the target repository and copy the listed files from Ancillary's `docs/`, keeping their relative paths. Replace the copies whenever the originals change.

| Target repository | Files |
|---|---|
| `AeroTech.AirAvail` (AirOffer) | `handover/AirOffer/spec.md`, `reference/Ancillary-Domain-Master.md`, `reference/Ancillary-Edge-Contract.md`, `phases/P1-Extra-Baggage/phase.md`, `phases/P2-Lounge-Access/phase.md` |
| `AeroTech.Ordering.Final` | `handover/Ordering/spec.md`, `handover/AirOffer/spec.md`, `reference/Ancillary-Domain-Master.md`, `reference/Ancillary-Edge-Contract.md` |

## The work

| Service | What it builds | Spec | Prompt |
|---|---|---|---|
| AirOffer | Reads Ancillary's published read model by SQL into a cached snapshot; evaluates it; `POST Service/v1/AncillaryOffers` (service list) and `POST Service/v1/AncillaryOffers/Details` (pricing the selected items); offer and offer-item ids. No call to Ancillary. | `AirOffer/spec.md` | `AirOffer/prompt.md` |
| Ordering | `GET …/Orders/{id}/ServiceList` and `POST …/Orders/{id}/Services` (through AirOffer); provider `Ancillary` on its existing reservation machinery, so every ancillary service is reserved and confirmed at Ancillary; EMD-A for confirmed services. | `Ordering/spec.md` | `Ordering/prompt.md` |

Then the manual run: `End-to-End.md`.

State of this work: `../phases/README.md`, "Work in other services".
