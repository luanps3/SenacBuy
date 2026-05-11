using System.Net.Http.Json;
using SenacBuy.Web.Models;

namespace SenacBuy.Web.Services;

/// <summary>
/// Serviço responsável pela comunicação com os endpoints de autenticação da API.
/// Realiza login, registro e upload de foto via HttpClient — sem acesso direto ao DbContext.
/// </summary>
public class AuthApiService
{
    private readonly HttpClient _http;

    public AuthApiService(IHttpClientFactory factory)
        => _http = factory.CreateClient("SenacBuyAPI");

    // ─────────────────────────────────────────────────────────────────────────
    // Login
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Autentica o usuário via POST /api/usuario/login</summary>
    public async Task<LoginResponseViewModel?> LoginAsync(LoginViewModel vm)
    {
        try
        {
            var payload = new { email = vm.Email, senha = vm.Senha };
            var response = await _http.PostAsJsonAsync("/api/usuario/login", payload);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();

            // 401 — credenciais inválidas: retorna objeto com Sucesso=false
            var erro = await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();
            return erro ?? new LoginResponseViewModel { Sucesso = false, Mensagem = "Credenciais inválidas." };
        }
        catch
        {
            return new LoginResponseViewModel { Sucesso = false, Mensagem = "Falha na comunicação com a API." };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Registro
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Registra novo usuário via POST /api/usuario, com upload de foto opcional</summary>
    public async Task<(bool Sucesso, string Mensagem)> RegistrarAsync(
        RegistrarViewModel vm, IFormFile? foto)
    {
        try
        {
            var payload = new { nome = vm.Nome, email = vm.Email, senha = vm.Senha };
            var response = await _http.PostAsJsonAsync("/api/usuario", payload);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, $"Erro ao registrar: {body}");
            }

            // Recuperar o usuário criado para obter o Id e fazer o upload da foto
            if (foto != null && foto.Length > 0)
            {
                var criado = await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();
                if (criado != null && criado.Id > 0)
                    await UploadFotoAsync(criado.Id, foto);
            }

            return (true, "Usuário registrado com sucesso!");
        }
        catch
        {
            return (false, "Falha na comunicação com a API.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Upload de foto de perfil
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Envia foto de perfil via POST /api/upload/usuarios/{id}</summary>
    public async Task<string?> UploadFotoAsync(int userId, IFormFile foto)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var stream  = foto.OpenReadStream();
            var fileContent   = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(foto.ContentType);
            content.Add(fileContent, "foto", foto.FileName);

            var response = await _http.PostAsync($"/api/upload/usuarios/{userId}", content);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return result?.TryGetValue("caminho", out var caminho) == true ? caminho : null;
        }
        catch { return null; }
    }
}
