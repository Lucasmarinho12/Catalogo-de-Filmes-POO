# Catalogo-de-Filmes-POO
Catalogo de Filmes/ POO

# Sistema de Catálogo de Filmes

## 1. Apresentação do Sistema

O **Sistema de Catálogo de Filmes** tem como objetivo permitir o gerenciamento de filmes, séries, avaliações e comentários realizados pelos usuários.

O projeto foi modelado em **C#**, utilizando os princípios da **Programação Orientada a Objetos (POO)**, visando modularidade, reutilização de código e facilidade de manutenção.

### Principais Funcionalidades

* Cadastro de usuários;
* Gerenciamento de filmes e séries;
* Registro de avaliações e comentários;
* Recomendações de conteúdo por gênero;
* Integração com API externa (TMDB);
* Exportação de dados em formato JSON.

---

# 2. Modelagem do Sistema

## Interface IExibivel

### Função

Padronizar a exibição de dados no console.

### Métodos

* `ExibirDetalhes()`

---

## Classe Abstrata Midia

### Função

Centralizar as avaliações e o cálculo da nota média, atuando como classe pai de Filme e Série, armazenando atributos comuns.

### Atributos

* `string Nome`
* `string Sinopse`
* `string AnoLancamento`
* `Genero[] Generos`
* `List<Avaliacao> Avaliacoes`
* `double MediaNota`

### Métodos

* `AdicionarAvaliacao(Avaliacao novaAvaliacao)`
* `RecalcularMedia()`

---

## Classe Filme (Herda de Midia)

### Função

Representar filmes de curta ou longa metragem.

### Atributos

* `int Duracao`

### Métodos

* `ExibirDetalhes()`

---

## Classe Serie (Herda de Midia)

### Função

Representar séries e conteúdos compostos por vários episódios.

### Atributos

* `int Temporadas`
* `List<Episodio> Episodios`

### Métodos

* `ExibirDetalhes()`

---

## Classe Episodio

### Função

Representar episódios individuais de uma série.

### Atributos

* `int Numero`
* `int Temporada`
* `string Titulo`
* `int Duracao`

### Métodos

* `ExibirDetalhes()`

---

## Classe Usuario

### Função

Representar o usuário do sistema.

### Atributos

* `int Id`
* `string Nome`
* `string Email`
* `string Senha`

### Métodos

* `MeusItensAvaliados()`

---

## Classe Avaliacao

### Função

Representar uma avaliação realizada por um usuário em uma mídia.

### Atributos

* `int UsuarioId`
* `double Nota`
* `string Comentario`

---

## Classe ListaPersonalizada

### Função

Agrupar mídias nas listas "Favoritos" e "Assistir Mais Tarde".

### Atributos

* `string Nome`
* `List<Midia> Itens`

### Métodos

* `Adicionar(Midia m)`
* `Remover(Midia m)`

---

## Classe Catalogo

### Função

Gerenciar regras envolvendo usuários e mídias.

### Atributos

* `List<Midia> Midias`
* `List<Usuario> Usuarios`

### Métodos

* `RegistrarAvaliacao(int usuarioId, int midiaId, double nota, string comentario)`

---

## Classe ExportadorDados

### Função

Exportar os dados do sistema para arquivos físicos.

### Atributos

* `string CaminhoArquivo`

### Métodos

* `ExportarParaJson(List<Midia> midias)`

Utiliza a biblioteca `System.Text.Json` para converter os dados em JSON e salvá-los em disco.

---

## Classe Recomendador

### Função

Analisar o perfil do usuário para sugerir novos conteúdos.

### Atributos

* Referência ao `Catalogo`

### Métodos

* `RecomendarPorGenero(Usuario usuario)`

O método identifica os gêneros mais bem avaliados pelo usuário e sugere mídias semelhantes ainda não assistidas.

---

## Classe TmdbApiClient

### Função

Integrar o sistema à API do TMDB para obtenção de dados reais de filmes e séries.

### Atributos

* `string ApiKey`
* `HttpClient client`

### Métodos

* `BuscarMidiaExterna(string titulo)`

Realiza requisições HTTP e retorna objetos preenchidos com informações reais, como nome, sinopse, ano de lançamento e duração.

---

## Diagrama UML do Sistema

> Inserir aqui a imagem do diagrama UML do projeto.

---

# 3. Aplicação dos Quatro Pilares da Programação Orientada a Objetos

## Abstração

A abstração consiste em representar apenas os elementos essenciais do domínio do problema.

### Interface IExibivel

Define o contrato de exibição através do método:

```csharp
ExibirDetalhes()
```

Cada classe implementa sua própria forma de exibição.

### Classe Abstrata Midia

Representa o conceito genérico de conteúdo disponível no catálogo.

Ela concentra atributos comuns a filmes e séries, como:

* Nome;
* Sinopse;
* Ano de lançamento;
* Gêneros;
* Avaliações;
* Nota média.

Dessa forma, evita duplicação de código entre as subclasses.

---

## Encapsulamento

O encapsulamento protege os dados internos dos objetos, permitindo acesso apenas por mecanismos controlados.

### Em Midia e Avaliacao

Atributos são mantidos privados, evitando alterações indevidas.

### Senha do Usuário

O atributo `Senha` permanece privado e não pode ser acessado diretamente por outras classes.

### Lista de Episódios

A lista `Episodios` é controlada pela classe `Serie`, impedindo manipulações inválidas.

---

## Herança

A herança permite reutilização de atributos e comportamentos.

### Hierarquia do Sistema

```text
Midia
├── Filme
└── Serie
```

Filmes e séries herdam:

* Nome;
* Sinopse;
* Ano de lançamento;
* Gêneros;
* Avaliações;
* Cálculo da média de notas.

Cada classe adiciona apenas suas características específicas.

---

## Polimorfismo

O polimorfismo permite que objetos diferentes respondam de maneiras diferentes à mesma chamada de método.

### ExibirDetalhes()

O método é sobrescrito por cada classe:

* Filme
* Serie
* Episodio

Assim, o catálogo pode percorrer uma lista de mídias e chamar:

```csharp
ExibirDetalhes();
```

Sem precisar saber o tipo específico do objeto.

Cada classe exibirá suas próprias informações.

---

# 4. Decisões Iniciais

## Linguagem Utilizada

* C#

## Divisão Inicial das Atividades

| Seção do Trabalho                   | Responsável(s)                                            |
| ----------------------------------- | --------------------------------------------------------- |
| Apresentação do Sistema             | Lucas Marinho Blon Rocha e Fábio Sérgio Carvalho Mendonça |
| Modelagem do Sistema                | Guilherme Cândido Vidulino e Eduardo Niquini Santiago     |
| Aplicação dos Quatro Pilares da POO | Matheus Souto dos Santos                                  |
| Decisões Iniciais                   | Heitor Freitas Fernandes                                  |
| Repositório GitHub                  | Lucas Marinho Blon Rocha e Fábio Sérgio Carvalho Mendonça |
