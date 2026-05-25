using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("ventas")]
[Index("CajaSesionId", Name = "idx_ventas_caja_sesion")]
[Index("ClienteId", Name = "idx_ventas_cliente")]
[Index("FechaCreacion", Name = "idx_ventas_fecha", AllDescending = true)]
[Index("SucursalId", Name = "idx_ventas_sucursal")]
[Index("SucursalId", "Serie", "Correlativo", Name = "ventas_sucursal_id_serie_correlativo_key", IsUnique = true)]
public partial class Venta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("caja_id")]
    public int CajaId { get; set; }

    [Column("caja_sesion_id")]
    public int? CajaSesionId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("cliente_id")]
    public int? ClienteId { get; set; }

    [Column("tipo_documento")]
    [StringLength(5)]
    public string TipoDocumento { get; set; } = null!;

    [Column("serie")]
    [StringLength(4)]
    public string Serie { get; set; } = null!;

    [Column("correlativo")]
    public int Correlativo { get; set; }

    [Column("subtotal")]
    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    [Column("impuesto")]
    [Precision(12, 2)]
    public decimal Impuesto { get; set; }

    [Column("descuento")]
    [Precision(12, 2)]
    public decimal? Descuento { get; set; }

    [Column("total")]
    [Precision(12, 2)]
    public decimal Total { get; set; }

    [Column("tipo_moneda")]
    [StringLength(3)]
    public string TipoMoneda { get; set; } = null!;

    [Column("tipo_cambio")]
    [Precision(10, 4)]
    public decimal? TipoCambio { get; set; }

    [Column("estado_sunat")]
    [StringLength(20)]
    public string? EstadoSunat { get; set; }

    [Column("codigo_hash")]
    [StringLength(255)]
    public string? CodigoHash { get; set; }

    [Column("url_pdf")]
    [StringLength(500)]
    public string? UrlPdf { get; set; }

    [Column("cliente_tipo_doc")]
    [StringLength(5)]
    public string? ClienteTipoDoc { get; set; }

    [Column("cliente_numero_doc")]
    [StringLength(20)]
    public string? ClienteNumeroDoc { get; set; }

    [Column("cliente_nombre")]
    [StringLength(500)]
    public string? ClienteNombre { get; set; }

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("CajaId")]
    [InverseProperty("Venta")]
    public virtual Caja Caja { get; set; } = null!;

    [ForeignKey("CajaSesionId")]
    [InverseProperty("Venta")]
    public virtual CajaSesione? CajaSesion { get; set; }

    [ForeignKey("ClienteId")]
    [InverseProperty("Venta")]
    public virtual Cliente? Cliente { get; set; }

    [InverseProperty("Venta")]
    public virtual ICollection<DocumentosElectronico> DocumentosElectronicos { get; set; } = new List<DocumentosElectronico>();

    [ForeignKey("SucursalId")]
    [InverseProperty("Venta")]
    public virtual Sucursale Sucursal { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Venta")]
    public virtual Usuario Usuario { get; set; } = null!;

    [InverseProperty("Venta")]
    public virtual ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();

    [InverseProperty("Venta")]
    public virtual ICollection<VentaPago> VentaPagos { get; set; } = new List<VentaPago>();
}
