# 🗺️ Roadmap — SenacBuy: Sistema de Controle de Vendas

> **Documento educacional** para alunos do curso técnico criarem o projeto do zero.

---

## 1. Visão Geral do Projeto

### Objetivo Didático

O SenacBuy é um projeto-base de **Projeto Integrador** para cursos técnicos. Ele demonstra na prática:
- Separação de responsabilidades em camadas
- Comunicação entre projetos distintos
- CRUD completo com persistência em banco de dados
- Autenticação simples com hash de senha
- Consumo de API REST por uma aplicação desktop e uma aplicação web

### Arquitetura Escolhida: Arquitetura em Camadas (Layered Architecture)

```
┌──────────────────────────────────────┐
│          SenacBuy.UI (WinForms)     │  ← Interface Desktop
│          SenacBuy.Web (MVC)         │  ← Interface Web
└────────────────┬─────────────────────┘
                 ↓ HTTP (REST)
┌────────────────────────────────────────┐
│         SenacBuy.API                  │  ← Exposição dos endpoints
│   (Controllers + DI + Swagger)        │
└────────────┬───────────────────────────┘
             ↓ depende de
┌────────────────────────────────────────┐
│        SenacBuy.Application           │  ← Regras de negócio + DTOs
│         (Services + DTOs)             │
└────────────┬───────────────────────────┘
             ↓ depende de
┌────────────────────────────────────────┐
│          SenacBuy.Domain              │  ← Entidades + Interfaces
│    (Entities + interfaces de repo)    │  ← NÃO depende de ninguém
└────────────────────────────────────────┘
             ↑ implementado por
┌────────────────────────────────────────┐
│       SenacBuy.Infrastructure         │  ← EF Core + SQL Server
│    (DbContext + Repositories)         │  ← Depende apenas de Domain
└────────────────────────────────────────┘
```

### Fluxo Geral da Aplicação

1. **UI (WinForms)** faz requisição HTTP para a **API**
2. **API** recebe e repassa para um **Service** (Application)
3. **Service** aplica as regras de negócio e chama o **Repository**
4. **Repository** (Infrastructure) usa o **DbContext** (EF Core) para acessar o banco
5. O banco retorna os dados, que sobem de volta pela cadeia

---

## 2. Ordem Sequencial de Criação da Solução

### Por que a ordem importa?
> As camadas dependem umas das outras. O Domain não depende de ninguém, portanto é SEMPRE criado primeiro.

```
1. SenacBuy.Domain         ← Base de tudo. Zero dependências.
2. SenacBuy.Infrastructure ← Depende apenas do Domain.
3. SenacBuy.Application    ← Depende apenas do Domain.
4. SenacBuy.API            ← Depende de Application + Infrastructure.
5. SenacBuy.UI             ← Independente (consome a API via HTTP).
6. SenacBuy.Web            ← Independente (consome a API via HTTP).
```

### Comandos CLI para criar a solution

```bash
# Criar pasta do projeto
mkdir SenacBuy
cd SenacBuy

# Criar a solution
dotnet new sln -n SenacBuy

# Criar cada projeto
dotnet new classlib -n SenacBuy.Domain -o SenacBuy.Domain
dotnet new classlib -n SenacBuy.Application -o SenacBuy.Application
dotnet new classlib -n SenacBuy.Infrastructure -o SenacBuy.Infrastructure
dotnet new webapi -n SenacBuy.API -o SenacBuy.API --no-openapi
dotnet new winforms -n SenacBuy.UI -o SenacBuy.UI
dotnet new mvc -n SenacBuy.Web -o SenacBuy.Web

# Adicionar todos à solution
dotnet sln add SenacBuy.Domain/SenacBuy.Domain.csproj
dotnet sln add SenacBuy.Application/SenacBuy.Application.csproj
dotnet sln add SenacBuy.Infrastructure/SenacBuy.Infrastructure.csproj
dotnet sln add SenacBuy.API/SenacBuy.API.csproj
dotnet sln add SenacBuy.UI/SenacBuy.UI.csproj
dotnet sln add SenacBuy.Web/SenacBuy.Web.csproj
```

---

## 3. Referências entre Projetos (com explicação)

| Projeto | Referencia | Motivo |
|---|---|---|
| Application | → Domain | Usa as entidades e interfaces |
| Infrastructure | → Domain | Implementa as interfaces de repositório |
| API | → Application | Usa os Services para lógica de negócio |
| API | → Infrastructure | Registra o DbContext e os repositórios no DI |
| UI | *(nenhuma)* | Consome a API via HTTP |
| Web | *(nenhuma)* | Consome a API via HTTP |

> **Regra de ouro:** Domain nunca referencia nenhum outro projeto. Isso é o que garante a independência.

