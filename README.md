# API REST — Brostería La Granja (.NET 8 + SQLite + Firebase Firestore)

Backend construido en **ASP.NET Core 8** siguiendo una **arquitectura híbrida dual resiliente**:
1. **SQLite Local (`LaGranjaLocal.db`)** con Entity Framework Core y sembrado automático de datos (`SeedDataIfEmpty`).
2. **Firebase Cloud Firestore** (`Google.Cloud.Firestore`) activado automáticamente cuando se detectan credenciales Admin SDK (sin fallar si se ejecuta sin clave en entorno local).
3. **Docker + Render Ready** escuchando dinámicamente en `PORT` (por defecto `10000`).

---

## Endpoints Principales

| Recurso | Rutas | Descripción |
| :--- | :--- | :--- |
| Salud | `GET /`, `GET /health` | Estado de la API, SQLite y Firebase |
| Auth | `POST /api/auth/login`, `POST /api/auth/register`, `GET /api/auth/usuarios` | Autenticación y registro de clientes/admin |
| Categorías | `GET /api/categorias`, `GET /api/categorias/{id}` | Broaster, Alitas, Salchipapas, Combos, Bebidas |
| Platos | `GET/POST/PUT/DELETE /api/productos` | CRUD completo con filtros `?categoria=&q=` |
| Promociones | `GET/POST/PUT/DELETE /api/promociones`, `GET /api/promociones/validar/{codigo}` | Promos activas y validación de cupones |
| Pedidos | `GET/POST/DELETE /api/pedidos`, `PUT /api/pedidos/{nroBoleta}/estado` | Emisión de boletas, stock y seguimiento |
| Reseñas | `GET/POST/DELETE /api/resenas` | Muro de opiniones y moderación |
| Reservas | `GET/POST/DELETE /api/reservas`, `PUT /api/reservas/{codigo}/estado` | Reserva de mesas en local |

### Cuentas Demo Sembradas
- **Admin:** `admin@lagranja.pe` / `admin123`
- **Cliente:** `cliente@lagranja.pe` / `cliente123`

---

## Ejecución Local

```bash
dotnet restore
ENABLE_SWAGGER=true dotnet run
```

La API quedará disponible en `http://localhost:10000` y Swagger UI en `http://localhost:10000/swagger`.

---

## Despliegue en GitHub y Render (Paso a Paso)

1. Sube la carpeta `Backend/` a un repositorio de GitHub (el archivo `.gitignore` ya protege `LaGranjaLocal.db` y `firebase-key.json`).
2. En **Render.com**, crea un **New Web Service** conectado a ese repositorio:
   - **Runtime:** `Docker`
   - **Dockerfile Path:** `./Dockerfile` (o usa `render.yaml`)
   - **Health Check Path:** `/health`
3. Agrega las variables de entorno en **Environment**:
   - `PORT` = `10000`
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ENABLE_SWAGGER` = `true`
   - `FIREBASE_PROJECT_ID` = `la-granja-4f66a`
   - `FIREBASE_SERVICE_ACCOUNT_JSON` = *(opcional: pega aquí el JSON de cuenta de servicio de Firebase para sincronizar en la nube)*
4. Verifica el despliegue entrando a:
   - `https://tu-servicio.onrender.com/health`
   - `https://tu-servicio.onrender.com/swagger`
5. Copia la URL asignada por Render en `Frontend/src/js/config.js` (o en la variable `VITE_API_URL` al compilar el frontend).
