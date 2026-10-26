#!/usr/bin/env python3
"""Reservation Pack v10 integrity, self-contained. Python stdlib only."""
from pathlib import Path
import hashlib, re, sys, json

HERE = Path(__file__).resolve().parent
EXPECTED = [
    '00-START-HERE.md',
    '01-MASTER-RESERVATION-CONTRACT-FA.md',
    '02-EXACT-ENDPOINTS-AND-DTOS.md',
    '03-DOMAIN-AND-LIFECYCLE.md',
    '04-INVENTORY-ALLOCATION-ALGORITHM.md',
    '05-SEATMAP-AND-ORDERING-HANDOFF.md',
    '06-FAILURE-RETRY-EXPIRY-AND-MIGRATION.md',
    '07-ACCEPTANCE-MATRIX.md',
    '08-BENCHMARK-AND-SOURCE-EVIDENCE.md',
    '09-SOURCE-TRACEABILITY.md',
    '10-AGENT-GATE0-AUDIT.md',
    '11-AGENT-GATE1-DOMAIN-CONTRACTS.md',
    '12-AGENT-GATE2-LOCAL-INVENTORY.md',
    '13-AGENT-GATE3-SEATMAP-AND-LIFECYCLE.md',
    '14-AGENT-GATE4-E2E-CLOSURE.md',
    'VERIFY-PACK.py',
]
errors = []
for name in EXPECTED:
    if not (HERE / name).is_file(): errors.append('MISSING ' + name)

manifest_file = HERE / 'SHA256SUMS.txt'
if not manifest_file.is_file(): errors.append('MISSING SHA256SUMS.txt')
else:
    rows = [line.strip().split('  ', 1) for line in manifest_file.read_text().splitlines() if line.strip()]
    checksums = {}
    for row in rows:
        if len(row) != 2:
            errors.append('INVALID MANIFEST LINE ' + str(row)); continue
        checksum, name = row
        if name in checksums: errors.append('DUP HASH ' + name)
        checksums[name] = checksum
        file = HERE / name
        if not file.is_file(): errors.append('MISSING HASHED ' + name)
        elif hashlib.sha256(file.read_bytes()).hexdigest() != checksum: errors.append('HASH MISMATCH ' + name)
    if set(checksums) != set(EXPECTED): errors.append('MANIFEST INVENTORY MISMATCH')

all_ids = []
for name in EXPECTED:
    if not name.endswith('.md') or not (HERE / name).is_file(): continue
    content = (HERE / name).read_text(encoding='utf-8')
    if content.count('```') % 2: errors.append('UNBALANCED FENCES ' + name)
    for block in re.findall(r'```json\s*\n(.*?)\n```', content, re.S):
        try: json.loads(block)
        except json.JSONDecodeError as exc: errors.append(f'INVALID JSON {name}: {exc}')
    for mention in re.findall(r'(?<![/\w])(?:0[0-9]|1[0-4])-[A-Za-z0-9-]+\.md', content):
        if mention not in EXPECTED: errors.append('MISSING LOCAL REFERENCE ' + name + ' -> ' + mention)

matrix = (HERE / '07-ACCEPTANCE-MATRIX.md').read_text(encoding='utf-8') if (HERE / '07-ACCEPTANCE-MATRIX.md').is_file() else ''
all_ids = re.findall(r'^\|\s*([AILSER]\d{2})\s*\|', matrix, re.M)
if len(all_ids) != len(set(all_ids)): errors.append('DUP TEST IDS')
if len(all_ids) < 80: errors.append('INSUFFICIENT TEST SCENARIOS: ' + str(len(all_ids)))
if 'فعلا' in '': pass

if errors:
    print('FAIL', len(errors), 'checks')
    print('\n'.join(errors))
    sys.exit(1)
print('PASS: {} required files, {} unique numbered acceptance tests, SHA-256, JSON, references, fences'.format(len(EXPECTED), len(all_ids)))
