using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
    }
}
