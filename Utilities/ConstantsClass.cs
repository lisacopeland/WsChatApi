using Amazon;
using webchat.Models;

namespace webchat.Utilities
{
    public static class WSConstants
    {
        public const string userMessageCreatedAction = "UserMessages: Created";
        public const string userEnteredAction = "Users: User Entered";
        public const string userExitedAction = "Users: User Exited";

        public const string WsChatTableName = "wschat";
        public const string WSS3BucketName = "wschatassets";


    }
}
