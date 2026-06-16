using System;
using System.Collections.Generic;
using System.Linq;
using CineLog.Exceptions;
using CineLog.Interfaces;
using CineLog.Models;

namespace CineLog.Services
{
    // Padrão Service Layer — concentra a lógica de negócio
    public class CatalogoService
    {
        private readonly IMidiaRepositorio _midias;
        private readonly IUsuarioRepositorio _usuarios;
        private readonly IAvaliacaoRepositorio _avaliacoes;

        // Injeção de Dependência — facilita testes e troca de repositórios
        public CatalogoService(IMidiaRepositorio midias,
                                IUsuarioRepositorio usuarios,
                                IAvaliacaoRepositorio avaliacoes)
        {
            _midias = midias;
            _usuarios = usuarios;
            _avaliacoes = avaliacoes;
            CarregarDados();
        }

        // ─── Mídias ────────────────────────────────────────────────────
        public Filme CadastrarFilme(string titulo, int ano, string genero, string sinopse,
                                    int duracao, string diretor)
        {
            var filme = new Filme(_midias.ProximoId(), titulo, ano, genero, sinopse, duracao, diretor);
            _midias.Adicionar(filme);
            _midias.Salvar();
            return filme;
        }

        public Serie CadastrarSerie(string titulo, int ano, string genero, string sinopse,
                                    int temporadas, int episodios, bool emAndamento, string criador)
        {
            var serie = new Serie(_midias.ProximoId(), titulo, ano, genero, sinopse,
                                  temporadas, episodios, emAndamento, criador);
            _midias.Adicionar(serie);
            _midias.Salvar();
            return serie;
        }

        public Midia ObterMidia(int id) => _midias.ObterPorId(id);

        public IEnumerable<Midia> ListarMidias() => _midias.ObterTodos();

        public IEnumerable<Midia> FiltrarMidias(string? genero = null, int? ano = null,
                                                  double? notaMinima = null)
        {
            var resultado = _midias.ObterTodos();
            if (!string.IsNullOrWhiteSpace(genero))
                resultado = resultado.Where(m =>
                    m.Genero.Contains(genero, StringComparison.OrdinalIgnoreCase));
            if (ano.HasValue)
                resultado = resultado.Where(m => m.AnoLancamento == ano.Value);
            if (notaMinima.HasValue)
                resultado = resultado.Where(m => m.NotaMedia >= notaMinima.Value);
            return resultado;
        }

        public IEnumerable<Midia> ListarFilmes() => _midias.ObterTodos().OfType<Filme>();
        public IEnumerable<Midia> ListarSeries() => _midias.ObterTodos().OfType<Serie>();

        // ─── Usuários ──────────────────────────────────────────────────
        public Usuario CadastrarUsuario(string nome, string email, string senha)
        {
            if (_usuarios.EmailExiste(email))
                throw new EmailDuplicadoException($"E-mail '{email}' já cadastrado.");
            var usuario = new Usuario(_usuarios.ProximoId(), nome, email, senha);
            _usuarios.Adicionar(usuario);
            _usuarios.Salvar();
            return usuario;
        }

        public Usuario Login(string email, string senha)
        {
            try
            {
                var usuario = _usuarios.ObterPorEmail(email);
                if (!usuario.ValidarSenha(senha))
                    throw new AutenticacaoException("Senha incorreta.");
                return usuario;
            }
            catch (EntidadeNaoEncontradaException)
            {
                throw new AutenticacaoException("Usuário não encontrado.");
            }
        }

        // ─── Avaliações ────────────────────────────────────────────────
        public Avaliacao AvaliarMidia(int usuarioId, int midiaId, double nota, string comentario)
        {
            var midia = _midias.ObterPorId(midiaId);
            var usuario = _usuarios.ObterPorId(usuarioId);

            if (!midia.PodeAvaliar(usuarioId))
                throw new AvaliacaoDuplicadaException(
                    $"Usuário '{usuario.Nome}' já avaliou '{midia.Titulo}'.");

            var avaliacao = new Avaliacao(_avaliacoes.ProximoId(), usuarioId, midiaId, nota, comentario);
            midia.AdicionarAvaliacao(avaliacao);
            _avaliacoes.Adicionar(avaliacao);

            _midias.Salvar();
            _avaliacoes.Salvar();
            return avaliacao;
        }

        public IEnumerable<Avaliacao> ObterAvaliacoesDaMidia(int midiaId) =>
            _midias.ObterPorId(midiaId).Avaliacoes;

        // ─── Listas ────────────────────────────────────────────────────
        public void AdicionarNaLista(int usuarioId, int midiaId, TipoLista lista)
        {
            var usuario = _usuarios.ObterPorId(usuarioId);
            _midias.ObterPorId(midiaId); // Valida que a mídia existe
            usuario.AdicionarNaLista(lista, midiaId);
            _usuarios.Salvar();
        }

        public void RemoverDaLista(int usuarioId, int midiaId, TipoLista lista)
        {
            var usuario = _usuarios.ObterPorId(usuarioId);
            usuario.RemoverDaLista(lista, midiaId);
            _usuarios.Salvar();
        }

        public IEnumerable<Midia> ObterListaDoUsuario(int usuarioId, TipoLista lista)
        {
            var usuario = _usuarios.ObterPorId(usuarioId);
            IReadOnlyList<int> ids = lista switch
            {
                TipoLista.QueroAssistir => usuario.QueroAssistir,
                TipoLista.Assistidos => usuario.Assistidos,
                TipoLista.Favoritos => usuario.Favoritos,
                _ => throw new DadosInvalidosException("Lista inválida.")
            };
            return ids.Select(id => _midias.ObterPorId(id));
        }

        // ─── Recomendações (extensão) ──────────────────────────────────
        public IEnumerable<Midia> RecomendarPorGenero(int usuarioId, int quantidade = 5)
        {
            var usuario = _usuarios.ObterPorId(usuarioId);
            var assistidos = usuario.Assistidos.ToHashSet();

            // Analisa gêneros dos favoritos para recomendar similares
            var generosPreferidos = usuario.Favoritos
                .Where(id => _midias.ObterTodos().Any(m => m.Id == id))
                .Select(id => _midias.ObterPorId(id).Genero)
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            if (!generosPreferidos.Any())
                return _midias.ObterTodos()
                    .Where(m => !assistidos.Contains(m.Id))
                    .OrderByDescending(m => m.NotaMedia)
                    .Take(quantidade);

            return _midias.ObterTodos()
                .Where(m => !assistidos.Contains(m.Id) &&
                            generosPreferidos.Any(g =>
                                m.Genero.Contains(g, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(m => m.NotaMedia)
                .Take(quantidade);
        }

        public IEnumerable<Midia> ObterTopMidias(int quantidade = 10) =>
            _midias.ObterTodos()
                .Where(m => m.Avaliacoes.Any())
                .OrderByDescending(m => m.NotaMedia)
                .Take(quantidade);

        private void CarregarDados()
        {
            _midias.Carregar();
            _usuarios.Carregar();
            _avaliacoes.Carregar();
        }
    }
}
