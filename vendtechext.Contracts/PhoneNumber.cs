using System.ComponentModel.DataAnnotations;

namespace vendtechext.Contracts
{
    public class PhoneNumberDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string NameOnNumber { get; set; }
        public string? Alias { get; set; }
        public string NumberMake { get; set; }
        public bool Enabled { get; set; }
        public string UserId { get; set; }
        public DateTime DateSaved { get; set; }
    }

    public class CreatePhoneNumberDto
    {
        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Phone number must be exactly 8 digits")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Phone number must contain only digits")]
        public string Number { get; set; }
        
        [Required]
        [StringLength(100)]
        public string NameOnNumber { get; set; }
        
        [StringLength(100)]
        public string? Alias { get; set; }
        
        [Required]
        [StringLength(50)]
        public string NumberMake { get; set; }
        
        public bool Enabled { get; set; } = true;
    }

    public class UpdatePhoneNumberDto : CreatePhoneNumberDto
    {
        public string Id { get; set; }
    }
}
