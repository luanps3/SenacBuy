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
   
        }

        // Configura colunas do DataGridView e tamanho das linhas
       
    }
}
