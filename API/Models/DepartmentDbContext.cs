using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace API.Models
{
    public class DepartmentDbContext : DbContext
    {
        public DepartmentDbContext(DbContextOptions<DepartmentDbContext> options) : base(options)
        {
        }

        public DbSet<Department> Department { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Employee;User Id=sa;Password=1234;TrustServerCertificate=true;");
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await Department.FromSqlRaw("EXEC GetAllDepartments").ToListAsync();
        }

        public async Task SaveDepartmentAsync(Department department)
        {
            await Database.ExecuteSqlRawAsync("EXEC SaveDepartments @DepartmentName, @DepartmentCode",
                new SqlParameter("@DepartmentName", department.DepartmentName),
                new SqlParameter("@DepartmentCode", department.DepartmentCode));
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            await Database.ExecuteSqlRawAsync("EXEC UpdateDepartments @Id, @DepartmentName, @DepartmentCode",
                new SqlParameter("@Id", department.Id),
                new SqlParameter("@DepartmentName", department.DepartmentName),
                new SqlParameter("@DepartmentCode", department.DepartmentCode));
        }

        public async Task DeleteDepartmentByIdAsync(int id)
        {
            await Database.ExecuteSqlRawAsync("EXEC DeleteDepartments @Id",
                new SqlParameter("@Id", id));
        }
    }
}
