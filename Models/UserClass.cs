using Amazon.DynamoDBv2.Model;
using System.Text.Json.Serialization;

namespace webchat.Models
{
    public class UserClass
    {
        [JsonPropertyName("pk")]
        public string Pk => "users";

        [JsonPropertyName("sk")]
        public string SK => UserName;
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool Online { get; set; }

        public static Dictionary<string, AttributeValue> CreatePutItem(UserClass user)
        {
            if (user.Id == null)
            {
              user.Id = Guid.NewGuid().ToString();
            }
            return new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S =  "users" } },
                { "sk", new AttributeValue { S =  user.UserName } },
                { "Id", new AttributeValue { S =  user.Id } },
                { "UserName", new AttributeValue { S = user.UserName } },
                { "Email", new AttributeValue{ S = user.Email } },
                { "DisplayName", new AttributeValue { S = user.DisplayName} },
                { "CreatedDate", new AttributeValue { S = user.CreatedDate.ToString() } },
                { "Online", new AttributeValue { BOOL = user.Online} }
            };
        }

        public static List<UserClass> GetUsersFromQueryResponse(List<Dictionary<string, AttributeValue>> queryResponse)
        {
            var users = new List<UserClass>();

            foreach (var item in queryResponse)
            {
                var user = new UserClass
                {
                    Id = item.TryGetValue("Id", out var id) ? id.S : null,
                    UserName = item.TryGetValue("UserName", out var userName) ? userName.S : null,
                    Email = item.TryGetValue("Email", out var email) ? email.S : null,
                    DisplayName = item.TryGetValue("DisplayName", out var displayName) ? displayName.S : null,
                    CreatedDate = item.TryGetValue("CreateDate", out var createDateStr) && DateTime.TryParse(createDateStr.S, out var createDate) ? createDate : (DateTime?)null,
                    Online = item.TryGetValue("Online", out var online) && online.BOOL
                };
                users.Add(user);
            }

            return users;
        }
    }
}
