using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class EmployeeContext:DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        { 
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<FTPSetting> fTPSettings { get; set; }
    }
}
