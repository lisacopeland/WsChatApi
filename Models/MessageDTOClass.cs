namespace WsChatApi.Models
{
    public class MessageDTOClass
    {
        public string? Id { get; set; }
        public string? Message { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public bool? Online { get; set; }
        public DateTime? MessageDate { get; set; }
    }
}
