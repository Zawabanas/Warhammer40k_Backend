﻿using WarHammer40K.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WarHammer40K.DTOs;
using WarHammer40K;

namespace LoginBackend20243S.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;

        public CuentasController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        private async Task<RespuestaAuthentication> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, credencialesUsuario.Email)
    };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            // Agregar otros reclamos si existen
            var claimsRoles = await userManager.GetClaimsAsync(usuario);
            claims.AddRange(claimsRoles);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["LlaveJWT"]));
            var cred = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddDays(1);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: cred
            );

            return new RespuestaAuthentication
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiration = expiracion,
            };
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAuthentication>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email");
            if (emailClaim == null)
            {
                return BadRequest(new MensajeError { Error = "Usuario no autenticado." });
            }

            var credencialesUsuario = new CredencialesUsuario
            {
                Email = emailClaim.Value
            };

            return await ConstruirToken(credencialesUsuario);
        }

        [HttpPost("registrar")]
        [Consumes("multipart/form-data")] // Indica que se aceptan datos de formulario de varias partes
        public async Task<ActionResult<RespuestaAuthentication>> Registrar([FromForm] CredencialesUsuario credencialesUsuario)
        {
            var usuarioExistente = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            if (usuarioExistente != null)
            {
                return BadRequest(new { Error = "El usuario ya está registrado." });
            }

            // Crear el usuario
            var usuario = new ApplicationUser
            {
                UserName = credencialesUsuario.Email,
                Email = credencialesUsuario.Email
            };

            // Guardar la imagen si se proporciona
            if (credencialesUsuario.ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(credencialesUsuario.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await credencialesUsuario.ProfileImage.CopyToAsync(stream);
                }

                usuario.ProfileImage = $"/profile-images/{fileName}"; // Asignar la URL de la imagen al usuario
            }

            // Crear usuario en Identity
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }

            var errores = resultado.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { Error = string.Join(", ", errores) });
        }



        [HttpPost("Login")]
        public async Task<ActionResult<RespuestaAuthentication>> Login(CredencialesUsuario credencialesUsuario)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);
            if (usuario == null)
            {
                return BadRequest(new MensajeError { Error = "Usuario no registrado." });
            }

            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }

            return BadRequest(new MensajeError { Error = "Login incorrecto. Verifique sus credenciales." });
        }

        [HttpGet("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfile()
        {
            var emailClaim = HttpContext.User.FindFirst(ClaimTypes.Email);
            if (emailClaim == null)
            {
                return Unauthorized(new { Error = "Usuario no autenticado" });
            }

            var usuario = await userManager.FindByEmailAsync(emailClaim.Value);
            if (usuario == null)
            {
                return NotFound(new { Error = "Usuario no encontrado" });
            }

            var userProfile = new UserProfileDTO
            {
                Email = usuario.Email,
                UserName = usuario.UserName,
                ProfileImage = usuario is ApplicationUser appUser ? appUser.ProfileImage : null
            };

            return Ok(userProfile);
        }


    }
}
