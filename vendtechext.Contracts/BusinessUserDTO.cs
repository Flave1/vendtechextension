namespace vendtechext.Contracts
{
    public class BusinessUserDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string ApiKey { get; set; }
        public string Email { get; set; }

    }

    public class BusinessUserCommandDTO
    {
        public string BusinessName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
    }
}
