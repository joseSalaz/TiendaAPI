using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("compras")]
[Index("SucursalId", "Serie", "Correlativo", Name = "compras_sucursal_id_serie_correlativo_key", IsUnique = true)]
[Index("Fecha", Name = "idx_compras_fecha", AllDescending = true)]
[Index("ProveedorId", Name = "idx_compras_proveedor")]
public partial class Compra
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("proveedor_id")]
    public int ProveedorId { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("usuario_id")]
    public int UsuarioId { get; set; }

    [Column("serie")]
    [StringLength(4)]
    public string Serie { get; set; } = null!;

    [Column("correlativo")]
    public int Correlativo { get; set; }

    [Column("fecha")]
    public DateTime Fecha { get; set; }

    [Column("subtotal")]
    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    [Column("impuesto")]
    [Precision(12, 2)]
    public decimal? Impuesto { get; set; }

    [Column("total")]
    [Precision(12, 2)]
    public decimal Total { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("observaciones")]
    public string? Observaciones { get; set; }

    [InverseProperty("Compra")]
    public virtual ICollection<CompraDetalle> CompraDetalles { get; set; } = new List<CompraDetalle>();

    [ForeignKey("ProveedorId")]
    [InverseProperty("Compras")]
    public virtual Proveedore Proveedor { get; set; } = null!;

    [ForeignKey("SucursalId")]
    [InverseProperty("Compras")]
    public virtual Sucursale Sucursal { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("Compras")]
    public virtual Usuario Usuario { get; set; } = null!;
}
