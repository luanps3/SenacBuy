namespace SenacBuy.UI
{
    public partial class frmLogin : Form
    {
         
        public frmLogin()
        {
            InitializeComponent();
        }

        private async Task btnEntrar_Click(object sender, EventArgs e)
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
                var resultado = 0; 
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
