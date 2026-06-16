using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CineLog.Exceptions;
using CineLog.Interfaces;
using CineLog.Models;

namespace CineLog.Repositories
{
    // DTOs para serialização JSON (evita problemas com classes abstratas)
    internal record MidiaDto(
        string Tipo, int Id, string Titulo, int Ano, string Genero, string Sinopse,
        int? DuracaoMinutos, string Diretor, int? NumeroTemporadas, int? NumeroEpisodios,
        bool? EmAndamento, string Criador,
        List<AvaliacaoDto> Avaliacoes);

    internal record AvaliacaoDto(int Id, int UsuarioId, int MidiaId, double Nota,
        string Comentario, DateTime Data);

    internal record UsuarioDto(int Id, string Nome, string Email, string SenhaHash,
        List<int> QueroAssistir, List<int> Assistidos, List<int> Favoritos);

    public class MidiaRepositorioJson : IMidiaRepositorio
    {
        private readonly string _arquivo;
        private readonly Dictionary<int, Midia> _midias = new();
        private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

        public MidiaRepositorioJson(string pasta = "data")
        {
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "midias.json");
        }

        public void Adicionar(Midia midia)
        {
            _midias[midia.Id] = midia;
        }

        public Midia ObterPorId(int id) =>
            _midias.TryGetValue(id, out var m) ? m
            : throw new EntidadeNaoEncontradaException($"Mídia {id} não encontrada.");

        public IEnumerable<Midia> ObterTodos() => _midias.Values.OrderBy(m => m.Titulo);

