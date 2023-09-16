using System;
using System.Text;
using System.Text.Json;

namespace Communication
{
    public class Protocol
    {
        public static class Commands
        {
            public const string Authenticate = "AUTH";
            public const string PublishProduct = "PUBL";
            public const string PurchaseProduct = "COMP";
        }
        
        private class ProtocolFrame
        {
            public string Header { get; set; }
            public string Body { get; set; }
        }
        
        public static string BuildRequest(string command, object data)
        {
            var requestFrame = new ProtocolFrame
            {
                Header = command,
                Body = JsonSerializer.Serialize(data)
            };
            return JsonSerializer.Serialize(requestFrame);
        }

        public static string BuildResponse(string command, object data)
        {
            var responseFrame = new ProtocolFrame
            {
                Header = command,
                Body = JsonSerializer.Serialize(data)
            };
            return JsonSerializer.Serialize(responseFrame);
        }
        
        public static (string command, T data) ParseRequest<T>(string request)
        {
            try
            {
                var requestData = JsonSerializer.Deserialize<ProtocolFrame>(request);
                return (requestData.Header, JsonSerializer.Deserialize<T>(requestData.Body));
            }
            catch (JsonException ex)
            {
                throw new ProtocolException("Error al analizar la solicitud JSON.", ex);
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Error al analizar la solicitud.", ex);
            }
        }

        public static (string command, T data) ParseResponse<T>(string response)
        {
            try
            {
                var responseData = JsonSerializer.Deserialize<ProtocolFrame>(response);
                return (responseData.Header, JsonSerializer.Deserialize<T>(responseData.Body));
            }
            catch (JsonException ex)
            {
                throw new ProtocolException("Error al analizar la respuesta JSON.", ex);
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Error al analizar la respuesta.", ex);
            }
        }
    }
}