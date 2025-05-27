using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using RevitTemplate.Models;

namespace RevitTemplate.Services
{
    public class WorkerServiceProxy : IDisposable
    {
        private Process _workerProcess;
        private AnonymousPipeServerStream _pipeOut;
        private AnonymousPipeServerStream _pipeIn;

        private StreamReader _streamIn;
        private StreamWriter _streamOut;


        private readonly Dictionary<string, Action<object[]>> _subscriptions = new Dictionary<string, Action<object[]>>();

        public void Dispose()
        {
            _workerProcess.Kill();
            _workerProcess.Dispose();
            _streamIn.Dispose();
            _streamOut.Dispose();
            _pipeOut.Dispose();
            _pipeIn.Dispose();
        }

        public void Start( string token)
        {

            _pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _workerProcess = new Process();
            _workerProcess.StartInfo.FileName = @"C:\Users\zearq\OneDrive\Documentos\projetos\Particular\revit-wpf-template\SignalRWorker\bin\Debug\net8.0\SignalRWorker.exe";
            _workerProcess.StartInfo.UseShellExecute = false;
            _workerProcess.StartInfo.CreateNoWindow = false;
            _workerProcess.StartInfo.Arguments = $"{_pipeOut.GetClientHandleAsString()} {_pipeIn.GetClientHandleAsString()} {token}";
            _workerProcess.Start();

            _pipeOut.DisposeLocalCopyOfClientHandle();
            _pipeIn.DisposeLocalCopyOfClientHandle();

            _streamIn = new StreamReader(_pipeIn);
            _streamOut = new StreamWriter(_pipeOut) { AutoFlush = true };

            Task.Run(() =>
            {
                try
                {
                    while(true)
                    {
                        var message = _streamIn.ReadLine();

                        ResponseWorker response = JsonConvert.DeserializeObject<ResponseWorker>(message);

                        _subscriptions[response.Method](response.Args);
                    }
                }
                catch (Exception ex)
                {
                }
            });
        }

        internal void On<T1>(string eventName, Action<T1> onData)
        {

            _subscriptions[eventName] = args => onData((T1)args[0]);

        }

        internal void Invoke(string method, params string[] args)
        {
            _streamOut.WriteLine(JsonConvert.SerializeObject(new { method, args }));
        }

        internal void Invoke(string method, byte[] args)
        {
            _streamOut.WriteLine(JsonConvert.SerializeObject(new { method, args }));
        }


        }
}
