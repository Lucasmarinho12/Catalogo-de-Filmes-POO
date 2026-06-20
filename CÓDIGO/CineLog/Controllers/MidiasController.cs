using Microsoft.AspNetCore.Mvc;
using CineLog.DTOs;
using CineLog.Exceptions;
using CineLog.Models;
using CineLog.Services;

namespace CineLog.Controllers
{
    [ApiController]
    [Route("api/midias")]
    public class MidiasController : ControllerBase
    {
        private readonly CatalogoService _service;
        public MidiasController(CatalogoService service) => _service = service;

        // GET /api/midias?busca=&tipo=&genero=&ano=&notaMinima=
        [HttpGet]
        public IActionResult Listar([FromQuery] string? busca, [FromQuery] string? tipo,
                                     [FromQuery] string? genero, [FromQuery] int? ano,
                                     [FromQuery] double? notaMinima)
        {
            var midias = _service.FiltrarMidias(tipo, genero, ano, notaMinima, busca)
                                  .Select(MidiaResponse.De);
            return Ok(midias);
        }

        // GET /api/midias/5
        [HttpGet("{id:int}")]
        public IActionResult ObterPorId(int id)
        {
            try { return Ok(MidiaResponse.De(_service.ObterMidia(id))); }
            catch (EntidadeNaoEncontradaException ex) { return NotFound(new ErroResponse(ex.Message)); }
        }

        // POST /api/midias/filme
        [HttpPost("filme")]
        public IActionResult CadastrarFilme([FromBody] FilmeRequest req)
        {
            try
            {
                var f = _service.CadastrarFilme(req.Titulo, req.Ano, req.Genero, req.Sinopse,
                                                  req.DuracaoMinutos, req.Diretor);
                return CreatedAtAction(nameof(ObterPorId), new { id = f.Id }, MidiaResponse.De(f));
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // POST /api/midias/serie
        [HttpPost("serie")]
        public IActionResult CadastrarSerie([FromBody] SerieRequest req)
        {
            try
            {
                var s = _service.CadastrarSerie(req.Titulo, req.Ano, req.Genero, req.Sinopse,
                                                  req.NumeroTemporadas, req.NumeroEpisodios,
                                                  req.EmAndamento, req.Criador);
                return CreatedAtAction(nameof(ObterPorId), new { id = s.Id }, MidiaResponse.De(s));
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // GET /api/midias/5/avaliacoes
        [HttpGet("{id:int}/avaliacoes")]
        public IActionResult ObterAvaliacoes(int id)
        {
            try
            {
                var avs = _service.ObterAvaliacoesDaMidia(id)
                    .OrderByDescending(a => a.DataAvaliacao)
                    .Select(a =>
                    {
                        string nome;
                        try { nome = _service.ObterUsuario(a.UsuarioId).Nome; }
                        catch { nome = "Usuário"; }
                        return new AvaliacaoResponse(a.Id, a.UsuarioId, nome, a.MidiaId, a.Nota, a.Comentario, a.DataAvaliacao);
                    });
                return Ok(avs);
            }
            catch (EntidadeNaoEncontradaException ex) { return NotFound(new ErroResponse(ex.Message)); }
        }

        // POST /api/midias/5/avaliacoes
        [HttpPost("{id:int}/avaliacoes")]
        public IActionResult Avaliar(int id, [FromBody] AvaliacaoRequest req)
        {
            try
            {
                var av = _service.AvaliarMidia(req.UsuarioId, id, req.Nota, req.Comentario);
                var nome = _service.ObterUsuario(req.UsuarioId).Nome;
                return Ok(new AvaliacaoResponse(av.Id, av.UsuarioId, nome, av.MidiaId, av.Nota, av.Comentario, av.DataAvaliacao));
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // GET /api/midias/top?quantidade=10
        [HttpGet("top")]
        public IActionResult Top([FromQuery] int quantidade = 10) =>
            Ok(_service.ObterTopMidias(quantidade).Select(MidiaResponse.De));

        // GET /api/midias/estatisticas
        [HttpGet("estatisticas")]
        public IActionResult Estatisticas()
        {
            var (total, filmes, series, avals) = _service.ObterEstatisticas();
            return Ok(new { totalMidias = total, totalFilmes = filmes, totalSeries = series, totalAvaliacoes = avals });
        }
    }

    [ApiController]
    [Route("api/usuarios/{usuarioId:int}")]
    public class ListasController : ControllerBase
    {
        private readonly CatalogoService _service;
        public ListasController(CatalogoService service) => _service = service;

        // GET /api/usuarios/3/listas/favoritos
        [HttpGet("listas/{tipo}")]
        public IActionResult ObterLista(int usuarioId, string tipo)
        {
            try
            {
                var tipoLista = ParseTipo(tipo);
                var midias = _service.ObterListaDoUsuario(usuarioId, tipoLista).Select(MidiaResponse.De);
                return Ok(midias);
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // POST /api/usuarios/3/listas/favoritos/5
        [HttpPost("listas/{tipo}/{midiaId:int}")]
        public IActionResult Adicionar(int usuarioId, string tipo, int midiaId)
        {
            try
            {
                _service.AdicionarNaLista(usuarioId, midiaId, ParseTipo(tipo));
                return Ok();
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // DELETE /api/usuarios/3/listas/favoritos/5
        [HttpDelete("listas/{tipo}/{midiaId:int}")]
        public IActionResult Remover(int usuarioId, string tipo, int midiaId)
        {
            try
            {
                _service.RemoverDaLista(usuarioId, midiaId, ParseTipo(tipo));
                return Ok();
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // GET /api/usuarios/3/recomendacoes
        [HttpGet("recomendacoes")]
        public IActionResult Recomendacoes(int usuarioId)
        {
            try { return Ok(_service.RecomendarPorGenero(usuarioId).Select(MidiaResponse.De)); }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        private static TipoLista ParseTipo(string tipo) => tipo.ToLower() switch
        {
            "quero" or "queroassistir" => TipoLista.QueroAssistir,
            "assistidos" => TipoLista.Assistidos,
            "favoritos" => TipoLista.Favoritos,
            _ => throw new DadosInvalidosException($"Lista '{tipo}' inválida.")
        };
    }
}
