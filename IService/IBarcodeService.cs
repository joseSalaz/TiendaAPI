using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Comon;
namespace IService
{
    public interface IBarcodeService
    {
        Task <ProductoScannedDto> ScanProductoAsync(string barcode);
        
    }
}
