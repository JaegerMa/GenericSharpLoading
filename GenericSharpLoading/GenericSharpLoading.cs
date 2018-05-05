using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericSharpLoading
{
	public delegate void Logger(string message, string logLevel);

	public static class GenericSharpLoading
	{
		public static event Logger OnLog;

		public static void Log(string message, string logLevel = LogLevel.INFO)
		{
			OnLog?.Invoke(message, logLevel);
		}

		public static class LogLevel
		{
			public const string DEBUG = "DEBUG";
			public const string INFO = "INFO";
			public const string WARNING = "WARNING";
			public const string ERROR = "ERROR";
		}
	}
}
