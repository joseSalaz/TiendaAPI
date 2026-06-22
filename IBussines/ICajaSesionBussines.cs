using Models.RequestResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilInterface;
using UtilPaginados.RequestResponse;

namespace IBussines
{
    public interface ICajaSesionBussines : ICRUDBussnies<CajaSesioneResponse, CajaSesioneRequest>
    {
        Task<CajaSesioneResponse> AbrirCajaAsync(AperturaCajaRequest request);
        Task<CajaSesioneResponse> CerrarCajaAsync(CierreCajaRequest request);
        Task<CajaSesioneResponse?> ObtenerSesionActivaAsync(int cajaId);
        VueltoResponse CalcularVuelto(VueltoRequest request);
    }
}
