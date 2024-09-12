using System.ComponentModel.DataAnnotations;

namespace vendtechext.BLL.DTO
{
    public class BusinessUserQueryDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string Clientkey { get; set; }
        public string ApiKey { get; set; }

    }

    public class BusinessUserCommandDTO
    {
        public Guid Id { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string Email { get; set; }

    }
}
