using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("proveedores")]
public partial class Proveedore
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ruc")]
    [StringLength(11)]
    public string? Ruc { get; set; }

    [Column("razon_social")]
    [StringLength(200)]
    public string RazonSocial { get; set; } = null!;

    [Column("nombre_contacto")]
    [StringLength(200)]
    public string? NombreContacto { get; set; }

    [Column("telefono")]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("direccion")]
    public string? Direccion { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Proveedor")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
