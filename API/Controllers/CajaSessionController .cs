using IBussines;
using Microsoft.AspNetCore.Mvc;
using Models.RequestResponse;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CajaSessionController : ControllerBase
    {
        private readonly ICajaSesionBussines _cajaSesionBussines;

        public CajaSessionController(ICajaSesionBussines cajaSesionBussines)
        {
            _cajaSesionBussines = cajaSesionBussines;
        }

        #region CRUD



        #endregion

        #region Métodos propios
        [HttpGet("{cajaId}/sesion-activa")]
        public async Task<IActionResult> ObtenerSesionActiva(int cajaId)
        {
            var sesion = await _cajaSesionBussines.ObtenerSesionActivaAsync(cajaId);
            if (sesion is null)
                return NotFound(new { mensaje = $"La caja {cajaId} no tiene sesión activa." });

            return Ok(sesion);
        }


        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AperturaCajaRequest request)
        {
            var response = await _cajaSesionBussines.AbrirCajaAsync(request);
            return Ok(response);
        }

        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaRequest request)
        {
            var response = await _cajaSesionBussines.CerrarCajaAsync(request);
            return Ok(response);
        }



        [HttpPost("calcular-vuelto")]
        public IActionResult CalcularVuelto([FromBody] VueltoRequest request)
        {
            var resultado = _cajaSesionBussines.CalcularVuelto(request);
            return Ok(resultado);
        }
    }
        #endregion
}
