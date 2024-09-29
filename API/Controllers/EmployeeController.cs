using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeController(IEmployeeRepository repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetALLEmployees()
        {
            var employees = await _repo.GetAllEmployeesAsync();
            return Ok(employees);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromForm] EmployeeToReturnDTO emp)
        {
            var employee = await _repo.AddEmployeeAsync(emp);
            return Ok(emp);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployee(int empId, [FromForm] EmployeeToReturnDTO emp)
        {
            var employee = await _repo.UpdateEmployeeAsync(empId, emp);
            return Ok(emp);
        }

        [HttpGet("employees/{id}/file")]
        public async Task<IActionResult> DownloadEmployeeFile(int id)
        {
            try
            {
                var employee = await _repo.GetEmployeeById(id);

                if (employee == null)
                {
                    return NotFound();
                }

                // Get the file content from the FTP server
                var ftpSettings = await _repo.GetFTPSettings();
                var ftpServerUrl = ftpSettings.ServerUrl;
                var ftpUsername = ftpSettings.UserName;
                var ftpPassword = ftpSettings.Password;
                var filePath = ftpServerUrl + employee.FileName;

                using (var client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                    var fileBytes = await client.DownloadDataTaskAsync(filePath);

                    // Return the file content as a downloadable file
                    return File(fileBytes, "application/octet-stream", employee.FileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("DownloadFile/{fileName}")]
        public async Task<IActionResult> DownloadEmployeeFile(string fileName)
        {
            try
            {
                //// Find the employee by ID
                //var employee = await _repo.GetEmployeeById(empId);
                //if (employee == null)
                //{
                //    throw new Exception("Employee not found.");
                //}

                //// Retrieve the file name from the employee entity
                //var fileName = employee.FileName;

                // Get the FTP server settings
                var ftpSettings = await _repo.GetFTPSettings();
                var ftpServerUrl = ftpSettings.ServerUrl;
                var ftpUsername = ftpSettings.UserName;
                var ftpPassword = ftpSettings.Password;

                // Create the FTP file URL
                var fileUrl = ftpServerUrl + fileName;

                // Create an FtpWebRequest
                var request = (FtpWebRequest)WebRequest.Create(fileUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Get the response from the FTP server
                using (var response = (FtpWebResponse)request.GetResponse())
                {
                    using (var ftpStream = response.GetResponseStream())
                    {
                        // Read the file content
                        using (var memoryStream = new MemoryStream())
                        {
                            ftpStream.CopyTo(memoryStream);
                            var fileContent = memoryStream.ToArray();

                            // Set the content type header based on the file extension
                            var contentType = GetContentType(fileName);
                            Response.Headers["Content-Type"] = contentType;

                            // Return the file content as a response
                            return File(fileContent, contentType);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private string GetContentType(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (fileExtension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".txt":
                    return "application/txt";
                case ".doc":
                case ".docx":
                    return "application/msword";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.ms-excel";
                // Add more file extensions and corresponding content types as needed
                default:
                    return "application/octet-stream";
            }
        }

            [HttpDelete("DeleteEmployee")]
        public async Task<IActionResult> DeleteEmployee(int empId)
        {
            await _repo.DeleteEmployeeAsync(empId);
            return Ok();
        }

        [HttpDelete("DeleteListOfEmployee")]
        public async Task<IActionResult> DeleteListOfEmployees([FromBody]List<int> empIds)
        {
            await _repo.DeleteListOfEmployeesAsync(empIds);
            return Ok();
        }

    }
}
