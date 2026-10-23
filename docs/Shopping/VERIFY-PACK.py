#!/usr/bin/env python3
"""Verify this AeroTech Ancillary v8 distribution after ZIP extraction.
Standard library only. This verifies the pack, NOT a build or runtime tests.
"""
from pathlib import Path
import hashlib, re, json, base64, hmac, sys
ROOT=Path(__file__).resolve().parent
DOCS=[
'00-START-HERE.md','01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md',
'02-EXACT-API-DTO-AND-FIELD-CONTRACT.md','03-A01-A24-VARIANT-DICTIONARY.md',
'04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md','05-ORDERING-HANDOFF-AND-TESTS.md',
'06-BENCHMARK-SOURCE-DECISIONS.md','07-RELEASE-READINESS-AND-UNRESOLVED.md',
'08-PACK-INTEGRITY-AND-QA.md','10-AGENT-GATE0-READONLY.md',
'11-AGENT-STAGE1-SECURITY-AND-PRICING.md','12-AGENT-STAGE2-SHOPPING.md',
'13-AGENT-STAGE3-TYPED-OPTIONS.md','14-AGENT-STAGE4-RESOLVE-AND-HANDOFF.md',
'15-AGENT-STAGE5-ACCEPTANCE-CLOSURE.md']
MANDATORY=DOCS+['VERIFY-PACK.py']
errors=[]
def check(cond,msg):
    if not cond: errors.append(msg)
for n in MANDATORY:check((ROOT/n).is_file(),f'MISSING: {n}')
if not errors:
    rows={}
    for line in (ROOT/'SHA256SUMS.txt').read_text().splitlines():
        m=re.fullmatch(r'([0-9a-f]{64})\s\s([^\\/]+)',line)
        if not m: errors.append('Invalid SHA256SUMS line '+line);continue
        if m.group(2) in rows:errors.append('Duplicate checksum '+m.group(2))
        rows[m.group(2)]=m.group(1)
    check(set(rows)==set(MANDATORY),'SHA256SUMS manifest has missing or extra entries')
    for name in MANDATORY:
        actual=hashlib.sha256((ROOT/name).read_bytes()).hexdigest()
        check(rows.get(name)==actual,'Checksum mismatch: '+name)
    texts={n:(ROOT/n).read_text(encoding='utf-8') for n in DOCS}
    legacy=re.compile(r'\b(?:v4|v5|v6|v7)\b',re.I)
    for name,t in texts.items():
        check(not legacy.search(t),'Legacy version reference in '+name)
        check(len(t.strip()) > 100, 'Suspicious empty file '+name)
        for ref in set(re.findall(r'\b(?:00|01|02|03|04|05|06|07|08|10|11|12|13|14|15)-[\w-]+\.md',t)):
            check(ref in DOCS,'Undelivered referred-to file '+ref+' in '+name)
        for index,block in enumerate(re.findall(r'```json\s*\n(.*?)\n```',t,re.S),start=1):
            try:json.loads(block)
            except Exception as e:errors.append(f'Invalid JSON in {name} block {index}: {e}')
    test=texts['05-ORDERING-HANDOFF-AND-TESTS.md']
    ids=re.findall(r'^\|\s*([SHTORN]\d{2})\s*\|',test,re.M)
    check(len(ids)==78,f'Expected 78 actual tests, found {len(ids)}')
    check(len(set(ids))==len(ids),'Duplicated test IDs')
    for prefix,n in [('S',16),('H',13),('T',19),('O',12),('R',14),('N',4)]:
        check({f'{prefix}{i:02}' for i in range(1,n+1)} == {x for x in ids if x.startswith(prefix)},'Missing/extra '+prefix+' tests')
    check('Quantity-only' in test and 'هیچ POST Options' in test,'Quantity exclusion not explicit in tests')
    check('Retired/Suspended' in test and 'R13' in test and 'R14' in test,'Retired blocking tests missing')
    for name in DOCS:
        if name.startswith(('11-','12-','13-','14-','15-')):
            check('VERIFY-PACK.py' in texts[name],'Missing authority gate in '+name)
    hmacdoc=texts['04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md']
    m=re.search(r'^ob = (\S+)\nsb = (\S+)',hmacdoc,re.M)
    check(bool(m),'No HMAC golden ob/sb')
    if m:
        b64=lambda x:base64.urlsafe_b64encode(x).rstrip(b'=').decode()
        ob=b64(hashlib.sha256(('AERO-OFFER-v1|'+b64(b'OFFER-TEST-01')+'|'+b64(b'v1')).encode()).digest())
        sb=b64(hashlib.sha256(b'AERO-SALE-v1|7|2|12|-|-|-|-').digest())
        check((ob,sb)==m.groups(),'Incorrect golden binding digests')
        for purpose in ('r','d'):
            payload='|'.join(['1',purpose,'5001','P',b64(b'ADT-1'),b64(b'B1'),'2' if purpose=='d' else '',ob,sb,'1792780800']).encode()
            tok=purpose+'_'+b64(payload)+'.'+b64(hmac.new(bytes(range(32)),('AERO-ANC-v1|'+purpose+'|').encode()+payload,hashlib.sha256).digest())
            check(f'{purpose}_token   = {tok}' in hmacdoc,'Wrong HMAC golden token '+purpose)
            check(f'{purpose}_payload = {payload.decode()}' in hmacdoc,'Wrong HMAC payload '+purpose)
    shop=texts['02-EXACT-API-DTO-AND-FIELD-CONTRACT.md']
    check('{ids:string[]}' in shop or 'ids:string[]' in shop,'Resolve signature absent')
    check('quantityOptions' in shop and 'Quantity-only' in test and 'هیچ POST Options' in test,'Quantity only contract absent')
    print('VERIFY PACK v8:', 'PASS' if not errors else 'FAIL')
    print(f'Files: {len(DOCS)} Markdown + VERIFY-PACK.py + SHA256SUMS.txt; test cases {len(ids)}')
    print('HMAC golden vectors: recomputed from fixture key')
    print('JSON examples and SHA256: checked')
for error in errors:print('ERROR:',error)
sys.exit(1 if errors else 0)
