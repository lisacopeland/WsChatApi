namespace webchat.Models
{
    public class ApiResponseClass
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public string? Id { get; set; }
    }
}
