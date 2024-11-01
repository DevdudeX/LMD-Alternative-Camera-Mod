using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class PhotoModeSettings : ModSettingsCategory
{
	private MelonPreferences_Entry<bool> _autoHideHud;
	private MelonPreferences_Entry<float> _cameraMovementSpeed;
	private MelonPreferences_Entry<float> _cameraMovementSpeedMultiplier;
	private MelonPreferences_Entry<string> _screenshotFolder;
	private MelonPreferences_Entry<string> _screenshotFilenameFormat;
	private MelonPreferences_Entry<int> _screenshotJpgQuality;


	public PhotoModeSettings(string filePath, LanguageConfig lng) : base("PhotoMode", filePath, lng)
	{
		_cameraMovementSpeed = CreateEntry(
			"CameraMovementSpeed", 1.8f,
			"Basic speed of the camera movement"
		);

		_cameraMovementSpeedMultiplier = CreateEntry(
			"CameraMovementSpeedMultiplier", 8f,
			"Speed multiplier applied when the according key/button is pressed to accelerate"
		);

		_autoHideHud = CreateEntry(
			"AutoHideHud", false,
			"Tells whether game hud should be auto-hidden when entering photo mode"
		);

		_screenshotFolder = CreateEntry(
			"ScreenshotFolder",
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Lonely Mountains - Downhill"),
			"Output folder where screenshots are saved, folder is created if not exists"
		);

		_screenshotFilenameFormat = CreateEntry(
			"ScreenshotFilenameFormat",
			"LMD_screenshot_{cnt2}_{w}x{h}_{d}_{t}.png",
			"Format used to create filenames to save screenshots; use extensions .png or .jpg\n" +
			"Placeholders:\n" +
			"{w}=screen width\n" +
			"{h}=screen height\n" +
			"{d1}=date as yyyy-MM-dd\n" +
			"{t1]=time as HH-mm-ss\n" +
			"{d2}=date as yyyyMMdd\n" +
			"{t2]=time as HHmmss\n" +
			"{cnt2}=2-digit counter, {cnt3}=3-digit counter, up to 4, 5"
		);

		_screenshotJpgQuality = CreateEntry(
			"ScreenshotJpgQuality", 75,
			"Quality of the JPG used for files, when filename ends with .jpg; 0 is worst, 100 is best"
		);
	}


	public MelonPreferences_Entry<float> CameraMovementSpeed
	{
		get { return _cameraMovementSpeed; }
	}

	public MelonPreferences_Entry<float> CameraMovementSpeedMultiplier
	{
		get { return _cameraMovementSpeedMultiplier; }
	}

	public MelonPreferences_Entry<string> ScreenshotFolder
	{
		get { return _screenshotFolder; }
	}

	public MelonPreferences_Entry<string> ScreenshotFilenameFormat
	{
		get { return _screenshotFilenameFormat; }
	}

	public MelonPreferences_Entry<int> ScreenshotJpgQuality
	{
		get { return _screenshotJpgQuality; }
	}

	public MelonPreferences_Entry<bool> AutoHideHud
	{
		get { return _autoHideHud; }
	}
}
