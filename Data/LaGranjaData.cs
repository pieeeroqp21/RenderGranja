using LaGranjaApi.Models;

namespace LaGranjaApi.Data;

/// <summary>
/// Catálogo semilla inicial de Brostería La Granja para inicialización SQLite y respaldo en memoria.
/// </summary>
public static class LaGranjaData
{
    public static List<Categoria> Categorias { get; } = new()
    {
        new Categoria
        {
            Id = 1,
            Nombre = "Broaster",
            Descripcion = "Presas doradas y extra crujientes con papas nativas y cremas de la casa.",
            Icono = "Drumstick",
            Orden = 1
        },
        new Categoria
        {
            Id = 2,
            Nombre = "Alitas",
            Descripcion = "Alitas crocantes bañadas en salsa BBQ, Honey Mustard, Acevichada o Picante.",
            Icono = "Flame",
            Orden = 2
        },
        new Categoria
        {
            Id = 3,
            Nombre = "Salchipapas",
            Descripcion = "Papas fritas artesanales con salchicha frankfurter, chorizo y pollo deshilachado.",
            Icono = "Utensils",
            Orden = 3
        },
        new Categoria
        {
            Id = 4,
            Nombre = "Combos",
            Descripcion = "Promociones familiares y para compartir con gaseosa de 1.5L incluida.",
            Icono = "Package",
            Orden = 4
        },
        new Categoria
        {
            Id = 5,
            Nombre = "Bebidas",
            Descripcion = "Chicha morada artesanal, limonada frozen y gaseosas heladas.",
            Icono = "CupSoda",
            Orden = 5
        }
    };

