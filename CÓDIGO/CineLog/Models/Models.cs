using System;
using System.Collections.Generic;
using System.Linq;
using CineLog.Exceptions;

namespace CineLog.Models
{
    // ═══════════════════════════════════════════════════════════════
    //  ABSTRAÇÃO: classe base que define o contrato comum a toda mídia
    // ═══════════════════════════════════════════════════════════════
    public abstract class Midia
    {
        // ENCAPSULAMENTO: setters privados, only mutável via métodos controlados
        public int Id { get; protected set; }
        public string Titulo { get; protected set; }
        public int AnoLancamento { get; protected set; }
        public string Genero { get; protected set; }
        public string Sinopse { get; protected set; }

        private readonly List<Avaliacao> _avaliacoes = new();
        public IReadOnlyList<Avaliacao> Avaliacoes => _avaliacoes.AsReadOnly();
        public double NotaMedia => _avaliacoes.Any() ? Math.Round(_avaliacoes.Average(a => a.Nota), 1) : 0;

        protected Midia(int id, string titulo, int anoLancamento, string genero, string sinopse)
        {
            ValidarDados(titulo, anoLancamento, genero);
            Id = id;
            Titulo = titulo;
            AnoLancamento = anoLancamento;
            Genero = genero;
            Sinopse = sinopse ?? string.Empty;
        }

        // ABSTRAÇÃO: cada subtipo define sua própria apresentação
        public abstract string ObterTipo();
        public abstract string ObterDetalhes();

        public void AdicionarAvaliacao(Avaliacao avaliacao)
        {
            if (_avaliacoes.Any(a => a.UsuarioId == avaliacao.UsuarioId))
                throw new AvaliacaoDuplicadaException("Você já avaliou esta mídia.");
            _avaliacoes.Add(avaliacao);
        }

        public bool PodeAvaliar(int usuarioId) => !_avaliacoes.Any(a => a.UsuarioId == usuarioId);

        private static void ValidarDados(string titulo, int ano, string genero)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new DadosInvalidosException("Título não pode ser vazio.");
            if (ano < 1888 || ano > DateTime.Now.Year + 5)
                throw new DadosInvalidosException($"Ano inválido: {ano}.");
            if (string.IsNullOrWhiteSpace(genero))
                throw new DadosInvalidosException("Gênero não pode ser vazio.");
        }

        // Usado pelo repositório ao reidratar a partir do JSON
        internal void RestaurarAvaliacao(Avaliacao avaliacao) => _avaliacoes.Add(avaliacao);

