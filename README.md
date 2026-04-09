# 🛒 SenacBuy — Sistema de Controle de Vendas

> Projeto Integrador Base | Curso Técnico SENAC | .NET 8 | Arquitetura em Camadas

---

## 📐 1. Descrição Arquitetural Detalhada

### Responsabilidade de cada camada

| Camada | Projeto | Responsabilidade |
|--------|---------|-----------------|
| **Domain** | `SenacBuy.Domain` | Entidades de negócio + contratos (interfaces) |
| **Application** | `SenacBuy.Application` | Regras de negócio, validações, conversão DTO↔Entidade |
| **Infrastructure** | `SenacBuy.Infrastructure` | Acesso a dados: EF Core, SqlServer, Migrations |
| **API** | `SenacBuy.API` | Endpoints REST, DI, Swagger |
| **UI** | `SenacBuy.UI` | Interface desktop WinForms (consome API via HTTP) |
| **Web** | `SenacBuy.Web` | Interface web MVC (consome API via HTTP) |

### Por que a UI não acessa diretamente o banco?

A UI acessa a **API** via HTTP, não o banco diretamente. Isso traz os benefícios:
1. **Segurança**: string de conexão fica só no servidor (API), não no cliente
2. **Escalabilidade**: múltiplos clientes (WinForms, Web, Mobile) compartilham a mesma API
3. **Manutenção**: regras de negócio ficam centralizadas na API, não replicadas nos clientes

### Por que Domain não depende de ninguém?

O Domain contém as **regras essenciais de negócio** — aquelas que são verdadeiras independente de qual banco usamos, qual framework usamos, ou como o usuário interage.

Se o Domain dependesse do EF Core, trocar o ORM exigiria alterar a camada de negócio — o que não faz sentido.

---

## 🔄 2. Fluxo Interno da Aplicação

### O que acontece ao clicar em "Entrar" no FormLogin?

```
[Usuário clica no botão "Entrar"]
        ↓
[WinForms - FormLogin.btnEntrar_Click()]
  - Valida se campos estão preenchidos
  - Monta objeto: { Email = "...", Senha = "..." }
        ↓
[HttpClient.PostAsJsonAsync("/api/Usuario/login", dados)]
  - Serializa o objeto em JSON: {"email":"x","senha":"y"}
  - Abre conexão TCP com localhost:5000
  - Envia HTTP POST com o JSON no corpo
        ↓
[ASP.NET Core - Middleware Pipeline]
  - Routing identifica: POST /api/Usuario/login → UsuarioController.Login()
        ↓
[UsuarioController.Login(LoginDto dto)]
  - Recebe e desserializa o JSON em LoginDto
  - Chama: await _usuarioService.AutenticarAsync(dto)
        ↓
[UsuarioService.AutenticarAsync(dto)]
  - Chama: _usuarioRepository.ObterPorEmailAsync(dto.Email)
        ↓
[UsuarioRepository.ObterPorEmailAsync(email)]
  - Executa: LINQ → _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email)
        ↓
[EF Core → SQL Server]
  - Gera SQL: SELECT TOP 1 * FROM Usuarios WHERE Email = @email
  - Executa no banco, retorna a linha
        ↓
[Retorno sobe pela cadeia]
  Repository → Service (valida hash SHA256 da senha)
  Service → Controller (retorna LoginResponseDto)
  Controller → HTTP 200 OK + JSON { sucesso: true, nome: "João" }
        ↓
[WinForms recebe HTTP 200]
  - Desserializa resposta em LoginResponse
  - Cria: new FormPrincipal(loginResponse.Nome, loginResponse.Id)
  - formPrincipal.Show()
  - this.Hide()
        ↓
[FormPrincipal é exibido com o nome do usuário]
```

### Como o FormPrincipal carrega os UserControls?

```csharp
// Quando o usuário clica em "Clientes" no menu:
private void menuClientes_Click(object sender, EventArgs e)
{
    LoadUserControl(new ClienteUserControl());
}

// LoadUserControl - o coração da navegação:
public void LoadUserControl(UserControl control)
{
    panelContainer.Controls.Clear(); // 1. Remove o UserControl anterior
    control.Dock = DockStyle.Fill;   // 2. Configura para ocupar 100% do espaço
    panelContainer.Controls.Add(control); // 3. Adiciona o novo UserControl
}
```

---

## 🚀 3. Guia Completo de Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado
- SQL Server (LocalDB já vem com o Visual Studio) ou SQL Server Express
- Visual Studio 2022 ou VS Code com extensões C#

### Passo 1: Configurar a string de conexão

Abrir `SenacBuy.API/appsettings.json` e ajustar se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SenacBuyDb;Trusted_Connection=True;"
  }
}
```

> **SQL Server Express:** trocar `(localdb)\\MSSQLLocalDB` por `localhost\\SQLEXPRESS`

### Passo 2: Restaurar pacotes

```bash
dotnet restore
```

### Passo 3: Aplicar Migrations (criar o banco)

```bash
dotnet ef migrations add InitialCreate \
  --project SenacBuy.Infrastructure \
  --startup-project SenacBuy.API

