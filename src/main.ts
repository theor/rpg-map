import './style.css'
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

L.Icon.Default.imagePath = 'img/icon/';

const map = L.map('map', {
    center: [20,20],
    crs: L.CRS.Simple,
    // minZoom: -5,
    // layers: [m_mono],
}).setView([0.0, 0.0], 4)
//.setMaxBounds(L.latLngBounds(L.latLng(0, 0), L.latLng(1000, 100)));;
L.tileLayer('{z}/{x}{y}.png', {
    maxNativeZoom:4,
    minZoom: 1,
    maxZoom: 8,
    noWrap: true,

}).addTo(map);


L.control.scale({
    imperial: false,
    maxWidth: 300
}).addTo(map);
