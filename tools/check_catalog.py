#!/usr/bin/env python3
"""Run: python tools/check_catalog.py (from repository root)."""
import json
from pathlib import Path
from urllib.parse import urlparse, parse_qs
import re

root = Path(__file__).resolve().parents[1]
catalog = json.loads((root / 'catalog.json').read_text(encoding='utf-8'))
assert catalog['schemaVersion'] == 1, 'schemaVersion must be 1'
ids = set()
for p in catalog['packages']:
    pid = p['id']
    assert pid not in ids, f'duplicate id: {pid}'
    ids.add(pid)
    assert re.fullmatch(r'com\.[a-z0-9]+(?:[.-][a-z0-9]+)+', pid), pid
    pkg_type = p.get('type', 'upm')
    if pkg_type == 'unitypackage':
        download = urlparse(p['downloadUrl'])
        assert download.scheme == 'https'
        assert download.hostname in ('github.com', 'raw.githubusercontent.com')
        assert download.path.endswith('.unitypackage')
        print(f'OK  {pid} v{p["version"]} (.unitypackage)')
        continue
    assert pkg_type == 'upm', (pid, pkg_type)
    package_dir = root / 'Packages' / pid
    meta = json.loads((package_dir / 'package.json').read_text(encoding='utf-8'))
    assert meta['name'] == pid, (pid, meta['name'])
    assert meta['version'] == p['version'], (pid, meta['version'], p['version'])
    url = urlparse(p['gitUrl'])
    assert url.scheme == 'https' and url.hostname == 'github.com', p['gitUrl']
    assert url.path.endswith('.git'), p['gitUrl']
    assert parse_qs(url.query) == {'path': [f'/Packages/{pid}']}, p['gitUrl']
    assert url.fragment and re.fullmatch(r'[a-zA-Z0-9][a-zA-Z0-9_.-]*',url.fragment), p['gitUrl']
    print(f'OK  {pid} v{p["version"]} tag={url.fragment}')
print('Catalog and local package.json files: OK')
