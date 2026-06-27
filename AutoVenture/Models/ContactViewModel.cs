using System.ComponentModel.DataAnnotations;

namespace AutoVenture.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Моля, въведете вашето име.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, въведете съобщение.")]
        public string Message { get; set; } = string.Empty;
    }
}
