using Microsoft.AspNetCore.Mvc;
using CineLog.DTOs;
using CineLog.Exceptions;
using CineLog.Services;

namespace CineLog.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly CatalogoService _service;
        public AuthController(CatalogoService service) => _service = service;

        // POST /api/auth/registrar
        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistroRequest req)
        {
            try
            {
                var usuario = _service.CadastrarUsuario(req.Nome, req.Email, req.Senha);
                return Ok(UsuarioResponse.De(usuario));
            }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            try
            {
                var usuario = _service.Login(req.Email, req.Senha);
                return Ok(UsuarioResponse.De(usuario));
            }
            catch (AutenticacaoException ex) { return Unauthorized(new ErroResponse(ex.Message)); }
            catch (CineLogException ex) { return BadRequest(new ErroResponse(ex.Message)); }
        }
    }
}
