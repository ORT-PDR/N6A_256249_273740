using Communication;
using Server.BusinessLogic;

namespace Server.UIHandler
{
    public class UserAuthorization
    {
        private readonly SocketHelper socketHelper;
        private readonly ConversionHandler conversionHandler;
        private readonly UserService _userService;

        public UserAuthorization(SocketHelper socketHelper, ConversionHandler conversionHandler, UserService userService)
        {
            this.socketHelper = socketHelper;
            this.conversionHandler = conversionHandler;
            _userService = userService;
        }

        public void Authenticate()
        {
            Console.WriteLine("Authentication requested by client.");

            byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] credentialsBytes = socketHelper.Receive(dataLength);
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

                if (authenticationResult)
                {
                    // TODO MainMenu();
                }
            }
            else
            {
                Console.WriteLine("Invalid credentials format.");
                byte[] responseBytes = conversionHandler.ConvertStringToBytes("Invalid credentials format");
                SendResponse(responseBytes);
            }
        }

        private void SendResponse(byte[] responseBytes)
        {
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            socketHelper.Send(lengthBytes);
            socketHelper.Send(responseBytes);
        }
    }
}