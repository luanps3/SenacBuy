using SenacBuy.UI.Services;
using SenacBuy.UI.Services.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SenacBuy.UI
{
    public partial class ucNovoUsuario : UserControl
    {

        private readonly UsuarioApiService _usuarioService = new();
        private string? _caminhoFotoLocal;
        private readonly int? _idEdicao;

        public ucNovoUsuario(int? id = null)
        {
            InitializeComponent();
            _idEdicao = id;
            this.Load += async (s, e) => await CarregarEdicaoAsync();
        }

        private async Task CarregarEdicaoAsync()
        {
            if (!_idEdicao.HasValue)
            {
                lblTitulo.Text = "👤  Novo Usuário";
                return;
            }

            lblTitulo.Text = "👤  Editar Usuário";
            btnSalvar.Text = "Atualizar Usuário";
            txtSenha.PlaceholderText = "Deixe em branco para não alterar";

            var usuario = await _usuarioService.GetUsuarioByIdAsync(_idEdicao.Value);
            if (usuario != null)
            {
                txtNome.Text = usuario.Nome;
                txtEmail.Text = usuario.Email;

                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    try
                    {
                        var url = $"{ApiClientService.ApiBaseUrl.TrimEnd('/')}/api/imagens/{usuario.FotoPerfil}";
                        picFoto.LoadAsync(url);
                    }
                    catch { }
                }

            }



        }




        private async void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                btnSalvar.Enabled = false;

                string nome = txtNome.Text.Trim();
                string email = txtEmail.Text.Trim();
                string senha = txtSenha.Text;

                if (!_idEdicao.HasValue && string.IsNullOrEmpty(senha))
                {
                    MessageBox.Show(
                        "Preencha todos os campos obrigatórios(*).",
                        "Atenção",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (!_idEdicao.HasValue && senha.Length < 6)
                {
                    MessageBox.Show(
                        "A senha deve conter pelo menos 6 caracteres.",
                        "Atenção",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string? caminhoFotoApi = null;
                if (!string.IsNullOrEmpty(_caminhoFotoLocal))
                {
                    btnSalvar.Text = "Enviando foto...";
                    caminhoFotoApi = await _usuarioService.UploadFotoAsync(_caminhoFotoLocal);
                    if (caminhoFotoApi == null)
                    {
                        return; // Mensagem de erro já exibida no UploadFotoAsync
                    }
                }

                if (_idEdicao.HasValue)
                {
                    btnSalvar.Text = "Atualizando usuário...";
                    var dto = new UsuarioDto
                    {
                        Id = _idEdicao.Value,
                        Nome = nome,
                        Email = email,
                        FotoPerfil = caminhoFotoApi // Se nova foto for enviada, atualiza; caso contrário, mantém a existente
                    };

                    var ok = await _usuarioService.AtualizarUsuarioAsync(dto);
                    if (ok)
                    {
                        MessageBox.Show("Usuário atualizado com sucesso!",
                            "Sucesso",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                            );
                        VoltarParaLista();
                    }

                }
                else
                {
                    btnSalvar.Text = "Salvando usuário...";

                    var novoDto =
                        await _usuarioService.CadastrarUsuarioAsync(nome, email, senha, caminhoFotoApi);
                    if (novoDto != null)
                    {
                        MessageBox.Show("Usuário cadastrado com sucesso!",
                           "Sucesso",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                           );
                        VoltarParaLista();
                    }
                }
            }
            finally
            {
                btnSalvar.Enabled = true;
                btnSalvar.Text = "Salvar Usuário";
            }
        }

        private void VoltarParaLista()
        {
            var principal = this.FindForm() as frmPrincipal;
            principal?.Navegar(new ucUsuarios());
        }

        private void btnSelecionarImagem_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _caminhoFotoLocal = ofd.FileName;
                picFoto.ImageLocation = _caminhoFotoLocal;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            VoltarParaLista();
        }
    }
}
