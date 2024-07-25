using Amazon.DynamoDBv2.Model;
using System.Text.Json.Serialization;

namespace webchat.Models
{
    public class MessageClass
    {
        [JsonPropertyName("pk")]
        public string Pk => $"messages:{Id}";

        [JsonPropertyName("sk")]
        public string SK => MessageDate.ToString();
        public string? Id { get; set; }

        public string Message { get; set; }

        public string? UserName { get; set; }

        public DateTime? MessageDate { get; set; }
        public static Dictionary<string, AttributeValue> CreatePutItem(MessageClass message)
        {
            message.Id = Guid.NewGuid().ToString();
            return new Dictionary<string, AttributeValue>
            {
                { "pk", new AttributeValue { S =  "messages" } },
                { "sk", new AttributeValue { S =  message.MessageDate.ToString() } },
                { "Id", new AttributeValue { S =  message.Id } },
                { "UserName", new AttributeValue { S = message.UserName } },
                { "Message", new AttributeValue{ S = message.Message } },
                { "MessageDate", new AttributeValue { S = message.MessageDate.ToString() } },
            };
        }

        public static List<MessageClass> GetMessagesFromQueryResponse(List<Dictionary<string, AttributeValue>> queryResponse)
        {
            var users = new List<MessageClass>();

            foreach (var item in queryResponse)
            {
                var user = new MessageClass
                {
                    Id = item.TryGetValue("Id", out var id) ? id.S : null,
                    UserName = item.TryGetValue("UserName", out var userName) ? userName.S : null,
                    Message = item.TryGetValue("Message", out var message) ? message.S : null,
                    MessageDate = item.TryGetValue("MessageDate", out var messageDateStr) && DateTime.TryParse(messageDateStr.S, out var messageDate) ? messageDate : (DateTime?)null,
                };
                users.Add(user);
            }

            return users;
        }
    }
}
