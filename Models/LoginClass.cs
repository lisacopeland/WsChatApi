using System.Text.Json.Serialization;

namespace webchat.Models
{
    public class LoginClass
    {
        public string UserName {  get; set; }
        public string Password { get; set; }
    }
    public class LogoutClass
    {
        public string Id { get; set; }
    }
}
