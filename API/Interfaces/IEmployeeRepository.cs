using API.DTOs;
using API.Entities;
using API.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IReadOnlyList<EmployeeToReturnDTO>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeById(int empId);
        Task<FTPSetting> GetFTPSettings();
        Task<Employee> AddEmployeeAsync(EmployeeToReturnDTO emp);
        Task<Employee> UpdateEmployeeAsync(int empId,EmployeeToReturnDTO emp);
        //Task<FileContentResult> DownloadEmployeeFileAsync(int empId);
        Task DeleteEmployeeAsync(int empId);
        Task DeleteListOfEmployeesAsync(List<int> EmployeeIds);

    }
}
