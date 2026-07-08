using Models.ApisPeru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IService.ConsultaDNI_RUC
{
    public interface IApisPeruConsultaDocumentoService
    {
        ApisPeruPersonaResponse PersonaPorDNI(string dni);
        ApisPeruEmpresaResponse EmpresaPorRUC(string dni);
    }
}
