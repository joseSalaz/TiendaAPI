using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Línea de productos en una compra. Snapshot del precio al momento.
/// </summary>
[Table("compra_detalles")]
public partial class CompraDetalle
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("compra_id")]
    public int CompraId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    [Precision(12, 2)]
    public decimal PrecioUnitario { get; set; }

    [Column("subtotal")]
    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    [Column("presentacion_id")]
    public int? PresentacionId { get; set; }

    [Column("presentacion_nombre")]
    [StringLength(100)]
    public string? PresentacionNombre { get; set; }

    [Column("cantidad_presentacion")]
    public int? CantidadPresentacion { get; set; }

    [Column("cantidad_unidades")]
    public int? CantidadUnidades { get; set; }

    [ForeignKey("CompraId")]
    [InverseProperty("CompraDetalles")]
    public virtual Compra Compra { get; set; } = null!;

    [ForeignKey("PresentacionId")]
    [InverseProperty("CompraDetalles")]
    public virtual ProductoPresentacione? Presentacion { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("CompraDetalles")]
    public virtual Producto Producto { get; set; } = null!;

    [InverseProperty("CompraDetalle")]
    public virtual ICollection<ProductoLote> ProductoLotes { get; set; } = new List<ProductoLote>();
}
