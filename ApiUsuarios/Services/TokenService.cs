using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ApiUsuarios.Models;

namespace ApiUsuarios.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        // ======================================================
        // 🔐 GENERAR TOKEN JWT (Inicio de sesión)
        // ======================================================
        public string GenerateToken(Usuario usuario)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("⚠️ No se encontró la clave JWT en la configuración.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? ""),
                new Claim(ClaimTypes.Name, usuario.Nombre ?? "SinNombre"),
                // 👉 Incluimos ambos formatos de rol para máxima compatibilidad
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "SinRol"),
                new Claim("role", usuario.Rol?.Nombre ?? "SinRol")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ======================================================
        // 🔑 GENERAR TOKEN PARA RECUPERACIÓN DE CONTRASEÑA
        // ======================================================
        public string GenerateRecoveryToken(Usuario usuario)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("⚠️ No se encontró la clave JWT en la configuración.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ✅ Solo incluye email y propósito de recuperación
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim("recovery", "true")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // ⏳ 30 minutos
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}