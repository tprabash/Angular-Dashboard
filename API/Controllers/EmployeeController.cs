using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeDbContext _employeeDbContext;

        public EmployeeController(EmployeeDbContext employeeDbContext)
        {
            _employeeDbContext = employeeDbContext;
        }

        [HttpGet]
        [Route("GetEmployee")]
        public async Task<IEnumerable<Employee>> GetEmployee()
        {
            return await _employeeDbContext.GetAllEmployeesAsync();
        }

        [HttpGet]
        [Route("GetDepartent")]
        public async Task<IEnumerable<Department>> GetDepartent()
        {
            return await _employeeDbContext.GetAllDepartmentAsync();
        }

        [HttpPost]
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeSave employeeSave)
        {
            if (employeeSave == null)
            {
                return BadRequest("Employee data is null");
            }

            await _employeeDbContext.SaveEmployeeAsync(employeeSave);
            return Ok(employeeSave);
        }

        [HttpPatch]
        [Route("UpdateEmployee")]
        public async Task<IActionResult> UpdateEmployee([FromBody] EmployeeSave employeeSave)
        {
            await _employeeDbContext.UpdateEmployeeAsync(employeeSave);
            return Ok(employeeSave);
        }

        [HttpDelete]
        [Route("DeleteEmployee/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await _employeeDbContext.DeleteEmployeeByIdAsync(id);
            return NoContent();
        }
    }
}
