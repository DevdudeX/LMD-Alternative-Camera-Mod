using MelonLoader;


namespace AlternativeCameraMod;

internal class Logger
{
	private readonly MelonLogger.Instance _loggerInstance;


	public Logger(MelonLogger.Instance loggerInstance)
	{
		_loggerInstance = loggerInstance;
	}


	public LogLevel Level { get; set; }


	public void LogVerbose(string msg, params object[] args)
	{
		if (Level < LogLevel.Verbose) return;
		_loggerInstance.Msg(System.ConsoleColor.DarkMagenta, msg, args);
	}


	public void LogVerbose(bool condition, string msg, params object[] args)
	{
		if (Level < LogLevel.Verbose || !condition) return;
		_loggerInstance.Msg(System.ConsoleColor.DarkMagenta, msg, args);
	}


	public void LogDebug(string msg, params object[] args)
	{
		if (Level < LogLevel.Debug) return;
		_loggerInstance.Msg(System.ConsoleColor.Magenta, msg, args);
	}


	public void LogDebug(bool condition, string msg, params object[] args)
	{
		if (Level < LogLevel.Debug || !condition) return;
		_loggerInstance.Msg(System.ConsoleColor.Magenta, msg, args);
	}


	public void LogInfo(string msg, params object[] args)
	{
		if (Level < LogLevel.Info) return;
		_loggerInstance.Msg(msg, args);
	}


	public void LogInfo(bool condition, string msg, params object[] args)
	{
		if (Level < LogLevel.Info || !condition) return;
		_loggerInstance.Msg(msg, args);
	}


	public void LogWarning(string msg, params object[] args)
	{
		if (Level < LogLevel.Warning) return;
		_loggerInstance.Warning(msg, args);
	}


	public void LogWarning(bool condition, string msg, params object[] args)
	{
		if (Level < LogLevel.Warning || !condition) return;
		_loggerInstance.Warning(msg, args);
	}


	public void LogError(string msg, params object[] args)
	{
		if (Level < LogLevel.Error) return;
		_loggerInstance.Error(msg, args);
	}


	public void LogError(bool condition, string msg, params object[] args)
	{
		if (Level < LogLevel.Error || !condition) return;
		_loggerInstance.Error(msg, args);
	}


	public void Log(LogLevel level, string msg, params object[] args)
	{
		if (Level < level) return;
		switch (level)
		{
			case LogLevel.Debug:
				LogDebug(msg, args);
				break;
			case LogLevel.Info:
				LogInfo(msg, args);
				break;
			case LogLevel.Warning:
				LogWarning(msg, args);
				break;
			case LogLevel.Error:
				LogError(msg, args);
				break;
		}
	}
}
