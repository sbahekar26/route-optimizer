import L from "leaflet";

const map = L.map("map").setView([43.4675, -79.6877], 11);

L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
  attribution: "© OpenStreetMap contributors",
}).addTo(map);

interface Stop {
  id: string;
  address: string;
  latitude: number;
  longitude: number;
  status: number;
}

const response = await fetch("http://localhost:5276/stops");
const stops: Stop[] = await response.json();

for (const stop of stops) {
  const marker = L.marker([stop.latitude, stop.longitude], { draggable: true })
    .addTo(map)
    .bindPopup(stop.address);

  marker.on("dragend", async () => {
    const pos = marker.getLatLng();

    // persist the new location
    await fetch(`http://localhost:5276/stops/${stop.id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        address: stop.address,
        latitude: pos.lat,
        longitude: pos.lng,
      }),
    });

    // update our local copy so the polyline uses the new position
    stop.latitude = pos.lat;
    stop.longitude = pos.lng;

    // re-optimize with the new layout
    await optimizeRoute();
  });
}

let routeLine: L.Polyline | null = null;

async function optimizeRoute() {
  const postResponse = await fetch("http://localhost:5276/optimize", {
    method: "POST",
  });
  const { jobId } = await postResponse.json();
  console.log("Job queued:", jobId);

  const result = await pollForResult(jobId);
  console.log("Route received:", result.route);

  drawRoute(result.route);
}

async function pollForResult(jobId: string): Promise<{ route: number[]; totalCost: number }> {
  while (true) {
    const response = await fetch(`http://localhost:5276/optimize/${jobId}`);

    if (response.ok) {
      return await response.json();
    }

    await new Promise((resolve) => setTimeout(resolve, 1000));
  }
}

function drawRoute(route: number[]) {
  const latlngs: [number, number][] = route.map((index) => [
    stops[index].latitude,
    stops[index].longitude,
  ]);

  if (routeLine) {
    routeLine.remove();
  }

  routeLine = L.polyline(latlngs, { color: "blue", weight: 4 }).addTo(map);
  map.fitBounds(routeLine.getBounds());
}

const OptimizeControl = L.Control.extend({
  options: { position: "topright" },
  onAdd: function () {
    const button = L.DomUtil.create("button");
    button.textContent = "Optimize route";
    button.style.padding = "8px 12px";
    button.style.cursor = "pointer";
    button.style.background = "white";
    button.style.border = "2px solid rgba(0,0,0,0.2)";
    button.style.borderRadius = "4px";
    button.style.fontWeight = "500";

    L.DomEvent.on(button, "click", async (e) => {
      L.DomEvent.stopPropagation(e);
      L.DomEvent.preventDefault(e);
      await optimizeRoute();
    });

    return button;
  },
});

map.addControl(new OptimizeControl());