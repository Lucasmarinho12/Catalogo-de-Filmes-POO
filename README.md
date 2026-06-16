# 🎬 CineLog — Catálogo de Filmes e Séries

Sistema orientado a objetos em C# para organização pessoal de mídias, com avaliações, listas personalizadas e recomendações.

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

#### Método 1: Abrir diretamente no navegador
1. Localize o arquivo cinelog.html na raiz do projeto
2. Dê um duplo clique no arquivo (abrirá no navegador padrão)
3. Ou clique com o botão direito → "Abrir com" → escolha seu navegador

#### Método 2: Via Live Server (VS Code)
1. Instale a extensão Live Server no VS Code
2. Abra o arquivo cinelog.html
3. Clique em "Go Live" no canto inferior direito
4. O navegador abrirá automaticamente, executando o front-end da aplicação.

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
|E-mail	|lm@cinelog.com|
|Senha|	123456|
|Nome	|Lucas|
|Tipo	|Usuário padrão|

#### Como utilizar

##### Versão Console (C#):

```bash
dotnet run
# Escolha a opção "Entrar / Login"
# Informe o e-mail e a senha conforme a tabela acima
```

##### Versão Web (HTML):

1. Abra o arquivo `cinelog.html` no navegador
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
|**LocalStorage**|Persistência na versão web|
|**Git**	|Controle de versão|

---

## 📐 Arquitetura

```
CineLog/
├── Models/
│   ├── Midia.cs          ← Classe base abstrata (ABSTRAÇÃO + ENCAPSULAMENTO)
│   ├── Filme.cs          ← Herda de Midia (HERANÇA + POLIMORFISMO)
│   ├── Serie.cs          ← Herda de Midia (HERANÇA + POLIMORFISMO)
│   └── Entidades.cs      ← Usuario, Avaliação, ListaPersonalizada, TipoLista
├── Exceptions/
│   └── Excecoes.cs       ← Hierarquia de exceções do domínio
├── Interfaces/
│   └── IRepositorios.cs  ← Contratos (Repository Pattern)
├── Repositories/
│   └── Repositorios.cs   ← Persistência em JSON
├── Services/
│   └── CatalogoService.cs ← Lógica de negócio (Service Layer Pattern)
├── UI/
│   └── ConsoleUI.cs      ← Interface de console (Facade Pattern)
└── Program.cs            ← Entry point + Seed de dados
```

## Fluxo de Dados

```
Usuário → ConsoleUI → CatalogoService → Repositórios → Arquivos JSON
                          ↓
                     Regras de Negócio
                     Validações
                     Cálculo de médias
                     Recomendações
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
A versão web utiliza localStorage com a chave cinelog_v1.

---


## 🔮 Evoluções Previstas

### Fase 2 — Interface Gráfica
- Migrar `ConsoleUI` para **Windows Forms** ou **WPF** (Desktop)
- Alternativa: **Blazor WebAssembly** para interface web
- A camada `Service` não precisará de mudanças (princípio de separação de responsabilidades)

### Fase 3 — Banco de Dados
- Substituir `*RepositorioJson` por `*RepositorioSqlite` ou `*RepositorioSqlServer`
- Usar **Entity Framework Core** com migrations
- As interfaces `IRepositorios.cs` garantem troca sem impacto no `CatalogoService`

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
