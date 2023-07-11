import './style.css'
import 'leaflet/dist/leaflet.css';
import L, { Map } from 'leaflet';
import * as Ably from 'ably';
L.Icon.Default.imagePath = '/rpg-map/';

const bounds = L.latLngBounds(L.latLng(-2 * 64, 0), L.latLng(0, 256));
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
    maxNativeZoom: 7,
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


let Position = L.Control.extend({
    _container: null,
    _latlng: null as HTMLElement | null,
    latlng: new L.LatLng(50, 50),
    options: {
        position: 'bottomleft'
    },

    onAdd: function (_map: Map) {
        var latlng = L.DomUtil.create('div', 'mouseposition');
        this._latlng = latlng;
        return latlng;
    },

    updateHTML: function (lat: number, lng: number) {
        var latlng = lat + ", " + lng;
        this.latlng = new L.LatLng(lat, lng);
        //this._latlng.innerHTML = "Latitude: " + lat + "   Longitiude: " + lng;
        this._latlng!.innerHTML = latlng;
    }
});


let mouseMarker = L.marker([-59, 156.11]).addTo(map);
mouseMarker.on("contextmenu", _ => {
    mouseMarker.remove()
});

let position = new Position();
map.addControl(position);
map.addEventListener('mousemove', (event) => {
    let lat = Math.round(event.latlng.lat * 100000) / 100000;
    let lng = Math.round(event.latlng.lng * 100000) / 100000;
    position.updateHTML(lat, lng);
}
);
const urlParams = new URLSearchParams(window.location.search);
const key = urlParams.get('key');
console.log(key)

const ably = new Ably.Realtime.Promise('0gg2PA.DEHU8g:' + key);
await ably.connection.once('connected');
console.log('Connected to Ably!');


const channel = ably.channels.get('getting-started');

interface PosMsg {
    latlng: L.LatLngLiteral,
    zoom: number,
}
await channel.subscribe("pos", m => {
    console.log(m);
    const data = m.data as PosMsg;
    mouseMarker.addTo(map);
    mouseMarker.setLatLng(data.latlng);
    map.flyTo(data.latlng, data.zoom);
});


map.addEventListener("click", async _ => {
    console.log(position._latlng?.innerText);
    let m: PosMsg = { latlng: position.latlng, zoom: map.getZoom() };
    await channel.publish("pos", m);
    // mouseMarker.setLatLng(position.latlng);
});