        public IEnumerable<Midia> FiltrarPorGenero(string genero) =>
            _midias.Values.Where(m => m.Genero.Contains(genero, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Midia> FiltrarPorAno(int ano) =>
            _midias.Values.Where(m => m.AnoLancamento == ano);

        public IEnumerable<Midia> FiltrarPorNotaMinima(double nota) =>
            _midias.Values.Where(m => m.NotaMedia >= nota);

        public int ProximoId() => _midias.Any() ? _midias.Keys.Max() + 1 : 1;

        public void Salvar()
        {
            var dtos = _midias.Values.Select(m => new MidiaDto(
                m.ObterTipo(), m.Id, m.Titulo, m.AnoLancamento, m.Genero, m.Sinopse,
                (m as Filme)?.DuracaoMinutos,
                (m as Filme)?.Diretor,
                (m as Serie)?.NumeroTemporadas,
                (m as Serie)?.NumeroEpisodios,
                (m as Serie)?.EmAndamento,
                (m as Serie)?.Criador,
                m.Avaliacoes.Select(a => new AvaliacaoDto(
                    a.Id, a.UsuarioId, a.MidiaId, a.Nota, a.Comentario, a.DataAvaliacao)).ToList()
            )).ToList();

            File.WriteAllText(_arquivo, JsonSerializer.Serialize(dtos, _opts));
        }

        public void Carregar()
        {
            if (!File.Exists(_arquivo)) return;
            var json = File.ReadAllText(_arquivo);
            var dtos = JsonSerializer.Deserialize<List<MidiaDto>>(json);
            if (dtos == null) return;

            _midias.Clear();
            foreach (var dto in dtos)
            {
                Midia midia = dto.Tipo == "Filme"
                    ? new Filme(dto.Id, dto.Titulo, dto.Ano, dto.Genero, dto.Sinopse,
                                dto.DuracaoMinutos ?? 90, dto.Diretor ?? "Desconhecido")
                    : (Midia)new Serie(dto.Id, dto.Titulo, dto.Ano, dto.Genero, dto.Sinopse,
                                dto.NumeroTemporadas ?? 1, dto.NumeroEpisodios ?? 1,
                                dto.EmAndamento ?? false, dto.Criador ?? "Desconhecido");

                foreach (var av in dto.Avaliacoes)
                    midia.AdicionarAvaliacao(new Avaliacao(av.Id, av.UsuarioId, av.MidiaId,
                        av.Nota, av.Comentario));

                _midias[midia.Id] = midia;
            }
        }
    }

    public class UsuarioRepositorioJson : IUsuarioRepositorio
    {
        private readonly string _arquivo;
        private readonly Dictionary<int, Usuario> _usuarios = new();
        private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

        public UsuarioRepositorioJson(string pasta = "data")
        {
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "usuarios.json");
        }

        public void Adicionar(Usuario u) => _usuarios[u.Id] = u;
        public Usuario ObterPorId(int id) =>
            _usuarios.TryGetValue(id, out var u) ? u
            : throw new EntidadeNaoEncontradaException($"Usuário {id} não encontrado.");

        public Usuario ObterPorEmail(string email)
        {
            var u = _usuarios.Values.FirstOrDefault(x =>
                x.Email.Equals(email.Trim().ToLower(), StringComparison.OrdinalIgnoreCase));
            return u ?? throw new EntidadeNaoEncontradaException("Usuário não encontrado.");
        }

        public bool EmailExiste(string email) =>
            _usuarios.Values.Any(u =>
                u.Email.Equals(email.Trim().ToLower(), StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Usuario> ObterTodos() => _usuarios.Values;
        public int ProximoId() => _usuarios.Any() ? _usuarios.Keys.Max() + 1 : 1;

        public void Salvar()
        {
            // Usa reflection para acessar o hash — em produção, expor via método de serialização
            var dtos = _usuarios.Values.Select(u => new UsuarioDto(
                u.Id, u.Nome, u.Email,
                ObterHashPrivado(u),
                u.QueroAssistir.ToList(),
                u.Assistidos.ToList(),
                u.Favoritos.ToList()
            )).ToList();
            File.WriteAllText(_arquivo, JsonSerializer.Serialize(dtos, _opts));
        }

        private static string ObterHashPrivado(Usuario u)
        {
            var campo = typeof(Usuario).GetField("_senhaHash",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return campo?.GetValue(u)?.ToString() ?? "";
        }

        public void Carregar()
        {
            if (!File.Exists(_arquivo)) return;
            var dtos = JsonSerializer.Deserialize<List<UsuarioDto>>(
                File.ReadAllText(_arquivo));
            if (dtos == null) return;

            _usuarios.Clear();
            foreach (var dto in dtos)
            {
                // Cria usuário com senha temporária e restaura o hash
                var u = CriarUsuarioComHash(dto);
                foreach (var id in dto.QueroAssistir) u.AdicionarNaLista(TipoLista.QueroAssistir, id);
                foreach (var id in dto.Assistidos) u.AdicionarNaLista(TipoLista.Assistidos, id);
                foreach (var id in dto.Favoritos) u.AdicionarNaLista(TipoLista.Favoritos, id);
                _usuarios[u.Id] = u;
            }
        }

        private static Usuario CriarUsuarioComHash(UsuarioDto dto)
        {
            var u = new Usuario(dto.Id, dto.Nome, dto.Email, "Temp@123");
            var campo = typeof(Usuario).GetField("_senhaHash",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            campo?.SetValue(u, dto.SenhaHash);
            return u;
        }
    }

    public class AvaliacaoRepositorioJson : IAvaliacaoRepositorio
    {
        private readonly string _arquivo;
        private readonly List<Avaliacao> _avaliacoes = new();
        private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

        public AvaliacaoRepositorioJson(string pasta = "data")
        {
            Directory.CreateDirectory(pasta);
            _arquivo = Path.Combine(pasta, "avaliacoes.json");
        }

        public void Adicionar(Avaliacao a) => _avaliacoes.Add(a);
        public IEnumerable<Avaliacao> ObterPorMidia(int id) => _avaliacoes.Where(a => a.MidiaId == id);
        public IEnumerable<Avaliacao> ObterPorUsuario(int id) => _avaliacoes.Where(a => a.UsuarioId == id);
        public int ProximoId() => _avaliacoes.Any() ? _avaliacoes.Max(a => a.Id) + 1 : 1;

        public void Salvar() =>
            File.WriteAllText(_arquivo, JsonSerializer.Serialize(
                _avaliacoes.Select(a => new AvaliacaoDto(
                    a.Id, a.UsuarioId, a.MidiaId, a.Nota, a.Comentario, a.DataAvaliacao)), _opts));

        public void Carregar()
        {
            if (!File.Exists(_arquivo)) return;
            var dtos = JsonSerializer.Deserialize<List<AvaliacaoDto>>(File.ReadAllText(_arquivo));
            _avaliacoes.Clear();
            if (dtos != null)
                foreach (var dto in dtos)
                    _avaliacoes.Add(new Avaliacao(dto.Id, dto.UsuarioId, dto.MidiaId, dto.Nota, dto.Comentario));
        }
    }
}
