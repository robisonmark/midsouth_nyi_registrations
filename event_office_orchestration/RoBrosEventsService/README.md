## Initial Data Contract Thoughts
```
event: {
  id: uuid,
  name: string,
  points: {
    first: int,
    second: int,
    third: int,
  }
}
```


# Product Requirements Document — Talent Competition Signup & Scheduling
## Summary / goal

A lightweight, low-cost C# service to manage timed signups for participants in talent-competition events across multiple rooms/locations. Admins define events, master times and available participant slots. Competitors (or their coach) search for themselves and reserve an available slot. The initial build is simple (no auth), later extensible to full registration and web UI.

## Actors & roles

Competitor / Coach — looks up participant(s) and signs a participant (or group) into an available time slot.

Admin — creates/manages events, locations, timeslots, rules, and can manually assign/override slots.

System — sends confirmations, exposes data for public schedule display (optional).

## High-level workflows
1) Admin: create an event

Admin creates Event metadata (name, category, rules, is-team/individual flag).

Admin assigns one or more Locations (rooms) to the Event.

Admin sets master schedule blocks (EventTimes) for that Event — each EventTime is a slot that participants can claim; it may also be a "master" block that contains many smaller participant slots.

Admin optionally sets capacity per slot and per time-block (for group/team events).

2) Competitor/Coach: sign up for a slot (simple flow)

User searches for participant(s) by name or ID (text search).

System shows Events and EventTimes with availability (room, start/end, remaining slots).

User chooses a time slot and provides contact name/phone/email (if needed).

System reserves the slot atomically and returns confirmation (and optionally sends email/SMS).

3) Admin: override / manage

Admin can move participants between slots, cancel reservations, or block/unblock slots.

Admin can bulk import participants or bulk assign for group events.

4) Optional: check-in and attendance

Admin/volunteer checks in a participant when they arrive; system logs check-in time and can mark “performed”.

## Core data model (relational) — minimal but extensible

Below are suggested tables, columns, and indexes. Use UUIDs for IDs to simplify distributed systems.

events (master event)
- id UUID PK
- name varchar
- category varchar (enum: music, arts, sports, etc.)
- type varchar (enum: individual, team)
- description text
- master_start datetime (optional)
- master_end datetime (optional)
- rules_id UUID FK -> event_rules (nullable)
- is_active bool
- created_at
- updated_at

Indexes: idx_events_category, idx_events_is_active

locations
- id UUID PK
- name varchar (e.g., "Room A — Gym 1")
- building varchar
- capacity int (optional)
- notes text
- created_at, updated_at
- event_locations (join)
- id UUID PK
- event_id UUID FK -> events
- location_id UUID FK -> locations

Composite unique index on (event_id, location_id).

event_time_blocks (master blocks — e.g., “Choir 9:00–11:00”)
- id UUID PK
- event_id UUID FK
- location_id UUID FK
- start_time datetime
- end_time datetime
- slot_duration_minutes int (if you want to auto-split into repeating participant slots) — optional
- manual_slots bool (true = admin created slots individually)
- max_capacity int (if multiple participants per time)
- notes
- created_at, updated_at

Index on (event_id, location_id, start_time)

event_slots (individual sign-up slots; generated from time_blocks or created manually)
- id UUID PK
- time_block_id UUID FK -> event_time_blocks
- event_id UUID FK (denormalized for quick queries)
- location_id UUID FK
- start_time datetime
- end_time datetime
- slot_number int (optional; e.g., 1..N in block)
- capacity int (how many participants can claim this slot)
- reserved_count int (cached, to avoid expensive counts) — update within transactions
- status enum (open, closed, cancelled)
- created_by_admin bool
- created_at, updated_at

Index: (event_id, start_time), (location_id, start_time)

Note: you can either generate event_slots ahead of time (recommended) or generate them on demand.


participants (registered externally; we store minimal searchable record)
- id UUID PK
- external_participant_id varchar (system that created participant; optional)
- first_name, last_name, display_name computed
- age, grade, group_id (if part of a team), school
- metadata jsonb (free-form)
- created_at, updated_at

