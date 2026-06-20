using CineLog.Models;

namespace CineLog.DTOs
{
    // ─── Saída (responses) ──────────────────────────────────────────
    public record MidiaResponse(
        int Id, string Tipo, string Titulo, int Ano, string Genero, string Sinopse,
        double NotaMedia, int TotalAvaliacoes,
        int? DuracaoMinutos, string? Diretor,
        int? NumeroTemporadas, int? NumeroEpisodios, bool? EmAndamento, string? Criador)
    {
        public static MidiaResponse De(Midia m) => new(
            m.Id, m.ObterTipo(), m.Titulo, m.AnoLancamento, m.Genero, m.Sinopse,
            m.NotaMedia, m.Avaliacoes.Count,
            (m as Filme)?.DuracaoMinutos, (m as Filme)?.Diretor,
            (m as Serie)?.NumeroTemporadas, (m as Serie)?.NumeroEpisodios,
            (m as Serie)?.EmAndamento, (m as Serie)?.Criador
        );
    }

    public record AvaliacaoResponse(int Id, int UsuarioId, string UsuarioNome, int MidiaId,
        double Nota, string Comentario, DateTime Data);

    public record UsuarioResponse(int Id, string Nome, string Email)
    {
        public static UsuarioResponse De(Usuario u) => new(u.Id, u.Nome, u.Email);
    }

    public record ErroResponse(string Mensagem);

    // ─── Entrada (requests) ─────────────────────────────────────────
    public record LoginRequest(string Email, string Senha);
    public record RegistroRequest(string Nome, string Email, string Senha);

    public record FilmeRequest(string Titulo, int Ano, string Genero, string Sinopse,
        int DuracaoMinutos, string Diretor);

    public record SerieRequest(string Titulo, int Ano, string Genero, string Sinopse,
        int NumeroTemporadas, int NumeroEpisodios, bool EmAndamento, string Criador);

    public record AvaliacaoRequest(int UsuarioId, double Nota, string Comentario);

    public record ListaRequest(int UsuarioId, int MidiaId);
}
