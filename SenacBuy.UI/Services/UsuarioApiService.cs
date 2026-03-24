using Microsoft.VisualBasic;
using SenacBuy.UI.Services.Models;
using System.Net.Http.Json;
using System.Text.Json;


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
                var response = await _http.PostAsJsonAsync("api/usuario/login", payload);

                var resultado = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                return resultado;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Não foi possivel conectar à API. \nVerifique se a API está rodando em {ApiClientService.ApiBaseUrl}\n\nDetalhes: {ex.Message}","Erro de conexão",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado no login: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public async Task<UsuarioDto?> CadastrarUsuarioAsync(string nome, string email, string senha, string? fotoPerfil = null)
        {
            try
            {
                var payload = new CriarUsuarioDto {Nome = nome, Email = email, Senha = senha , FotoPerfil = fotoPerfil};
                var response = await _http.PostAsJsonAsync("api/usuario", payload);

                if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<UsuarioDto>();

                //409 Conflito: email já cadastrado
                var erro = await response.Content.ReadAsStringAsync();
                var msg = ExtrairMensagemErro(erro);
                MessageBox.Show(msg,"Erro ao cadastrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Sem conexão com a API: {ex.Message}", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }





        private static string ExtrairMensagemErro(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("mensagem", out var m))
                    return m.GetString() ?? json; // Se não tiver "mensagem", retorna o JSON inteiro como fallback
            }
            catch{/* retorna o texto bruto */}
                return json; // Se não for um JSON válido, retorna o texto original
        }


    }
}
