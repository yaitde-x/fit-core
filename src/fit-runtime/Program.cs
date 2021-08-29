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

namespace fit_runtime
{

    internal class FitnesseRuntime
    {
        private List<StateVar> _locals = new List<StateVar>();
        private List<StateVar> _globals = new List<StateVar>();

        public FitnesseRuntime()
        {
            State = new ApplicationState();
        }

        public ApplicationState State { get; private set; }
        public StateVar[] Locals { get { return _locals.ToArray(); } }
        public StateVar[] Globals { get { return _globals.ToArray(); } }

        public FitnesseResponse Exec(FitnesseRequest request)
        {
            return StaticResponses.GetResponse(request.LineNumber);
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

            response.Globals = response.Globals.Select(sv =>
            {
                if (sv.Key.Equals("session", StringComparison.OrdinalIgnoreCase))
                    sv.Value = Id;

                return sv;
            }).ToArray();

            var responseBuffer = JsonConvert.SerializeObject(response);
            SendAsync(responseBuffer + "\r\n");

            // If the buffer starts with '!' the disconnect the current session
            if (request.Control == "!")
                Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    class FitnesseDebugServer : TcpServer
    {
        private readonly FitnesseRuntime _fitnessRuntime;

        public FitnesseDebugServer(IPAddress address, int port, FitnesseRuntime fitnesseRunTime)
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
        static void Main(string[] args)
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

            var runtime = new FitnesseRuntime();
            var server = new FitnesseDebugServer(IPAddress.Any, port, runtime);

            Console.Write("Fitnesse Debug Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                    continue;
                }

                // Multicast admin message to all sessions
                line = "(admin) " + line;
                server.Multicast(line);
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }
    }
}
