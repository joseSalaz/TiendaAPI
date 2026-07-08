# Refactorización de `VentaBussines`

Se mantuvo la funcionalidad pública existente de `IVentaBussines` y de `VentaController`.

## Objetivo

`Bussines/VentaBussines.cs` concentraba demasiadas responsabilidades en un solo archivo. Para bajar el acoplamiento visual y facilitar mantenimiento, se dividió la clase usando `partial class`, sin cambiar las firmas públicas ni la inyección de dependencias existente.

## Nueva estructura

```txt
Bussines/
├─ VentaBussines.cs
└─ Ventas/
   ├─ Anulacion/
   │  └─ VentaBussines.Anulacion.cs
   ├─ Caja/
   │  └─ VentaBussines.Caja.cs
   ├─ Consultas/
   │  └─ VentaBussines.Consultas.cs
   ├─ Facturacion/
   │  ├─ VentaBussines.ArchivosElectronicos.cs
   │  ├─ VentaBussines.ConsultaSunat.cs
   │  └─ VentaBussines.EmisionElectronica.cs
   ├─ NotasCredito/
   │  └─ VentaBussines.NotaCredito.cs
   ├─ Stock/
   │  └─ VentaBussines.Stock.cs
   └─ Validaciones/
      └─ VentaBussines.Validaciones.cs
```

## Qué quedó en cada archivo

- `VentaBussines.cs`: dependencias, constructor y flujo principal de registro de venta.
- `Ventas/Stock`: descuento FEFO, lotes y movimientos de stock por venta.
- `Ventas/Caja`: movimientos de caja generados por pagos en efectivo.
- `Ventas/Validaciones`: validación de request y resolución/creación de cliente.
- `Ventas/Facturacion`: emisión electrónica, reintentos, descarga PDF/XML y consultas SUNAT.
- `Ventas/NotasCredito`: emisión de nota de crédito manual.
- `Ventas/Anulacion`: anulación de venta, nota de crédito, devolución de stock y caja.
- `Ventas/Consultas`: listado paginado, detalle completo y helpers de visualización.

## Consideraciones

- No se modificó `IVentaBussines`.
- No se modificó `VentaController`.
- No se cambió el registro DI: `IVentaBussines -> VentaBussines` sigue igual.
- El proyecto usa SDK-style `.csproj`, por lo que los nuevos `.cs` se incluyen automáticamente.
- En este entorno no estaba disponible el comando `dotnet`, por eso no se pudo ejecutar `dotnet build` aquí. Ejecuta localmente:

```bash
dotnet restore
dotnet build BackendMinimarket.slnx
```

