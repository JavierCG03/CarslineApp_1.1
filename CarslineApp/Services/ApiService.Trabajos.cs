using CarslineApp.Models;
using System.Net.Http.Json;

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

    }

}