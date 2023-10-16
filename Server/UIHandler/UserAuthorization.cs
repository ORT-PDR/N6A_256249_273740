using Communication;
using Server.BusinessLogic;

namespace Server.UIHandler
{
    public class UserAuthorization
    {
        private readonly NetworkDataHelper networkDataHelper;
        private readonly ConversionHandler conversionHandler;
        private readonly UserService _userService;

        public UserAuthorization(NetworkDataHelper networkDataHelper, ConversionHandler conversionHandler, UserService userService)
        {
            this.networkDataHelper = networkDataHelper;
            this.conversionHandler = conversionHandler;
            _userService = userService;
        }

        public void Authenticate()
        {
            byte[] lengthBytes = networkDataHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] credentialsBytes = networkDataHelper.Receive(dataLength);
            string credentials = conversionHandler.ConvertBytesToString(credentialsBytes);
            string[] credentialsParts = credentials.Split(':');

            if (credentialsParts.Length == 2)
            {
                string username = credentialsParts[0];
                string password = credentialsParts[1];

                bool authenticationResult = _userService.AuthenticateUser(username, password);

                string response = authenticationResult ? "Authentication successful" : "Authentication failed";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                SendResponse(responseBytes);
            }
            else
            {
                Console.WriteLine("Invalid credentials format.");
                byte[] responseBytes = conversionHandler.ConvertStringToBytes("Invalid credentials format");
                SendResponse(responseBytes);
            }
        }

        public void CreateUser()
        {
            byte[] lengthBytes = networkDataHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] credentialsBytes = networkDataHelper.Receive(dataLength);
            string credentials = conversionHandler.ConvertBytesToString(credentialsBytes);
            string[] credentialsParts = credentials.Split(':');

            if (credentialsParts.Length == 2)
            {
                try
                {
                    string username = credentialsParts[0];
                    string password = credentialsParts[1];
                    _userService.CreateUser(username, password);

                    Console.WriteLine("User created successfully.");
                    byte[] responseBytes = conversionHandler.ConvertStringToBytes("success");
                    SendResponse(responseBytes);
                }
                catch(Exception e)
                {
                    Console.WriteLine("New user credentials were not valid.");
                    byte[] responseBytes = conversionHandler.ConvertStringToBytes(e.Message);
                    SendResponse(responseBytes);
                }
            }
            else
            {
                Console.WriteLine("Invalid credentials format for new user.");
                byte[] responseBytes = conversionHandler.ConvertStringToBytes("Invalid credentials format");
                SendResponse(responseBytes);
            }
        }

        private void SendResponse(byte[] responseBytes)
        {
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            networkDataHelper.Send(lengthBytes);
            networkDataHelper.Send(responseBytes);
        }
    }
}