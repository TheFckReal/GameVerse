using System.ComponentModel.DataAnnotations;

namespace GameVerse_recommendation.DTO
{
    public class RegistrationUserObject
    {
        [Required(ErrorMessage = "Адрес электронной почты должен быть непустым!")]
        [EmailAddress]
        [Display(Name = "Почта", Prompt = "name@example.com")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Никнейм должен быть непустым!")]
        [Display(Name = "Никнейм")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Пароль должен быть непустым!")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

    }
}
