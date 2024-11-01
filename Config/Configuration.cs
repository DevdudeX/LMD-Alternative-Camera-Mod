using AlternativeCameraMod.Language;
using Il2CppSystem.Text;


namespace AlternativeCameraMod.Config;

internal class Configuration
{
	private const string LcConfigFilePath = "UserData/AlternativeCameraWithPhotoMode.{0}.ini";
	public const string ConfigFilePath = "UserData/AlternativeCameraWithPhotoMode.ini";
	public const string GameColor = "#E9AC4F";
	public const string GameColor2 = "#8FBC0D";

	private readonly Dictionary<string, ModSettingsCategory> _categories = new();
	private readonly string _filePath;


	public static Configuration Create(LanguageConfig lng)
	{
		var fp = new FileInfo(ConfigFilePath).FullName;
		return new Configuration(lng, fp);
	}


	public static Configuration CreateForLanguage(LanguageConfig lng)
	{
		var lngFilePath = String.Format(LcConfigFilePath, lng.LanguageCode);
		var fp = new FileInfo(lngFilePath).FullName;
		return new Configuration(lng, fp);
	}


	private Configuration(LanguageConfig lng, string filePath)
	{
		if (lng == null)
		{
			throw new ArgumentNullException(nameof(lng));
		}

		if (filePath == null)
		{
			throw new ArgumentNullException(nameof(filePath));
		}

		_filePath = filePath;
#if DEBUG
		Delete();
#endif

		_categories.Add(nameof(CommonSettings), new CommonSettings(_filePath, lng));
		_categories.Add(nameof(CameraSettings), new CameraSettings(_filePath, lng));
		_categories.Add(nameof(ControllerSettings), new ControllerSettings(_filePath, lng));
		_categories.Add(nameof(MouseSettings), new MouseSettings(_filePath, lng));
		_categories.Add(nameof(KeyboardSettings), new KeyboardSettings(_filePath, lng));
		_categories.Add(nameof(PlayModeSettings), new PlayModeSettings(_filePath, lng));
		_categories.Add(nameof(PhotoModeSettings), new PhotoModeSettings(_filePath, lng));
	}


	public void Save()
	{
		foreach (var category in _categories.Values)
		{
			category.Save();
		}
		Beautify();
	}


	public void Delete()
	{
		File.Delete(_filePath); // to refresh config
	}


	private void Beautify()
	{
		try
		{
			var text = File.ReadAllText(_filePath, System.Text.Encoding.UTF8);
			// cleanup line endings
			text = text.Replace(Environment.NewLine, "\n");
			text = text.Replace("\r", "\n");
			var lines = text.Split("\n");

			StringBuilder output = new StringBuilder(lines.Sum(l => l.Length));
			bool lastIsEntry = false;
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				if (String.IsNullOrWhiteSpace(line)) continue;

				if (line.StartsWith("[")) // category
				{
					if (i > 0)
					{
						output.AppendLine().AppendLine(); // two lines spacing before category
					}
					output.AppendLine(line);
					lastIsEntry = false;
				}
				else if (line.StartsWith("#")) // comment
				{
					var trimLine = line.Trim();
					if (trimLine == "#") // not an empty comment
					{
						output.AppendLine(); // omit empty #
					}
					else
					{
						if (lastIsEntry)
						{
							output.AppendLine(); // one line spacing
						}
						output.AppendLine(line);
						lastIsEntry = false;
					}
				}
				else // entry
				{
					output.AppendLine(line);
					lastIsEntry = true;
				}
			}

			File.WriteAllText(_filePath, output.ToString(), System.Text.Encoding.UTF8);
		}
		catch
		{
			// ignore
		}
	}


	public string? Validate(LanguageConfig language)
	{
		var sb = new StringBuilder();
		foreach (var category in _categories.Values)
		{
			var err = category.Validate(language);
			if (!String.IsNullOrEmpty(err))
			{
				sb.AppendLine(err);
			}
		}

		return sb.ToString().Trim();
	}


	public CommonSettings Common
	{
		get { return (CommonSettings)_categories[nameof(CommonSettings)]; }
	}

	public PlayModeSettings PlayMode
	{
		get { return (PlayModeSettings)_categories[nameof(PlayModeSettings)]; }
	}

	public ControllerSettings Controller
	{
		get { return (ControllerSettings)_categories[nameof(ControllerSettings)]; }
	}

	public KeyboardSettings Keyboard
	{
		get { return (KeyboardSettings)_categories[nameof(KeyboardSettings)]; }
	}

	public PhotoModeSettings PhotoMode
	{
		get { return (PhotoModeSettings)_categories[nameof(PhotoModeSettings)]; }
	}

	public MouseSettings Mouse
	{
		get { return (MouseSettings)_categories[nameof(MouseSettings)]; }
	}

	public CameraSettings Camera
	{
		get { return (CameraSettings)_categories[nameof(CameraSettings)]; }
	}
}