        public override string ToString() => $"[{ObterTipo()}] {Titulo} ({AnoLancamento})";
    }

    // ═══════════════════════════════════════════════════════════════
    //  HERANÇA + POLIMORFISMO
    // ═══════════════════════════════════════════════════════════════
    public class Filme : Midia
    {
        public int DuracaoMinutos { get; private set; }
        public string Diretor { get; private set; }

        public Filme(int id, string titulo, int anoLancamento, string genero, string sinopse,
                     int duracaoMinutos, string diretor)
            : base(id, titulo, anoLancamento, genero, sinopse)
        {
            DuracaoMinutos = duracaoMinutos > 0 ? duracaoMinutos
                : throw new DadosInvalidosException("Duração deve ser maior que zero.");
            Diretor = !string.IsNullOrWhiteSpace(diretor) ? diretor
                : throw new DadosInvalidosException("Diretor não pode ser vazio.");
        }

        public override string ObterTipo() => "Filme";
        public override string ObterDetalhes() => $"Diretor: {Diretor} | Duração: {DuracaoMinutos} min";
    }

    public class Serie : Midia
    {
        public int NumeroTemporadas { get; private set; }
        public int NumeroEpisodios { get; private set; }
        public bool EmAndamento { get; private set; }
        public string Criador { get; private set; }

        public Serie(int id, string titulo, int anoLancamento, string genero, string sinopse,
                     int numeroTemporadas, int numeroEpisodios, bool emAndamento, string criador)
            : base(id, titulo, anoLancamento, genero, sinopse)
        {
            NumeroTemporadas = numeroTemporadas > 0 ? numeroTemporadas
                : throw new DadosInvalidosException("Número de temporadas deve ser maior que zero.");
            NumeroEpisodios = numeroEpisodios > 0 ? numeroEpisodios
                : throw new DadosInvalidosException("Número de episódios deve ser maior que zero.");
            EmAndamento = emAndamento;
            Criador = !string.IsNullOrWhiteSpace(criador) ? criador
                : throw new DadosInvalidosException("Criador não pode ser vazio.");
        }

        public override string ObterTipo() => "Série";
        public override string ObterDetalhes() =>
            $"Criador: {Criador} | {NumeroTemporadas} temporadas | {NumeroEpisodios} episódios | " +
            (EmAndamento ? "Em andamento" : "Finalizada");
    }

    // ═══════════════════════════════════════════════════════════════
    //  Usuario — ENCAPSULAMENTO das listas pessoais
    // ═══════════════════════════════════════════════════════════════
    public class Usuario
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        private string _senhaHash;

        private readonly List<int> _queroAssistir = new();
        private readonly List<int> _assistidos = new();
        private readonly List<int> _favoritos = new();

        public IReadOnlyList<int> QueroAssistir => _queroAssistir.AsReadOnly();
        public IReadOnlyList<int> Assistidos => _assistidos.AsReadOnly();
        public IReadOnlyList<int> Favoritos => _favoritos.AsReadOnly();

        public Usuario(int id, string nome, string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DadosInvalidosException("Nome não pode ser vazio.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
                throw new DadosInvalidosException("E-mail inválido.");
            if (string.IsNullOrEmpty(senha) || senha.Length < 6)
                throw new DadosInvalidosException("Senha deve ter pelo menos 6 caracteres.");

            Id = id;
            Nome = nome.Trim();
            Email = email.Trim().ToLower();
            _senhaHash = Hash(senha);
        }

        public bool ValidarSenha(string senha) => _senhaHash == Hash(senha);

        private static string Hash(string senha) =>
            Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(senha + "cinelog_salt_v1")));

        public void AdicionarNaLista(TipoLista lista, int midiaId)
        {
            switch (lista)
            {
                case TipoLista.QueroAssistir:
                    if (!_queroAssistir.Contains(midiaId)) _queroAssistir.Add(midiaId);
                    break;
                case TipoLista.Assistidos:
                    if (!_assistidos.Contains(midiaId)) _assistidos.Add(midiaId);
                    _queroAssistir.Remove(midiaId);
                    break;
                case TipoLista.Favoritos:
                    if (!_favoritos.Contains(midiaId)) _favoritos.Add(midiaId);
                    break;
            }
        }

        public void RemoverDaLista(TipoLista lista, int midiaId)
        {
            switch (lista)
            {
                case TipoLista.QueroAssistir: _queroAssistir.Remove(midiaId); break;
                case TipoLista.Assistidos: _assistidos.Remove(midiaId); break;
                case TipoLista.Favoritos: _favoritos.Remove(midiaId); break;
            }
        }

        // Usado pelo repositório ao reidratar a partir do JSON
        internal void RestaurarHash(string hash) => _senhaHash = hash;
        internal string ObterHash() => _senhaHash;
        internal void RestaurarListas(List<int> quero, List<int> assistidos, List<int> favoritos)
        {
            _queroAssistir.Clear(); _queroAssistir.AddRange(quero);
            _assistidos.Clear(); _assistidos.AddRange(assistidos);
            _favoritos.Clear(); _favoritos.AddRange(favoritos);
        }
    }

    public enum TipoLista { QueroAssistir, Assistidos, Favoritos }

    public class Avaliacao
    {
        public int Id { get; private set; }
        public int UsuarioId { get; private set; }
        public int MidiaId { get; private set; }
        public double Nota { get; private set; }
        public string Comentario { get; private set; }
        public DateTime DataAvaliacao { get; private set; }

        public Avaliacao(int id, int usuarioId, int midiaId, double nota, string comentario, DateTime? data = null)
        {
            if (nota < 0 || nota > 10)
                throw new DadosInvalidosException("Nota deve estar entre 0 e 10.");

            Id = id;
            UsuarioId = usuarioId;
            MidiaId = midiaId;
            Nota = Math.Round(nota, 1);
            Comentario = comentario?.Trim() ?? string.Empty;
            DataAvaliacao = data ?? DateTime.Now;
        }
    }
}
