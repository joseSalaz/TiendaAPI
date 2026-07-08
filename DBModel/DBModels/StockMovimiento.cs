using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Historial de movimientos de inventario: ventas, compras, ajustes.
/// </summary>
[Table("stock_movimientos")]
[Index("FechaCreacion", Name = "idx_stock_mov_fecha", AllDescending = true)]
[Index("ProductoId", "FechaCreacion", Name = "idx_stock_mov_producto", IsDescending = new[] { false, true })]
public partial class StockMovimiento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("tipo")]
    [StringLength(20)]
    public string Tipo { get; set; } = null!;

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("stock_anterior")]
    public int StockAnterior { get; set; }

    [Column("stock_nuevo")]
    public int StockNuevo { get; set; }

    [Column("motivo")]
    [StringLength(200)]
    public string? Motivo { get; set; }

    [Column("referencia_tabla")]
    [StringLength(50)]
    public string? ReferenciaTabla { get; set; }

    [Column("referencia_id")]
    public int? ReferenciaId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("lote_id")]
    public int? LoteId { get; set; }

    [ForeignKey("LoteId")]
    [InverseProperty("StockMovimientos")]
    public virtual ProductoLote? Lote { get; set; }

    [ForeignKey("ProductoId")]
    [InverseProperty("StockMovimientos")]
    public virtual Producto Producto { get; set; } = null!;

    [ForeignKey("SucursalId")]
    [InverseProperty("StockMovimientos")]
    public virtual Sucursale Sucursal { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("StockMovimientos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