Index: full-text index on name fields for quick lookup (e.g., PostgreSQL's GIN on to_tsvector)


slot_reservations
- id UUID PK
- slot_id UUID FK -> event_slots (denormalized to include event_id, location_id)
- participant_id UUID FK -> participants (nullable if coach enters free-form name)
- reserved_name varchar (the name input at reservation; for non-registered users)
- reserved_contact varchar (phone/email)
- status enum (reserved, confirmed, cancelled, checked_in, no_show)
- created_at, reserved_at datetime
- checked_in_at datetime nullable
- expires_at datetime nullable (if provisional holds are used)
- version int (optimistic concurrency)

Unique constraints: (slot_id, participant_id) optional if duplicates disallowed

Indexes: (slot_id, status), (participant_id, status).

event_rules
- id UUID PK
- event_id UUID FK (optional)
- rule_blob jsonb (scoring/configuration)
- description text

audit_logs (audit trail)
- id UUID PK
- actor varchar (admin name or system)
- action varchar
- target_type varchar
- target_id UUID
- payload jsonb (what changed)
- created_at

## Example SQL (Postgres-ish) — skeleton
```CREATE TABLE events (id uuid PRIMARY KEY, name text, category text, type text, master_start timestamptz, master_end timestamptz, is_active boolean DEFAULT true, created_at timestamptz DEFAULT now(), updated_at timestamptz DEFAULT now());

CREATE TABLE locations (id uuid PRIMARY KEY, name text, building text, capacity int, created_at timestamptz DEFAULT now());

CREATE TABLE event_time_blocks (id uuid PRIMARY KEY, event_id uuid REFERENCES events(id), location_id uuid REFERENCES locations(id), start_time timestamptz, end_time timestamptz, slot_duration_minutes int, manual_slots boolean DEFAULT false);

CREATE TABLE event_slots (id uuid PRIMARY KEY, time_block_id uuid REFERENCES event_time_blocks(id), event_id uuid, location_id uuid, start_time timestamptz, end_time timestamptz, capacity int DEFAULT 1, reserved_count int DEFAULT 0, status text DEFAULT 'open');

CREATE TABLE participants (id uuid PRIMARY KEY, external_participant_id text, first_name text, last_name text, metadata jsonb, created_at timestamptz DEFAULT now());

CREATE TABLE slot_reservations (id uuid PRIMARY KEY, slot_id uuid REFERENCES event_slots(id), participant_id uuid REFERENCES participants(id), reserved_name text, reserved_contact text, status text DEFAULT 'reserved', created_at timestamptz DEFAULT now(), version int DEFAULT 1);
```

## Concurrency & race conditions (critical)

Multiple users may try to reserve the same slot concurrently. You must enforce atomic reserve semantics.

Options:
Database transaction with row lock (recommended for RDS/Postgres):
```
Begin transaction.

SELECT ... FOR UPDATE the event_slots row.

Check reserved_count < capacity.

Insert into slot_reservations and increment reserved_count.

Commit.
```
Pros: simple, correct, no extra infrastructure. Cons: transactions block briefly.

Optimistic concurrency (version number):

Read slot with version.

Attempt UPDATE event_slots SET reserved_count = reserved_count + 1, version = version + 1 WHERE id = ? AND version = ? AND reserved_count < capacity.

If update affected 1 row, proceed to insert reservation. If not, retry.

Pros: scales well, fewer locks. Cons: requires retry logic.

Distributed lock (Redis/Redlock):

Acquire lock for slot:{id} for short time, do reserve, release.

Use when you need cross-db operations or extreme scale.

Adds infrastructure complexity.

Recommendation: start with DB transactions (SELECT FOR UPDATE) or optimistic concurrency; add Redis lock only if performance issues arise.

APIs (HTTP/REST) — surface for front end or lambda

Design as small REST endpoints (for C# minimal APIs or Lambda API Gateway).

Public / Competitor endpoints

GET /events — list events (filter by category/active)

GET /events/{eventId}/timeblocks — list time blocks and slot summary

GET /slots?eventId=&locationId=&from=&to= — list available slots (paginated)

GET /participants/search?q=name — search participants (text search)

POST /slots/{slotId}/reserve — reserve a slot

Payload: { participantId?, reservedName, contact, metadata? }

Response: success + reservation id + slot details

Admin endpoints (secure)

POST /events — create event

PUT /events/{id} — update

POST /events/{id}/timeblocks — create timeblock

POST /timeblocks/{blockId}/generate_slots — generate slots (given duration/capacity)

POST /slots/{slotId}/override-reserve — admin reserve

GET /reservations — list reservations (filters)

PUT /reservations/{id} — update/cancel/check-in

Webhooks / Notifications

POST /webhooks/reservation-created — notify downstream (optional)

Email/SMS: integrate with SES/Twilio via worker.

Data validation & UX considerations

Show remaining capacity on UI and grey out slots that are full.

Prevent long holds: implement expires_at for unconfirmed provisional holds (if you want "hold" semantics).

For group signups (choir), allow coach to reserve one slot and attach multiple participant_ids to that reservation (or have a reservation_members table linking reservation->participant).

Search: fuzzy name search; allow partial matches and show matches with school/grade for disambiguation.

If no auth, rate-limit reservation endpoints and add captcha if public.

Non-functional requirements

Availability: Use multi-AZ DB or serverless DB; design for eventual high concurrency (hundreds of concurrent reservations).

Latency: Reservation response < 1s ideally.

Cost: Minimize server costs by using serverless (Lambda) + RDS serverless or DynamoDB.

Auditability: All reservation changes logged in audit_logs.

Security: Admin APIs protected (JWT or API key); all endpoints over HTTPS.

Storage options & tradeoffs
Relational DB (Postgres / RDS / Aurora)

Pros: ACID, easy transactions, complex queries, joins, full-text search (Postgres).

Cons: Slightly higher cost if always-on; but RDS serverless or single small instance can be cheap.

Best when you want transactions + relational queries (recommended).

DynamoDB (NoSQL)

Pros: Serverless, scales easily, low ops.

Cons: More complex to implement transactional decrements for slots (but possible using conditional writes/transactions). Text search requires ElasticSearch/OpenSearch or use DynamoDB Streams + search service.

Use if you want pay-per-request scale and are comfortable designing keys and conditional writes.

Recommendation: Start with Postgres (managed RDS or Aurora Serverless) — simplest for correctness and development speed.

Deployment options (C#)

AWS Lambda + API Gateway

Pros: Low cost at low traffic, auto-scale, pay-per-invocation. Use Amazon RDS Proxy for DB connections or use serverless DB.

Cons: Cold start (mitigate with provisioned concurrency), connection pooling considerations.

Good if you want cost-effective, event-driven.

Lightsail / EC2 (single small instance)

Pros: Simple, predictable; easier local debugging.

Cons: Less elastic; ops overhead.

Good if you need persistent state or easier debugging.

Container (ECS/Fargate)

Pros: Balanced for small-medium scale; control over runtime.

Cons: More configuration than Lambda.

Recommendation: For minimal ops + low cost: Lambda + RDS Proxy + Aurora Serverless v2 / RDS. If you prefer a simpler single instance while iterating, run a small EC2 or Lightsail and migrate later.

Security & operational concerns

Protect admin endpoints (API key, OAuth, or simple username/password initially).

Sanitize/validate input to prevent SQL injection (use parameterized queries / ORMs).

Add rate limiting on public reservation endpoints to prevent abuse.

Backups: nightly DB backups; test restore.

Monitoring: CloudWatch (Lambda), RDS metrics, request error rates and reservation conflicts.

Search / UX: how participants find themselves

Implement full text search on participants (Postgres GIN index).

Provide fuzzy matching and show multiple fields (school, age) to disambiguate.

Allow anonymous reservations using just reserved_name if participant wasn't found.

Future features / enhancements

Authentication / user accounts (coaches/competitors).

Self-service registration (create participant records).

Payment integration (if event fees).

Mobile-friendly web UI for signups and admin.

Public schedule display widget (read-only API endpoint).

Print/export schedules and CSV export for judges.

Real-time UI updates via WebSockets/SignalR for admin dashboard.

Waitlist functionality & auto-assignment when slot opens.

Multi-slot group reservations (attach multiple participants to one reservation).

Conflict detection across events (avoid double-booking a participant).

Acceptance criteria / success metrics

Competitor can search & reserve an open slot in under 3 clicks.

Concurrent reservations never cause overbooking (tested under load).

Admin can create events & slots and move/cancel reservations.

Reservation audit trail available for troubleshooting.

System recovers from DB failover without data loss.

Testing plan (high-level)

Unit tests for reservation logic, especially concurrency (simulate concurrent reserve attempts).

Integration tests with DB using transactions and rollback.

Load test with concurrent users reserving the same slot to validate locking/retry strategy.

E2E flows for group reservation, check-in, admin override.

## Minimal MVP implementation plan (4–6 sprints)

Sprint 1: Schema + migrations; participants + events + slots + simple API; admin CRUD via CLI or basic endpoints.

Sprint 2: Reservation endpoint w/ row-locking or optimistic concurrency; participant search; unit tests.

Sprint 3: Admin UI (basic) to create time blocks and generate slots; audit logs; cancellation.

Sprint 4: Notifications (email), check-in endpoint, export CSV.

Sprint 5: Hardening: auth for admin, rate limits, monitoring + load testing.

Sprint 6: Optional features: waitlist, group reservations, public schedule UI.

## Example C# notes / ORM choices

Use Entity Framework Core with Postgres provider for rapid development.

If using Lambdas, consider Dapper for low overhead or EF Core with RDS Proxy to manage DB connections.

Implement repository pattern for reservation logic and wrap the reserve operation in an explicit transaction scope.

## Quick ER summary (text)

events 1—* event_time_blocks 1—* event_slots — slot_reservations *—1 participants. events — locations via event_locations. event_rules attach to events. audit_logs record changes to any table.