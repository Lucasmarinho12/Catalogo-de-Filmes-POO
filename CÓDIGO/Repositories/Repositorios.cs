using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CineLog.Exceptions;
using CineLog.Interfaces;
using CineLog.Models;

namespace CineLog.Repositories
{
    // DTOs internos de serialização (classes abstratas não serializam bem)
    internal record MidiaDto(
        string Tipo, int Id, string Titulo, int Ano, string Genero, string Sinopse,
        int? DuracaoMinutos, string? Diretor,
        int? NumeroTemporadas, int? NumeroEpisodios, bool? EmAndamento, string? Criador);

    internal record AvaliacaoDto(int Id, int UsuarioId, int MidiaId, double Nota, string Comentario, DateTime Data);

    internal record UsuarioDto(int Id, string Nome, string Email, string SenhaHash,
        List<int> QueroAssistir, List<int> Assistidos, List<int> Favoritos);

    // Travas simples para evitar corrupção em escritas concorrentes (vários
    // clientes do frontend podem chamar a API ao mesmo tempo)
    internal static class FileLock
    {
        public static readonly object Sync = new();
    }

    public class MidiaRepositorioJson : IMidiaRepositorio
    {
        private readonly string _arquivo;
        private readonly Dictionary<int, Midia> _midias = new();
        private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

        public MidiaRepositorioJson(string pasta)
        {
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "midias.json");
            Carregar();
        }

        public void Adicionar(Midia midia)
        {
            lock (FileLock.Sync)
            {
                _midias[midia.Id] = midia;
                SalvarInterno();
            }
        }

        public Midia ObterPorId(int id) =>
            _midias.TryGetValue(id, out var m) ? m
            : throw new EntidadeNaoEncontradaException($"Mídia {id} não encontrada.");

        public IEnumerable<Midia> ObterTodos() => _midias.Values.OrderBy(m => m.Titulo).ToList();

        public int ProximoId() => _midias.Any() ? _midias.Keys.Max() + 1 : 1;

        public void Salvar() { lock (FileLock.Sync) SalvarInterno(); }

