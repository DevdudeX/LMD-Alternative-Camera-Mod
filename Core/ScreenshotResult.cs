namespace AlternativeCameraMod;

internal class ScreenshotResult
{
	public ScreenshotResult(string filePath, string errorMessage)
	{
		FilePath = filePath;
		ErrorMessage = errorMessage;
		Timestamp = DateTime.Now;
	}


	public string FilePath { get; }
	public DateTime Timestamp { get; }
	public string ErrorMessage { get; }
}
