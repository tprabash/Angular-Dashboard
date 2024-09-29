using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^(011|012|010)\d{8}$", ErrorMessage = "Invalid mobile phone number")]
        public string Mobile { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string FileName { get; set; }


    }
}
