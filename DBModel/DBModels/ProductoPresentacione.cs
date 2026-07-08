using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("producto_presentaciones")]
[Index("CodigoBarras", Name = "producto_presentaciones_codigo_barras_key", IsUnique = true)]
public partial class ProductoPresentacione
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("codigo_barras")]
    [StringLength(50)]
    public string? CodigoBarras { get; set; }

    [Column("cantidad_unidades")]
    public int CantidadUnidades { get; set; }

    [Column("precio_compra")]
    [Precision(12, 2)]
    public decimal? PrecioCompra { get; set; }

    [Column("precio_venta")]
    [Precision(12, 2)]
    public decimal PrecioVenta { get; set; }

    [Column("precio_mayoreo")]
    [Precision(12, 2)]
    public decimal? PrecioMayoreo { get; set; }

    [Column("es_unidad_base")]
    public bool EsUnidadBase { get; set; }

    [Column("permite_venta")]
    public bool PermiteVenta { get; set; }

    [Column("permite_compra")]
    public bool PermiteCompra { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Presentacion")]
    public virtual ICollection<CompraDetalle> CompraDetalles { get; set; } = new List<CompraDetalle>();

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoPresentaciones")]
    public virtual Producto Producto { get; set; } = null!;

    [InverseProperty("Presentacion")]
    public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
}
