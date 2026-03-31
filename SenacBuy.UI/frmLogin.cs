using SenacBuy.UI.Services;

namespace SenacBuy.UI
{
    public partial class frmLogin : Form
    {
        private readonly UsuarioApiService _usuarioApiService = new();

        public frmLogin()
        {
            InitializeComponent();
        }

        private async void btnEntrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                MessageBox.Show("Preencha email e senha.", "Atenção",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnEntrar.Enabled = false;
            btnEntrar.Text = "Entrando...";

            try
            {
                var resultado = await _usuarioApiService.LoginAsync(
                    email: txtEmail.Text.Trim(),
                    senha: txtSenha.Text);

                if (resultado == null)
                {
                    // Mensagem de erro já exibida pelo usuarioApiService
                    return;
                }

                if (resultado.Sucesso)
                {
                    var principal = new frmPrincipal();
                    principal.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show($"Acesso negado. \n{resultado.Mensagem}",
                        "Autenticação falhou",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            finally
            {
                btnEntrar.Enabled = true;
                btnEntrar.Text = "Entrar";
            }


        }

        private void lklCadastrar_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmCadastroUsuario form = new();
            form.ShowDialog();
        }
    }
}
