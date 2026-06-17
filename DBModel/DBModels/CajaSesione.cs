using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

/// <summary>
/// Control de turnos de caja (apertura/cierre)
/// </summary>
[Table("caja_sesiones")]
[Index("FechaApertura", Name = "idx_caja_sesiones_fecha", AllDescending = true)]
public partial class CajaSesione
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("caja_id")]
    public int CajaId { get; set; }

    [Column("usuario_apertura_id")]
    public int UsuarioAperturaId { get; set; }

    [Column("fecha_apertura")]
    public DateTime FechaApertura { get; set; }

    [Column("monto_inicial")]
    [Precision(12, 2)]
    public decimal MontoInicial { get; set; }

    [Column("fecha_cierre")]
    public DateTime? FechaCierre { get; set; }

    [Column("usuario_cierre_id")]
    public int? UsuarioCierreId { get; set; }

    [Column("monto_cierre")]
    [Precision(12, 2)]
    public decimal? MontoCierre { get; set; }

    [Column("diferencia")]
    [Precision(12, 2)]
    public decimal? Diferencia { get; set; }

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [ForeignKey("CajaId")]
    [InverseProperty("CajaSesiones")]
    public virtual Caja Caja { get; set; } = null!;

    [ForeignKey("UsuarioAperturaId")]
    [InverseProperty("CajaSesioneUsuarioAperturas")]
    public virtual Usuario UsuarioApertura { get; set; } = null!;

    [ForeignKey("UsuarioCierreId")]
    [InverseProperty("CajaSesioneUsuarioCierres")]
    public virtual Usuario? UsuarioCierre { get; set; }

    [InverseProperty("CajaSesion")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
