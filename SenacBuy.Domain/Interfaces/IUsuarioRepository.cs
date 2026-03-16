using SenacBuy.Domain.Entities;

namespace SenacBuy.Domain.Interfaces;

/// <summary>
/// Interface que define o contrato de acesso a dados para a entidade Usuario.
/// O Domain define O QUE fazer, a Infrastructure define COMO fazer.
/// Isso garante que a camada de negócio não dependa de banco de dados.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> ListarTodosAsync();
    Task AdicionarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
    Task RemoverAsync(int id);
}
