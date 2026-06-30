using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Terminales de cobro por sucursal. Una sucursal puede tener varias cajas activas.
/// </summary>
[Table("cajas")]
[Index("SucursalId", "Numero", Name = "cajas_sucursal_id_numero_key", IsUnique = true)]
public partial class Caja
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sucursal_id")]
    public int SucursalId { get; set; }

    [Column("numero")]
    [StringLength(20)]
    public string Numero { get; set; } = null!;

    [Column("nombre")]
    [StringLength(100)]
    public string? Nombre { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Caja")]
    public virtual ICollection<CajaMovimiento> CajaMovimientos { get; set; } = new List<CajaMovimiento>();

    [InverseProperty("Caja")]
    public virtual ICollection<CajaSesione> CajaSesiones { get; set; } = new List<CajaSesione>();

    [ForeignKey("SucursalId")]
    [InverseProperty("Cajas")]
    public virtual Sucursale Sucursal { get; set; } = null!;

    [InverseProperty("Caja")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
