# Route Optimizer — Frontend

A Vite + TypeScript single-page app with a Leaflet map. It's the interactive
layer on top of the Route Optimizer backend.

## What it does

- Loads delivery stops from the Stops API and renders them as pins on a map of
  the Greater Toronto Area.
- Pins are draggable — dropping one persists its new location (`PUT /stops/{id}`)
  and re-runs optimization.
- An **Optimize route** control triggers the backend pipeline, polls for the
  result, and draws the optimized route — following actual roads, using geometry
  returned by the backend from OSRM.

## Running

Requires the backend running (Stops API on port 5276, plus the Solver worker,
RabbitMQ, Postgres, and OSRM — see the root README).

```bash
npm install
npm run dev
```

The app runs at http://localhost:5173.

## Stack

TypeScript · Vite · Leaflet · the browser Fetch API.
