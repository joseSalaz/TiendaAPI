using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("categorias")]
[Index("Codigo", Name = "categorias_codigo_key", IsUnique = true)]
public partial class Categoria
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

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("color")]
    [StringLength(7)]
    public string? Color { get; set; }

    [Column("icono")]
    [StringLength(50)]
    public string? Icono { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [InverseProperty("Categoria")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
