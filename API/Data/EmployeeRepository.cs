using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace API.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeContext _context;

        public EmployeeRepository(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetEmployeeById(int empId)
        {
            return await _context.Employees.FindAsync(empId);
        }

        public async Task<FTPSetting> GetFTPSettings() // Replace with the correct type
        {
            return await _context.fTPSettings.FirstOrDefaultAsync();
        }

        private async Task DeleteFileFromFtp(string fileName)
        {
            // Get FTP settings
            var ftpSettings = _context.fTPSettings.FirstOrDefault();
            var ftpServerUrl = ftpSettings.ServerUrl;
            var ftpUsername = ftpSettings.UserName;
            var ftpPassword = ftpSettings.Password;
            var filePath = ftpServerUrl + fileName;

            // Create an FtpWebRequest
            var request = (FtpWebRequest)WebRequest.Create(filePath);
            // Set the method to DeleteFile
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            // Set the NetworkCredentials
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            // Send the request to delete the file
            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                // Check if the deletion was successful
                if (response.StatusCode != FtpStatusCode.FileActionOK)
                {
                    throw new Exception("Failed to delete the file from the FTP server.");
                }
            }
        }

        public async Task DeleteEmployeeAsync(int empId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(empId);
                if (employee != null)
                {
                    await DeleteFileFromFtp(employee.FileName);
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting an employee.", ex);
            }
        }

        public async Task DeleteListOfEmployeesAsync(List<int> employeeIds)
        {
            try
            {
                var employeesToDelete = await _context.Employees
                    .Where(e => employeeIds.Contains(e.Id))
                    .ToListAsync();

                if (employeesToDelete.Any())
                {
                    // Delete files from FTP server
                    foreach (var employee in employeesToDelete)
                    {
                        await DeleteFileFromFtp(employee.FileName);
                    }

                    // Remove employees from the database
                    _context.Employees.RemoveRange(employeesToDelete);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting a list of employees.", ex);
            }
        }

        private async Task<byte[]> GetFileFromFtp(string fileName)
        {
            var ftpSettings = _context.fTPSettings.FirstOrDefault();
            var ftpServerUrl = ftpSettings.ServerUrl;
            var ftpUsername = ftpSettings.UserName;
            var ftpPassword = ftpSettings.Password;
            var filePath = ftpServerUrl + fileName;

            // Create an FtpWebRequest
            var request = (FtpWebRequest)WebRequest.Create(filePath);
            // Set the method to DownloadFile
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            // Set the NetworkCredentials
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<IReadOnlyList<EmployeeToReturnDTO>> GetAllEmployeesAsync()
        {
            try
            {
                var employees = await _context.Employees.ToListAsync();

                var employeeToReturn = employees
                    .Select(employee => new EmployeeToReturnDTO
                    {
                        Id = employee.Id,
                        Name = employee.Name,
                        Email = employee.Email,
                        Mobile = employee.Mobile,
                        Address = employee.Address,
                        FileName = employee.FileName,
                        File = null // Initialize the File property to avoid null reference
                    })
                    .ToList();

                // Load the file content for each employee
                foreach (var employee in employeeToReturn)
                {
                    employee.File = await LoadFileAsync(employee.FileName);
                }

                return employeeToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all employees.", ex);
            }
        }

        private async Task<IFormFile> LoadFileAsync(string fileName)
        {
            var ftpSettings = _context.fTPSettings.FirstOrDefault();
            var ftpServerUrl = ftpSettings.ServerUrl;
            var ftpUsername = ftpSettings.UserName;
            var ftpPassword = ftpSettings.Password;
            var filePath = ftpServerUrl + fileName;

            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Download the file as a byte array
                var fileBytes = await client.DownloadDataTaskAsync(filePath);

                // Create an IFormFile instance from the byte array
                var memoryStream = new MemoryStream(fileBytes);
                return new FormFile(memoryStream, 0, fileBytes.Length, null, fileName);
            }
        }

        public async Task<Employee> AddEmployeeAsync(EmployeeToReturnDTO emp)
        {
            try
            {
                // Check for duplicate email
                var emailExists = await IsEmailDuplicate(emp.Email);
                if (emailExists)
                {
                    throw new Exception("An employee with the same email already exists.");
                }

                // Check for duplicate phone number
                var phoneExists = await IsPhoneNumberDuplicate(emp.Mobile);
                if (phoneExists)
                {
                    throw new Exception("An employee with the same phone number already exists.");
                }

                // Upload file to FTP server
                var ftpSettings = _context.fTPSettings.FirstOrDefault();
                var ftpServerUrl = ftpSettings.ServerUrl;
                var ftpUsername = ftpSettings.UserName;
                var ftpPassword = ftpSettings.Password;
                var fileId = Guid.NewGuid();
                var fileName = fileId + emp.File.FileName;
                var filePath = ftpServerUrl + fileName;

                await UploadFileToFtp(emp.File.OpenReadStream(), filePath, ftpUsername, ftpPassword);

                // Create an Employee instance and map the properties
                var employee = MapToEmployee(emp, fileName);

                // Add employee to the database
                await AddEmployeeToDatabase(employee);

                // Return the mapped Employee instance
                return employee;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding an employee.", ex);
            }
        }

        public async Task<Employee> UpdateEmployeeAsync(int employeeId, EmployeeToReturnDTO emp)
        {
            try
            {
                // Find the employee by ID
                var existingEmployee = await _context.Employees.FindAsync(employeeId);
                if (existingEmployee == null)
                {
                    throw new Exception("Employee not found.");
                }

                // Check for duplicate email
                var emailExists = await _context.Employees.AnyAsync(e => e.Email == emp.Email && e.Id != employeeId && e.Id != emp.Id);
                if (emailExists)
                {
                    throw new Exception("An employee with the same email already exists.");
                }

                // Check for duplicate phone number
                var phoneExists = await _context.Employees.AnyAsync(e => e.Mobile == emp.Mobile && e.Id != employeeId && e.Id != emp.Id);
                if (phoneExists)
                {
                    throw new Exception("An employee with the same phone number already exists.");
                }


                // Upload file to FTP server (if a new file is provided)
                if (emp.File != null)
                {
                    if(existingEmployee.FileName != emp.FileName)
                    {
                        var ftpSettings = _context.fTPSettings.FirstOrDefault();
                        var ftpServerUrl = ftpSettings.ServerUrl;
                        var ftpUsername = ftpSettings.UserName;
                        var ftpPassword = ftpSettings.Password;
                        var fileId = Guid.NewGuid();
                        var fileName = fileId + emp.File.FileName;
                        var filePath = ftpServerUrl + fileName;

                        // Check if employee already has a file
                        if (!string.IsNullOrEmpty(existingEmployee.FileName))
                        {
                            await DeleteFileFromFtp(existingEmployee.FileName);
                        }
                        await UploadFileToFtp(emp.File.OpenReadStream(), filePath, ftpUsername, ftpPassword);

                        // Update the file name in the Employee entity
                        existingEmployee.FileName = fileName;
                    }
                }

                // Update other properties of the Employee entity
                existingEmployee.Name = emp.Name;
                existingEmployee.Email = emp.Email;
                existingEmployee.Mobile = emp.Mobile;
                existingEmployee.Address = emp.Address;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Return the updated Employee instance
                return existingEmployee;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the employee.", ex);
            }
        }

        //public async Task<FileContentResult> DownloadEmployeeFileAsync(int employeeId)
        //{
        //    try
        //    {
        //        // Find the employee by ID
        //        var employee = await _context.Employees.FindAsync(employeeId);
        //        if (employee == null)
        //        {
        //            throw new Exception("Employee not found.");
        //        }

        //        // Retrieve the file name from the employee entity
        //        var fileName = employee.FileName;

        //        // Get the FTP server settings
        //        var ftpSettings = _context.fTPSettings.FirstOrDefault();
        //        var ftpServerUrl = ftpSettings.ServerUrl;
        //        var ftpUsername = ftpSettings.UserName;
        //        var ftpPassword = ftpSettings.Password;

        //        // Create the FTP file URL
        //        var fileUrl = ftpServerUrl + fileName;

        //        // Create an FtpWebRequest
        //        var request = (FtpWebRequest)WebRequest.Create(fileUrl);
        //        request.Method = WebRequestMethods.Ftp.DownloadFile;
        //        request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        //        // Get the response from the FTP server
        //        using (var response = (FtpWebResponse)request.GetResponse())
        //        {
        //            using (var ftpStream = response.GetResponseStream())
        //            {
        //                // Read the file content
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    ftpStream.CopyTo(memoryStream);
        //                    var fileContent = memoryStream.ToArray();

        //                    // Determine the file's content type
        //                    var contentType = "application/octet-stream"; // Default content type

        //                    // Get the file extension from the file name
        //                    var fileExtension = Path.GetExtension(fileName);

        //                    // Create a new FileExtensionContentTypeProvider
        //                    var provider = new FileExtensionContentTypeProvider();

        //                    // Try to determine the content type based on the file extension
        //                    if (provider.TryGetContentType(fileExtension, out var resolvedContentType))
        //                    {
        //                        contentType = resolvedContentType;
        //                    }

        //                    // Return the file content as a response
        //                    return new FileContentResult(fileContent, contentType)
        //                    {
        //                        FileDownloadName = fileName
        //                    };
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("An error occurred while downloading the file.", ex);
        //    }
        //}



        private async Task<bool> IsEmailDuplicate(string email)
        {
            return await _context.Employees.AnyAsync(e => e.Email == email);
        }

        private async Task<bool> IsPhoneNumberDuplicate(string phoneNumber)
        {
            return await _context.Employees.AnyAsync(e => e.Mobile == phoneNumber);
        }

        private Employee MapToEmployee(EmployeeToReturnDTO emp, string fileName)
        {
            return new Employee
            {
                Name = emp.Name,
                Email = emp.Email,
                Mobile = emp.Mobile,
                Address = emp.Address,
                FileName = fileName
            };
        }

        private async Task AddEmployeeToDatabase(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        private async Task UploadFileToFtp(Stream fileStream, string filePath, string username, string password)
        {
            // Create an FtpWebRequest
            var request = (FtpWebRequest)WebRequest.Create(filePath);
            // Set the method to UploadFile
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // Set the NetworkCredentials
            request.Credentials = new NetworkCredential(username, password);

            // Upload the file to the FTP server
            using (var stream = fileStream)
            {
                using (var ftpStream = request.GetRequestStream())
                {
                    stream.CopyTo(ftpStream);
                }
            }
        }

    }
}