### Comandos CLI para configurar referências

```bash
dotnet add SenacBuy.Application/SenacBuy.Application.csproj reference SenacBuy.Domain/SenacBuy.Domain.csproj
dotnet add SenacBuy.Infrastructure/SenacBuy.Infrastructure.csproj reference SenacBuy.Domain/SenacBuy.Domain.csproj
dotnet add SenacBuy.API/SenacBuy.API.csproj reference SenacBuy.Application/SenacBuy.Application.csproj SenacBuy.Infrastructure/SenacBuy.Infrastructure.csproj
```

---

## 4. Instalação de Pacotes NuGet

### Infrastructure (EF Core + SQL Server)

```bash
dotnet add SenacBuy.Infrastructure/SenacBuy.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add SenacBuy.Infrastructure/SenacBuy.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools
```

### API (Swagger)

```bash
dotnet add SenacBuy.API/SenacBuy.API.csproj package Swashbuckle.AspNetCore
```

> **Por que o EF Tools vai na Infrastructure e não na API?**
> Porque o DbContext está na Infrastructure. O EF Tools precisa estar no mesmo projeto que o DbContext para as migrations funcionarem.

---

## 5. Implementação por Etapas (Checklist)

### ✅ Etapa 1: SenacBuy.Domain

- [ ] Criar pasta `Entities/`
- [ ] Criar `Entities/Usuario.cs` (Id, Nome, Email, SenhaHash)
- [ ] Criar `Entities/Cliente.cs` (Id, Nome, CPF + navegação para Pedidos)
- [ ] Criar `Entities/Produto.cs` (Id, Nome, Preco)
- [ ] Criar `Entities/Pedido.cs` (Id, ClienteId, DataPedido, Total + método RecalcularTotal)
- [ ] Criar `Entities/ItemPedido.cs` (Id, PedidoId, ProdutoId, Quantidade, PrecoUnitario)
- [ ] Criar pasta `Interfaces/`
- [ ] Criar `Interfaces/IUsuarioRepository.cs`
- [ ] Criar `Interfaces/IClienteRepository.cs`
- [ ] Criar `Interfaces/IProdutoRepository.cs`
- [ ] Criar `Interfaces/IPedidoRepository.cs`
- [ ] Verificar: build do Domain deve passar sem erros (`dotnet build SenacBuy.Domain`)

### ✅ Etapa 2: SenacBuy.Infrastructure

- [ ] Criar pasta `Data/`
- [ ] Criar `Data/SenacBuyDbContext.cs` herdando de `DbContext`
  - Adicionar `DbSet<Usuario>`, `DbSet<Cliente>`, etc.
  - Configurar `OnModelCreating` com relacionamentos e índices únicos
- [ ] Criar pasta `Repositories/`
- [ ] Criar `Repositories/UsuarioRepository.cs` implementando `IUsuarioRepository`
- [ ] Criar `Repositories/ClienteRepository.cs` implementando `IClienteRepository`
- [ ] Criar `Repositories/ProdutoRepository.cs` implementando `IProdutoRepository`
- [ ] Criar `Repositories/PedidoRepository.cs` implementando `IPedidoRepository`
- [ ] Build da Infrastructure deve passar

### ✅ Etapa 3: Configurar Migrations

Após configurar o `DbContext` e a `ConnectionString` no `appsettings.json` da API, execute as migrations. O comando varia conforme o terminal que você estiver usando.

> [!IMPORTANT]
> Os comandos do Entity Framework devem ser executados a partir da **pasta raiz da solution** (onde está o arquivo `.slnx` ou `.sln`), especificando os projetos via parâmetros. Se preferir, você também pode navegar até a pasta do projeto que contém o `DbContext` (`SenacBuy.Infrastructure`) antes de executar — nesse caso, omita o parâmetro `--project`.

---

#### 🖥️ Opção 1 — Console do Gerenciador de Pacotes (Package Manager Console — Visual Studio)

O **Package Manager Console** usa a sintaxe NuGet/EF diretamente, sem o prefixo `dotnet`. Para acessar: menu **Ferramentas → Gerenciador de Pacotes NuGet → Console do Gerenciador de Pacotes**.

```powershell
# Console do Gerenciador de Pacotes:
Add-Migration Inicial -Project SenacBuy.Infrastructure -StartupProject SenacBuy.API

# Console do Gerenciador de Pacotes:
Update-Database -Project SenacBuy.Infrastructure -StartupProject SenacBuy.API
```

---

#### 💻 Opção 2 — PowerShell

O PowerShell usa a **CLI do .NET** (`dotnet ef`). Certifique-se de ter o pacote `dotnet-ef` instalado globalmente (`dotnet tool install --global dotnet-ef`).

