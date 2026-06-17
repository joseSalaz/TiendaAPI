using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

/// <summary>
/// Stock individual por producto y sucursal
/// </summary>
[Table("producto_stock")]
[Index("ProductoId", Name = "idx_producto_stock_producto")]
[Index("SucursalId", Name = "idx_producto_stock_sucursal")]
[Index("ProductoId", "SucursalId", Name = "producto_stock_producto_id_sucursal_id_key", IsUnique = true)]
public partial class ProductoStock
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("stock_actual")]
    public int StockActual { get; set; }

    [Column("stock_minimo")]
    public int? StockMinimo { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoStocks")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("SucursalId")]
    [InverseProperty("ProductoStocks")]
    public virtual Sucursale Sucursal { get; set; } = null!;
}
