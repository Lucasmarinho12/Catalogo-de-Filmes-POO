using System;
using System.Collections.Generic;
using System.Linq;
using CineLog.Exceptions;

namespace CineLog.Models
{
    // ABSTRAÇÃO: Classe base abstrata que define o contrato para todas as mídias
    public abstract class Midia
    {
        // ENCAPSULAMENTO: propriedades com acesso controlado
        public int Id { get; protected set; }
        public string Titulo { get; protected set; }
        public int AnoLancamento { get; protected set; }
        public string Genero { get; protected set; }
        public string Sinopse { get; protected set; }
        private readonly List<Avaliacao> _avaliacoes;

        public IReadOnlyList<Avaliacao> Avaliacoes => _avaliacoes.AsReadOnly();
        public double NotaMedia => _avaliacoes.Any() ? Math.Round(_avaliacoes.Average(a => a.Nota), 1) : 0;

        protected Midia(int id, string titulo, int anoLancamento, string genero, string sinopse)
        {
            ValidarDados(titulo, anoLancamento, genero);
            Id = id;
            Titulo = titulo;
            AnoLancamento = anoLancamento;
            Genero = genero;
            Sinopse = sinopse;
            _avaliacoes = new List<Avaliacao>();
        }

        // ABSTRAÇÃO: Método abstrato — cada subclasse define sua apresentação
        public abstract string ObterTipo();
        public abstract string ObterDetalhes();

        public void AdicionarAvaliacao(Avaliacao avaliacao)
        {
            if (_avaliacoes.Any(a => a.UsuarioId == avaliacao.UsuarioId))
                throw new AvaliacaoDuplicadaException("Usuário já avaliou esta mídia.");
            _avaliacoes.Add(avaliacao);
        }

        public bool PodeAvaliar(int usuarioId) =>
            !_avaliacoes.Any(a => a.UsuarioId == usuarioId);

        // ENCAPSULAMENTO: validação centralizada
        private static void ValidarDados(string titulo, int ano, string genero)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new DadosInvalidosException("Título não pode ser vazio.");
            if (ano < 1888 || ano > DateTime.Now.Year + 5)
                throw new DadosInvalidosException($"Ano inválido: {ano}.");
            if (string.IsNullOrWhiteSpace(genero))
                throw new DadosInvalidosException("Gênero não pode ser vazio.");
        }

        public override string ToString() =>
            $"[{ObterTipo()}] {Titulo} ({AnoLancamento}) | Gênero: {Genero} | Nota: {NotaMedia:F1}";
    }
}
