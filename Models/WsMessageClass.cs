namespace webchat.Models
{

    public interface IPayload
    {
        // This can be empty or contain common properties/methods
    }
    public class User : IPayload
    {
        public UserClass UserClass { get; set; }
    }

    public class UserId : IPayload
    {
        public string id { get; set; }
    }

    public class Message : IPayload
    {
        public MessageClass MessageClass { get; set; }
    }
    public class ActionPayload
    {
        public string Action { get; set; }
        public IPayload Payload { get; set; }
    }
}
