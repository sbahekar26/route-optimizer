# ADR-0001: Monorepo with Per-Service Folders

## Status

Accepted

## Context

This project consists of multiple independently deployable services (a Stops
API, a Solver service, and a self-hosted OSRM routing engine). Early on, we must
decide how to organize source control: a single repository containing all
services (a "monorepo"), or a separate repository per service ("polyrepo").

As a solo developer, working on one machine, in the early stages of a learning-
focused project, the cost of coordinating changes across multiple repositories
is high: a change that touches two services would require two clones, two
branches, two pull requests, and careful ordering to keep them in sync. There is
currently no team, no independent release cadence per service, and no need for
per-service access control that would justify that overhead.

## Decision

We will use a single monorepo. Its layout is:

- `services/` — one subfolder per deployable service
- `infra/` — Docker Compose and, later, Kubernetes manifests
- `docs/adr/` — Architecture Decision Records

Each service keeps its own project files, Dockerfile, and (later) its own
database, preserving service independence at the boundary level even though the
code lives together.

## Consequences

Positive:
- A single change spanning multiple services can be made in one atomic commit
  and one pull request.
- One clone, one place to search, simpler local development.
- Shared conventions (ADRs, CI config) live in one place.

Negative:
- Services cannot be versioned or released fully independently from the repo.
- As CI is added later, all services share one pipeline unless deliberately
  split.
- The physical convenience of shared code can erode logical service boundaries
  if discipline slips; we mitigate this with the strict rule of no shared
  database and no shared code except versioned message contracts.