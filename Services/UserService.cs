using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using webchat.Models;
using webchat.Utilities;

namespace webchat.Service
{
    public class UserService
    {
        private readonly IConfiguration _config;
        private readonly AmazonDynamoDBClient _client;
        private readonly string _pkVal;

        public UserService(IConfiguration config)
        {
            _config = config;
            string awsAccessKey = _config["AWS_ACCESS_KEY"];
            string awsSecretKey = _config["AWS_SECRET_KEY"];
            var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            _client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.USWest2);
            _pkVal = "users";
        }

        public async Task<List<UserClass>> GetAllAsync()
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
                }
            };
            QueryResponse response = await _client.QueryAsync(request);
            var responseList = response.Items;
            List<UserClass> users = UserClass.GetUsersFromQueryResponse(responseList);
            return users;
        }

        public async Task<List<UserClass>> GetLoggedInUsersAsync()
        {
            var request = new QueryRequest
            {
                TableName = WSConstants.WsChatTableName,
                KeyConditionExpression = "Online = :online AND pk = :pkval",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":online",
                        new AttributeValue { BOOL = true }
                    },
                    {
                        ":pkval",
                        new AttributeValue { S = _pkVal }
                    }
                }
            };
            QueryResponse response = await _client.QueryAsync(request);
            var UserResponseList = response.Items;
            List<UserClass> users = UserClass.GetUsersFromQueryResponse(UserResponseList);
            return users;
        }

        public async Task<UserClass> GetAsync(string id)
        {
            var request = new ScanRequest
            {
                TableName = WSConstants.WsChatTableName,
                FilterExpression = "pk = :pk and Id = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":pk",
                        new AttributeValue { S = _pkVal }
                    },
                    {
                        ":id",
                        new AttributeValue { S = id }
                    }
                }
            };
            ScanResponse response = await _client.ScanAsync(request);
            var UserResponseList = response.Items;
            List<UserClass> users = UserClass.GetUsersFromQueryResponse(UserResponseList);
            return users.FirstOrDefault();
        }

        public async Task<UserClass> GetByUsernameAsync(string userName)
        {
            var request = new QueryRequest
            {
                TableName = WSConstants.WsChatTableName,
                KeyConditionExpression = "pk = :pkval and sk = :skval",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":skval",
                        new AttributeValue { S = userName }
                    },
                    {
                        ":pkval",
                        new AttributeValue { S = _pkVal }
                    }
                }
            };
            QueryResponse response = await _client.QueryAsync(request);
            var UserResponseList = response.Items;
            List<UserClass> users = UserClass.GetUsersFromQueryResponse(UserResponseList);
            return users.FirstOrDefault();
        }

        public async Task<ApiResponseClass> CreateAsync(UserClass newUser)
        {
            UserClass user = await GetByUsernameAsync(newUser.UserName);
            ApiResponseClass result;
            if (user != null)
            {
                result = new ApiResponseClass { Success = false };
                result.Message = "User already exists";
                return result;
            }
            newUser.Online = false;
            newUser.CreatedDate = DateTime.Now;
            Dictionary<string, AttributeValue> putUser = UserClass.CreatePutItem(newUser);
            var putItemRequest = new PutItemRequest
            {
                TableName = WSConstants.WsChatTableName,
                Item = putUser
            };
            PutItemResponse response = await _client.PutItemAsync(putItemRequest);

            result = new ApiResponseClass { Success = true };
            result.Id = newUser.Id;
            result.Message = "User created successfully";
            return result;
        }

        public async Task<ApiResponseClass> UpdateAsync(string id, UserClass newUser)
        {
            ApiResponseClass result;
            try
            {
                UserClass user = await GetAsync(id);
                if (user == null)
                {
                    result = new ApiResponseClass { Success = false };
                    result.Message = "Cant find user";
                    return result;
                }
                Dictionary<string, AttributeValue> putUser = UserClass.CreatePutItem(newUser);
                var putItemRequest = new PutItemRequest
                {
                    TableName = WSConstants.WsChatTableName,
                    Item = putUser
                };
                PutItemResponse response = await _client.PutItemAsync(putItemRequest);

                result = new ApiResponseClass { Success = true };
                result.Id = newUser.Id;
                result.Message = "User updated successfully";
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got error from update {ex.Message}");
                result = new ApiResponseClass { Success = false };
                result.Message = "Error updating user";
                return result;
            }
        }
    }
}