```powershell
# PowerShell:
dotnet ef migrations add Inicial --project SenacBuy.Infrastructure --startup-project SenacBuy.API

# PowerShell:
dotnet ef database update --project SenacBuy.Infrastructure --startup-project SenacBuy.API
```

---

#### 🖥️ Opção 3 — Prompt de Comando (CMD)

O Prompt de Comando (CMD) usa exatamente os mesmos comandos `dotnet ef` do PowerShell:

```cmd
REM CMD:
dotnet ef migrations add Inicial --project SenacBuy.Infrastructure --startup-project SenacBuy.API

REM CMD:
dotnet ef database update --project SenacBuy.Infrastructure --startup-project SenacBuy.API
```

---

> **Resumo das diferenças:**
> | Terminal | Sintaxe | Requer |
> |---|---|---|
> | Package Manager Console (VS) | `Add-Migration` / `Update-Database` | Aberto dentro do Visual Studio |
> | PowerShell | `dotnet ef migrations add` / `dotnet ef database update` | `dotnet-ef` tool global |
> | CMD | `dotnet ef migrations add` / `dotnet ef database update` | `dotnet-ef` tool global |

---

**Como verificar se funcionou:**
- Abrir o SQL Server Management Studio (SSMS) ou Azure Data Studio
- Conectar em `(localdb)\MSSQLLocalDB`
- Verificar se o banco `SenacBuyDb` foi criado com as tabelas: `Usuarios`, `Clientes`, `Produtos`, `Pedidos`, `ItensPedido`

### ✅ Etapa 4: SenacBuy.Application

- [ ] Criar pasta `DTOs/`
- [ ] Criar `DTOs/UsuarioDto.cs` (UsuarioDto, CriarUsuarioDto, LoginDto, LoginResponseDto)
- [ ] Criar `DTOs/ClienteDto.cs` (ClienteDto, CriarClienteDto, AtualizarClienteDto)
- [ ] Criar `DTOs/ProdutoDto.cs` (ProdutoDto, CriarProdutoDto, AtualizarProdutoDto)
- [ ] Criar `DTOs/PedidoDto.cs` (PedidoDto, ItemPedidoDto, CriarPedidoDto)
- [ ] Criar pasta `Services/`
- [ ] Criar `Services/UsuarioService.cs` com método `AutenticarAsync` usando SHA256
- [ ] Criar `Services/ClienteService.cs` com validação de CPF único
- [ ] Criar `Services/ProdutoService.cs` com validação de preço não-negativo
- [ ] Criar `Services/PedidoService.cs` com validação de mínimo 1 item e cálculo de Total

### ✅ Etapa 5: SenacBuy.API

- [ ] Criar `Controllers/UsuarioController.cs` (Login + CRUD)
- [ ] Criar `Controllers/ClienteController.cs` (CRUD)
- [ ] Criar `Controllers/ProdutoController.cs` (CRUD)
- [ ] Criar `Controllers/PedidoController.cs` (CRUD)
- [ ] Configurar `Program.cs`:
  - AddDbContext com SQL Server
  - AddScoped para repositórios e services
  - AddCors (permite qualquer origem para desenvolvimento)
  - AddSwaggerGen
- [ ] Testar no Swagger: abrir `http://localhost:5000` e testar cada endpoint

### ✅ Etapa 6: SenacBuy.UI (Windows Forms)

- [ ] Criar `FormLogin.cs` + `FormLogin.Designer.cs`
  - Campos: txtEmail, txtSenha, btnEntrar
  - Evento btnEntrar_Click: PostAsJsonAsync para `/api/Usuario/login`
  - Em caso de sucesso: abrir FormPrincipal
- [ ] Criar `FormPrincipal.cs` + `FormPrincipal.Designer.cs`
  - Header com nome do usuário (lblUsuario)
  - Sidebar com botões de menu
  - `panelContainer` para receber UserControls
  - Método `LoadUserControl(UserControl control)` que limpa e adiciona o controle com Dock=Fill
- [ ] Criar `ClienteUserControl.cs` + Designer
  - DataGridView para listar clientes
  - Campos: txtNome, txtCPF
  - Botões: Novo, Salvar, Excluir
- [ ] Criar `ProdutoUserControl.cs` + Designer
  - Same pattern do ClienteUserControl
- [ ] Alterar `Program.cs` para iniciar com `new FormLogin()`
- [ ] Testar: login, navegação por menu, CRUD de cliente e produto

---

## 6. Fluxo Completo da Aplicação

### Login passo a passo

