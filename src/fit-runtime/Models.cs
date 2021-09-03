
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace fit_runtime
{

    public class FitnesseRequest
    {
        public string RequestId { get; set; }
        public string Control { get; set; }
        public int LineNumber { get; set; }
        public string Statement { get; set; }
    }

    // public class ApplicationState
    // {
    //     public int ConnectionId {get; set;}
    //     public string ActiveForm { get; set; }
    //     public string TestNumber { get; set; }
    //     public string DataSet { get; set; }
    //     public string User { get; set; }
    //     public int UserId { get; set; }
    //     public DateTime SystemTime { get; set; }
    // }

    public class StateVar
    {
        public StateVar(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class FitnesseResponse
    {
        public string RequestId { get; set; }
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
        public int Latency { get; set; }
    }

    public static class StateVarBuilder
    {
        private static StateVar[] processVar(StateVar sv, StateVar[] vars)
        {
            var parts = sv.Name.Split('.', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return vars.Concat(new[] { sv }).ToArray();
            }

            var parent = new StateVar(String.Empty, vars);
            var topLevel = parent;

            for (var index = 0; index < parts.Length - 1; index++)
            {
                var varName = parts[index];
                var newParent = FindVar(parent, varName);

                if (newParent == null)
                {
                    newParent = new StateVar(varName, new StateVar[0]);
                    parent.Value = (parent.Value as StateVar[])?.Concat(new[] { newParent }).ToArray();
                }

                parent = newParent;
            }

            var props = parent.Value as StateVar[];
            var propName = parts[parts.Length - 1];
            parent.Value = props?.Concat(new[] { new StateVar(propName, sv.Value) }).ToArray();

            return topLevel.Value as StateVar[];
        }

        public static StateVar[] TreeFromFlatArray(StateVar[] vars)
        {
            if (vars.Length == 0)
                return vars;

            var newVars = new StateVar[0];

            foreach (var v in vars)
                newVars = processVar(v, newVars);

            return newVars;
        }

        private static bool EqualsIgnoreCase(this string val, string compare)
        {
            return string.Equals(val, compare, StringComparison.OrdinalIgnoreCase);
        }
        private static StateVar FindVar(StateVar parent, string key)
        {
            if (parent.Name.EqualsIgnoreCase(key))
                return parent;

            var props = parent.Value as StateVar[];

            if (props != null)
            {
                foreach (var prop in props)
                {
                    var match = FindVar(prop, key);
                    if (match != null)
                        return match;
                }
            }

            return default(StateVar);
        }
    }

    public static class StaticResponses
    {
        public static IDictionary<string, (FitnesseResponse, FitnesseResponseStorageModel)> _responses = new ConcurrentDictionary<string, (FitnesseResponse, FitnesseResponseStorageModel)>();

        static StaticResponses()
        {
            var mockResults = GetMockResponses();
            foreach (var result in mockResults)
            {
                _responses.Add(result.LineNumber.ToString(), (new FitnesseResponse()
                {
                    Globals = StateVarBuilder.TreeFromFlatArray(result.Globals),
                    Locals = StateVarBuilder.TreeFromFlatArray(result.Locals),
                    Result = result.Result
                }, result));
            }
        }

        public static IEnumerable<FitnesseResponseStorageModel> GetMockResponses()
        {
            var path = "/Users/sakamoto/Code/fit-core/mock-results.json";
            return JsonConvert.DeserializeObject<IEnumerable<FitnesseResponseStorageModel>>(File.ReadAllText(path));
        }

        public static FitnesseResponse GetResponse(int lineNumber, string requestId)
        {
            var key = lineNumber.ToString();
            var delay = 100;
            var response = new FitnesseResponse()
            {
                RequestId = requestId,
                Globals = new StateVar[0],
                Locals = new StateVar[0]
            };

            if (_responses.ContainsKey(key))
            {
                response = _responses[key].Item1;
                delay = Math.Max(delay, _responses[key].Item2.Latency);
            }

            Thread.Sleep(delay);

            return response;
        }
    }
}