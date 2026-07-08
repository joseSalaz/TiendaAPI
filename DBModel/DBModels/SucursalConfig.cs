using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Configuración por sucursal: series, correlativos, credenciales SUNAT.
/// </summary>
[Table("sucursal_config")]
public partial class SucursalConfig
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("serie_factura")]
    [StringLength(4)]
    public string? SerieFactura { get; set; }

    [Column("serie_boleta")]
    [StringLength(4)]
    public string? SerieBoleta { get; set; }

    [Column("correlativo_factura")]
    public int? CorrelativoFactura { get; set; }

    [Column("correlativo_boleta")]
    public int? CorrelativoBoleta { get; set; }

    [Column("sunat_usuario")]
    [StringLength(50)]
    public string? SunatUsuario { get; set; }

    [Column("sunat_password")]
    [StringLength(255)]
    public string? SunatPassword { get; set; }

    [Column("sunat_endpoint")]
    [StringLength(500)]
    public string? SunatEndpoint { get; set; }

    [Column("decimales_cantidad")]
    public int? DecimalesCantidad { get; set; }

    [Column("decimales_precio")]
    public int? DecimalesPrecio { get; set; }

    [Column("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; }

    [ForeignKey("SucursalId")]
    [InverseProperty("SucursalConfigs")]
    public virtual Sucursale Sucursal { get; set; } = null!;
}
