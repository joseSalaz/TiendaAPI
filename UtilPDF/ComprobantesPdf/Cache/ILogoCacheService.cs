using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilPDF.ComprobantesPdf.Cache
{
    public interface ILogoCacheService
    {
        Task<byte[]> GetLogoBytesAsync(string? logoUrl);
    }
}
