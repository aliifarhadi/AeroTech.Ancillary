#!/usr/bin/env python3
"""Verify the shipped docs from SHA256SUMS.txt without network access."""
from pathlib import Path
import hashlib
import sys

root = Path(__file__).resolve().parent
manifest = root / 'SHA256SUMS.txt'
required = [f'{i:02d}-' for i in range(15)]
if not manifest.is_file():
    sys.exit('FAIL: missing SHA256SUMS.txt')
lines = [line for line in manifest.read_text(encoding='utf-8').splitlines() if line.strip()]
seen = set()
errors = []
for line in lines:
    try:
        expected, name = line.split('  ', 1)
    except ValueError:
        errors.append('invalid manifest line')
        continue
    if '/' in name or '\\' in name or name.startswith('.') or name in seen:
        errors.append(f'invalid/duplicate path: {name}')
        continue
    seen.add(name)
    p = root / name
    if not p.is_file():
        errors.append(f'missing: {name}')
    elif hashlib.sha256(p.read_bytes()).hexdigest() != expected:
        errors.append(f'hash mismatch: {name}')
for prefix in required:
    if not any(name.startswith(prefix) and name.endswith('.md') for name in seen):
        errors.append(f'missing document prefix {prefix}')
if 'VERIFY-PACK.py' not in seen:
    errors.append('verifier missing from manifest')
if len(seen) != 16:
    errors.append(f'expected 15 markdown files plus verifier, got {len(seen)} entries')
if errors:
    sys.exit('FAIL:\n' + '\n'.join(errors))
print(f'PASS: {len(seen)} files verified with SHA-256; docs 00..14 present')
