using System;
using System.Collections.Generic;
using System.Linq;
using CineLog.Exceptions;

namespace CineLog.Models
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        private string _senhaHash;

        // ENCAPSULAMENTO: Listas privadas expostas como somente-leitura
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
            if (!email.Contains('@') || !email.Contains('.'))
                throw new DadosInvalidosException("E-mail inválido.");
            if (senha?.Length < 6)
                throw new DadosInvalidosException("Senha deve ter pelo menos 6 caracteres.");

            Id = id;
            Nome = nome;
            Email = email.ToLower().Trim();
            _senhaHash = BCryptHash(senha);
        }

        public bool ValidarSenha(string senha) => _senhaHash == BCryptHash(senha);

        // Simulação simples de hash (em produção usar BCrypt real)
        private static string BCryptHash(string senha) =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(senha + "cinelog_salt"));

        public void AdicionarNaLista(TipoLista lista, int midiaId)
        {
            switch (lista)
            {
                case TipoLista.QueroAssistir:
                    if (!_queroAssistir.Contains(midiaId)) _queroAssistir.Add(midiaId);
                    break;
                case TipoLista.Assistidos:
                    if (!_assistidos.Contains(midiaId)) _assistidos.Add(midiaId);
                    _queroAssistir.Remove(midiaId); // Remove de "quero assistir" ao marcar como assistido
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

        public override string ToString() => $"[{Id}] {Nome} ({Email})";
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

        public Avaliacao(int id, int usuarioId, int midiaId, double nota, string comentario)
        {
            if (nota < 0 || nota > 10)
                throw new DadosInvalidosException("Nota deve estar entre 0 e 10.");

            Id = id;
            UsuarioId = usuarioId;
            MidiaId = midiaId;
            Nota = Math.Round(nota, 1);
            Comentario = comentario?.Trim() ?? string.Empty;
            DataAvaliacao = DateTime.Now;
        }

        public override string ToString() =>
            $"Nota: {Nota:F1}/10 | \"{Comentario}\" — {DataAvaliacao:dd/MM/yyyy}";
    }

    // Padrão: Value Object — lista personalizada com nome e tipo
    public class ListaPersonalizada
    {
        public int Id { get; private set; }
        public int UsuarioId { get; private set; }
        public string Nome { get; private set; }
        private readonly List<int> _midias = new();
        public IReadOnlyList<int> Midias => _midias.AsReadOnly();

        public ListaPersonalizada(int id, int usuarioId, string nome)
        {
            Id = id;
            UsuarioId = usuarioId;
            Nome = string.IsNullOrWhiteSpace(nome)
                ? throw new DadosInvalidosException("Nome da lista não pode ser vazio.")
                : nome;
        }

        public void Adicionar(int midiaId) { if (!_midias.Contains(midiaId)) _midias.Add(midiaId); }
        public void Remover(int midiaId) => _midias.Remove(midiaId);
        public override string ToString() => $"📋 {Nome} ({_midias.Count} itens)";
    }
}
