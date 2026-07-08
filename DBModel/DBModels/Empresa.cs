using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

[Table("empresas")]
[Index("Ruc", Name = "empresas_ruc_key", IsUnique = true)]
public partial class Empresa
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ruc")]
    [StringLength(11)]
    public string Ruc { get; set; } = null!;

    [Column("razon_social")]
    [StringLength(200)]
    public string RazonSocial { get; set; } = null!;

    [Column("nombre_comercial")]
    [StringLength(200)]
    public string? NombreComercial { get; set; }

    [Column("direccion_fiscal")]
    public string DireccionFiscal { get; set; } = null!;

    [Column("ubigeo")]
    [StringLength(10)]
    public string Ubigeo { get; set; } = null!;

    [Column("codigo_pais")]
    [StringLength(2)]
    public string CodigoPais { get; set; } = null!;

    [Column("departamento")]
    [StringLength(100)]
    public string Departamento { get; set; } = null!;

    [Column("provincia")]
    [StringLength(100)]
    public string Provincia { get; set; } = null!;

    [Column("distrito")]
    [StringLength(100)]
    public string Distrito { get; set; } = null!;

    [Column("urbanizacion")]
    [StringLength(100)]
    public string Urbanizacion { get; set; } = null!;

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; }

    [InverseProperty("Empresa")]
    public virtual ICollection<Sucursale> Sucursales { get; set; } = new List<Sucursale>();
}
