namespace ProjectManagementApp.API.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
         public string Lastname { get; set; }
         public string Email { get; set; }

         public  RegisterModel()
        {
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}