using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiUsuarios.Data;
using ApiUsuarios.Models;
using ApiUsuarios.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace ApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, TokenService tokenService, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _config = config;
            _emailService = emailService;
        }

        // ======================================================
        // 🔐 LOGIN
        // ======================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Sucursal)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Credenciales incorrectas");

            if (!user.Activo)
                return Unauthorized("El usuario está temporalmente inactivo");

            var token = _tokenService.GenerateToken(user);

            // 🧩 Permisos por rol (módulos visibles en el frontend)
            var modulosPermitidos = new List<string>();

            switch (user.Rol?.Nombre)
            {
                case "Administrador":
                    modulosPermitidos.AddRange(new[] {
                        "dashboard", "usuarios", "productos", "clientes", "proveedores",
                        "ventas", "compras", "stock", "reportes", "reglas-precio", "descuentos","giftcards"
                    });
                    break;

                case "Cajero":
                    modulosPermitidos.AddRange(new[] {
                        "dashboard", "ventas", "clientes", "reportes","giftcards"
                    });
                    break;

                case "Inventario":
                    modulosPermitidos.AddRange(new[] {
                        "dashboard", "productos", "proveedores", "compras", "stock", "reportes", "reglas-precio"
                    });
                    break;

                default:
                    modulosPermitidos.Add("dashboard");
                    break;
            }

            // 🟢 Respuesta al frontend
            return Ok(new
            {
                accessToken = token,
                id = user.Id,
                nombre = user.Nombre,
                rol = user.Rol?.Nombre ?? "SinRol",
                sucursalId = user.SucursalId,
                sucursalNombre = user.Sucursal?.Nombre ?? "No asignada",
                modulos = modulosPermitidos
            });
        }

        // ======================================================
        // 🔧 REGISTER
        // ======================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Usuario usuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                return BadRequest("El correo ya está registrado");

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario creado correctamente");
        }

        // ======================================================
        // 🔑 RECUPERAR CONTRASEÑA
        // ======================================================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Ok(new { message = "Si el correo está registrado, recibirás un enlace de recuperación." });

            try
            {
                var token = _tokenService.GenerateRecoveryToken(user);
                var resetLink = $"http://localhost:3000/reset-password?token={token}";

                await _emailService.SendEmailAsync(user.Email, "Recuperar contraseña",
                    $"Haz clic en el siguiente enlace para restablecer tu contraseña:<br/><a href='{resetLink}'>Resetear contraseña</a>");

                return Ok(new { message = "Si el correo está registrado, recibirás un enlace de recuperación." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error enviando correo: {ex.Message}");
            }
        }

        // ======================================================
        // 🔐 RESET PASSWORD
        // ======================================================
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtKey = _config["Jwt:Key"];

            try
            {
                var principal = handler.ValidateToken(dto.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"]
                }, out SecurityToken validatedToken);

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Token inválido: no contiene email");

                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return NotFound("Usuario no encontrado");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                return Ok("Contraseña restablecida correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest($"Token inválido o expirado: {ex.Message}");
            }
        }
    }
}