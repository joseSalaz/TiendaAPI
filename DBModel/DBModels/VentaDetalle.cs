using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("venta_detalles")]
[Index("VentaId", Name = "idx_venta_detalles_venta")]
public partial class VentaDetalle
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("venta_id")]
    public int VentaId { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("codigo_barras")]
    [StringLength(50)]
    public string? CodigoBarras { get; set; }

    [Column("nombre_producto")]
    [StringLength(500)]
    public string NombreProducto { get; set; } = null!;

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("unidad_medida")]
    [StringLength(20)]
    public string UnidadMedida { get; set; } = null!;

    [Column("precio_unitario")]
    [Precision(12, 2)]
    public decimal PrecioUnitario { get; set; }

    [Column("descuento")]
    [Precision(12, 2)]
    public decimal? Descuento { get; set; }

    [Column("subtotal")]
    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    [Column("producto_codigo_interno")]
    [StringLength(30)]
    public string? ProductoCodigoInterno { get; set; }

    [Column("producto_imagen_url")]
    [StringLength(500)]
    public string? ProductoImagenUrl { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("VentaDetalles")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("VentaId")]
    [InverseProperty("VentaDetalles")]
    public virtual Venta Venta { get; set; } = null!;
}
