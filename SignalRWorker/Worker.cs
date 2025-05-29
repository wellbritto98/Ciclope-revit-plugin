using System.IO.Pipes;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRWorker
{
    public class Worker : BackgroundService
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly string _inPipe;
        private readonly string _outPipe;
        private readonly string _token;

        private AnonymousPipeClientStream _pipeOut;
        private AnonymousPipeClientStream _pipeIn;

        private StreamReader _streamIn;
        private StreamWriter _streamOut;

        private HubConnection _hubConnection;

        public Worker(string inPipe, string outPipe, string token)
        {
            _inPipe = inPipe;
            _outPipe = outPipe;
            _token = token;

            Console.WriteLine(_token);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, _outPipe);
            _pipeIn = new AnonymousPipeClientStream(PipeDirection.In, _inPipe);

            _streamOut = new StreamWriter(_pipeOut) { AutoFlush = true };
            _streamIn = new StreamReader(_pipeIn);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri("https://localhost:6102/revitHub"), options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        if (handler is HttpClientHandler httpClientHandler)
                        {
                            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                        }
                        return handler;
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("RevitProjetoElementos", (msg) =>
            {
                Console.WriteLine(msg);
                _streamOut.WriteLine(JsonSerializer.Serialize(new 
                { 
                    Method = "RevitProjetoElementos", 
                    Args = new string[] { msg } 
                }));
            });

            _hubConnection.On<string>("RevitVisualizarElemento", (msg) =>
            {
                Console.WriteLine(msg);
                _streamOut.WriteLine(JsonSerializer.Serialize(new
                {
                    Method = "RevitVisualizarElemento",
                    Args = new string[] { msg }
                }));
            });




            await _hubConnection.StartAsync();

            Console.WriteLine("Conectado ao SignalR");

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _streamIn.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    continue;
                }

                var request = JsonSerializer.Deserialize<Request>(message, _jsonSerializerOptions);

                if (request == null)
                {
                    continue;
                }

                var compressedMessage = CompressionService.Compress(request.Args[0]);

                await _hubConnection.InvokeAsync(request.Method, compressedMessage, stoppingToken);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _pipeOut.Dispose();
            _pipeIn.Dispose();
            _streamOut.Dispose();
            _streamIn.Dispose();
        }
    }
}
