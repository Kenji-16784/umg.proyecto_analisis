namespace ApiUsuarios.Models
{
    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;   // El token enviado al correo
        public string NewPassword { get; set; } = string.Empty; // Nueva contraseña del usuario
    }
}
