using System.ComponentModel.DataAnnotations;

namespace SickDiary.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login is required.")]
        [StringLength(50,MinimumLength =3, ErrorMessage = "Login must be between 3 and 50 characters.")]
        public string Login {  get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength =6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = string.Empty;

    }
}
