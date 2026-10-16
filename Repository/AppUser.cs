namespace AriaPrint.Repository
{
    public class AppUser
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin" یا "User"
    }
}
