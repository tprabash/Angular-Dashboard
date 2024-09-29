using System.Text.Json.Serialization;

namespace API.DTOs
{
    public class EmployeeToReturnDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        [JsonIgnore]
        public IFormFile File { get; set; }
        public string FileName { get; set; }
    }
}
