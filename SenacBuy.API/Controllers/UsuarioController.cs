using Microsoft.AspNetCore.Mvc;
using SenacBuy.Application.DTOs;
using SenacBuy.Application.Services;

namespace SenacBuy.API.Controllers;

/// <summary>
/// Controller responsável pela autenticação de usuários.
/// Expõe endpoint de Login e CRUD de Usuários.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuarioController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Realiza o login do usuário.
    /// Valida email e senha (hash) e retorna os dados do usuário autenticado.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var resultado = await _usuarioService.AutenticarAsync(loginDto);
            if (!resultado.Sucesso)
                return Unauthorized(resultado);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Lista todos os usuários (sem senhas)</summary>
    [HttpGet]
    public async Task<IActionResult> ListarTodos()
    {
        var usuarios = await _usuarioService.ListarTodosAsync();
        return Ok(usuarios);
    }

    /// <summary>Busca um usuário pelo Id</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);
        if (usuario == null)
            return NotFound(new { mensagem = $"Usuário com Id {id} não encontrado." });

        return Ok(usuario);
    }

    /// <summary>Cria um novo usuário</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarUsuarioDto dto)
    {
        try
        {
            var usuario = await _usuarioService.CriarAsync(dto);
            return CreatedAtAction(nameof(ListarTodos), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    /// <summary>Remove um usuário pelo Id</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remover(int id)
    {
        try
        {
            await _usuarioService.RemoverAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza Nome e Email de um usuário existente.
    /// O ID da rota deve ser igual ao ID no corpo da requisição.
    /// SenhaHash não é alterada por este endpoint.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioDto dto)
    {
        // 400 BadRequest: ID da rota e do body devem ser iguais
        if (id != dto.Id)
            return BadRequest(new { mensagem = "O ID da rota não corresponde ao ID do corpo da requisição." });

        var usuarioAtualizado = await _usuarioService.UpdateAsync(dto);

        // 404 NotFound: usuário não existe no banco
        if (usuarioAtualizado == null)
            return NotFound(new { mensagem = $"Usuário com Id {id} não encontrado." });

        // 200 OK: retorna os dados atualizados
        return Ok(usuarioAtualizado);
    }
}
