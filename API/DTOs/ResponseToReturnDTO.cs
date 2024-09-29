namespace API.DTOs
{
    public class ResponseToReturnDTO
    {
        public List<EmployeeToReturnDTO> Employee { get; set; }
        public string FileLink { get; set; }
    }
}