```
1. Usuário preenche email e senha no FormLogin
2. Clica em "Entrar"
3. WinForms → HttpClient.PostAsJsonAsync("http://localhost:5000/api/Usuario/login", { email, senha })
4. HTTP POST chega no UsuarioController.Login()
5. Controller chama: await _usuarioService.AutenticarAsync(loginDto)
6. UsuarioService:
   a. Chama _usuarioRepository.ObterPorEmailAsync(email)
   b. Repository faz EF Core: SELECT * FROM Usuarios WHERE Email = @email
   c. Compara SHA256(senha_digitada) com usuario.SenhaHash
7. Se correto → retorna LoginResponseDto { Sucesso = true, Nome = "..." }
8. HTTP 200 OK volta para o WinForms
9. WinForms desserializa a resposta e abre FormPrincipal
10. FormPrincipal exibe: "👤 [Nome do Usuário]"
```

### Cadastro de Cliente passo a passo

```
1. No FormPrincipal, usuário clica em "👥 Clientes" no menu lateral
2. FormPrincipal.menuClientes_Click() é chamado
3. Chama: LoadUserControl(new ClienteUserControl())
4. LoadUserControl():
   a. panelContainer.Controls.Clear()  ← remove UserControl anterior
   b. control.Dock = DockStyle.Fill    ← ocupa todo o espaço
   c. panelContainer.Controls.Add(control)
5. ClienteUserControl carrega → chama CarregarClientesAsync()
6. GET http://localhost:5000/api/Cliente → retorna lista
7. Usuário preenche Nome e CPF, clica "Salvar"
8. WinForms → HttpClient.PostAsJsonAsync("/api/Cliente", { nome, cpf })
9. ClienteController.Criar() → ClienteService.CriarAsync()
10. Service valida: CPF não pode ser duplicado
11. Se válido → cria entidade Cliente, chama _clienteRepository.AdicionarAsync()
12. Repository → _context.Clientes.AddAsync(cliente) → _context.SaveChangesAsync()
13. EF Core gera: INSERT INTO Clientes (Nome, CPF) VALUES (@nome, @cpf)
14. HTTP 201 Created retorna para WinForms
15. Lista de clientes é recarregada
```

---

## 7. Erros Comuns e Soluções

### Erro: "A project already has a reference to..."
**Causa:** Tentativa de referência circular (ex: Domain referenciando Application).
**Solução:** O Domain NUNCA pode referenciar outros projetos. Revise o .csproj.

### Erro: Migration não encontrada
**Causa:** Comando executado no projeto errado ou falta do `--startup-project`.
**Solução:**
```bash
dotnet ef migrations add InitialCreate --project SenacBuy.Infrastructure --startup-project SenacBuy.API
```

### Erro: "Cannot open database SenacBuyDb"
**Causa:** Migration não foi aplicada ou string de conexão errada.
**Solução:**
1. Verifique `appsettings.json` da API
2. Execute `dotnet ef database update --project SenacBuy.Infrastructure --startup-project SenacBuy.API`

### Erro: API não inicia - "address already in use"
**Causa:** Porta 5000 em uso por outro processo.
**Solução:** Mude a porta no `launchSettings.json` ou encerre o processo que usa a porta.

### Erro: HttpClient retornando erro no WinForms
**Causa:** API não está rodando quando o WinForms tenta se conectar.
**Solução:**
1. Inicie a API ANTES de iniciar a UI
2. Verifique se a URL no HttpClient coincide com a porta da API (`http://localhost:5000`)

### Erro: "No suitable constructor found for entity type..."
**Causa:** Entidade sem construtor padrão (sem parâmetros).
**Solução:** Adicione um construtor vazio `public Entidade() { }` na entidade.

---

## 8. Checklist Final de Validação

### Login
- [ ] FormLogin abre ao iniciar a aplicação
- [ ] Login com email e senha corretos → abre FormPrincipal
- [ ] Login com credenciais erradas → mensagem de erro, não abre FormPrincipal
- [ ] Campo senha usa máscara (PasswordChar)

### CRUD de Clientes
- [ ] Lista carrega ao abrir ClienteUserControl
- [ ] Cadastro de novo cliente funciona
- [ ] Tentativa de CPF duplicado exibe erro
- [ ] Atualização funciona (selecionar na grid, editar, salvar)
- [ ] Exclusão exibe confirmação e remove o registro

### CRUD de Produtos
- [ ] Lista carrega ao abrir ProdutoUserControl
- [ ] Cadastro de novo produto funciona
- [ ] Tentativa de preço negativo exibe erro
- [ ] Atualização e exclusão funcionam

### Banco de Dados
- [ ] Dados persistem após fechar e reabrir a aplicação
- [ ] Banco `SenacBuyDb` criado com todas as tabelas

### Comunicação entre Camadas
- [ ] API retorna JSON correto para todos os endpoints
- [ ] Swagger acessível em `http://localhost:5000`
- [ ] Web MVC exibe listagem de clientes e produtos
