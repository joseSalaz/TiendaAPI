using Microsoft.EntityFrameworkCore;
using UtilPaginados.RequestResponse;

public static class QueryableExtensions
{
    public static async Task<PaginacionResponse<T>> PaginarAsync<T>(
        this IQueryable<T> query,
        int pagina,
        int cantidad)
    {
        pagina = Math.Max(1, pagina);

        cantidad = Math.Clamp(
            cantidad,
            1,
            100);
        var total = await query.CountAsync();

        var items = await query
            .Skip((pagina - 1) * cantidad)
            .Take(cantidad)
            .ToListAsync();

        return new PaginacionResponse<T>
        {
            Items = items,
            Total = total,
            PaginaActual = pagina,
            TotalPaginas = (int)Math.Ceiling(total / (double)cantidad),
            CantidadPorPagina = cantidad
        };
    }
}