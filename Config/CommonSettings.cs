using LMSR_AlternativeCameraMod.Language;
using MelonLoader;


namespace LMSR_AlternativeCameraMod.Config;

internal class CommonSettings : ModSettingsCategory
{
	private MelonPreferences_Entry<LogLevel> _logLevel;
	private MelonPreferences_Entry<string> _language;


	public CommonSettings(string filePath, LanguageConfig lng) : base("Common", filePath, lng)
	{
		_logLevel = CreateEntry(
			"LogLevel", LMSR_AlternativeCameraMod.LogLevel.Error,
			"Logging level, output in MelonLoader console\n" +
			"Error, Warning, Information, Debug, Verbose"
		);

		_language = CreateEntry(
			"Language", "default",
			"Language file to use\n" +
			"default     : OS language detected\n" +
			"en, de, .. : language code related to the part in the language file name, e.g. the 'en' in lang.en.ini"
		);
	}


	public MelonPreferences_Entry<LogLevel> LogLevel
	{
		get { return _logLevel; }
	}

	public MelonPreferences_Entry<string> Language
	{
		get { return _language; }
	}
}
