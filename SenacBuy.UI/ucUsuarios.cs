// Importações usadas neste UserControl de gerenciamento de usuários
using System; // Tipos básicos e conversões
using System.Collections.Generic; // List<T>
using System.Linq; // Extensões LINQ (Where, ToList, FirstOrDefault)
using System.Threading.Tasks; // Task e async/await
using System.Windows.Forms; // Controles Windows Forms
using SenacBuy.UI.Services; // Serviço que consome a API de usuários
using SenacBuy.UI.Services.Models; // Modelos/DTOs usados pela UI

namespace SenacBuy.UI
{
    // UserControl que lista usuários, permite pesquisar, criar, editar e excluir
    public partial class ucUsuarios : UserControl
    {
        // Serviço que faz chamadas HTTP para os endpoints de usuário
        private readonly UsuarioApiService _usuarioService = new();

        // Cache local da lista de usuários (evita chamadas repetidas à API ao filtrar)
        private List<UsuarioDto> _usuarios = new();

        // Construtor do UserControl
        public ucUsuarios()
        {
            InitializeComponent(); // Inicializa componentes gerados pelo Designer
            ConfigurarInterface(); // Configura colunas do DataGridView e tamanho das linhas
            Load += async (s, e) => await CarregarUsuariosAsync(); // Carrega usuários ao carregar o controle
        }

        // Configura colunas do DataGridView e tamanho das linhas
        private void ConfigurarInterface()
        {
            // Aumenta altura da linha para acomodar imagens de perfil
            dgvUsuarios.RowTemplate.Height = 50;

            // Adiciona coluna de imagem para foto do usuário
            dgvUsuarios.Columns.Add(new DataGridViewImageColumn
            {
                Name = "colFoto",
                HeaderText = "Foto",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                FillWeight = 40
            });

            // Colunas adicionais: ID, Nome, E-mail
            dgvUsuarios.Columns.Add("colId", "ID");
            dgvUsuarios.Columns.Add("colNome", "Nome");
            dgvUsuarios.Columns.Add("colEmail", "E-mail");

            // Ajusta preenchimento proporcional de colunas para melhor visualização
            dgvUsuarios.Columns["colId"]!.FillWeight = 30;
            dgvUsuarios.Columns["colNome"]!.FillWeight = 200;
            dgvUsuarios.Columns["colEmail"]!.FillWeight = 200;
        }

        public async Task CarregarUsuariosAsync(string filtro = "")
        {
            if (_usuarios.Count == 0 || string.IsNullOrEmpty(filtro))
                _usuarios = await _usuarioService.ListarUsuariosAsync();

            AtualizarGrid(_usuarios, filtro);

        }

        private void AtualizarGrid(List<UsuarioDto> lista, string filtro = "")
        {
            dgvUsuarios.Rows.Clear();

            var exibidos = string.IsNullOrWhiteSpace(filtro) ? lista : lista.Where(
                usuario =>
                usuario.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                usuario.Email.Contains(filtro, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var usuario in exibidos)
            {
                int rowIndex = dgvUsuarios.Rows.Add(null, usuario.Id, usuario.Nome, usuario.Email);
                _ = CarregarImagemAsync(rowIndex, usuario.FotoPerfil); // Carrega imagem de forma assíncrona para não travar a UI e ignora erros de carregamento (ex: sem imagem, erro de rede)
            }

        }



        private void txtBuscaUsuario_TextChanged(object sender, EventArgs e)
        {
            AtualizarGrid(_usuarios, txtBuscaUsuario.Text);
        }

        private async Task CarregarImagemAsync(int rowIndex, string? caminhoRelativo)
        {
            if (string.IsNullOrEmpty(caminhoRelativo)) return; // Sem imagem, sai
            try
            {
                // Constrói URL para a imagem a partir do baseUrl configurado
                var url = $"{ApiClientService.ApiBaseUrl.TrimEnd('/')}/api/imagens/{caminhoRelativo}";
                using var stream = await ApiClientService.Cliente.GetStreamAsync(url);
                var img = System.Drawing.Image.FromStream(stream);

                // Se a linha ainda existir, coloca a imagem na célula específica
                if (dgvUsuarios.Rows.Count > rowIndex)
                    dgvUsuarios.Rows[rowIndex].Cells["colFoto"].Value = img;
            }
            catch { /* Ignora erro de carregamento (ex: 404, sem rede) */ }
        }

        private void btnNovoUsuario_Click(object sender, EventArgs e)
        {
            (this.FindForm() as frmPrincipal)?.Navegar(new ucNovoUsuario());
        }

        private void btnEditarUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null)
            {
                MessageBox.Show(
                    "Selecione um usuário para editar",
                    "Atenção",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvUsuarios.CurrentRow.Cells["colId"].Value);
            (this.FindForm() as frmPrincipal)?.Navegar(new ucNovoUsuario(id));

        }

        private async void btnExcluirUsuario_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.CurrentRow == null)
            {
                MessageBox.Show(
              "Selecione um usuário para excluir",
              "Atenção",
              MessageBoxButtons.OK,
              MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvUsuarios.CurrentRow.Cells["colId"].Value);
            string nome = dgvUsuarios.CurrentRow.Cells["colNome"].Value?.ToString() ?? "";

            if (MessageBox.Show($"Excluir usuário \"{nome}\"?\nEsta ação não poderá ser desfeita.", 
                "Confirmar Exclusão", 
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) 
                return;
            bool ok = await _usuarioService.ExcluirUsuarioAsync(id);
            if (ok)
            {
                MessageBox.Show("Usuário excluído com sucesso!", 
                    "Sucesso", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                _usuarios.Clear();
                await CarregarUsuariosAsync();
            }

        }
    }

}
