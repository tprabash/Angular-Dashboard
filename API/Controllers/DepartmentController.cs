using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentDbContext _departmentDbContext;

        public DepartmentController(DepartmentDbContext departmentDbContext)
        {
            _departmentDbContext = departmentDbContext;
        }

        [HttpGet]
        [Route("GetDepartment")]
        public async Task<IEnumerable<Department>> GetDepartment()
        {
            return await _departmentDbContext.GetAllDepartmentsAsync();
        }

        [HttpPost]
        [Route("AddDepartment")]
        public async Task<IActionResult> AddDepartment([FromBody] Department department)
        {
            if (department == null)
            {
                return BadRequest("Department data is null");
            }

            await _departmentDbContext.SaveDepartmentAsync(department);
            return Ok(department);
        }

        [HttpPatch]
        [Route("UpdateDepartment")]
        public async Task<IActionResult> UpdateDepartment([FromBody] Department department)
        {
            await _departmentDbContext.UpdateDepartmentAsync(department);
            return Ok(department);
        }

        [HttpDelete]
        [Route("DeleteDepartment/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            await _departmentDbContext.DeleteDepartmentByIdAsync(id);
            return NoContent();
        }
    }
}
