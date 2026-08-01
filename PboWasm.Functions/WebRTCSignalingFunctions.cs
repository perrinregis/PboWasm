using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PboWasm.Functions
{
    public class WebRTCSignalingFunctions
    {
        private readonly ILogger _logger;

        public WebRTCSignalingFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebRTCSignalingFunctions>();
        }

        [Function("negotiate")]
        public IActionResult Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfoInput(HubName = "signalingHub")] string connectionInfo)
        {
            _logger.LogInformation("SignalR negotiate requested.");
            return new ContentResult
            {
                Content = connectionInfo,
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [Function("SendSignal")]
        [SignalROutput(HubName = "signalingHub")]
        public async Task<SignalRMessageAction> SendSignal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("Broadcasting signal...");
            using var reader = new StreamReader(req.Body);
            string signal = await reader.ReadToEndAsync();

            return new SignalRMessageAction("ReceiveSignal")
            {
                Arguments = new object[] { signal }
            };
        }
    }
}
