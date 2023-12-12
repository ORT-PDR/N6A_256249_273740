using Communication;
using GrpcMainServer.Server.BusinessLogic;

namespace GrpcMainServer.Server.UIHandler;

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

        public async Task Authenticate()
        {
            byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] credentialsBytes = await networkDataHelper.ReceiveAsync(dataLength);
            string credentials = conversionHandler.ConvertBytesToString(credentialsBytes);
            string[] credentialsParts = credentials.Split("#");

            if (credentialsParts.Length == 2)
            {
                string username = credentialsParts[0];
                string password = credentialsParts[1];

                bool authenticationResult = _userService.AuthenticateUser(username, password);

                string response = authenticationResult ? "Authentication successful" : "Authentication failed";
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                await SendResponse(responseBytes);
            }
            else
            {
                Console.WriteLine("Invalid credentials format.");
                byte[] responseBytes = conversionHandler.ConvertStringToBytes("Invalid credentials format");
                await SendResponse(responseBytes);
            }
        }

        public async Task CreateUser()
        {
            byte[] lengthBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
            int dataLength = conversionHandler.ConvertBytesToInt(lengthBytes);
            byte[] credentialsBytes = await networkDataHelper.ReceiveAsync(dataLength);
            string credentials = conversionHandler.ConvertBytesToString(credentialsBytes);
            string[] credentialsParts = credentials.Split("#");

            if (credentialsParts.Length == 2)
            {
                try
                {
                    string username = credentialsParts[0];
                    string password = credentialsParts[1];
                    _userService.CreateUser(username, password);

                    Console.WriteLine("User created successfully.");
                    byte[] responseBytes = conversionHandler.ConvertStringToBytes("success");
                    await SendResponse(responseBytes);
                }
                catch(Exception e)
                {
                    Console.WriteLine("New user credentials were not valid.");
                    byte[] responseBytes = conversionHandler.ConvertStringToBytes(e.Message);
                    await SendResponse(responseBytes);
                }
            }
            else
            {
                Console.WriteLine("Invalid credentials format for new user.");
                byte[] responseBytes = conversionHandler.ConvertStringToBytes("Invalid credentials format");
                await SendResponse(responseBytes);
            }
        }

        private async Task SendResponse(byte[] responseBytes)
        {
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            await networkDataHelper.SendAsync(lengthBytes);
            await networkDataHelper.SendAsync(responseBytes);
        }
    }