using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("notas_credito")]
[Index("TipoDocumento", "Serie", "Correlativo", Name = "uq_notas_credito_comprobante", IsUnique = true)]
public partial class NotasCredito
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

    [Column("tipo_documento_afectado")]
    [StringLength(10)]
    public string TipoDocumentoAfectado { get; set; } = null!;

    [Column("serie_afectada")]
    [StringLength(4)]
    public string SerieAfectada { get; set; } = null!;

    [Column("correlativo_afectado")]
    public int CorrelativoAfectado { get; set; }

    [Column("codigo_motivo")]
    [StringLength(10)]
    public string CodigoMotivo { get; set; } = null!;

    [Column("descripcion_motivo")]
    public string DescripcionMotivo { get; set; } = null!;

    [Column("subtotal")]
    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    [Column("impuesto")]
    [Precision(12, 2)]
    public decimal Impuesto { get; set; }

    [Column("total")]
    [Precision(12, 2)]
    public decimal Total { get; set; }

    [Column("estado_sunat")]
    [StringLength(30)]
    public string EstadoSunat { get; set; } = null!;

    [Column("sunat_codigo")]
    [StringLength(20)]
    public string? SunatCodigo { get; set; }

    [Column("sunat_mensaje")]
    public string? SunatMensaje { get; set; }

    [Column("sunat_hash")]
    public string? SunatHash { get; set; }

    [Column("respuesta_sunat_json")]
    public string? RespuestaSunatJson { get; set; }

    [Column("fecha_emision")]
    public DateTime FechaEmision { get; set; }

    [Column("fecha_envio_sunat")]
    public DateTime? FechaEnvioSunat { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [ForeignKey("VentaId")]
    [InverseProperty("NotasCreditos")]
    public virtual Venta Venta { get; set; } = null!;
}
