# Route Optimizer

A distributed delivery-route optimization system built in .NET — the problem
companies like Amazon and DPD solve millions of times a day: given one driver
and a set of delivery stops, find the fastest order to visit them all.

The domain is deliberately small. The point is the **architecture**: independent
services communicating asynchronously through a message broker, computing routes
from *real road drive-times* rather than straight-line distance, with a live map
frontend on top.

> Built in public, one commit at a time. The bugs are in the commit history, not
> hidden.

---

## What this demonstrates

- **Genuinely distributed microservices** — separate services communicating over
  HTTP and a durable message queue; the Stops API owns its database and the
  Solver is a stateless worker. Not a monolith split into folders.
- **Asynchronous job processing** — an API accepts a request and returns
  immediately (`202 Accepted`); a separate worker consumes the job, processes it,
  and publishes the result back for the API to store and serve. The frontend
  polls for the result and renders it when ready.
- **Real-world routing** — self-hosted OSRM computes actual road drive-times from
  an OpenStreetMap extract. Straight-line distance says Burlington→Toronto is
  ~50 km; the road says ~57 km. That gap is Lake Ontario.
- **Constraint solving** — Google OR-Tools solves the Traveling Salesman Problem
  over an asymmetric, real-world cost matrix.
- **A working frontend** — a Vite + TypeScript app with a Leaflet map. Stops
  render as pins; one click triggers the whole distributed pipeline and draws the
  optimized route back on the map.
- **Engineering discipline** — Architecture Decision Records, a full Git workflow,
  tests that verify the solver against hand-calculated answers and mock the HTTP
  boundary, and infrastructure defined as code.

**Stack:** C# / .NET 10 · ASP.NET Core · EF Core / PostgreSQL · RabbitMQ ·
Google OR-Tools · self-hosted OSRM · TypeScript / Vite / Leaflet · Docker Compose

---

## The frontend

The newest layer is a map. Open the app, and the delivery stops appear as pins on
a real map of the Greater Toronto Area. Click **Optimize route** and:

1. The frontend POSTs to the API, which returns a job ID immediately.
2. It polls `GET /optimize/{jobId}` — getting `404 Not Found` until the result is
   ready, then `200 OK` with the route. (That polling loop is the client side of
   the async pattern: submit, then keep asking until the answer exists.)
3. The optimized visiting order is drawn as a polyline across the stops.

