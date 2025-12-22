using CarslineApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CarslineApp.Services
{
    public partial class ApiService
    {
        public async Task<AuthResponse> AsignarTecnicoAsync(int trabajoId, int tecnicoId, int jefeId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Put,
                    $"{BaseUrl}/Trabajos/{trabajoId}/asignar-tecnico/{tecnicoId}")
                {
                    Content = null
                };
                httpRequest.Headers.Add("X-User-Id", jefeId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    return result ?? new AuthResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> ReasignarTecnicoAsync(int trabajoId, int nuevoTecnicoId, int jefeId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Put,
                    $"{BaseUrl}/Trabajos/{trabajoId}/reasignar-tecnico/{nuevoTecnicoId}")
                {
                    Content = null
                };
                httpRequest.Headers.Add("X-User-Id", jefeId.ToString());

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    return result ?? new AuthResponse
                    {
                        Success = false,
                        Message = "Error al procesar la respuesta"
                    };
                }

                return new AuthResponse
                {
                    Success = false,
                    Message = "Error en la solicitud"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<MisTrabajosResponseDto?> ObtenerMisTrabajosAsync(
           int tecnicoId,
           int? estadoFiltro = null)
        {
            try
            {
                var url = $"{BaseUrl}/Trabajos/mis-trabajos/{tecnicoId}";

                // Agregar filtro si viene
                if (estadoFiltro.HasValue)
                    url += $"?estadoFiltro={estadoFiltro.Value}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<MisTrabajosResponseDto>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error ObtenerMisTrabajosAsync: {ex.Message}");
                return null;
            }
        }
    }

}