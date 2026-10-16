using System.ComponentModel.DataAnnotations;

namespace AriaPrint.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "فرمت شماره موبایل اشتباه است")]
        public long PhoneNumber { get; set; }
    }
}
