using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

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

    [ForeignKey("CompraId")]
    [InverseProperty("CompraDetalles")]
    public virtual Compra Compra { get; set; } = null!;

    [ForeignKey("ProductoId")]
    [InverseProperty("CompraDetalles")]
    public virtual Producto Producto { get; set; } = null!;
}
