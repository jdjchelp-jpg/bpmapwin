import json, sqlite3, sys

source, target = sys.argv[1:3]
db = sqlite3.connect(target)
db.executescript('''
PRAGMA journal_mode=WAL;
CREATE TABLE IF NOT EXISTS places(id INTEGER PRIMARY KEY, name TEXT NOT NULL, category TEXT, latitude REAL, longitude REAL, address TEXT);
CREATE VIRTUAL TABLE IF NOT EXISTS places_fts USING fts5(name, address, category, content='places', content_rowid='id');
''')
with open(source, encoding='utf-8') as f:
    data = json.load(f)
for i, feature in enumerate(data.get('features', []), 1):
    props = feature.get('properties') or {}
    name = props.get('name')
    geometry = feature.get('geometry') or {}
    coords = geometry.get('coordinates') or []
    if not name or len(coords) < 2 or geometry.get('type') != 'Point':
        continue
    category = props.get('amenity') or props.get('shop') or props.get('highway') or ''
    address = ' '.join(str(props.get(k, '')) for k in ('addr:housenumber', 'addr:street', 'addr:city')).strip()
    db.execute('INSERT OR REPLACE INTO places VALUES (?, ?, ?, ?, ?, ?)', (i, name, category, coords[1], coords[0], address))
db.execute("INSERT INTO places_fts(places_fts) VALUES('rebuild')")
db.commit(); db.close()
