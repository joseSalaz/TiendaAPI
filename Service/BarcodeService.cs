using IService;
using Models.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service
{
    public class BarcodeService:IBarcodeService
    {
        private readonly HttpClient _httpClient;

        public BarcodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Configurar un User-Agent es obligatorio para Open Food Facts (evita que bloqueen la petición)
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MiSistemaInventario/1.0 (NET Backend)");
        }

        public async Task<ProductoScannedDto> ScanProductoAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return new ProductoScannedDto { Encontrado = false };
            }

            ProductoScannedDto? resultadoDto = null;

            // 1. Intentar primero con Open Food Facts
            var resultOff = await TryOpenFoodFactsAsync(barcode);
            if (resultOff != null && resultOff.Encontrado)
            {
                // Guardamos el resultado en la variable en lugar de retornar inmediatamente
                resultadoDto = resultOff;
            }
            else
            {
                // 2. Fallback: Si no se encuentra en el primero, intentar con UPCitemdb
                var resultUpc = await TryUpcItemDbAsync(barcode);
                if (resultUpc != null && resultUpc.Encontrado)
                {
                    // Guardamos el resultado en la variable
                    resultadoDto = resultUpc;
                }
            }

            // Ahora el flujo SÍ llegará aquí de forma obligatoria si el producto fue encontrado
            if (resultadoDto != null && resultadoDto.Encontrado)
            {
                resultadoDto.Nombre = await TraducirAlEspanolAsync(resultadoDto.Nombre ?? "");
                resultadoDto.Descripcion = await TraducirAlEspanolAsync(resultadoDto.Descripcion ?? "");
                resultadoDto.CategoriaTexto = await TraducirAlEspanolAsync(resultadoDto.CategoriaTexto ?? "");

                return resultadoDto;
            }

            // Si ninguna API encontró nada
            return new ProductoScannedDto
            {
                CodigoBarras = barcode,
                Encontrado = false,
                SourceApi = "Ninguna"
            };
        }
        private async Task<ProductoScannedDto?> TryOpenFoodFactsAsync(string barcode)
        {
            try
            {
                // Usamos el subdominio 'es.' 
                string url = $"https://es.openfoodfacts.org/api/v2/product/{barcode}.json";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var statusProp) && statusProp.GetInt32() == 1)
                {
                    if (root.TryGetProperty("product", out var productProp))
                    {
                        var dto = new ProductoScannedDto
                        {
                            CodigoBarras = barcode,
                            Encontrado = true,
                            SourceApi = "OpenFoodFacts"
                        };

                        if (productProp.TryGetProperty("product_name_es", out var nameEs) && !string.IsNullOrEmpty(nameEs.GetString()))
                            dto.Nombre = nameEs.GetString();
                        else if (productProp.TryGetProperty("product_name", out var nameGen))
                            dto.Nombre = nameGen.GetString();

                        
                        if (productProp.TryGetProperty("brands", out var brandProp) && !string.IsNullOrEmpty(brandProp.GetString()))
                        {
                            dto.Nombre = $"{dto.Nombre} {brandProp.GetString()}".Trim();
                        }

                      
                        if (productProp.TryGetProperty("categories_old", out var catProp))
                            dto.CategoriaTexto = catProp.GetString();
                        else if (productProp.TryGetProperty("categories", out var catGenProp))
                            dto.CategoriaTexto = catGenProp.GetString();

                     
                        if (productProp.TryGetProperty("image_url", out var imgProp))
                            dto.ImagenUrl = imgProp.GetString();

                 
                        if (productProp.TryGetProperty("generic_name_es", out var descEs))
                            dto.Descripcion = descEs.GetString();

                        return dto;
                    }
                }
            }
            catch
            {
                // En producción puedes loguear el error, aquí permitimos que continúe al siguiente fallback
            }

            return null;
        }

        private async Task<ProductoScannedDto?> TryUpcItemDbAsync(string barcode)
        {
            try
            {
                string url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={barcode}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.GetArrayLength() > 0)
                {
                    var firstItem = itemsProp.EnumerateArray().First();

                    var dto = new ProductoScannedDto
                    {
                        CodigoBarras = barcode,
                        Encontrado = true,
                        SourceApi = "UPCitemdb",
                        Nombre = firstItem.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : "",
                        Descripcion = firstItem.TryGetProperty("description", out var descProp) ? descProp.GetString() : "",
                        CategoriaTexto = firstItem.TryGetProperty("category", out var catProp) ? catProp.GetString() : ""
                    };

                    if (firstItem.TryGetProperty("images", out var imagesProp) && imagesProp.GetArrayLength() > 0)
                    {
                        dto.ImagenUrl = imagesProp.EnumerateArray().First().GetString();
                    }

                    return dto;
                }
            }
            catch { }
            return null;
        }

        private async Task<string> TraducirAlEspanolAsync(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return texto;

            try
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=es&dt=t&q={Uri.EscapeDataString(texto)}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return texto;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                var root = doc.RootElement;

                // Google devuelve un array principal. El primer elemento (índice 0) contiene las líneas traducidas.
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var lineasArray = root[0];
                    if (lineasArray.ValueKind == JsonValueKind.Array)
                    {
                        var textoCompletoTraducido = new StringBuilder();

                        // Recorremos todas las partes que Google tradujo (ya que fragmenta el texto largo)
                        foreach (var elemento in lineasArray.EnumerateArray())
                        {
                            if (elemento.ValueKind == JsonValueKind.Array && elemento.GetArrayLength() > 0)
                            {
                                // El índice 0 de cada sub-array es siempre el fragmento de texto traducido
                                var fragmentoTraducido = elemento[0].GetString();
                                if (!string.IsNullOrEmpty(fragmentoTraducido))
                                {
                                    textoCompletoTraducido.Append(fragmentoTraducido);
                                }
                            }
                        }

                        if (textoCompletoTraducido.Length > 0)
                        {
                            return textoCompletoTraducido.ToString();
                        }
                    }
                }
            }
            catch
            {
                // En caso de cualquier error imprevisto, retorna el texto original para no colgar la app
            }

            return texto;
        }
    }
}


