using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using LaGranjaApi.Models;

namespace LaGranjaApi.Services;

/// <summary>
/// Servicio de sincronización en nube con Firebase Cloud Firestore (Modo Híbrido Resiliente).
/// Si no existen credenciales Admin SDK en entorno local, la API continúa operando sobre SQLite sin fallos.
/// </summary>
public class FirebaseService
{
    private readonly FirestoreDb? _firestoreDb;
    private readonly ILogger<FirebaseService> _logger;
    public bool IsAvailable => _firestoreDb != null;
    public string ProjectId { get; }

    public FirebaseService(IConfiguration configuration, ILogger<FirebaseService> logger)
    {
        _logger = logger;
        ProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")
            ?? configuration["Firebase:ProjectId"]
            ?? "la-granja-4f66a";

        try
        {
            // 1. Variable de entorno directa (Render: FIREBASE_SERVICE_ACCOUNT_JSON)
            var serviceAccountJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
            if (!string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                var credential = GoogleCredential.FromJson(serviceAccountJson);
                _firestoreDb = new FirestoreDbBuilder
                {
                    ProjectId = ProjectId,
                    Credential = credential
                }.Build();
                _logger.LogInformation("[Firebase] Conectado exitosamente a Cloud Firestore (Proyecto: {ProjectId}).", ProjectId);
                return;
            }

            // 2. Ruta de credenciales por variable GOOGLE_APPLICATION_CREDENTIALS o detección automática
            string? credentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            if (string.IsNullOrWhiteSpace(credentialPath) || !File.Exists(credentialPath))
            {
                var candidateLocations = new List<string>
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "firebase-key.json"),
                    Path.Combine(AppContext.BaseDirectory, "firebase-key.json")
                };

                try
                {
                    var currentDir = Directory.GetCurrentDirectory();
                    candidateLocations.AddRange(Directory.GetFiles(currentDir, "*firebase*.json"));
                    candidateLocations.AddRange(Directory.GetFiles(currentDir, "*la-granja*.json"));

                    var parentDir = Directory.GetParent(currentDir)?.FullName;
                    if (!string.IsNullOrWhiteSpace(parentDir) && Directory.Exists(parentDir))
                    {
                        candidateLocations.AddRange(Directory.GetFiles(parentDir, "*firebase*.json"));
                        candidateLocations.AddRange(Directory.GetFiles(parentDir, "*la-granja*.json"));
                    }
                }
                catch
                {
                    // Ignorar errores de escaneo de carpetas en entornos restringidos
                }

                credentialPath = candidateLocations.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f) && File.Exists(f));
            }

            if (!string.IsNullOrWhiteSpace(credentialPath) && File.Exists(credentialPath))
            {
                var credential = GoogleCredential.FromFile(credentialPath);
                _firestoreDb = new FirestoreDbBuilder
                {
                    ProjectId = ProjectId,
                    Credential = credential
                }.Build();
                _logger.LogInformation("[Firebase] Conectado exitosamente a Cloud Firestore vía: {Path}", credentialPath);
                return;
            }

            _logger.LogWarning("[Firebase] Modo dual activo: Operando con SQLite local (LaGranjaLocal.db). Firestore se activará al configurar FIREBASE_SERVICE_ACCOUNT_JSON o firebase-key.json.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Firebase] No se pudo inicializar Firestore: {Message}. Operando en modo local SQLite.", ex.Message);
            _firestoreDb = null;
        }
    }

    // ==========================================
    // PRODUCTOS / CARTA (colección: productos)
    // ==========================================
    public async Task GuardarProductoEnNubeAsync(Producto prod, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            var docRef = _firestoreDb.Collection("productos").Document(prod.Codigo);
            var datos = new Dictionary<string, object>
            {
                { "id", prod.Id },
                { "codigo", prod.Codigo },
                { "nombre", prod.Nombre },
                { "categoria", prod.Categoria },
                { "porcion", prod.Porcion },
                { "precio", (double)prod.Precio },
                { "descuento", prod.Descuento },
                { "stock", prod.Stock },
                { "imagen", prod.Imagen },
                { "descripcion", prod.Descripcion },
                { "disponible", prod.Disponible },
                { "fecha", prod.Fecha.ToString("o") }
            };
            await docRef.SetAsync(datos, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al sincronizar producto {Codigo}: {Message}", prod.Codigo, ex.Message);
        }
    }

    public async Task EliminarProductoEnNubeAsync(string codigo, CancellationToken ct = default)
    {
        if (_firestoreDb == null || string.IsNullOrWhiteSpace(codigo)) return;
        try
        {
            await _firestoreDb.Collection("productos").Document(codigo).DeleteAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al eliminar producto {Codigo}: {Message}", codigo, ex.Message);
        }
    }

    // ==========================================
    // PROMOCIONES (colección: promociones)
    // ==========================================
    public async Task GuardarPromocionEnNubeAsync(Promocion promo, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            var docRef = _firestoreDb.Collection("promociones").Document($"promo-{promo.Id}");
            var datos = new Dictionary<string, object>
            {
                { "id", promo.Id },
                { "titulo", promo.Titulo },
                { "detalle", promo.Detalle },
                { "codigoCupon", promo.CodigoCupon },
                { "porcentajeDescuento", promo.PorcentajeDescuento },
                { "vigencia", promo.Vigencia },
                { "activa", promo.Activa },
                { "fecha", promo.Fecha.ToString("o") }
            };
            await docRef.SetAsync(datos, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al sincronizar promoción {Id}: {Message}", promo.Id, ex.Message);
        }
    }

    public async Task EliminarPromocionEnNubeAsync(int id, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            await _firestoreDb.Collection("promociones").Document($"promo-{id}").DeleteAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al eliminar promoción {Id}: {Message}", id, ex.Message);
        }
    }

    // ==========================================
    // PEDIDOS Y BOLETAS (colección: pedidos)
    // ==========================================
    public async Task GuardarPedidoEnNubeAsync(Pedido pedido, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            var docRef = _firestoreDb.Collection("pedidos").Document(pedido.NroBoleta);
            var datos = new Dictionary<string, object>
            {
                { "nroBoleta", pedido.NroBoleta },
                { "userId", pedido.UserId },
                { "clienteNombre", pedido.ClienteNombre },
                { "clienteEmail", pedido.ClienteEmail },
                { "dni", pedido.Dni },
                { "telefono", pedido.Telefono },
                { "tipoEntrega", pedido.TipoEntrega },
                { "direccion", pedido.Direccion },
                { "metodoPago", pedido.MetodoPago },
                { "cuponAplicado", pedido.CuponAplicado },
                { "descuento", (double)pedido.Descuento },
                { "subtotal", (double)pedido.Subtotal },
                { "igv", (double)pedido.Igv },
                { "total", (double)pedido.Total },
                { "estado", pedido.Estado },
                { "itemsJson", pedido.ItemsJson },
                { "fecha", pedido.Fecha.ToString("o") }
            };
            await docRef.SetAsync(datos, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al sincronizar pedido {Boleta}: {Message}", pedido.NroBoleta, ex.Message);
        }
    }

    // ==========================================
    // RESEÑAS (colección: resenas)
    // ==========================================
    public async Task GuardarResenaEnNubeAsync(Resena resena, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            var docRef = _firestoreDb.Collection("resenas").Document($"resena-{resena.Id}");
            var datos = new Dictionary<string, object>
            {
                { "id", resena.Id },
                { "nombre", resena.Nombre },
                { "estrellas", resena.Estrellas },
                { "comentario", resena.Comentario },
                { "fecha", resena.Fecha.ToString("o") }
            };
            await docRef.SetAsync(datos, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al sincronizar reseña {Id}: {Message}", resena.Id, ex.Message);
        }
    }

    public async Task EliminarResenaEnNubeAsync(int id, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            await _firestoreDb.Collection("resenas").Document($"resena-{id}").DeleteAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al eliminar reseña {Id}: {Message}", id, ex.Message);
        }
    }

    // ==========================================
    // RESERVAS DE MESAS (colección: reservas)
    // ==========================================
    public async Task GuardarReservaEnNubeAsync(ReservaMesa reserva, CancellationToken ct = default)
    {
        if (_firestoreDb == null) return;
        try
        {
            var docRef = _firestoreDb.Collection("reservas").Document(reserva.Codigo);
            var datos = new Dictionary<string, object>
            {
                { "codigo", reserva.Codigo },
                { "nombreCliente", reserva.NombreCliente },
                { "telefono", reserva.Telefono },
                { "email", reserva.Email },
                { "fecha", reserva.Fecha },
                { "hora", reserva.Hora },
                { "personas", reserva.Personas },
                { "zona", reserva.Zona },
                { "notas", reserva.Notas },
                { "estado", reserva.Estado },
                { "creadoEnUtc", reserva.CreadoEnUtc.ToString("o") }
            };
            await docRef.SetAsync(datos, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("[Firebase] Error al sincronizar reserva {Codigo}: {Message}", reserva.Codigo, ex.Message);
        }
    }
}
