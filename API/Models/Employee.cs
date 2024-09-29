﻿using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }

        public string DOB { get; set; }

        public int Age { get; set; }
        
        public int Salary { get; set; }
        
        public int DepartmentId { get; set; }
        
        public string DepartmentName { get; set; }
        
        public string DepartmentCode { get; set; }
    }
}