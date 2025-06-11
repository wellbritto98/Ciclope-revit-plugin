using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace RevitTemplate.Services
{
    /// <summary>
    /// Serviço estático para gerenciar logs da aplicação
    /// </summary>
    public static class LogService
    {
        private static readonly object _lock = new object();
        private static ObservableCollection<LogEntry> _logs = new ObservableCollection<LogEntry>();
        private static bool _isInitialized = false;
        private static Dispatcher _uiDispatcher;

        /// <summary>
        /// Construtor estático para inicialização segura
        /// </summary>
        static LogService()
        {
            try
            {
                // Inicialização segura do serviço
                _isInitialized = true;
            }
            catch
            {
                // Se houver erro na inicialização, continuamos sem crash
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Define o Dispatcher da UI para operações thread-safe
        /// Deve ser chamado da UI thread durante a inicialização da aplicação
        /// </summary>
        /// <param name="dispatcher">Dispatcher da UI thread</param>
        public static void Initialize(Dispatcher dispatcher)
        {
            _uiDispatcher = dispatcher;
        }

        /// <summary>
        /// Evento disparado quando um novo log é adicionado
        /// </summary>
        public static event EventHandler<LogEntry> LogAdded;

        /// <summary>
        /// Coleção de logs observável (thread-safe)
        /// </summary>
        public static ObservableCollection<LogEntry> Logs
        {
            get
            {
                lock (_lock)
                {
                    return _logs;
                }
            }
        }

        /// <summary>
        /// Adiciona uma mensagem de informação ao log
        /// </summary>
        /// <param name="message">Mensagem a ser logada</param>
        public static void LogInfo(string message)
        {
            AddLog(LogLevel.Info, message);
        }

        /// <summary>
        /// Adiciona uma mensagem de erro ao log
        /// </summary>
        /// <param name="message">Mensagem a ser logada</param>
        public static void LogError(string message)
        {
            AddLog(LogLevel.Error, message);
        }

        /// <summary>
        /// Adiciona uma mensagem de warning ao log
        /// </summary>
        /// <param name="message">Mensagem a ser logada</param>
        public static void LogWarning(string message)
        {
            AddLog(LogLevel.Warning, message);
        }

        /// <summary>
        /// Adiciona uma mensagem de debug ao log
        /// </summary>
        /// <param name="message">Mensagem a ser logada</param>
        public static void LogDebug(string message)
        {
            AddLog(LogLevel.Debug, message);
        }        /// <summary>
        /// Limpa todos os logs
        /// </summary>
        public static void ClearLogs()
        {
            if (_uiDispatcher != null)
            {
                if (_uiDispatcher.CheckAccess())
                {
                    // Já estamos na UI thread
                    ClearLogEntries();
                }
                else
                {
                    // Não estamos na UI thread, precisamos fazer Invoke
                    _uiDispatcher.Invoke(ClearLogEntries);
                }
            }
            else
            {
                // Fallback: limpa diretamente se não há Dispatcher disponível
                ClearLogEntries();
            }
        }

        /// <summary>
        /// Limpa as entradas de log da coleção (deve ser chamado na UI thread)
        /// </summary>
        private static void ClearLogEntries()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }/// <summary>
        /// Adiciona um log à coleção de forma thread-safe
        /// </summary>
        /// <param name="level">Nível do log</param>
        /// <param name="message">Mensagem</param>
        private static void AddLog(LogLevel level, string message)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            };            // Verifica se temos acesso ao Dispatcher da UI
            if (_uiDispatcher != null)
            {
                // Garante que a adição à ObservableCollection seja feita na UI thread
                if (_uiDispatcher.CheckAccess())
                {
                    // Já estamos na UI thread
                    AddLogEntry(logEntry);
                }
                else
                {
                    // Não estamos na UI thread, precisamos fazer Invoke
                    _uiDispatcher.Invoke(() => AddLogEntry(logEntry));
                }
            }
            else
            {
                // Fallback: adiciona diretamente se não há Dispatcher disponível
                AddLogEntry(logEntry);
            }
        }

        /// <summary>
        /// Adiciona a entrada de log à coleção (deve ser chamado na UI thread)
        /// </summary>
        /// <param name="logEntry">Entrada de log a ser adicionada</param>
        private static void AddLogEntry(LogEntry logEntry)
        {
            lock (_lock)
            {
                _logs.Add(logEntry);
            }
            LogAdded?.Invoke(null, logEntry);
        }
    }

    /// <summary>
    /// Entrada de log
    /// </summary>
    public class LogEntry : INotifyPropertyChanged
    {
        private DateTime _timestamp;
        private LogLevel _level;
        private string _message;

        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged(nameof(Timestamp));
                OnPropertyChanged(nameof(FormattedTimestamp));
            }
        }

        public LogLevel Level
        {
            get => _level;
            set
            {
                _level = value;
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(LevelText));
                OnPropertyChanged(nameof(LevelColor));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
                OnPropertyChanged(nameof(FormattedMessage));
            }
        }

        public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");

        public string LevelText => Level.ToString().ToUpper();

        public string LevelColor
        {
            get
            {
                if (Level == LogLevel.Error)
                    return "#FF5555";
                else if (Level == LogLevel.Warning)
                    return "#FFAA00";
                else if (Level == LogLevel.Info)
                    return "#55FF55";
                else if (Level == LogLevel.Debug)
                    return "#5555FF";
                else
                    return "#FFFFFF";
            }
        }

        public string FormattedMessage => $"[{FormattedTimestamp}] [{LevelText}] {Message}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Níveis de log
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
