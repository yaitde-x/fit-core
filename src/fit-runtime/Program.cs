using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Linq;
using TcpClient = System.Net.Sockets.TcpClient;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using fitSharp.Parser;
using fitSharp.Machine.Engine;
using fitSharp.IO;
using fitSharp.Fit.Runner;

namespace fit_runtime
{

    internal class FitnesseRuntime
    {
        private List<StateVar> _locals = new List<StateVar>();
        private List<StateVar> _globals = new List<StateVar>();

        public FitnesseRuntime()
        {
            
        }

        public StateVar[] Locals { get { return _locals.ToArray(); } }
        public StateVar[] Globals { get { return _globals.ToArray(); } }

        public FitnesseResponse Exec(FitnesseRequest request)
        {
            return StaticResponses.GetResponse(request.LineNumber, request.RequestId);
        }
    }

    class DebugClientSession : TcpSession
    {
        private FitnesseRuntime _runTime;
        public DebugClientSession(TcpServer server, FitnesseRuntime runTime) : base(server)
        {
            _runTime = runTime;
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Fitnesse debugger sessioned connected: {Id}");

            // var response = new FitnesseResponse()
            // {
            //     State = _runTime.State,
            //     Locals = _runTime.Locals,
            //     Globals = _runTime.Globals
            // };

            // var message = JsonConvert.SerializeObject(response);
            // SendAsync(message);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("Incoming: " + message);

            var request = JsonConvert.DeserializeObject<FitnesseRequest>(message);
            var response = _runTime.Exec(request);

            // response.Globals = response.Globals.Select(sv =>
            // {
            //     if (sv.Key.Equals("session", StringComparison.OrdinalIgnoreCase))
            //         sv.Value = Id;

            //     return sv;
            // }).ToArray();

            var responseBuffer = JsonConvert.SerializeObject(response);
            SendAsync(responseBuffer + "\r\n\r\n");

            // If the buffer starts with '!' the disconnect the current session
            if (request.Control == "!")
                Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    class FitnesseDebugServer
    {
        private readonly ILogger _logger;
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly FitnesseRuntime _fitnesseRuntime;

        internal FitnesseDebugServer(ILogger logger, IPAddress address, int port, FitnesseRuntime fitnesseRunTime)
        {
            _logger = logger;
            _address = address;
            _port = port;
            _fitnesseRuntime = fitnesseRunTime;
        }

        async Task HandleClient(TcpClient client, CancellationToken token)
        {

            var buffer = new byte[80000];
            var bufferLocation = 0;
            var clientId = Guid.NewGuid();
            var stream = client.GetStream();
            var eotMarkerCount = 0;

            while (client.Connected && !token.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0 + bufferLocation, 1);
                if (bytesRead > 0)
                {
                    if (buffer[bufferLocation] == '\r' || buffer[bufferLocation] == '\n')
                        eotMarkerCount++;
                    else {
                        eotMarkerCount = 0;
                        bufferLocation++;
                    }

                    if (eotMarkerCount == 4)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, bufferLocation);
                        bufferLocation = 0;

                        Console.WriteLine("Incoming: " + message);

                        var request = JsonConvert.DeserializeObject<FitnesseRequest>(message);
                        var response = _fitnesseRuntime.Exec(request);

                        response.RequestId = request.RequestId;
                        // response.Globals = response.Globals.Select(sv =>
                        // {
                        //     if (sv.Key.Equals("session", StringComparison.OrdinalIgnoreCase))
                        //         sv.Value = clientId.ToString();

                        //     return sv;
                        // }).ToArray();

                        var responseBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                        await stream.WriteAsync(responseBuffer);
                    }
                }
            }
        }

        internal async Task Run(CancellationToken cancellationToken)
        {
            var listener = new TcpListener(_address, _port);
            listener.Start();
            cancellationToken.Register(listener.Stop);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Connection received...");

                    var clientTask = HandleClient(client, cancellationToken)
                        .ContinueWith(antecedent => client.Dispose())
                        .ContinueWith(antecedent => _logger.LogInformation("Client disposed."));
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("TcpListener stopped listening because cancellation was requested.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(), ex, $"Error handling client: {ex.Message}");
                }
            }
        }
    }

    class FitnesseDebugServerX : TcpServer
    {
        private readonly FitnesseRuntime _fitnessRuntime;

        public FitnesseDebugServerX(IPAddress address, int port, FitnesseRuntime fitnesseRunTime)
            : base(address, port)
        {
            _fitnessRuntime = fitnesseRunTime;
        }

        protected override TcpSession CreateSession()
        {
            return new DebugClientSession(this, this._fitnessRuntime);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Socket error: {error}");
        }
    }

    class Program
    {
        static void Maintest(string[] args) {
            var runner = new GuiRunner();
            runner.Run(new List<string>() 
            {
                "-i", "/Users/sakamoto/Code/public/vscode-fit-debug/sampleWorkspace",
                "-o", "/Users/sakamoto/Code/public/vscode-fit-debug/sampleWorkspace/out"
            });
        }

        static async Task Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };

            // var path = "/Users/sakamoto/Code/fit-core/mock-results.json";
            // var models = StaticResponses._responses.OrderBy(kv => kv.Key).Select(kv => 
            // new TempResult(){
            //     LineNumber = Convert.ToInt32(kv.Key),
            //     State = kv.Value.State,
            //     Globals= kv.Value.Globals,
            //     Locals = kv.Value.Locals,
            //     Result = kv.Value.Result
            // });

            // File.WriteAllText(path, JsonConvert.SerializeObject(models));
            // return;

            int port = 1111;
            if (args.Length > 0)
                port = int.Parse(args[0]);

            Console.WriteLine($"TCP server port: {port}");
            Console.WriteLine();

            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger("fit-debug");
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var runtime = new FitnesseRuntime();
            var server = new FitnesseDebugServer(logger, IPAddress.Any, port, runtime);

            Console.Write("Fitnesse Debug Server starting...");
            var task = server.Run(token);
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

            }

            // Stop the server
            Console.Write("Server stopping...");
            tokenSource.Cancel();
            await task;
            Console.WriteLine("Done!");
        }
    }

    class Programx
    {
        static void Mainx(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 9999);
            // we set our IP address as server's address, and we also set the port: 9999

            server.Start();  // this will start the server

            while (true)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)
                hello = Encoding.Default.GetBytes("hello world");  //conversion string => byte array

                ns.Write(hello, 0, hello.Length);     //sending the message

                while (client.Connected)  //while the client is connected, we look for incoming messages
                {
                    byte[] msg = new byte[1024];     //the messages arrive as byte array
                    ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                    Console.WriteLine(Encoding.UTF8.GetString(msg).Trim());
                }
            }

        }
    }
}
