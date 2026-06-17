using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

[Table("venta_pagos")]
public partial class VentaPago
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("venta_id")]
    public int VentaId { get; set; }

    [Column("medio_pago_id")]
    public int MedioPagoId { get; set; }

    [Column("monto")]
    [Precision(12, 2)]
    public decimal Monto { get; set; }

    [Column("referencia")]
    [StringLength(100)]
    public string? Referencia { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [ForeignKey("MedioPagoId")]
    [InverseProperty("VentaPagos")]
    public virtual MediosPago MedioPago { get; set; } = null!;

    [ForeignKey("VentaId")]
    [InverseProperty("VentaPagos")]
    public virtual Venta Venta { get; set; } = null!;
}
