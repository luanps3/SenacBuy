using SenacBuy.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SenacBuy.UI.Services
{
    public class UsuarioApiService
    {
        private readonly HttpClient _http = ApiClientService.Cliente;

        public async Task<LoginResponseDto?> LoginAsync(string email, string senha)
        {
            try
            {
                var payload = new LoginDto { Email = email, Senha = senha };
                var response = await _http.PostAsJsonAsync("/api/usuario/login", payload);

                var resultado = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                return resultado;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Não foi possivel conectar à API. \nVerifique se a API está rodando em {ApiClientService.ApiBaseUrl}\n\nDetalhes: {ex.Message}","Erro de conexão",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


    }
}
