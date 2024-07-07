using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Polygon.Client.Responses;
using Polygon.Client.Requests;

namespace MarketDataProvider.Infrastructure.Repository
{
    public class AggregatesLiveFeed(IConfiguration configuration,
        ILogger<AggregatesLiveFeed> logger) : BackgroundService
    {
        private bool _isConnected = false;
        private bool _isAuthenticated = false;
        private bool _isSubscribed = false;

        private readonly List<PolygonWebsocketAggregateResponse> _polygonAggregateResponses = [];

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var socket = new ClientWebSocket();

                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Connecting to PolygonApi Websocket at: {time}", DateTimeOffset.Now);

                    await socket.ConnectAsync(new Uri("wss://delayed.polygon.io/stocks"), cancellationToken);

                    var buffer = new byte[3000000];
                    while (socket.State == WebSocketState.Open)
                    {
                        var result = await socket.ReceiveAsync(buffer, cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            logger.LogInformation("Disconnected from PolygonApi Websocket.");
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                            return;
                        }

                        var response = JsonSerializer.Deserialize<List<PolygonWebsocketAggregateResponse>>(Encoding.ASCII.GetString(buffer, 0, result.Count));

                        if (_isConnected && _isAuthenticated && _isSubscribed)
                        {
                            var aggregateResponses = response.Where(item => item.Event is "T");
                            //logger.LogInformation("Received {count} aggregates.", aggregateResponses.Count());
                            _polygonAggregateResponses.AddRange(aggregateResponses);
                            continue;
                        }

                        var firstResponse = response.First();

                        if (!_isConnected)
                        {
                            if (firstResponse.Status is "connected")
                            {
                                _isConnected = true;
                                logger.LogInformation("Connected to PolygonApi Websocket successfully.");
                            }
                        }

                        if (_isConnected && !_isAuthenticated)
                        {
                            if (firstResponse.Status is "auth_success")
                            {
                                _isAuthenticated = true;
                                logger.LogInformation("Authenticated to PolygonApi Websocket successfully.");
                            }
                            else
                            {
                                var request = JsonSerializer.Serialize(new PolygonWebsocketRequest
                                {
                                    Action = "auth",
                                    Params = configuration.GetSection("Tokens").GetValue<string>("PolygonApi")
                                });

                                await socket.SendAsync(Encoding.UTF8.GetBytes(request), WebSocketMessageType.Text, true, cancellationToken);
                            }
                        }

                        if (_isConnected && _isAuthenticated && !_isSubscribed)
                        {
                            if (firstResponse.Status is "success")
                            {
                                _isSubscribed = true;
                                logger.LogInformation("Subscribed to PolygonApi Websocket successfully.");
                            }
                            else
                            {
                                var request = JsonSerializer.Serialize(new PolygonWebsocketRequest
                                {
                                    Action = "subscribe",
                                    Params = "A.*"
                                });

                                await socket.SendAsync(Encoding.UTF8.GetBytes(request), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError("Error while reading from PolygonApi Websocket: {error}", e.Message);
            }
        }
    }
}
