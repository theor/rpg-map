import './style.css'
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

L.Icon.Default.imagePath = 'img/icon/';

const bounds = L.latLngBounds(L.latLng(-2*64, 0), L.latLng(0, 256));
const map = L.map('map', {
    // center: [20,20],
    crs: L.CRS.Simple,
    // minZoom: -5,
    // layers: [m_mono],
})
// .setMaxBounds()
.setView([-64.0, 128.0], 3);;
L.tileLayer('{z}/{x}.{y}.webp', {
    errorTileUrl: "error.webp",
    maxNativeZoom:7,
    minZoom: 1,
    maxZoom: 8,
    noWrap: true,
    bounds: bounds,

}).addTo(map);
console.log(map.getBounds())

L.control.scale({
    imperial: false,
    maxWidth: 300
}).addTo(map);
