To generate devcerts for docker compose : mkcert -key-file carsties.local.key -cert-file carsties.local.crt app.carsties.local api.carsties.local id.carsties.local

To generate devcerts for local kubernetes deployment : mkcert -key-file server.key -cert-file server.crt app.carsties.local api.carsties.local id.carsties.local
