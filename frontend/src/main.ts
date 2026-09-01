import L from "leaflet";

const map = L.map("map").setView([43.4675, -79.6877], 11);

L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
  attribution: "© OpenStreetMap contributors",
}).addTo(map);