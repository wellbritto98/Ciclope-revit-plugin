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

        private bool _disposed = false;
        private readonly Dictionary<string, Action<object[]>> _subscriptions = new Dictionary<string, Action<object[]>>();

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                if (_workerProcess != null && !_workerProcess.HasExited)
                {
                    _workerProcess.Kill();
                }
            }
            catch { }

            try
            {
                _workerProcess?.Dispose();
            }
            catch { }

            try
            {
                _streamIn?.Dispose();
            }
            catch { }

            try
            {
                _streamOut?.Dispose();
            }
            catch { }

            try
            {
                _pipeOut?.Dispose();
            }
            catch { }

            try
            {
                _pipeIn?.Dispose();
            }
            catch { }
        }

        public void Start( string token)
        {

            _pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            _workerProcess = new Process();
            
            // Obtém o caminho do diretório onde está o addin
            string addinDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // O SignalRWorker está na pasta Worker/
            string workerExePath = Path.Combine(addinDirectory, "Worker", "SignalRWorker.exe");
            
            // Verifica se o arquivo do worker existe
            if (!File.Exists(workerExePath))
            {
                throw new FileNotFoundException($"SignalRWorker.exe não encontrado em: {workerExePath}");
            }
            
            _workerProcess.StartInfo.FileName = workerExePath;
            _workerProcess.StartInfo.UseShellExecute = false; // Mantém false para usar pipes
            _workerProcess.StartInfo.CreateNoWindow = true; // Oculta a janela do console
            _workerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // Garante que a janela fique oculta
            _workerProcess.StartInfo.WorkingDirectory = Path.Combine(addinDirectory, "Worker"); // Define o diretório de trabalho como a pasta Worker
            
            // O executável é self-contained, não precisa configurar ambiente .NET
            
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
