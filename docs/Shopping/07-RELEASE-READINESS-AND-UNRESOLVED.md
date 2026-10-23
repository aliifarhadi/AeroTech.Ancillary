# 07 — Release gates Pack v8 و موارد حل‌نشده (بدون فرض مخفی)

**این سند شرایط اجرای قابل‌کنترل را جدا می‌کند از شروط راه‌اندازی Production.**

| موضوع | تصمیم بسته‌شده در Pack | وضعیت لازم برای Prod |
|---|---|---|
| `d_` و `r_` | HMAC+Expiry, issuer eligibility, bounded to sales+offer | authorized key/config, tests |
| Qty | `quantityOptions[]` from Shopping; **no GET/POST/Snapshot** | H05, S11, T04, R05 and 3 Surface tests |
| Typed | 1 DB table, non-consumptive resolve | query/migration tests |
| TTL | `min(sourceValidUntil, issueTime+maxAge)`, **proposed 15min** | Owner reviews/approves setting, not a hidden rule |
| POST retry | may produce new `s_`, Resolve repeat stable | UX follows latest ID; duplicates prevented in Ordering |
| Active-only Rate | Pricing parent/Provision/Definition active at issuer, Typed POST and Resolve; Retired/Suspended rejected | H13/T14/T17–T19/R13–R14 pass |
| PTC/POS | issuer verifies candidate before HMAC | negative forged Rate/Unit tests |
| Cross-Order | Ordering verifies offer/sales/unit binding | cannot claim actual prod integration from Ancillary-only code |
| A17 | add two Owner-approved Domain fields | still gated until approved encryption/handling |
| A18 | typed Infant/Guardian | gated if trusted refs/age cannot be proved in Options |
| A19 | Guardian text/bool mismatch | gated pending Owner-approved model correction |
| A09 | ExtraSeat source from FlightFlow | gated pending verified source |
| Insurance | none in current 24 variants | no invented new Domain |
| Currency EUR | `currencyId=47` per Owner | actual Catalog verified in Gate0 |
| Free pricing | Owner-approved Paid/Filed Rate(0) | verify business/document effects; no fake ID |
| Expiry physical cleanup | later phase | logical Expiry already enforced |

**A blocking result does not authorize an alternative architecture.** Agent reports exact source path/test, marks item GATED and continues only with independently safe vertical-slice flow. If it blocks all progress, stop for Owner, not guess.
