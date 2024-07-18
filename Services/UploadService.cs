using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using webchat.Models;
using webchat.Utilities;

namespace webchat.Service
{
    public class UploadService
    {
        private readonly IConfiguration _config;
        private readonly IAmazonS3 _s3Client;
        public UploadService(
            IConfiguration config)
        {
            _config = config;
            string secretKey = _config["awsSecretKey"];
            string accessKey = _config["awsAccessKey"];
            _s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USWest2);
        }


        public async Task<ApiResponseClass> UploadMessageAsset(string messageId, IFormFile file)
        {
            string location;
            string bucketName = WSConstants.WSS3BucketName;
            ApiResponseClass result;
            String extension = Path.GetExtension(file.FileName);
            if (extension != null)
            {
                location = $"Assets/{messageId}{extension}";
            } else
            {
                location = $"Assets/{messageId}";
            }
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var putRequest = new PutObjectRequest
                    {
                        Key = location,
                        BucketName = bucketName,
                        InputStream = stream,
                        AutoCloseStream = true,
                        ContentType = file.ContentType
                    };
                    var putObjectResponse = await _s3Client.PutObjectAsync(putRequest);
                }
                result = new ApiResponseClass()
                {
                    Success = true,
                    Message = "File uploaded successfully" 
                };
            }
            catch (Exception ex)
            {
                result = new ApiResponseClass()
                {
                    Success = false,
                    Message = ex.Message
                };

            }
            return result;
        }
    }
}
