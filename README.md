# 🎬 CineLog — Catálogo de Filmes e Séries

Sistema orientado a objetos em C# para organização pessoal de mídias, com avaliações, listas personalizadas, recomendações e API REST.

---

## 🚀 Como Executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Navegador web moderno (Chrome, Firefox, Edge, etc.) para a versão web

### Execução via Console (VS Code)
```bash
cd CineLog
dotnet run
```

### Execução da Versão Web
A versão web é um arquivo **HTML único** (`cinelog.html`) que contém todo o CSS e JavaScript embutidos. Ela utiliza `localStorage` para persistência, compartilhando o mesmo modelo de dados da versão C#.

Acesse **http://localhost:5000** — o próprio ASP.NET Core serve o `index.html` (pasta `wwwroot`) e a API ao mesmo tempo, então não há problema de CORS mesmo se você preferir abrir por outra porta (CORS já está liberado no `Program.cs`).

Login de demonstração: `demo@cinelog.com` / `Demo@123`

### Conferir a API isoladamente
```
GET  http://localhost:5000/api/midias
GET  http://localhost:5000/api/midias/top
GET  http://localhost:5000/api/midias/estatisticas
POST http://localhost:5000/api/auth/login        { "email": "...", "senha": "..." }
```

---

### 🧪 Credenciais para Teste
O sistema já vem com dois usuários pré-cadastrados para facilitar os testes. Utilize as credenciais abaixo:

#### Usuário Administrador
| Campo	| Valor |
|--------|------|
| E-mail	| admin@cinelog.com |
| Senha	| Admin@123 |
| Nome	| Admin Demo |
| Tipo	| Administrador (acesso total) |

#### Usuário Comum
|Campo	|Valor|
|--------|------|
|E-mail	|demo@cinelog.com|
|Senha|	Demo@123|
|Nome	|Usuário Demo|
|Tipo	|Usuário padrão|

#### Como utilizar

##### Versão Console (C#):

```bash
dotnet run
# Escolha a opção "Entrar / Login"
# Informe o e-mail e a senha conforme a tabela acima
```

##### Versão Web (HTML):

