#!/usr/bin/env python3
import csv
import json
import re
import sys
from pathlib import Path


def normalize_header(h):
    return h.strip()


def split_events(cell):
    if not cell:
        return []
    parts = re.split(r"[;,/|]+", cell)
    return [p.strip() for p in parts if p and p.strip()]


def main():
    if len(sys.argv) < 3:
        print("Usage: csv_to_registrants_json.py <input.csv> <output.json>")
        sys.exit(2)

    input_path = Path(sys.argv[1])
    output_path = Path(sys.argv[2])

    if not input_path.exists():
        print(f"Input file not found: {input_path}")
        sys.exit(2)
#!/usr/bin/env python3
import csv
import json
import sys
import re
from pathlib import Path


STATE_MAP = {
    'Alabama': 'AL','Alaska': 'AK','Arizona': 'AZ','Arkansas': 'AR','California': 'CA','Colorado': 'CO',
    'Connecticut': 'CT','Delaware': 'DE','Florida': 'FL','Georgia': 'GA','Hawaii': 'HI','Idaho': 'ID',
    'Illinois': 'IL','Indiana': 'IN','Iowa': 'IA','Kansas': 'KS','Kentucky': 'KY','Louisiana': 'LA',
    'Maine': 'ME','Maryland': 'MD','Massachusetts': 'MA','Michigan': 'MI','Minnesota': 'MN','Mississippi': 'MS',
    'Missouri': 'MO','Montana': 'MT','Nebraska': 'NE','Nevada': 'NV','New Hampshire': 'NH','New Jersey': 'NJ',
    'New Mexico': 'NM','New York': 'NY','North Carolina': 'NC','North Dakota': 'ND','Ohio': 'OH','Oklahoma': 'OK',
    'Oregon': 'OR','Pennsylvania': 'PA','Rhode Island': 'RI','South Carolina': 'SC','South Dakota': 'SD',
    'Tennessee': 'TN','Texas': 'TX','Utah': 'UT','Vermont': 'VT','Virginia': 'VA','Washington': 'WA',
    'West Virginia': 'WV','Wisconsin': 'WI','Wyoming': 'WY','District of Columbia': 'DC'
}

BUCKET_KEYS = ['Arts','Academics','Creative Ministries','Music','Individual Sports','Team Sports']


def normalize_header(h):
    return h.strip()


def split_cells(cell):
    if not cell:
        return []
    parts = re.split(r'[;,/|]+', cell)
    return [p.strip() for p in parts if p and p.strip()]


def parse_address(addr):
    if not addr:
        return None
    parts = [p.strip() for p in addr.split(',') if p and p.strip()]
    street = parts[0] if len(parts) > 0 else None
    city = parts[1] if len(parts) > 1 else None
    state = None
    postal = None
    country = 'USA'

    remainder = parts[2] if len(parts) > 2 else ''
    # attempt to extract postal code
    m = re.search(r'(\d{5}(?:-\d{4})?)', remainder)
    if m:
        postal = m.group(1)

    # Find state by name or abbreviation
    # check remainder tokens
    tokens = re.split(r'[\s]+', remainder)
    for tok in tokens:
        tclean = re.sub(r'[^A-Za-z\-]', '', tok).strip()
        if not tclean:
            continue
        # full name
        title = tclean.title()
        if title in STATE_MAP:
            state = STATE_MAP[title]
            break
        # abbreviation
        up = tclean.upper()
        if up in STATE_MAP.values():
            state = up
            break

    # fallback: try lookups in the second part (some addresses have 'City State Zip' in parts[1])
    if not state and city:
        m2 = re.search(r'([A-Za-z ]+)\s+([A-Za-z]{2})\s*(\d{5})?$', city)
        if m2:
            # city may contain the state abbreviation
            city = m2.group(1).strip()
            if m2.group(2):
                state = m2.group(2).upper()
            if m2.group(3):
                postal = m2.group(3)

    return {
        'streetAddress1': street or None,
        'city': city or None,
        'postalCode': postal or None,
        'country': country,
        'state': state or None
    }


