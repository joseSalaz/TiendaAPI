using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("clientes")]
public partial class Cliente
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo_documento")]
    [StringLength(5)]
    public string TipoDocumento { get; set; } = null!;

    [Column("numero_documento")]
    [StringLength(20)]
    public string? NumeroDocumento { get; set; }

    [Column("nombres")]
    [StringLength(200)]
    public string? Nombres { get; set; }

    [Column("apellidos")]
    [StringLength(200)]
    public string? Apellidos { get; set; }

    [Column("razon_social")]
    [StringLength(200)]
    public string? RazonSocial { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("direccion")]
    public string? Direccion { get; set; }

    [Column("ubigeo")]
    [StringLength(10)]
    public string? Ubigeo { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Cliente")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
