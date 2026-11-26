using System.Security.Cryptography;
using System.Text;
using LaGranjaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LaGranjaApi.Data;

/// <summary>
/// Contexto Entity Framework Core SQLite para almacenamiento local resiliente de Brostería La Granja.
/// </summary>
public class LaGranjaDbContext : DbContext
{
    public LaGranjaDbContext(DbContextOptions<LaGranjaDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<Resena> Resenas => Set<Resena>();
    public DbSet<ReservaMesa> ReservasMesas => Set<ReservaMesa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Codigo).IsUnique();
        });

        modelBuilder.Entity<Promocion>(entity =>
        {
            entity.HasKey(p => p.Id);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.NroBoleta).IsUnique();
        });

        modelBuilder.Entity<Resena>(entity =>
        {
            entity.HasKey(r => r.Id);
        });

        modelBuilder.Entity<ReservaMesa>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Codigo).IsUnique();
        });
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password + "_lagranja_huancayo_salt");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        return HashPassword(password) == storedHash;
    }

    public void SeedDataIfEmpty()
    {
        // 1. Usuarios iniciales (Admin y Cliente demo)
        if (!Usuarios.Any())
        {
            Usuarios.AddRange(
                new Usuario
                {
                    Email = "admin@lagranja.pe",
                    PasswordHash = HashPassword("admin123"),
                    Nombre = "Administrador La Granja",
                    Telefono = "999999999",
                    Direccion = "Jr. Real 742, Huancayo",
                    Rol = "admin",
                    Activo = true
                },
                new Usuario
                {
                    Email = "cliente@lagranja.pe",
                    PasswordHash = HashPassword("cliente123"),
                    Nombre = "Carlos Rojas",
                    Telefono = "987654321",
                    Direccion = "Av. Giráldez 450, Huancayo",
                    Rol = "cliente",
                    Activo = true
                }
            );
            SaveChanges();
        }

        // 2. Platos iniciales de la carta (14 platos across 5 categorías)
        if (!Productos.Any())
        {
            foreach (var p in LaGranjaData.Productos)
            {
                Productos.Add(new Producto
                {
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    CategoriaId = p.CategoriaId,
                    Categoria = p.Categoria,
                    Porcion = p.Porcion,
                    Precio = p.Precio,
                    Descuento = p.Descuento,
                    Stock = p.Stock,
                    Imagen = p.Imagen,
                    Descripcion = p.Descripcion,
                    Disponible = p.Disponible,
                    Fecha = DateTime.UtcNow
                });
            }
            SaveChanges();
        }

        // 3. Promociones y cupones iniciales
        if (!Promociones.Any())
        {
            Promociones.AddRange(
                new Promocion
                {
                    Titulo = "Martes y Jueves Crujientes — 10% OFF",
                    Detalle = "Aplica el cupón GRANJA10 en tu carrito y obtén 10% de descuento en toda tu orden web.",
                    CodigoCupon = "GRANJA10",
                    PorcentajeDescuento = 10,
                    Vigencia = "Válido todo el mes en web",
                    Activa = true,
                    Fecha = DateTime.UtcNow
                },
                new Promocion
                {
                    Titulo = "Festival Familiar La Granja — 20% OFF",
                    Detalle = "Disfruta 20% de descuento usando el cupón FAMILIA20 en pedidos superiores a S/ 50.00.",
                    CodigoCupon = "FAMILIA20",
                    PorcentajeDescuento = 20,
                    Vigencia = "Viernes a Domingo",
                    Activa = true,
                    Fecha = DateTime.UtcNow
                }
            );
            SaveChanges();
        }

        // 4. Pedidos iniciales para historial y bandeja administrativa
        if (!Pedidos.Any())
        {
            Pedidos.AddRange(
                new Pedido
                {
                    NroBoleta = "B001-104521",
                    UserId = "2",
                    ClienteNombre = "Carlos Rojas",
                    ClienteEmail = "cliente@lagranja.pe",
                    Dni = "71245890",
                    Telefono = "987654321",
                    TipoEntrega = "Delivery",
                    Direccion = "Av. Giráldez 450, Huancayo",
                    MetodoPago = "Yape / Plin",
                    CuponAplicado = "GRANJA10",
                    Descuento = 4.00m,
                    Subtotal = 30.51m,
                    Igv = 5.49m,
                    Total = 36.00m,
                    Estado = "Entregado",
                    ItemsJson = "[{\"productoId\":1,\"nombre\":\"1/4 Pollo Broaster Clásico\",\"precio\":18.00,\"cantidad\":1},{\"productoId\":3,\"nombre\":\"Mostrito Broaster Especial\",\"precio\":22.00,\"cantidad\":1}]",
                    Fecha = DateTime.UtcNow.AddDays(-1)
                },
                new Pedido
                {
                    NroBoleta = "B001-104522",
                    UserId = "2",
                    ClienteNombre = "Lucía Mendoza",
                    ClienteEmail = "lucia.mendoza@correo.com",
                    Dni = "45892104",
                    Telefono = "945123980",
                    TipoEntrega = "Recojo en tienda",
                    Direccion = "Jr. Real 742, Huancayo",
                    MetodoPago = "Tarjeta",
                    CuponAplicado = "",
                    Descuento = 0m,
                    Subtotal = 61.02m,
                    Igv = 10.98m,
                    Total = 72.00m,
                    Estado = "En Preparación",
                    ItemsJson = "[{\"productoId\":10,\"nombre\":\"Combo Familiar Granja (8 Presas)\",\"precio\":72.00,\"cantidad\":1}]",
                    Fecha = DateTime.UtcNow.AddHours(-2)
                }
            );
            SaveChanges();
        }

        // 5. Reseñas iniciales
        if (!Resenas.Any())
        {
            Resenas.AddRange(
                new Resena
                {
                    Nombre = "Carlos Rojas",
                    Estrellas = 5,
                    Comentario = "El broaster más crocante de Huancayo. Llegó súper caliente por delivery y el ají de la casa es buenísimo.",
                    Fecha = DateTime.UtcNow.AddDays(-2)
                },
                new Resena
                {
                    Nombre = "Mariela Quispe",
                    Estrellas = 5,
                    Comentario = "Pedimos el Combo Familiar para el cumpleaños de mi hermano y alcanzó perfecto. Muy buena atención.",
                    Fecha = DateTime.UtcNow.AddDays(-1)
                },
                new Resena
                {
                    Nombre = "Diego Huamán",
                    Estrellas = 4,
                    Comentario = "Las alitas acevichadas están espectaculares y la chicha morada es natural. Recomendado.",
                    Fecha = DateTime.UtcNow.AddHours(-8)
                }
            );
            SaveChanges();
        }

        // 6. Reservas de mesa iniciales
        if (!ReservasMesas.Any())
        {
            ReservasMesas.AddRange(
                new ReservaMesa
                {
                    Codigo = "RES-20261126-101",
                    NombreCliente = "Carlos Rojas",
                    Telefono = "987654321",
                    Email = "cliente@lagranja.pe",
                    Fecha = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                    Hora = "19:30",
                    Personas = 4,
                    Zona = "Zona Familiar",
                    Notas = "Mesa cerca a ventana por favor",
                    Estado = "Confirmada",
                    CreadoEnUtc = DateTime.UtcNow
                }
            );
            SaveChanges();
        }
    }
}