def bucket_events(row, reader_fieldnames):
    buckets = {k: [] for k in BUCKET_KEYS}

    def add_to_bucket(bucket, item):
        if not item:
            return
        item = item.strip()
        if item and item not in buckets[bucket]:
            buckets[bucket].append(item)

    # Process the 'Events' free-form column first
    events_raw = (row.get('Events') or '').strip()
    if events_raw:
        for ev in split_cells(events_raw):
            # if already prefixed like 'Arts: ...'
            if ':' in ev:
                pref, rest = ev.split(':', 1)
                pref = pref.strip()
                rest = rest.strip()
                # normalize prefix to match a bucket
                for bk in BUCKET_KEYS:
                    if pref.lower() == bk.lower():
                        # split sub-items
                        for sub in split_cells(rest):
                            add_to_bucket(bk, sub)
                        break
                else:
                    # try heuristic matching
                    lowered = ev.lower()
                    if 'music' in lowered:
                        add_to_bucket('Music', ev)
                    elif 'poetry' in lowered or 'prose' in lowered or 'impromptu' in lowered or 'original oratory' in lowered:
                        add_to_bucket('Creative Ministries', ev)
                    elif 'math' in lowered or 'science' in lowered or 'academ' in lowered:
                        add_to_bucket('Academics', ev)
                    elif any(sport in lowered for sport in ('soccer','basketball','softball','volleyball','flag','dodgeball','gaga','ultimate','team')):
                        add_to_bucket('Team Sports', ev)
                    else:
                        # default to Arts if unclear
                        add_to_bucket('Arts', ev)
            else:
                # no prefix, try keyword heuristics
                lowered = ev.lower()
                if any(k in lowered for k in ('math','science','history','academ')):
                    add_to_bucket('Academics', ev)
                elif any(k in lowered for k in ('poetry','prose','preaching','drama','impromptu','storytelling','puppets','video','human video')):
                    add_to_bucket('Creative Ministries', ev)
                elif any(k in lowered for k in ('vocal','instrumental','worship','band','solo','duet','choir','keyboard','woodwinds','brass','strings')):
                    add_to_bucket('Music', ev)
                elif any(k in lowered for k in ('100 m','5k','sprint','cross country','golf','table tennis','pickleball','swimming','disc golf','chess','track')):
                    add_to_bucket('Individual Sports', ev)
                elif any(k in lowered for k in ('soccer','basketball','softball','volleyball','flag football','dodgeball','gaga','ultimate')):
                    add_to_bucket('Team Sports', ev)
                else:
                    add_to_bucket('Arts', ev)

    # Process explicit event columns
    for col in BUCKET_KEYS:
        val = (row.get(col) or '').strip()
        if not val:
            continue
        # If it's a simple yes/selected mark, add bucket name as indicator
        if val.lower() in ('yes','y','selected','x'):
            add_to_bucket(col, col)
        else:
            # value may be a list
            for e in split_cells(val):
                add_to_bucket(col, e)

    # Also capture any additional columns that look like event categories
    for key in reader_fieldnames:
        if key in ('Church','Form Submission Date','First Name','Last Name','Shirt Size','Gender',
                   'Registration Type','Participation Status','Grade Level','Address','Events','Price','Paid','Total Remaining'):
            continue
        if key in BUCKET_KEYS:
            continue
        if any(w in key.lower() for w in ('event','competition','arts','music','sport','creative','academ')):
            val = (row.get(key) or '').strip()
            if not val:
                continue
            for e in split_cells(val):
                # best-effort: place into matching bucket
                lowered = e.lower()
                if 'music' in lowered:
                    add_to_bucket('Music', e)
                elif any(k in lowered for k in ('poetry','prose','preaching','drama','impromptu')):
                    add_to_bucket('Creative Ministries', e)
                elif any(k in lowered for k in ('math','science','academ')):
                    add_to_bucket('Academics', e)
                elif any(k in lowered for k in ('soccer','basketball','softball','volleyball','flag','dodgeball','gaga','ultimate')):
                    add_to_bucket('Team Sports', e)
                else:
                    add_to_bucket('Arts', e)

    return buckets


def main():
    if len(sys.argv) < 3:
        print("Usage: csv_to_registrants_json.py <input.csv> <output.json>")
        sys.exit(2)

    input_path = Path(sys.argv[1])
    output_path = Path(sys.argv[2])

    if not input_path.exists():
        print(f"Input file not found: {input_path}")
        sys.exit(2)

    records = []
    with input_path.open(newline='', encoding='utf-8-sig') as fh:
        reader = csv.DictReader(fh)
        # Normalize header keys
        reader.fieldnames = [normalize_header(h) for h in reader.fieldnames]

        for row in reader:
            # Helper to safe-get values
            def g(col):
                return (row.get(col) or '').strip()

            givenName = g('First Name')
            familyName = g('Last Name')
            shirtSize = g('Shirt Size') or None
            gender = g('Gender') or None
            registrationType = g('Registration Type') or None
            participationStatus = g('Participation Status') or None
            gradeLevel = g('Grade Level') or None

            church_name = g('Church')
            church = {'name': church_name} if church_name else None

            # Address parsing
            raw_addr = g('Address')
            if not raw_addr:
                # try other address-like columns
                for key in reader.fieldnames:
                    if 'address' in key.lower() and key.lower() != 'address':
                        raw_addr = g(key)
                        if raw_addr:
                            break

            address = parse_address(raw_addr) if raw_addr else None

            events_buckets = bucket_events(row, reader.fieldnames)

            registrant = {
                'givenName': givenName,
                'familyName': familyName,
                'shirtSize': shirtSize,
                'gender': gender,
                'registrationType': registrationType,
                'participationStatus': participationStatus,
                'gradeLevel': gradeLevel,
                'church': church,
                'address': address,
                'events': events_buckets
            }

            records.append(registrant)

    # Write output
    output_path.write_text(json.dumps(records, ensure_ascii=False, indent=2), encoding='utf-8')
    print(f"Wrote {len(records)} records to {output_path}")


if __name__ == '__main__':
    main()
