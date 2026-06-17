using IBussines;
using Microsoft.AspNetCore.Mvc;
using Models.RequestResponse;

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

        #endregion
    }
}