1. Acesse [http://localhost:5000](http://localhost:5000)
2. Clique em **"Entrar"**
3. Preencha e-mail e senha
4. Clique em **"Entrar"**

---

## Tecnologias Utilizadas

|Tecnologia	|Finalidade|
|--------|------|
|**C# (.NET 8)**|	Linguagem principal do sistema|
|**System.Text.Json**	|Serialização/desserialização JSON|
|**HTML5 / CSS3 / JavaScript**|	Interface web complementar|
| **ASP.NET Core** | API REST e servidor web |
| **fetch() / AJAX** | Comunicação frontend ↔ backend |
|**Git**	|Controle de versão|

---

## 📐 Arquitetura

```
CineLog/
├── Models/Models.cs          ← Midia (abstrata), Filme, Serie, Usuario, Avaliacao
├── Exceptions/Excecoes.cs    ← Hierarquia de exceções do domínio
├── Interfaces/IRepositorios.cs
├── Repositories/Repositorios.cs   ← Persistência em JSON (escrita atômica + lock)
├── Services/CatalogoService.cs    ← Regras de negócio (única fonte da verdade)
├── DTOs/DTOs.cs               ← Contratos de entrada/saída da API
├── Controllers/
│   ├── AuthController.cs      ← /api/auth/login, /api/auth/registrar
│   └── MidiasController.cs    ← /api/midias/*, /api/usuarios/{id}/listas/*
├── wwwroot/index.html         ← Frontend completo (servido pela própria API)
├── Program.cs                  ← Configuração ASP.NET Core + seed de dados
└── data/                       ← Gerado em runtime: midias.json, usuarios.json, avaliacoes.json
```

## Fluxo de Dados

```
┌─────────────────────┐     HTTP/REST (fetch)       ┌──────────────────────┐
│   Navegador (HTML)  │ ─────────────────────────►  │   ASP.NET Core API   │
│  wwwroot/index.html │ ◄─────────────────────────  │   Controllers        │
└─────────────────────┘     JSON                    └───────────┬──────────┘
                                                                │
                                                      CatalogoService (regras)
                                                                │
                                                      Repositórios (persistência)
                                                                │
                                                                ▼
                                                      data/*.json (disco)
```
---

## 🏛️ Os 4 Pilares da POO Aplicados

### 1. Abstração
- `Midia` é uma classe **abstrata** que define o contrato comum para Filme e Série
- Métodos abstratos `ObterTipo()` e `ObterDetalhes()` forçam cada subclasse a definir sua apresentação
- Interfaces `IMidiaRepositorio`, `IUsuarioRepositorio`, `IAvaliacaoRepositorio` abstraem o acesso a dados

### 2. Encapsulamento
- Campos privados com propriedades somente-leitura (`IReadOnlyList`)
- Listas internas (`_avaliacoes`, `_queroAssistir`) acessíveis apenas via métodos públicos controlados
- Validações centralizadas no construtor (fail-fast)
- Senha armazenada como hash, nunca em texto puro

### 3. Herança
- `Filme` e `Serie` herdam de `Midia` e adicionam atributos específicos
- Hierarquia de exceções: `CineLogException` → `DadosInvalidosException`, `AvaliacaoDuplicadaException`, etc.

### 4. Polimorfismo
- `ObterTipo()` retorna `"Filme"` ou `"Série"` dependendo do tipo real
- `ObterDetalhes()` exibe campos específicos de cada subtipo
- `CatalogoService` trabalha com `Midia` (tipo base), sem saber o tipo concreto

---

## 🎨 Padrões de Projeto Aplicados

| Padrão | Onde | Descrição |
|--------|------|-----------|
| **Repository** | `IRepositorios.cs` / `Repositorios.cs` | Abstrai a camada de dados. Hoje: JSON. Amanhã: SQLite/SQL Server sem mudar o Service |
| **Service Layer** | `CatalogoService.cs` | Concentra toda a lógica de negócio em um único ponto |
| **Facade** | `ConsoleUI.cs` | Simplifica a interação com o sistema complexo via console |
| **Dependency Injection** | `Program.cs` | Repositórios injetados no Service via construtor |
| **Value Object** | `Avaliacao`, `ListaPersonalizada` | Objetos imutáveis definidos pelos seus atributos |
| **DTO (Data Transfer Object)** | DTOs/DTOs.cs | Isola o modelo de domínio do contrato JSON exposto pela API, evitando vazar detalhes internos (como hash de senha) |
---

## ✅ Regras de Negócio Implementadas

- ✅ Usuário não pode avaliar a mesma mídia mais de uma vez (`AvaliacaoDuplicadaException`)
- ✅ Nota deve estar entre 0 e 10
- ✅ Média recalculada automaticamente a cada nova avaliação
- ✅ Ao marcar como "Assistido", remove automaticamente de "Quero Assistir"
- ✅ E-mail único por usuário (`EmailDuplicadoException`)
- ✅ Senha com mínimo de 6 caracteres, armazenada como hash

---

## 💾 Persistência de Dados

Os dados são salvos automaticamente em arquivos JSON na pasta `data/`:
- `data/midias.json` — filmes e séries com suas avaliações embutidas
- `data/usuarios.json` — usuários com listas personalizadas
- `data/avaliacoes.json` — histórico de avaliações
A versão web utiliza a API REST para todas as operações. O backend persiste os dados em arquivos JSON na pasta `data/` com escrita atômica (`File.Move`). A versão console e a versão web compartilham **o mesmo banco de dados** no servidor.

---

## 🔮 Evoluções Previstas

### Fase 2 — Interface Gráfica
- Migrar `ConsoleUI` para **Windows Forms** ou **WPF** (Desktop)
- Alternativa: **Blazor WebAssembly** para interface web
- A camada `Service` não precisará de mudanças (princípio de separação de responsabilidades)

### Fase 3 — Banco de Dados
A troca de JSON para SQL é restrita à camada `Repositories/`:
```csharp
// Troca no Program.cs:
builder.Services.AddSingleton<IMidiaRepositorio>(_ => new MidiaRepositorioJson(dataPath));
// por:
builder.Services.AddDbContext<CineLogContext>(...);
builder.Services.AddScoped<IMidiaRepositorio, MidiaRepositorioEfCore>();
```
Nada em `CatalogoService`, Controllers ou no `index.html` precisa mudar.

### Fase 4 — Extensões
- Integração com [TMDB API](https://www.themoviedb.org/documentation/api) para buscar metadados
- Exportação para PDF/Excel
- Sistema de seguir outros usuários
- Notificações de novos episódios

---

## 📋 Tratamento de Exceções

Todas as exceções do domínio herdam de `CineLogException`, permitindo:
```csharp
try { service.AvaliarMidia(...); }
catch (AvaliacaoDuplicadaException ex) { /* feedback específico */ }
catch (CineLogException ex)            { /* qualquer erro do domínio */ }
catch (Exception ex)                   { /* erro inesperado */ }
```

---

## Grupo Desenvolvedor
|Nome|	Responsabilidade|
|--------|------|
|Lucas Marinho Blon Rocha	|Apresentação do Sistema, Repositório GitHub|
|Fábio Sérgio Carvalho Mendonça	|Apresentação do Sistema, Repositório GitHub|
|Guilherme Cândido Vidulino	|Modelagem do Sistema|
|Eduardo Niquini Santiago	|Modelagem do Sistema|
|Matheus Souto dos Santos	|Aplicação dos Quatro Pilares da POO|
|Heitor Freitas Fernandes	|Decisões Iniciais|

---

## Licença

Este projeto foi desenvolvido para fins acadêmicos na disciplina de Programação Orientada a Objetos.

---
