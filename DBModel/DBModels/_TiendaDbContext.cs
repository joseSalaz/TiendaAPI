using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DBModel.Models;

public partial class _TiendaDbContext : DbContext
{
    public _TiendaDbContext()
    {
    }

    public _TiendaDbContext(DbContextOptions<_TiendaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<CajaSesione> CajaSesiones { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<CompraDetalle> CompraDetalles { get; set; }

    public virtual DbSet<DocumentosElectronico> DocumentosElectronicos { get; set; }

    public virtual DbSet<MediosPago> MediosPagos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoStock> ProductoStocks { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<SecuenciasDocumento> SecuenciasDocumentos { get; set; }

    public virtual DbSet<StockMovimiento> StockMovimientos { get; set; }

    public virtual DbSet<SucursalConfig> SucursalConfigs { get; set; }

    public virtual DbSet<Sucursale> Sucursales { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<VentaDetalle> VentaDetalles { get; set; }

    public virtual DbSet<VentaPago> VentaPagos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Tienda_DB;Username=postgres;Password=1234");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Caja>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cajas_pkey");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Cajas).HasConstraintName("cajas_sucursal_id_fkey");
        });

        modelBuilder.Entity<CajaSesione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("caja_sesiones_pkey");

            entity.ToTable("caja_sesiones", tb => tb.HasComment("Control de turnos de caja (apertura/cierre)"));

