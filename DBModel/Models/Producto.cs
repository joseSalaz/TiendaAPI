using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("productos")]
[Index("CategoriaId", Name = "idx_productos_categoria")]
[Index("CodigoBarras", Name = "productos_codigo_barras_key", IsUnique = true)]
public partial class Producto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("codigo_barras")]
    [StringLength(50)]
    public string? CodigoBarras { get; set; }

    [Column("codigo_interno")]
    [StringLength(30)]
    public string? CodigoInterno { get; set; }

    [Column("nombre")]
    [StringLength(200)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("unidad_medida")]
    [StringLength(20)]
    public string UnidadMedida { get; set; } = null!;

    [Column("precio_compra")]
    [Precision(12, 2)]
    public decimal? PrecioCompra { get; set; }

    [Column("precio_venta")]
    [Precision(12, 2)]
    public decimal PrecioVenta { get; set; }

    [Column("precio_mayoreo")]
    [Precision(12, 2)]
    public decimal? PrecioMayoreo { get; set; }

    [Column("permite_stock_negativo")]
    public bool? PermiteStockNegativo { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("imagen_url")]
    [StringLength(500)]
    public string? ImagenUrl { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("Productos")]
    public virtual Categoria? Categoria { get; set; }

    [InverseProperty("Producto")]
    public virtual ICollection<CompraDetalle> CompraDetalles { get; set; } = new List<CompraDetalle>();

    [InverseProperty("Producto")]
    public virtual ICollection<ProductoStock> ProductoStocks { get; set; } = new List<ProductoStock>();

    [InverseProperty("Producto")]
    public virtual ICollection<StockMovimiento> StockMovimientos { get; set; } = new List<StockMovimiento>();

    [InverseProperty("Producto")]
    public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
}