dotnet ef database update --project SenacBuy.Infrastructure --startup-project SenacBuy.API
```

**OU pelo Package Manager Console (Visual Studio):**
```
Add-Migration InitialCreate -Project SenacBuy.Infrastructure -StartupProject SenacBuy.API
Update-Database -Project SenacBuy.Infrastructure -StartupProject SenacBuy.API
```

### Passo 4: Ordem de execução dos projetos

```
① SenacBuy.API    → dotnet run --project SenacBuy.API
                    Aguarde: "Now listening on: http://localhost:5000"

② SenacBuy.UI    → Iniciar pelo Visual Studio (F5) ou dotnet run --project SenacBuy.UI

③ SenacBuy.Web   → dotnet run --project SenacBuy.Web
```

### Passo 5: Testar o Swagger

Abrir no navegador: **http://localhost:5000**

O Swagger UI exibe todos os endpoints disponíveis para teste interativo.

### Como validar que tudo está funcionando

1. Swagger carrega sem erros ✅
2. `GET /api/Cliente` retorna `[]` (lista vazia inicialmente) ✅
3. FormLogin abre ao iniciar o SenacBuy.UI ✅

---

## 🧪 4. Guia de Teste Manual

### 1. Criar um usuário (via Swagger)

```
POST /api/Usuario
Body:
{
  "nome": "João Silva",
  "email": "joao@senac.br",
  "senha": "123456"
}
```

Resposta esperada: `201 Created`

### 2. Fazer Login (WinForms)

- Abrir SenacBuy.UI
- Informar: `joao@senac.br` / `123456`
- Clicar em **Entrar**
- Resultado esperado: FormPrincipal abre com "👤 João Silva"

### 3. Cadastrar um Cliente

Via WinForms → menu "👥 Clientes":
- Nome: `Maria Santos`
- CPF: `123.456.789-00`
- Clicar **Salvar**

Via Swagger:
```
POST /api/Cliente
{ "nome": "Maria Santos", "cpf": "123.456.789-00" }
```

### 4. Cadastrar um Produto

Via WinForms → menu "📦 Produtos":
- Nome: `Notebook Dell`
- Preço: `3500.00`
- Clicar **Salvar**

### 5. Confirmar persistência no banco

- Fechar e reabrir a aplicação
- Verificar se os dados criados ainda aparecem na lista

---

## ⚠️ 5. Problemas Frequentes e Soluções

| Problema | Causa | Solução |
|---|---|---|
| `HttpRequestException` no login | API não está rodando | Inicie `SenacBuy.API` antes da UI |
| Banco `SenacBuyDb` não existe | Migration não aplicada | Execute `Update-Database` |
| `Conflict 409` ao salvar cliente | CPF já cadastrado | Use um CPF diferente |
| `Bad Request 400` ao salvar produto | Preço negativo | Insira um preço ≥ 0 |
| Swagger não abre | API inicia com erro | Verifique o console da API por erros |
| `Unauthorized 401` no login | Email ou senha incorretos | Verifique as credenciais |

---

## 📁 Estrutura de Arquivos

```
SenacBuy/
├── SenacBuy.sln
├── roadmap.md
├── README.md
│
├── SenacBuy.Domain/
│   ├── Entities/
│   │   ├── Usuario.cs
│   │   ├── Cliente.cs
│   │   ├── Produto.cs
│   │   ├── Pedido.cs
│   │   └── ItemPedido.cs
│   └── Interfaces/
│       ├── IUsuarioRepository.cs
│       ├── IClienteRepository.cs
│       ├── IProdutoRepository.cs
│       └── IPedidoRepository.cs
│
├── SenacBuy.Infrastructure/
│   ├── Data/
│   │   └── SenacBuyDbContext.cs
│   └── Repositories/
│       ├── UsuarioRepository.cs
│       ├── ClienteRepository.cs
│       ├── ProdutoRepository.cs
│       └── PedidoRepository.cs
│
├── SenacBuy.Application/
│   ├── DTOs/
│   │   ├── UsuarioDto.cs
│   │   ├── ClienteDto.cs
│   │   ├── ProdutoDto.cs
│   │   └── PedidoDto.cs
│   └── Services/
│       ├── UsuarioService.cs
│       ├── ClienteService.cs
│       ├── ProdutoService.cs
│       └── PedidoService.cs
│
├── SenacBuy.API/
│   ├── Controllers/
│   │   ├── UsuarioController.cs
│   │   ├── ClienteController.cs
│   │   ├── ProdutoController.cs
│   │   └── PedidoController.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── SenacBuy.UI/
│   ├── FormLogin.cs / .Designer.cs
│   ├── FormPrincipal.cs / .Designer.cs
│   ├── ClienteUserControl.cs / .Designer.cs
│   ├── ProdutoUserControl.cs / .Designer.cs
│   └── Program.cs
│
└── SenacBuy.Web/
    ├── Controllers/
    │   ├── HomeController.cs
    │   ├── ClienteController.cs
    │   └── ProdutoController.cs
    ├── Views/
    │   ├── Cliente/Index.cshtml
    │   └── Produto/Index.cshtml
    └── Program.cs
```
