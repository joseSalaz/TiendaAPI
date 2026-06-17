using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("secuencias_documentos")]
[Index("SucursalId", "TipoDocumento", "Serie", Name = "secuencias_documentos_sucursal_id_tipo_documento_serie_key", IsUnique = true)]
public partial class SecuenciasDocumento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("tipo_documento")]
    [StringLength(10)]
    public string TipoDocumento { get; set; } = null!;

    [Column("serie")]
    [StringLength(4)]
    public string Serie { get; set; } = null!;

    [Column("correlativo_actual")]
    public int? CorrelativoActual { get; set; }

    [ForeignKey("SucursalId")]
    [InverseProperty("SecuenciasDocumentos")]
    public virtual Sucursale Sucursal { get; set; } = null!;
}
