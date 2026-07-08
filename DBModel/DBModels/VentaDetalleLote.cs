using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("venta_detalle_lotes")]
[Index("VentaDetalleId", Name = "idx_venta_detalle_lotes_detalle")]
[Index("LoteId", Name = "idx_venta_detalle_lotes_lote")]
public partial class VentaDetalleLote
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("venta_detalle_id")]
    public int VentaDetalleId { get; set; }

    [Column("lote_id")]
    public int LoteId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("LoteId")]
    [InverseProperty("VentaDetalleLotes")]
    public virtual ProductoLote Lote { get; set; } = null!;

    [ForeignKey("VentaDetalleId")]
    [InverseProperty("VentaDetalleLotes")]
    public virtual VentaDetalle VentaDetalle { get; set; } = null!;
}
