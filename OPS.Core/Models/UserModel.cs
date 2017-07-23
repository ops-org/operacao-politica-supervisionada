using System.ComponentModel.DataAnnotations;

namespace OPS.Core.Models
{
	public class UserModel
    {
        [Required]
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string FullName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter mais de {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação de senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não correspondem.")]
        public string ConfirmPassword { get; set; }
    }

    public class PasswordRecoverModel
    {
        [Required]
        [Display(Name = "Identificador de Usuário")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Token")]
        public string Token { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter mais de {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação de senha")]
        [Compare("NewPassword", ErrorMessage = "A senha e a confirmação de senha não correspondem.")]
        public string ConfirmNewPassword { get; set; }
    }

    public class VerifyEmailModel
    {
        [Required]
        [Display(Name = "Identificador de Usuário")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Token")]
        public string Token { get; set; }
    }

}