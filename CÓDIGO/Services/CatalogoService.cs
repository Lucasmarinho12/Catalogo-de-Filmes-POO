using System;
using System.Collections.Generic;
using System.Linq;
using CineLog.Exceptions;
using CineLog.Interfaces;
using CineLog.Models;

namespace CineLog.Services
{
    // Service Layer: única fonte de regras de negócio. Tanto o front quanto
    // qualquer outro consumidor da API passam por aqui — nunca direto no repositório.
    public class CatalogoService
    {
        private readonly IMidiaRepositorio _midias;
        private readonly IUsuarioRepositorio _usuarios;
        private readonly IAvaliacaoRepositorio _avaliacoes;

        public CatalogoService(IMidiaRepositorio midias, IUsuarioRepositorio usuarios,
                                IAvaliacaoRepositorio avaliacoes)
        {
            _midias = midias;
            _usuarios = usuarios;
            _avaliacoes = avaliacoes;
        }

        // ─── Mídias ────────────────────────────────────────────────
        public Filme CadastrarFilme(string titulo, int ano, string genero, string sinopse,
                                     int duracao, string diretor)
        {
            var filme = new Filme(_midias.ProximoId(), titulo, ano, genero, sinopse, duracao, diretor);
            _midias.Adicionar(filme); // já salva em disco imediatamente
            return filme;
        }

        public Serie CadastrarSerie(string titulo, int ano, string genero, string sinopse,
                                     int temporadas, int episodios, bool emAndamento, string criador)
        {
            var serie = new Serie(_midias.ProximoId(), titulo, ano, genero, sinopse,
                                   temporadas, episodios, emAndamento, criador);
            _midias.Adicionar(serie);
            return serie;
        }

        public Midia ObterMidia(int id) => _midias.ObterPorId(id);
        public IEnumerable<Midia> ListarMidias() => _midias.ObterTodos();

        public IEnumerable<Midia> FiltrarMidias(string? tipo, string? genero, int? ano, double? notaMinima, string? busca)
        {
            var resultado = _midias.ObterTodos();
            if (!string.IsNullOrWhiteSpace(busca))
                resultado = resultado.Where(m =>
                    m.Titulo.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                    m.Genero.Contains(busca, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(tipo))
                resultado = resultado.Where(m => m.ObterTipo().Equals(tipo, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(genero))
                resultado = resultado.Where(m => m.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
            if (ano.HasValue)
                resultado = resultado.Where(m => m.AnoLancamento == ano.Value);
            if (notaMinima.HasValue)
                resultado = resultado.Where(m => m.NotaMedia >= notaMinima.Value);
            return resultado;
        }

        // ─── Usuários ──────────────────────────────────────────────
        public Usuario CadastrarUsuario(string nome, string email, string senha)
        {
            if (_usuarios.EmailExiste(email))
                throw new EmailDuplicadoException($"O e-mail '{email}' já está cadastrado.");
            var usuario = new Usuario(_usuarios.ProximoId(), nome, email, senha);
            _usuarios.Adicionar(usuario);
            return usuario;
        }

        public Usuario Login(string email, string senha)
        {
            Usuario usuario;
            try { usuario = _usuarios.ObterPorEmail(email); }
            catch (EntidadeNaoEncontradaException) { throw new AutenticacaoException("E-mail ou senha incorretos."); }

            if (!usuario.ValidarSenha(senha))
                throw new AutenticacaoException("E-mail ou senha incorretos.");
            return usuario;
        }

        public Usuario ObterUsuario(int id) => _usuarios.ObterPorId(id);

        // ─── Avaliações ────────────────────────────────────────────
        public Avaliacao AvaliarMidia(int usuarioId, int midiaId, double nota, string comentario)
        {
            var midia = _midias.ObterPorId(midiaId);
            _usuarios.ObterPorId(usuarioId); // valida existência

            if (!midia.PodeAvaliar(usuarioId))
                throw new AvaliacaoDuplicadaException("Você já avaliou esta mídia.");

            var avaliacao = new Avaliacao(_avaliacoes.ProximoId(), usuarioId, midiaId, nota, comentario);
            midia.AdicionarAvaliacao(avaliacao); // recalcula NotaMedia em memória
            _avaliacoes.Adicionar(avaliacao);    // persiste avaliação
            _midias.Salvar();                    // persiste estado da mídia (idempotente, não duplica)
            return avaliacao;
        }

        public IEnumerable<Avaliacao> ObterAvaliacoesDaMidia(int midiaId) =>
            _midias.ObterPorId(midiaId).Avaliacoes;

        // ─── Listas pessoais ───────────────────────────────────────
        public void AdicionarNaLista(int usuarioId, int midiaId, TipoLista lista)
        {
            var usuario = _usuarios.ObterPorId(usuarioId);
            _midias.ObterPorId(midiaId); // valida existência
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
            return ids.Select(id => { try { return _midias.ObterPorId(id); } catch { return null; } })
                      .Where(m => m != null)!;
        }

        // ─── Top / Recomendações ─────────────────────────────────────
        public IEnumerable<Midia> ObterTopMidias(int quantidade = 10) =>
            _midias.ObterTodos().Where(m => m.Avaliacoes.Any())
                   .OrderByDescending(m => m.NotaMedia).ThenByDescending(m => m.Avaliacoes.Count)
                   .Take(quantidade);

        public IEnumerable<Midia> RecomendarPorGenero(int usuarioId, int quantidade = 5)
        {
            var usuario = _usuarios.ObterPorId(usuarioId);
            var assistidos = usuario.Assistidos.ToHashSet();
            var todasMidias = _midias.ObterTodos().ToList();

            var generosPreferidos = usuario.Favoritos
                .Select(id => todasMidias.FirstOrDefault(m => m.Id == id))
                .Where(m => m != null)
                .GroupBy(m => m!.Genero)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            if (!generosPreferidos.Any())
                return todasMidias.Where(m => !assistidos.Contains(m.Id))
                                   .OrderByDescending(m => m.NotaMedia).Take(quantidade);

            return todasMidias
                .Where(m => !assistidos.Contains(m.Id) && generosPreferidos.Contains(m.Genero))
                .OrderByDescending(m => m.NotaMedia)
                .Take(quantidade);
        }

        // ─── Estatísticas ────────────────────────────────────────────
        public (int totalMidias, int totalFilmes, int totalSeries, int totalAvaliacoes) ObterEstatisticas()
        {
            var midias = _midias.ObterTodos().ToList();
            return (
                midias.Count,
                midias.Count(m => m.ObterTipo() == "Filme"),
                midias.Count(m => m.ObterTipo() == "Série"),
                _avaliacoes.ObterTodos().Count()
            );
        }
    }
}
