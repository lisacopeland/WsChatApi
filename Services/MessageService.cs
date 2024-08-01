using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using webchat.Models;
using webchat.Utilities;
using WsChatApi.Models;

namespace webchat.Service
{
    public class MessageService
    {
        private readonly IConfiguration _config;
        private readonly AmazonDynamoDBClient _client;
        private readonly UserService _userService;
        private readonly string _pkVal;

        public MessageService(IConfiguration Config, UserService UserService)
        {
            _pkVal = "messages";
            _config = Config;
            string awsAccessKey = _config["AWS_ACCESS_KEY"];
            string awsSecretKey = _config["AWS_SECRET_KEY"];
            var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USWest2);
            _userService = UserService;
        }

        // Used
        public async Task<List<MessageClass>> GetAllAsync()
        {
            var request = new QueryRequest
            {
                TableName = WSConstants.WsChatTableName,
                KeyConditionExpression = "pk =:pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":pk",
                        new AttributeValue { S = _pkVal }
                    }
                },
                ScanIndexForward = false,
            };
            QueryResponse response = await _client.QueryAsync(request);
            var responseList = response.Items;
            List<MessageClass> messages = MessageClass.GetMessagesFromQueryResponse(responseList);
            return messages;
        }

        public async Task<List<MessageDTOClass>> GetDisplayMessagesAsync()
        {
            List<UserClass> users = new List<UserClass>();
            var request = new QueryRequest
            {
                TableName = WSConstants.WsChatTableName,
                KeyConditionExpression = "pk =:pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":pk",
                        new AttributeValue { S = _pkVal }
                    }
                },
                ScanIndexForward = false,
            };
            QueryResponse response = await _client.QueryAsync(request);
            var responseList = response.Items;
            List<MessageClass> messages = MessageClass.GetMessagesFromQueryResponse(responseList);

            var userNames = messages.Select(m => m.UserName).Distinct().ToList();

            if (userNames.Count > 0)
            {
                users = await _userService.GetUsersAsync(userNames);
            }

            var userMap = users.ToDictionary(user => user.UserName, user => user);

            List<MessageDTOClass> messagesWithUserDetails = messages
                .Select(message => new MessageDTOClass
                {
                    Id = message.Id,
                    Message = message.Message,
                    UserName = message.UserName,
                    MessageDate = message.MessageDate,
                    DisplayName = userMap[message.UserName].DisplayName,
                    Online = userMap[message.UserName].Online
                })
                .ToList();
            return messagesWithUserDetails;
        }

        //using
        public async Task<ApiResponseClass> CreateAsync(MessageClass newMessage)
        {
            ApiResponseClass result;
            Dictionary<string, AttributeValue> putMessage = MessageClass.CreatePutItem(newMessage);
            var putItemRequest = new PutItemRequest
            {
                TableName = WSConstants.WsChatTableName,
                Item = putMessage
            };
            PutItemResponse response = await _client.PutItemAsync(putItemRequest);

            result = new ApiResponseClass { Success = true };
            result.Id = newMessage.Id;
            result.Message = "Message created successfully";
            return result;
        }
    }
}