    public static List<Producto> Productos { get; } = new()
    {
        new Producto
        {
            Id = 1,
            Codigo = "GRJ-001",
            Nombre = "1/4 Pollo Broaster Clásico",
            CategoriaId = 1,
            Categoria = "Broaster",
            Porcion = "1/4 Pollo (Pecho o Pierna) + Papas + Ensalada",
            Precio = 18.00m,
            Descuento = 0,
            Stock = 45,
            Imagen = "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Presa jugosa marinada 12 horas con especias de la casa, doble empanizado extra crujiente, papas nativas y cremas tradicionales.",
            Disponible = true
        },
        new Producto
        {
            Id = 2,
            Codigo = "GRJ-002",
            Nombre = "1/2 Pollo Broaster La Granja",
            CategoriaId = 1,
            Categoria = "Broaster",
            Porcion = "2 Presas + Porción Doble de Papas + Ensalada",
            Precio = 33.00m,
            Descuento = 10,
            Stock = 30,
            Imagen = "https://images.unsplash.com/photo-1562967914-608f82629710?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Dos generosas presas de pollo broaster dorado al punto exacto, acompañado de abundantes papas fritas y ensalada fresca.",
            Disponible = true
        },
        new Producto
        {
            Id = 3,
            Codigo = "GRJ-003",
            Nombre = "Mostrito Broaster Especial",
            CategoriaId = 1,
            Categoria = "Broaster",
            Porcion = "1/4 Pollo Broaster + Chaufa + Papas",
            Precio = 22.50m,
            Descuento = 5,
            Stock = 35,
            Imagen = "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Fusión favorita de Huancayo: arroz chaufa ahumado al wok, papas crocantes y 1/4 de pollo broaster recién salido.",
            Disponible = true
        },
        new Producto
        {
            Id = 4,
            Codigo = "GRJ-004",
            Nombre = "Alitas BBQ x 6 Unidades",
            CategoriaId = 2,
            Categoria = "Alitas",
            Porcion = "6 Alitas + Papas Fritas + Salsa Ranch",
            Precio = 21.00m,
            Descuento = 0,
            Stock = 40,
            Imagen = "https://images.unsplash.com/photo-1567620832903-9fc6debc209f?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Seis alitas crocantes glaseadas en salsa barbacoa ahumada con miel, servidas con papas fritas doradas.",
            Disponible = true
        },
        new Producto
        {
            Id = 5,
            Codigo = "GRJ-005",
            Nombre = "Alitas Acevichadas & Maracuyá x 8",
            CategoriaId = 2,
            Categoria = "Alitas",
            Porcion = "8 Alitas Mixtas + Papas Nativas",
            Precio = 27.00m,
            Descuento = 15,
            Stock = 28,
            Imagen = "https://images.unsplash.com/photo-1608039755401-742074f0548d?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Mitad alitas en salsa acevichada cremosa con chicharrón de ajonjolí y mitad en reducción dulce de maracuyá.",
            Disponible = true
        },
        new Producto
        {
            Id = 6,
            Codigo = "GRJ-006",
            Nombre = "Alitas Picantes Búfalo x 10",
            CategoriaId = 2,
            Categoria = "Alitas",
            Porcion = "10 Alitas Búfalo + Bastones + Dip Azul",
            Precio = 32.00m,
            Descuento = 0,
            Stock = 22,
            Imagen = "https://images.unsplash.com/photo-1527477396000-e27163b481c2?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Para amantes del picante: alitas bañadas en auténtica salsa búfalo artesanal con dip suave de queso.",
            Disponible = true
        },
        new Producto
        {
            Id = 7,
            Codigo = "GRJ-007",
            Nombre = "Salchipapa Clásica La Granja",
            CategoriaId = 3,
            Categoria = "Salchipapas",
            Porcion = "Porción Personal Generosa + Cremas",
            Precio = 14.00m,
            Descuento = 0,
            Stock = 50,
            Imagen = "https://images.unsplash.com/photo-1585109649139-366815a0d713?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Papas nativas amarillas crocantes con salchicha frankfurter dorada, ají de la casa, tártara y golf.",
            Disponible = true
        },
        new Producto
        {
            Id = 8,
            Codigo = "GRJ-008",
            Nombre = "Salchibroster Brutal",
            CategoriaId = 3,
            Categoria = "Salchipapas",
            Porcion = "Salchipapa + Trozos Broaster + Huevo + Queso",
            Precio = 24.00m,
            Descuento = 10,
            Stock = 30,
            Imagen = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=80",
            Descripcion = "La reina de la carta: salchipapa completa coronada con trozos crujientes de pollo broaster, huevo frito y queso fundido.",
            Disponible = true
        },
        new Producto
        {
            Id = 9,
            Codigo = "GRJ-009",
            Nombre = "Combo Dúo Crujiente",
            CategoriaId = 4,
            Categoria = "Combos",
            Porcion = "2 Cuartos Broaster + 2 Gaseosas Personales",
            Precio = 38.00m,
            Descuento = 10,
            Stock = 25,
            Imagen = "https://images.unsplash.com/photo-1513639776629-7b61b0ac49cb?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Ideal para compartir en pareja o entre amigos: dos 1/4 de pollo broaster completos más dos bebidas personales heladas.",
            Disponible = true
        },
        new Producto
        {
            Id = 10,
            Codigo = "GRJ-010",
            Nombre = "Combo Familiar Granja (8 Presas)",
            CategoriaId = 4,
            Categoria = "Combos",
            Porcion = "8 Presas Broaster + Papas Familiares + Gaseosa 1.5L",
            Precio = 72.00m,
            Descuento = 15,
            Stock = 18,
            Imagen = "https://images.unsplash.com/photo-1569058242253-92a9c755a0ec?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Banquete familiar completo con 8 presas crujientes, mega fuente de papas fritas, ensalada familiar, cremas y gaseosa 1.5L.",
            Disponible = true
        },
        new Producto
        {
            Id = 11,
            Codigo = "GRJ-011",
            Nombre = "Combo Alitas + Salchipapa Party",
            CategoriaId = 4,
            Categoria = "Combos",
            Porcion = "12 Alitas Mixtas + Salchipapa Grande + Gaseosa 1.5L",
            Precio = 58.00m,
            Descuento = 10,
            Stock = 20,
            Imagen = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Perfecto para reuniones: 12 alitas en dos salsas a elección más salchipapa grande y gaseosa de 1.5 litros.",
            Disponible = true
        },
        new Producto
        {
            Id = 12,
            Codigo = "GRJ-012",
            Nombre = "Chicha Morada Artesanal 1 Litro",
            CategoriaId = 5,
            Categoria = "Bebidas",
            Porcion = "Jarra / Botella 1 Litro",
            Precio = 12.00m,
            Descuento = 0,
            Stock = 40,
            Imagen = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Preparada diariamente con maíz morado seleccionado, piña, manzana, canela, clavo y toque fresco de limón.",
            Disponible = true
        },
        new Producto
        {
            Id = 13,
            Codigo = "GRJ-013",
            Nombre = "Limonada Frozen Hierbabuena",
            CategoriaId = 5,
            Categoria = "Bebidas",
            Porcion = "Vaso Grande 500 ml",
            Precio = 8.50m,
            Descuento = 0,
            Stock = 55,
            Imagen = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Refrescante limonada frozen granizada con hojas frescas de hierbabuena, ideal para acompañar tu broaster.",
            Disponible = true
        },
        new Producto
        {
            Id = 14,
            Codigo = "GRJ-014",
            Nombre = "Inca Kola / Coca Cola 1.5 Litros",
            CategoriaId = 5,
            Categoria = "Bebidas",
            Porcion = "Botella 1.5L Helada",
            Precio = 11.00m,
            Descuento = 0,
            Stock = 60,
            Imagen = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?auto=format&fit=crop&w=800&q=80",
            Descripcion = "Bebida gaseosa bien helada para acompañar combos familiares y pedidos por delivery.",
            Disponible = true
        }
    };

    public static int ResolveCategoriaId(string categoria) => categoria.Trim().ToLower() switch
    {
        "broaster" => 1,
        "alitas" => 2,
        "salchipapas" => 3,
        "combos" => 4,
        "bebidas" => 5,
        _ => 1
    };
}
