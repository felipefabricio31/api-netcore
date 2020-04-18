using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api_NetCore.Data;
using Api_NetCore.Models;
using Api_NetCore.Repositories;
using Api_NetCore.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_NetCore.Controllers
{
    [Route("v1/usuarios")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("login/")]
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context, [FromBody] User model)
        {
            var user = await context.User
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .AsNoTracking()
                .ToListAsync();

            return user;
        }

        [HttpPost]
        [Route("getByUsuario/")]
        public async Task<ActionResult<dynamic>> GetByUsuario([FromServices] DataContext context, [FromBody] User model)
        {
            var user = await context.User
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user != null)
            {
                // Gera o Token
                var token = TokenService.GenerateToken(user);
                // Oculta a senha
                user.Password = "";
                // Retorna os dados
                return new
                {
                    usuario = user.Username,
                    role = user.Role,
                    token = token
                };
            }
            else
            {
                return new
                {
                    erro = "Login/Senha inválidos."
                };
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Post([FromServices] DataContext context, [FromBody] User model)
        {
            if (ModelState.IsValid)
            {
                context.User.Add(model);
                await context.SaveChangesAsync();

                var token = TokenService.GenerateToken(model);

                return new
                {
                    user = model.Username.ToString(),
                    token = token
                };
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
