using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("producto_lotes")]
public partial class ProductoLote
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("compra_detalle_id")]
    public int? CompraDetalleId { get; set; }

    [Column("codigo_lote")]
    [StringLength(100)]
    public string? CodigoLote { get; set; }

    [Column("fecha_vencimiento")]
    public DateOnly? FechaVencimiento { get; set; }

    [Column("stock_inicial")]
    public int StockInicial { get; set; }

    [Column("stock_actual")]
    public int StockActual { get; set; }

    [Column("costo_unitario")]
    [Precision(12, 4)]
    public decimal? CostoUnitario { get; set; }

    [Column("fecha_ingreso")]
    public DateTime FechaIngreso { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [ForeignKey("CompraDetalleId")]
    [InverseProperty("ProductoLotes")]
    public virtual CompraDetalle? CompraDetalle { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("ProductoLotes")]
    public virtual Producto Producto { get; set; } = null!;

    [InverseProperty("Lote")]
    public virtual ICollection<StockMovimiento> StockMovimientos { get; set; } = new List<StockMovimiento>();

    [ForeignKey("SucursalId")]
    [InverseProperty("ProductoLotes")]
    public virtual Sucursale Sucursal { get; set; } = null!;

    [InverseProperty("Lote")]
    public virtual ICollection<VentaDetalleLote> VentaDetalleLotes { get; set; } = new List<VentaDetalleLote>();
}
