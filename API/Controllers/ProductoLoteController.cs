using IBussines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.RequestResponse;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoLoteController : ControllerBase
    {
        private readonly IProductoLoteBussines _productoLoteBussines;

        public ProductoLoteController(IProductoLoteBussines entradaStockBussines)
        {
            _productoLoteBussines = entradaStockBussines;
        }

        [HttpPost("Registrar/Entrada")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaStockRequest request)
        {
            var response = await _productoLoteBussines.RegistrarEntradaAsync(request);
            return Ok(response);
        }
    }
}
