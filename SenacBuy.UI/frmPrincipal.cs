using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SenacBuy.UI
{
    public partial class frmPrincipal : Form
    {
        public frmPrincipal()
        {
            InitializeComponent();
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            SetActiveButton(btnUsuarios);
            LoadUserControl(new ucUsuarios());
        }

        private void SetActiveButton(Guna2Button activeBtn)
        {
            var botoes = new[] { btnDashboard, btnClientes, btnProdutos, btnPedidos, btnUsuarios };
            foreach (var btn in botoes)
            {
                btn.FillColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(160,170,204);
            }
            activeBtn.FillColor = Color.FromArgb(0, 123, 204);
            activeBtn.ForeColor = Color.White;
        }

        private void LoadUserControl(UserControl uc)
        {
           panelContainer.Controls.Clear();
           uc.Dock = DockStyle.Fill;
           panelContainer.Controls.Add(uc);
        }

    }
}
