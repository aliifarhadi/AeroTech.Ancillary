#!/usr/bin/env python3
from pathlib import Path
import hashlib,re,json,sys
p=Path(__file__).resolve().parent
expected=['00-START-HERE.md', '01-MASTER-AND-OWNER-DECISIONS-FA.md', '02-EXACT-RESERVATION-API-AND-DTOS.md', '03-VARIANT-BY-VARIANT.md', '04-AGGREGATE-AND-RELATIONAL-STORAGE.md', '05-INVENTORY-AND-LIFECYCLE.md', '06-V10-REFORM-AND-MIGRATION.md', '07-ACCEPTANCE-MATRIX.md', '08-SOURCE-AND-BENCHMARK-FA.md', '10-AGENT-GATE0-SOURCE-AUDIT.md', '11-AGENT-GATE1-WIRE-CONTRACT.md', '12-AGENT-GATE2-TYPED-DOMAIN.md', '13-AGENT-GATE3-INVENTORY-HOLD.md', '14-AGENT-GATE4-SEAT-AND-LIFECYCLE.md', '15-AGENT-GATE5-E2E-CLOSURE.md']
assert sorted(f.name for f in p.glob('*.md'))==sorted(expected), 'Missing/extra markdown files'
checksum=dict((name,digest) for digest,name in (line.split('  ',1) for line in (p/'SHA256SUMS.txt').read_text().splitlines() if line))
assert set(checksum)==set(expected), 'checksum manifest wrong'
for name in expected:
 s=(p/name).read_text(encoding='utf-8')
 assert s.strip() and len(s)>300,(name,'empty')
 assert hashlib.sha256((p/name).read_bytes()).hexdigest()==checksum[name],(name,'checksum mismatch')
 assert s.count('```')%2==0,(name,'unclosed code fence')
 for match in re.finditer(r'```json\s*\n(.*?)\n```',s,re.S):
  try: json.loads(match.group(1))
  except Exception as exc: raise AssertionError(f'{name}: bad JSON {str(exc)}')
# tests unique
matrix=(p/'07-ACCEPTANCE-MATRIX.md').read_text()
ids=re.findall(r'^\|\s*([WDI LR][0-9]{2})\s*\|',matrix,re.M)
assert len(ids)>=75 and len(ids)==len(set(ids)),('test IDs',len(ids),len(set(ids)))
# no out-of-pack references to prior versions as authorities
for name in expected:
 s=(p/name).read_text()
 for fname in re.findall(r'`([0-9]{2}-[A-Z][^`]+?\.md)`',s):
  if not (p/fname).exists(): raise AssertionError(f'{name}: missing referenced {fname}')
# mandatory gate files
assert all(any(n.startswith(f'{i}-') and (p/n).exists() for n in expected) for i in range(10,16))
print(f'PASS: {len(expected)} Markdown, {len(ids)} unique tests, SHA-256, JSON syntax, fences, gate files, references')
