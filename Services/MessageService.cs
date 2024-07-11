﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using webchat.Models;
using webchat.Utilities;

namespace webchat.Service
{
    public class MessageService
    {
        private readonly IConfiguration _config;
        private readonly AmazonDynamoDBClient _client;
        private readonly string _pkVal;
        public MessageService(
            IConfiguration config)
        {
            _config = config;
            _client = new AmazonDynamoDBClient();
            _pkVal = "messages";
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
                }
            };
            QueryResponse response = await _client.QueryAsync(request);
            var responseList = response.Items;
            List<MessageClass> messages = MessageClass.GetMessagesFromQueryResponse(responseList);
            return messages;
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
