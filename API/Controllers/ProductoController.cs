using IBussines;
using Microsoft.AspNetCore.Mvc;
using Models.RequestResponse;
using System.Text.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoBussines _productoBussines;

        public ProductoController(
            IProductoBussines productoBussines)
        {
            _productoBussines = productoBussines;
        }

        #region CRUD

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista =
                await _productoBussines.GetAllAsync();

            return Ok(lista);
        }
        [HttpGet("paget/all")]
        public async Task<IActionResult> GetAllProductsPaged(int pagina = 1, int cantidad = 10)
        {
            return Ok(await _productoBussines.GetAllProductsPaged(
                pagina,
                cantidad));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var producto =
                await _productoBussines
                    .GetByIdAsync(id);

            if (producto == null)
                return NotFound();

            return Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ProductoRequest request)
        {
            var producto =
                await _productoBussines
                    .CreateAsync(request);

            return Ok(producto);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] ProductoRequest request)
        {
            var producto =
                await _productoBussines
                    .UpdateAsync(request);

            return Ok(producto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            await _productoBussines
                .DeleteAsync(id);

            return Ok();
        }

        #endregion

        #region Métodos propios
        [HttpPost("crear/stock")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProductoConStock([FromForm] ProductoFormDataRequest form)
        {
            if (string.IsNullOrWhiteSpace(form.ProductoJson))
                return BadRequest("No se recibió la información del producto.");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var request = JsonSerializer.Deserialize<ProductoRequest>(
                form.ProductoJson,
                options
            );

            if (request is null)
                return BadRequest("La información del producto no tiene un formato válido.");

            // De momento solo recibimos la imagen.
            // Luego aquí o en un servicio la subiremos a Firebase.
            // form.Imagen contiene el archivo.

            var response = await _productoBussines.CreateProductosStock(request);

            return Ok(response);
        }

        [HttpPut("actualizar/presentaciones")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProductoConPresentaciones([FromForm] ProductoFormDataRequest form)
        {
            if (string.IsNullOrWhiteSpace(form.ProductoJson))
                return BadRequest("No se recibió la información del producto.");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var request = JsonSerializer.Deserialize<ProductoRequest>(
                form.ProductoJson,
                options
            );

            if (request is null)
                return BadRequest("La información del producto no tiene un formato válido.");

            if (request.Id <= 0)
                return BadRequest("El id del producto es obligatorio para actualizar.");

            // Luego aquí subimos form.Imagen a Firebase.
            // Por ahora solo actualizamos la data.
            var response = await _productoBussines.UpdateProductoConPresentaciones(request);

            return Ok(response);
        }

        [HttpGet("{id}/Presentaciones")]
        public async Task<IActionResult> GetByIdProductosPrecentaciones(int id)
        {
            var response = await _productoBussines.GetByIdAsync(id);

            if (response == null)
                return NotFound();

            return Ok(response);
        }

        //[HttpGet("GetByName")]
        //public async Task<IActionResult>
        //    GetByName(string nombre)
        //{
        //    var producto =
        //        await _productoBussines
        //            .GetByNameAsync(nombre);

        //    if (producto == null)
        //        return NotFound(
        //            new
        //            {
        //                message =
        //                    "Producto no encontrado."
        //            });

        //    return Ok(producto);
        //}

        //[HttpGet("CatProducto")]
        //public async Task<IActionResult>
        //    GetProductoCategoria(
        //        int idCategoria,
        //        int? idSubcategoria)
        //{
        //    var productos =
        //        await _productoBussines
        //            .GetProductoCategoria(
        //                idCategoria,
        //                idSubcategoria);

        //    return Ok(productos);
        //}

        [HttpGet("scan/{barcode}")]
        public async Task<IActionResult> ScanBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return BadRequest(new { message = "El código de barras no puede estar vacío." });
            }

            var resultado = await _productoBussines.ScanByBarcodeAsync(barcode);

            return Ok(resultado);
        }

        [HttpGet("buscar-venta")]


        public async Task<IActionResult> BuscarParaVenta([FromQuery] ProductoVentaFiltroRequest filtro)
        {
            var response = await _productoBussines.BuscarParaVentaAsync(filtro);
            return Ok(response);

        }
    }
}

        #endregion