The route order is computed from real OSRM drive-times; the line itself is drawn
as straight segments between stops in visiting order. (Tracing the actual roads —
using OSRM's route geometry — is on the roadmap.)

---

## Architecture

A request flows through the system asynchronously:

1. The **frontend** (or any client) POSTs to the **Stops API** to trigger
   optimization.
2. The API reads the stops it owns from **PostgreSQL**, publishes an
   `OptimizationRequested` message to **RabbitMQ**, and returns `202 Accepted`
   with a job ID.
3. The **Solver** service — a long-running worker — consumes the message, calls
   **OSRM** for a real drive-time matrix, solves the route with **OR-Tools**, and
   publishes a `RouteOptimized` result back to RabbitMQ.
4. A background consumer in the Stops API stores the result in PostgreSQL.
5. The client retrieves the finished route via `GET /optimize/{jobId}`.

The two services share **only** a message-contract library — no shared database,
no shared business logic. That boundary is what keeps them independent.

![Architecture diagram](docs/architecture.svg)

---

## Repository structure

```
route-optimizer/
├── frontend/               # Vite + TypeScript + Leaflet map (click-to-optimize)
├── services/
│   ├── stops-api/          # ASP.NET Core API — owns stops, publishes jobs,
│   │                       #   consumes results (background service)
│   └── solver/
│       ├── Solver/         # Worker: consumes jobs, OSRM + OR-Tools, publishes results
│       └── Solver.Tests/   # xUnit tests: solver, distance math, matrix, OSRM client
├── contracts/
│   └── RouteOptimizer.Contracts/   # Shared message contracts (the only coupling)
├── infra/
│   ├── docker-compose.yml  # PostgreSQL + RabbitMQ + OSRM for local development
│   └── osrm/               # OSRM map data (git-ignored binaries)
├── tools/
│   └── TestPublisher/      # Dev tool: publish a test job without the full API
└── docs/
    ├── adr/                # Architecture Decision Records
    └── architecture.svg    # Architecture diagram
```

---

## Design decisions (ADRs)

Key choices are documented as Architecture Decision Records in
[`docs/adr/`](docs/adr). So far:

- **[ADR-0001] Monorepo with Per-Service Folders**
  ([`0001-monorepo-and-services.md`](docs/adr/0001-monorepo-and-services.md)) —
  one repo, physically separate services, shared contracts in their own project.
  Trades independent versioning for atomic cross-service changes and simpler
  local development.

Other deliberate choices worth calling out (not yet written up as ADRs):

- **Self-hosted OSRM for routing** — real road drive-times over straight-line
  distance. Introduces a heavy one-time preprocessing step and ~1 GB of RAM, but
  produces materially different (correct) routes — and the drive-time matrix is
  *asymmetric*, because real roads have one-way streets and different on-ramps.
- **Async messaging over synchronous HTTP** between API and solver — the API
  stays responsive, the solver can be down without dropping work (durable queues
  hold messages), and the system can scale to multiple solver instances pulling
  from one queue.
- **`Guid` identifiers** — unique without coordination across services.
- **Database-per-service** — enforced physically, not just by convention.

---

## Running it locally

**Prerequisites:** .NET 10 SDK, Node.js 20+, Docker Desktop. (Apple Silicon /
ARM64 works natively for .NET and OR-Tools; OSRM runs under emulation.)

### 1. One-time: prepare the OSRM map data

OSRM serves a *preprocessed* OpenStreetMap extract. This step is done once — the
processed files are git-ignored and reused afterward.

```bash
mkdir -p infra/osrm/data && cd infra/osrm/data

# Download a regional extract (Ontario shown; ~900 MB)
curl -L -O https://download.geofabrik.de/north-america/canada/ontario-latest.osm.pbf

# Preprocess (extract → partition → customize)
docker run -t -v "${PWD}:/data" ghcr.io/project-osrm/osrm-backend \
  osrm-extract -p /opt/car.lua /data/ontario-latest.osm.pbf
docker run -t -v "${PWD}:/data" ghcr.io/project-osrm/osrm-backend \
  osrm-partition /data/ontario-latest.osrm
docker run -t -v "${PWD}:/data" ghcr.io/project-osrm/osrm-backend \
  osrm-customize /data/ontario-latest.osrm

cd ../../..
```

### 2. Start the infrastructure

PostgreSQL, RabbitMQ, and OSRM all come up together:

```bash
cd infra
docker compose up -d
docker compose ps          # all three should be "Up"
cd ..
```

RabbitMQ's management UI is at http://localhost:15672 (guest / guest).
OSRM is served on port **5050** (mapped from its internal 5000 to avoid a
conflict with macOS AirPlay Receiver).

### 3. Apply database migrations

```bash
cd services/stops-api
dotnet ef database update
cd ../..
```

### 4. Run the services (each in its own terminal)

```bash
# Solver worker
cd services/solver/Solver && dotnet run

# Stops API
cd services/stops-api && dotnet run

# Frontend (Vite dev server)
cd frontend && npm install && npm run dev
```

The frontend runs at http://localhost:5173.

### 5. Use it

Add a few stops, then open the frontend and click **Optimize route** — or drive
the API directly:

```bash
# Add some stops (repeat for each; use the API's port)
curl -X POST http://localhost:5276/stops \
  -H "Content-Type: application/json" \
  -d '{"address":"Burlington","latitude":43.3255,"longitude":-79.7990}'

# Trigger optimization — returns 202 with a jobId
curl -i -X POST http://localhost:5276/optimize

# Retrieve the optimized route once processing completes
curl http://localhost:5276/optimize/<jobId>
```

---

## Testing

```bash
dotnet test services/solver/Solver.Tests
```

The tests cover the pure logic and the external boundary:

- The **solver** against a hand-calculated optimal route (a 4-stop problem whose
  answer, cost 80, was worked out by hand before any code — so a regression turns
  the test red).
- The **haversine** distance and the **matrix converter** (pure functions).
- The **OSRM client**, using a fake `HttpMessageHandler` to test URL construction
  and JSON parsing without a running OSRM server.

Cross-service messaging (the consumers and endpoints) would be covered by
integration tests with real RabbitMQ/PostgreSQL — noted on the roadmap.

---

## Roadmap

- [ ] Draggable pins on the map that re-optimize on drop
- [ ] Draw the actual road path (OSRM route geometry) instead of straight segments
- [ ] Deployment to a self-hosted Kubernetes cluster (k3s / OKE on ARM) — *future*
- [ ] Integration tests for the message consumers and API endpoints
- [ ] Multi-vehicle routing (VRP), time windows, capacity constraints
