using System.ComponentModel.DataAnnotations;

namespace AriaPrint.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "نام الزامی است")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "رمز عبور الزامی است"), DataType(DataType.Password), MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "تکرار رمز عبور الزامی است"), DataType(DataType.Password), Compare("Password", ErrorMessage = "رمز عبور و تکرار آن یکسان نیست")]
        public string ConfirmPassword { get; set; } = "";
    }
}
