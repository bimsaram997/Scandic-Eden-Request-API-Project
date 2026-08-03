namespace EdenRequest.Api.Data
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Housekeeper"; 

        public string? PushEndpoint { get; set; }
        public string? PushP256DH { get; set; }
        public string? PushAuth { get; set; }

    }

    public class LoginRequestModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


}
