using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Pista de auditoría de envíos a SUNAT. JSON enviado, respuesta, CDR.
/// </summary>
[Table("documentos_electronicos")]
[Index("TipoDocumento", "Serie", "Correlativo", Name = "documentos_electronicos_tipo_documento_serie_correlativo_key", IsUnique = true)]
public partial class DocumentosElectronico
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("venta_id")]
    public int VentaId { get; set; }

    [Column("tipo_documento")]
    [StringLength(10)]
    public string TipoDocumento { get; set; } = null!;

    [Column("serie")]
    [StringLength(4)]
    public string Serie { get; set; } = null!;

    [Column("correlativo")]
    public int Correlativo { get; set; }

    [Column("fecha_envio")]
    public DateTime? FechaEnvio { get; set; }

    [Column("fecha_respuesta")]
    public DateTime? FechaRespuesta { get; set; }

    [Column("codigo_respuesta")]
    [StringLength(10)]
    public string? CodigoRespuesta { get; set; }

    [Column("descripcion_respuesta")]
    public string? DescripcionRespuesta { get; set; }

    [Column("cdr_numero")]
    [StringLength(20)]
    public string? CdrNumero { get; set; }

    [Column("cdr_descripcion")]
    public string? CdrDescripcion { get; set; }

    [Column("xml_enviado")]
    public string? XmlEnviado { get; set; }

    [Column("xml_respuesta")]
    public string? XmlRespuesta { get; set; }

    [Column("codigo_hash")]
    [StringLength(255)]
    public string? CodigoHash { get; set; }

    [Column("sunat_xml_id")]
    [StringLength(100)]
    public string? SunatXmlId { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("VentaId")]
    [InverseProperty("DocumentosElectronicos")]
    public virtual Venta Venta { get; set; } = null!;
}
