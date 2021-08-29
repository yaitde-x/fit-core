
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace fit_runtime
{

    public class FitnesseRequest
    {
        public string Control { get; set; }
        public int LineNumber { get; set; }
        public string Statement { get; set; }
    }

    public class ApplicationState
    {
        public int ConnectionId {get; set;}
        public string ActiveForm { get; set; }
        public string TestNumber { get; set; }
        public string DataSet { get; set; }
        public string User { get; set; }
        public int UserId { get; set; }
        public DateTime SystemTime { get; set; }
    }

    public class StateVar
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }

    public class FitnesseResponse
    {
        public ApplicationState State { get; set; }
        public StateVar[] Locals { get; set; }
        public StateVar[] Globals { get; set; }
        public ExecutionResult Result { get; set; }
    }

    public class ExecutionError
    {
        public string Code { get; set; }
        public string[] Stack { get; set; }
        public string[] Messages { get; set; }
    }
    public class ExecutionResult
    {
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Exceptions { get; set; }
        public string[] Messages { get; set; }
        public ExecutionError Error { get; set; }
    }

    public class FitnesseResponseStorageModel : FitnesseResponse
    {
        public int LineNumber { get; set; }
    }

    public static class StaticResponses
    {
        public static IDictionary<string, FitnesseResponse> _responses = new ConcurrentDictionary<string, FitnesseResponse>();

        static StaticResponses()
        {
            var path = "/Users/sakamoto/Code/fit-core/mock-results.json";
            var mockResults = JsonConvert.DeserializeObject<IEnumerable<FitnesseResponseStorageModel>>(File.ReadAllText(path));
            foreach (var result in mockResults)
            {
                _responses.Add(result.LineNumber.ToString(), new FitnesseResponse()
                {
                    State = result.State,
                    Globals = result.Globals,
                    Locals = result.Locals,
                    Result = result.Result
                });
            }
        }

        public static FitnesseResponse GetResponse(int lineNumber)
        {
            var key = lineNumber.ToString();

            if (_responses.ContainsKey(key))
                return _responses[key];

            return new FitnesseResponse()
            {
                State = new ApplicationState()
                {
                    ActiveForm = null,
                    User = null,
                    UserId = -1,
                    TestNumber = null,
                    DataSet = null
                },
                Globals = new StateVar[0],
                Locals = new StateVar[0]
            };
        }
    }
}