using IBussines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.RequestResponse;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteBussines _ClienteBussines;

        public ClienteController(
            IClienteBussines ClienteBussines)
        {
            _ClienteBussines = ClienteBussines;
        }

        #region CRUD

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista =
                await _ClienteBussines.GetAllAsync();

            return Ok(lista);
        }
        [HttpGet("paget/all")]
        public async Task<IActionResult> GetAllProductsPaged(int pagina = 1, int cantidad = 10)
        {
            return Ok(await _ClienteBussines.GetAllClientesPaged(
                pagina,
                cantidad));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            var Cliente =
                await _ClienteBussines
                    .GetByIdAsync(id);

            if (Cliente == null)
                return NotFound();

            return Ok(Cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] ClienteRequest request)
        {
            var Cliente =
                await _ClienteBussines
                    .CreateAsync(request);

            return Ok(Cliente);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] ClienteRequest request)
        {
            var Cliente =
                await _ClienteBussines
                    .UpdateAsync(request);

            return Ok(Cliente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            await _ClienteBussines
                .DeleteAsync(id);

            return Ok();
        }

        #endregion

        #region APisPeru
        [HttpGet("dni/{tipoDocumento}/{nroDocumento}")]
        public async Task<IActionResult> GetClienteByDocumento(string tipoDocumento, string nroDocumento)
        {
            var cliente = await _ClienteBussines.GetByTipoNroDocumento(tipoDocumento, nroDocumento);
            return Ok(cliente);
        }
        #endregion
    }
}
