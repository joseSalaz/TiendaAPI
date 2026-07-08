using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("caja_movimientos")]
[Index("CajaId", "FechaCreacion", Name = "idx_caja_movimientos_caja", IsDescending = new[] { false, true })]
[Index("ReferenciaTabla", "ReferenciaId", Name = "idx_caja_movimientos_referencia")]
[Index("CajaSesionId", "FechaCreacion", Name = "idx_caja_movimientos_sesion", IsDescending = new[] { false, true })]
[Index("UsuarioId", Name = "idx_caja_movimientos_usuario")]
public partial class CajaMovimiento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("caja_sesion_id")]
    public int CajaSesionId { get; set; }

    [Column("caja_id")]
    public int CajaId { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("tipo_movimiento")]
    [StringLength(20)]
    public string TipoMovimiento { get; set; } = null!;

    [Column("concepto")]
    [StringLength(40)]
    public string Concepto { get; set; } = null!;

    [Column("medio_pago_id")]
    public int? MedioPagoId { get; set; }

    [Column("monto")]
    [Precision(12, 2)]
    public decimal Monto { get; set; }

    [Column("afecta_efectivo")]
    public bool AfectaEfectivo { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("referencia_tabla")]
    [StringLength(50)]
    public string? ReferenciaTabla { get; set; }

    [Column("referencia_id")]
    public int? ReferenciaId { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("CajaId")]
    [InverseProperty("CajaMovimientos")]
    public virtual Caja Caja { get; set; } = null!;

    [ForeignKey("CajaSesionId")]
    [InverseProperty("CajaMovimientos")]
    public virtual CajaSesione CajaSesion { get; set; } = null!;

    [ForeignKey("MedioPagoId")]
    [InverseProperty("CajaMovimientos")]
    public virtual MediosPago? MedioPago { get; set; }

    [ForeignKey("SucursalId")]
    [InverseProperty("CajaMovimientos")]
    public virtual Sucursale Sucursal { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("CajaMovimientos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