        private void SalvarInterno()
        {
            var dtos = _midias.Values.Select(m => new MidiaDto(
                m.ObterTipo(), m.Id, m.Titulo, m.AnoLancamento, m.Genero, m.Sinopse,
                (m as Filme)?.DuracaoMinutos, (m as Filme)?.Diretor,
                (m as Serie)?.NumeroTemporadas, (m as Serie)?.NumeroEpisodios,
                (m as Serie)?.EmAndamento, (m as Serie)?.Criador
            )).ToList();
            var tmp = _arquivo + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Opts));
            File.Move(tmp, _arquivo, overwrite: true); // escrita atômica
        }

        public void Carregar()
        {
            lock (FileLock.Sync)
            {
                _midias.Clear();
                if (!File.Exists(_arquivo)) return;
                var dtos = JsonSerializer.Deserialize<List<MidiaDto>>(File.ReadAllText(_arquivo));
                if (dtos == null) return;

                foreach (var dto in dtos)
                {
                    Midia midia = dto.Tipo == "Filme"
                        ? new Filme(dto.Id, dto.Titulo, dto.Ano, dto.Genero, dto.Sinopse,
                                    dto.DuracaoMinutos ?? 90, dto.Diretor ?? "Desconhecido")
                        : new Serie(dto.Id, dto.Titulo, dto.Ano, dto.Genero, dto.Sinopse,
                                    dto.NumeroTemporadas ?? 1, dto.NumeroEpisodios ?? 1,
                                    dto.EmAndamento ?? false, dto.Criador ?? "Desconhecido");
                    _midias[midia.Id] = midia;
                }
            }
        }

        // Usado pelo AvaliacaoRepositorio para reidratar avaliações na mídia certa
        internal void RestaurarAvaliacaoEm(int midiaId, Avaliacao avaliacao)
        {
            if (_midias.TryGetValue(midiaId, out var m)) m.RestaurarAvaliacao(avaliacao);
        }
    }

    public class UsuarioRepositorioJson : IUsuarioRepositorio
    {
        private readonly string _arquivo;
        private readonly Dictionary<int, Usuario> _usuarios = new();
        private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

        public UsuarioRepositorioJson(string pasta)
        {
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "usuarios.json");
            Carregar();
        }

        public void Adicionar(Usuario u)
        {
            lock (FileLock.Sync)
            {
                _usuarios[u.Id] = u;
                SalvarInterno();
            }
        }

        public Usuario ObterPorId(int id) =>
            _usuarios.TryGetValue(id, out var u) ? u
            : throw new EntidadeNaoEncontradaException($"Usuário {id} não encontrado.");

        public Usuario ObterPorEmail(string email)
        {
            var e = email.Trim().ToLower();
            var u = _usuarios.Values.FirstOrDefault(x => x.Email == e);
            return u ?? throw new EntidadeNaoEncontradaException("Usuário não encontrado.");
        }

        public bool EmailExiste(string email)
        {
            var e = email.Trim().ToLower();
            return _usuarios.Values.Any(u => u.Email == e);
        }

        public IEnumerable<Usuario> ObterTodos() => _usuarios.Values.ToList();

        public int ProximoId() => _usuarios.Any() ? _usuarios.Keys.Max() + 1 : 1;

        public void Salvar() { lock (FileLock.Sync) SalvarInterno(); }

        private void SalvarInterno()
        {
            var dtos = _usuarios.Values.Select(u => new UsuarioDto(
                u.Id, u.Nome, u.Email, u.ObterHash(),
                u.QueroAssistir.ToList(), u.Assistidos.ToList(), u.Favoritos.ToList()
            )).ToList();
            var tmp = _arquivo + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Opts));
            File.Move(tmp, _arquivo, overwrite: true);
        }

        public void Carregar()
        {
            lock (FileLock.Sync)
            {
                _usuarios.Clear();
                if (!File.Exists(_arquivo)) return;
                var dtos = JsonSerializer.Deserialize<List<UsuarioDto>>(File.ReadAllText(_arquivo));
                if (dtos == null) return;

                foreach (var dto in dtos)
                {
                    // Senha temporária só para satisfazer o construtor; o hash real é restaurado a seguir
                    var u = new Usuario(dto.Id, dto.Nome, dto.Email, "Temp@123");
                    u.RestaurarHash(dto.SenhaHash);
                    u.RestaurarListas(dto.QueroAssistir, dto.Assistidos, dto.Favoritos);
                    _usuarios[u.Id] = u;
                }
            }
        }
    }

    public class AvaliacaoRepositorioJson : IAvaliacaoRepositorio
    {
        private readonly string _arquivo;
        private readonly List<Avaliacao> _avaliacoes = new();
        private readonly MidiaRepositorioJson _midiaRepo; // para reidratar a avaliação na mídia
        private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

        public AvaliacaoRepositorioJson(string pasta, MidiaRepositorioJson midiaRepo)
        {
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "avaliacoes.json");
            _midiaRepo = midiaRepo;
            Carregar();
        }

        public void Adicionar(Avaliacao a)
        {
            lock (FileLock.Sync)
            {
                _avaliacoes.Add(a);
                SalvarInterno();
            }
        }

        public IEnumerable<Avaliacao> ObterPorMidia(int midiaId) =>
            _avaliacoes.Where(a => a.MidiaId == midiaId).ToList();

        public IEnumerable<Avaliacao> ObterTodos() => _avaliacoes.ToList();

        public int ProximoId() => _avaliacoes.Any() ? _avaliacoes.Max(a => a.Id) + 1 : 1;

        public void Salvar() { lock (FileLock.Sync) SalvarInterno(); }

        private void SalvarInterno()
        {
            var dtos = _avaliacoes.Select(a =>
                new AvaliacaoDto(a.Id, a.UsuarioId, a.MidiaId, a.Nota, a.Comentario, a.DataAvaliacao));
            var tmp = _arquivo + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Opts));
            File.Move(tmp, _arquivo, overwrite: true);
        }

        public void Carregar()
        {
            lock (FileLock.Sync)
            {
                _avaliacoes.Clear();
                if (!File.Exists(_arquivo)) return;
                var dtos = JsonSerializer.Deserialize<List<AvaliacaoDto>>(File.ReadAllText(_arquivo));
                if (dtos == null) return;

                foreach (var dto in dtos)
                {
                    var av = new Avaliacao(dto.Id, dto.UsuarioId, dto.MidiaId, dto.Nota, dto.Comentario, dto.Data);
                    _avaliacoes.Add(av);
                    // Reconecta a avaliação à mídia correspondente, para que NotaMedia funcione
                    _midiaRepo.RestaurarAvaliacaoEm(dto.MidiaId, av);
                }
            }
        }
    }
}
