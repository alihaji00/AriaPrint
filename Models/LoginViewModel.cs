using System.ComponentModel.DataAnnotations;

namespace AriaPrint.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "رمز عبور الزامی است"), DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
