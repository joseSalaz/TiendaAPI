using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("comprobante_series")]
[Index("SucursalId", "TipoComprobante", "Serie", Name = "comprobante_series_sucursal_id_tipo_comprobante_serie_key", IsUnique = true)]
[Index("SucursalId", "TipoComprobante", "Estado", Name = "idx_comprobante_series_sucursal_tipo")]
public partial class ComprobanteSeries
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("tipo_comprobante")]
    [StringLength(2)]
    public string TipoComprobante { get; set; } = null!;

    [Column("serie")]
    [StringLength(10)]
    public string Serie { get; set; } = null!;

    [Column("correlativo_actual")]
    public int CorrelativoActual { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("SucursalId")]
    [InverseProperty("ComprobanteSeries")]
    public virtual Sucursale Sucursal { get; set; } = null!;
}
