using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.DBModels;

/// <summary>
/// Personal que opera el sistema: admins, vendedores, cajeros. Login con username/password.
/// </summary>
[Table("usuarios")]
[Index("Username", Name = "usuarios_username_key", IsUnique = true)]
public partial class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("nombres")]
    [StringLength(100)]
    public string Nombres { get; set; } = null!;

    [Column("apellidos")]
    [StringLength(100)]
    public string? Apellidos { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("rol")]
    [StringLength(30)]
    public string Rol { get; set; } = null!;

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_actualizacion")]
    public DateTime FechaActualizacion { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<CajaMovimiento> CajaMovimientos { get; set; } = new List<CajaMovimiento>();

    [InverseProperty("UsuarioApertura")]
    public virtual ICollection<CajaSesione> CajaSesioneUsuarioAperturas { get; set; } = new List<CajaSesione>();

    [InverseProperty("UsuarioCierre")]
    public virtual ICollection<CajaSesione> CajaSesioneUsuarioCierres { get; set; } = new List<CajaSesione>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [InverseProperty("Usuario")]
    public virtual ICollection<StockMovimiento> StockMovimientos { get; set; } = new List<StockMovimiento>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
