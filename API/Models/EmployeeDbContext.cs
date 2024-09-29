using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace API.Models
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employee { get; set; }

        public DbSet<Department> Department { get; set; }

        public DbSet<EmployeeSave> EmployeeSave { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Employee;User Id=sa;Password=1234;TrustServerCertificate=true;");
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await Employee.FromSqlRaw("EXEC GetAllEmployees").ToListAsync();
        }

        public async Task<List<Department>> GetAllDepartmentAsync()
        {
            return await Department.FromSqlRaw("EXEC GetAllDepartments").ToListAsync();
        }

        public async Task SaveEmployeeAsync(EmployeeSave employeeSave)
        {
            await Database.ExecuteSqlRawAsync("EXEC SaveEmployees @FirstName, @LastName, @Email, @DOB, @Age, @Salary, @DepartmentId",
                new SqlParameter("@FirstName", employeeSave.FirstName),
                new SqlParameter("@LastName", employeeSave.LastName),
                new SqlParameter("@Email", employeeSave.Email),
                new SqlParameter("@DOB", employeeSave.DOB),
                new SqlParameter("@Age", employeeSave.Age),
                new SqlParameter("@Salary", employeeSave.Salary),
                new SqlParameter("@DepartmentId", employeeSave.DepartmentId));
        }

        public async Task UpdateEmployeeAsync(EmployeeSave employeeSave)
        {
            await Database.ExecuteSqlRawAsync("EXEC UpdateEmployees @Id, @FirstName, @LastName, @Email, @DOB, @Age, @Salary, @DepartmentId",
                new SqlParameter("@Id", employeeSave.Id),
                new SqlParameter("@FirstName", employeeSave.FirstName),
                new SqlParameter("@LastName", employeeSave.LastName),
                new SqlParameter("@Email", employeeSave.Email),
                new SqlParameter("@DOB", employeeSave.DOB),
                new SqlParameter("@Age", employeeSave.Age),
                new SqlParameter("@Salary", employeeSave.Salary),
                new SqlParameter("@DepartmentId", employeeSave.DepartmentId));
        }

        public async Task DeleteEmployeeByIdAsync(int id)
        {
            await Database.ExecuteSqlRawAsync("EXEC DeleteEmployees @Id",
                new SqlParameter("@Id", id));
        }
    }
}

