# 🎬 CineLog — Frontend + Backend C# integrados

Sistema completo: **frontend HTML/CSS/JS** se comunicando em tempo real com uma **API REST em C# (ASP.NET Core)**, com persistência simultânea em arquivos JSON no servidor.

---

## 🔌 Como a comunicação funciona

```
┌─────────────────────┐         fetch() / JSON         ┌──────────────────────┐
│   wwwroot/index.html │ ───────────────────────────►   │   ASP.NET Core API   │
│   (HTML + CSS + JS)  │ ◄───────────────────────────   │   (Controllers)      │
└─────────────────────┘          HTTP REST              └───────────┬──────────┘
                                                                      │
                                                          CatalogoService (regras)
                                                                      │
                                                          Repositórios (IMidiaRepositorio...)
                                                                      │
                                                                      ▼
                                                          data/*.json (disco)
```

- O HTML **não guarda mais dados no `localStorage`**. Toda operação (login, avaliação, cadastro, listas) é uma chamada `fetch` para `http://localhost:5000/api/...`.
- O C# responde em JSON (camelCase) e, **a cada escrita, grava imediatamente** no arquivo correspondente em `data/` — não existe um "salvar depois". A persistência é simultânea à ação do usuário.
- Se a API cair, o indicador no canto superior esquerdo (●) fica vermelho e uma faixa de aviso aparece — o front detecta a perda de conexão automaticamente.

---

## 📂 Estrutura do projeto

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

---

## 🚀 Como executar

### Pré-requisito
[.NET 8 SDK](https://dotnet.microsoft.com/download)

### Rodar
```bash
cd CineLog
dotnet run
```

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

## ✅ O que prova que a integração é real (não simulada)

1. **Sem cópia local de dados** — o JS nunca mantém um array com todas as mídias; cada tela busca da API (`api.listarMidias`, `api.obterMidia`, etc.) toda vez que é exibida.
2. **Persistência simultânea** — abra `data/avaliacoes.json` em um editor enquanto usa o site: a cada avaliação enviada, o arquivo muda no disco imediatamente (escrita atômica via arquivo temporário + `File.Move`).
3. **Validações no servidor, não só no cliente** — tente reenviar a mesma avaliação duas vezes via `curl`: a API retorna `400` com `AvaliacaoDuplicadaException`, independente do que o JS permita.
4. **Múltiplos clientes sincronizados** — abra duas abas, avalie um filme em uma, atualize a outra: a nota nova aparece, porque ambas leem do mesmo backend.
5. **Indicador de conexão ao vivo** — o ponto colorido no canto do logo reflete o resultado real da última chamada `fetch`.

---

## 🏛️ Pilares da POO (mantidos do projeto original)

| Pilar | Onde |
|---|---|
| Abstração | `Midia` é abstrata; `IMidiaRepositorio` define o contrato de persistência |
| Encapsulamento | Listas internas de `Usuario` e `Midia` só mutam via métodos controlados; senha nunca trafega em texto puro armazenado (hash SHA-256) |
| Herança | `Filme`/`Serie` herdam de `Midia`; hierarquia de `CineLogException` |
| Polimorfismo | `ObterTipo()`/`ObterDetalhes()` variam por subtipo; `MidiaResponse.De()` funciona para qualquer `Midia` |

## 🎨 Padrões de projeto

- **Repository** — troque `MidiaRepositorioJson` por uma versão com banco de dados sem tocar no `CatalogoService` nem nos Controllers.
- **Service Layer** — `CatalogoService` é a única porta de entrada para regras de negócio; os Controllers apenas traduzem HTTP ↔ chamadas de método.
- **DTO** — `DTOs.cs` isola o modelo de domínio do contrato JSON exposto, evitando vazar detalhes internos (como o hash de senha) para o cliente.

---

## 🔮 Próxima evolução: banco de dados

A troca de JSON para SQL é restrita à camada `Repositories/`:
```csharp
// Troca apenas isto no Program.cs:
builder.Services.AddSingleton<IMidiaRepositorio>(_ => new MidiaRepositorioJson(dataPath));
// por:
builder.Services.AddDbContext<CineLogContext>(...);
builder.Services.AddScoped<IMidiaRepositorio, MidiaRepositorioEfCore>();
```
Nada em `CatalogoService`, Controllers ou no `index.html` precisa mudar.
