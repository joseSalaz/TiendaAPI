using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("medios_pago")]
[Index("Codigo", Name = "medios_pago_codigo_key", IsUnique = true)]
public partial class MediosPago
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

    [Column("tipo")]
    [StringLength(20)]
    public string Tipo { get; set; } = null!;

    [Column("icono")]
    [StringLength(50)]
    public string? Icono { get; set; }

    [Column("permite_vuelto")]
    public bool? PermiteVuelto { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("MedioPago")]
    public virtual ICollection<VentaPago> VentaPagos { get; set; } = new List<VentaPago>();
}
