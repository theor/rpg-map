import './style.css'
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

L.Icon.Default.imagePath = 'img/icon/';

const F = 2;
const map = L.map('map', {
    // center: [20,20],
    crs: L.CRS.Simple,
    // minZoom: -5,
    // layers: [m_mono],
}).setView([-64.0, 128.0], 2)
// .setMaxBounds(L.latLngBounds(L.latLng(-164*F, -128*F), L.latLng(164*F, 128*F)));;
L.tileLayer('{z}/{x}.{y}.webp', {
    maxNativeZoom:7,
    minZoom: 1,
    maxZoom: 8,
    noWrap: true,

}).addTo(map);
console.log(map.getBounds())

L.control.scale({
    imperial: false,
    maxWidth: 300
}).addTo(map);
