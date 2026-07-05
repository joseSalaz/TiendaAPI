using Models.ApisPeru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilPDF.ComprobantesPdf.RequestResponse;

namespace UtilPDF.ComprobantesPdf
{
    public interface IComprobantePdfService
    {
        Task<byte[]> GenerarA4Async(ComprobantePdfData data);
        byte[] GenerarTicket80mm(ComprobantePdfData data);
        Task<byte[]> GenerarTicket80mmAsync(ComprobantePdfData data);
    }
}
