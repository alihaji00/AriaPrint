namespace AriaPrint.Models
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; } = "کاربر";
        public string LastName { get; set; } = "";
        public string Phone { get; set; } = "_";
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public string Initial { get; set; }
    }
}