            entity.Property(e => e.Estado).HasDefaultValueSql("'ABIERTA'::character varying");
            entity.Property(e => e.FechaApertura).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Caja).WithMany(p => p.CajaSesiones).HasConstraintName("caja_sesiones_caja_id_fkey");

            entity.HasOne(d => d.UsuarioApertura).WithMany(p => p.CajaSesioneUsuarioAperturas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caja_sesiones_usuario_apertura_id_fkey");

            entity.HasOne(d => d.UsuarioCierre).WithMany(p => p.CajaSesioneUsuarioCierres).HasConstraintName("caja_sesiones_usuario_cierre_id_fkey");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categorias_pkey");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clientes_pkey");

            entity.HasIndex(e => new { e.TipoDocumento, e.NumeroDocumento }, "idx_clientes_doc").HasFilter("(numero_documento IS NOT NULL)");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
            entity.Property(e => e.TipoDocumento).HasDefaultValueSql("'DNI'::character varying");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("compras_pkey");

            entity.Property(e => e.Estado).HasDefaultValueSql("'COMPLETADO'::character varying");
            entity.Property(e => e.Fecha).HasDefaultValueSql("now()");
            entity.Property(e => e.Impuesto).HasDefaultValueSql("0");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("compras_proveedor_id_fkey");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("compras_sucursal_id_fkey");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("compras_usuario_id_fkey");
        });

        modelBuilder.Entity<CompraDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("compra_detalles_pkey");

            entity.HasOne(d => d.Compra).WithMany(p => p.CompraDetalles).HasConstraintName("compra_detalles_compra_id_fkey");

            entity.HasOne(d => d.Producto).WithMany(p => p.CompraDetalles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("compra_detalles_producto_id_fkey");
        });

        modelBuilder.Entity<DocumentosElectronico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documentos_electronicos_pkey");

            entity.ToTable("documentos_electronicos", tb => tb.HasComment("Pista de auditoría de envíos a SUNAT"));

            entity.Property(e => e.Estado).HasDefaultValueSql("'PENDIENTE'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Venta).WithMany(p => p.DocumentosElectronicos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("documentos_electronicos_venta_id_fkey");
        });

        modelBuilder.Entity<MediosPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("medios_pago_pkey");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
            entity.Property(e => e.PermiteVuelto).HasDefaultValue(false);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("productos_pkey");

            entity.HasIndex(e => e.CodigoBarras, "idx_productos_barras").HasFilter("(codigo_barras IS NOT NULL)");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("now()");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
            entity.Property(e => e.PermiteStockNegativo).HasDefaultValue(false);
            entity.Property(e => e.UnidadMedida).HasDefaultValueSql("'UND'::character varying");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos).HasConstraintName("productos_categoria_id_fkey");
        });

        modelBuilder.Entity<ProductoStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("producto_stock_pkey");

            entity.ToTable("producto_stock", tb => tb.HasComment("Stock individual por producto y sucursal"));

            entity.Property(e => e.StockActual).HasDefaultValue(0);
            entity.Property(e => e.StockMinimo).HasDefaultValue(0);

            entity.HasOne(d => d.Producto).WithMany(p => p.ProductoStocks).HasConstraintName("producto_stock_producto_id_fkey");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.ProductoStocks).HasConstraintName("producto_stock_sucursal_id_fkey");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("proveedores_pkey");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<SecuenciasDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("secuencias_documentos_pkey");

            entity.Property(e => e.CorrelativoActual).HasDefaultValue(0);

            entity.HasOne(d => d.Sucursal).WithMany(p => p.SecuenciasDocumentos).HasConstraintName("secuencias_documentos_sucursal_id_fkey");
        });

        modelBuilder.Entity<StockMovimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stock_movimientos_pkey");

            entity.ToTable("stock_movimientos", tb => tb.HasComment("Historial de movimientos de inventario"));

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Producto).WithMany(p => p.StockMovimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_movimientos_producto_id_fkey");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.StockMovimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_movimientos_sucursal_id_fkey");

            entity.HasOne(d => d.Usuario).WithMany(p => p.StockMovimientos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stock_movimientos_usuario_id_fkey");
        });

        modelBuilder.Entity<SucursalConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sucursal_config_pkey");

            entity.Property(e => e.CorrelativoBoleta).HasDefaultValue(0);
            entity.Property(e => e.CorrelativoFactura).HasDefaultValue(0);
            entity.Property(e => e.DecimalesCantidad).HasDefaultValue(2);
            entity.Property(e => e.DecimalesPrecio).HasDefaultValue(2);
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("now()");
            entity.Property(e => e.SerieBoleta).HasDefaultValueSql("'B001'::character varying");
            entity.Property(e => e.SerieFactura).HasDefaultValueSql("'F001'::character varying");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.SucursalConfigs).HasConstraintName("sucursal_config_sucursal_id_fkey");
        });

        modelBuilder.Entity<Sucursale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sucursales_pkey");

            entity.ToTable("sucursales", tb => tb.HasComment("Puntos de venta (sucursales)"));

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usuarios_pkey");

            entity.Property(e => e.Estado).HasDefaultValueSql("'ACTIVO'::character varying");
            entity.Property(e => e.FechaActualizacion).HasDefaultValueSql("now()");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
            entity.Property(e => e.Rol).HasDefaultValueSql("'VENDEDOR'::character varying");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ventas_pkey");

            entity.Property(e => e.Descuento).HasDefaultValueSql("0");
            entity.Property(e => e.EstadoSunat).HasDefaultValueSql("'PENDIENTE'::character varying");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");
            entity.Property(e => e.TipoCambio).HasDefaultValueSql("1");
            entity.Property(e => e.TipoMoneda).HasDefaultValueSql("'PEN'::character varying");

            entity.HasOne(d => d.Caja).WithMany(p => p.Venta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ventas_caja_id_fkey");

            entity.HasOne(d => d.CajaSesion).WithMany(p => p.Venta).HasConstraintName("ventas_caja_sesion_id_fkey");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Venta).HasConstraintName("ventas_cliente_id_fkey");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Venta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ventas_sucursal_id_fkey");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Venta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ventas_usuario_id_fkey");
        });

        modelBuilder.Entity<VentaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("venta_detalles_pkey");

            entity.Property(e => e.Descuento).HasDefaultValueSql("0");

            entity.HasOne(d => d.Producto).WithMany(p => p.VentaDetalles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("venta_detalles_producto_id_fkey");

            entity.HasOne(d => d.Venta).WithMany(p => p.VentaDetalles).HasConstraintName("venta_detalles_venta_id_fkey");
        });

        modelBuilder.Entity<VentaPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("venta_pagos_pkey");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("now()");

            entity.HasOne(d => d.MedioPago).WithMany(p => p.VentaPagos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("venta_pagos_medio_pago_id_fkey");

            entity.HasOne(d => d.Venta).WithMany(p => p.VentaPagos).HasConstraintName("venta_pagos_venta_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
