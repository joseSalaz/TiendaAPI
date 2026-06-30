using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Sucursales o puntos de venta. El sistema soporta múltiples sucursales con stock independiente.
/// </summary>
[Table("sucursales")]
[Index("Codigo", Name = "sucursales_codigo_key", IsUnique = true)]
public partial class Sucursale
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("codigo")]
    [StringLength(20)]
    public string Codigo { get; set; } = null!;

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("direccion")]
    public string? Direccion { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("empresa_id")]
    public int EmpresaId { get; set; }

    [InverseProperty("Sucursal")]
    public virtual ICollection<CajaMovimiento> CajaMovimientos { get; set; } = new List<CajaMovimiento>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<ComprobanteSeries> ComprobanteSeries { get; set; } = new List<ComprobanteSeries>();

    [ForeignKey("EmpresaId")]
    [InverseProperty("Sucursales")]
    public virtual Empresa Empresa { get; set; } = null!;

    [InverseProperty("Sucursal")]
    public virtual ICollection<ProductoLote> ProductoLotes { get; set; } = new List<ProductoLote>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<ProductoStock> ProductoStocks { get; set; } = new List<ProductoStock>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<SecuenciasDocumento> SecuenciasDocumentos { get; set; } = new List<SecuenciasDocumento>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<StockMovimiento> StockMovimientos { get; set; } = new List<StockMovimiento>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<SucursalConfig> SucursalConfigs { get; set; } = new List<SucursalConfig>();

    [InverseProperty("Sucursal")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
