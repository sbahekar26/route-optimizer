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
  L.marker([stop.latitude, stop.longitude])
    .addTo(map)
    .bindPopup(stop.address);
}