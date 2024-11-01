using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class MouseSettings : ModSettingsCategory
{
	private MelonPreferences_Entry<float> _sensitivityHorizontal;
	private MelonPreferences_Entry<float> _sensitivityVertical;
	private MelonPreferences_Entry<float> _sensitivityMultiplier;
	private MelonPreferences_Entry<bool> _invertHorizontalLook;
	private MelonPreferences_Entry<bool> _invertVerticalLook;


	public MouseSettings(string filePath, LanguageConfig lng) : base("Mouse", filePath, lng)
	{
		_sensitivityHorizontal = CreateEntry(
			"HorizontalSensitivity", 0.8f,
			"Mouse sensitivity for horizontal movement, increase if mouse moves too slow, decrease if too fast"
		);

		_sensitivityVertical = CreateEntry(
			"VerticalSensitivity", 0.8f,
			"Mouse sensitivity for vertical movement, increase if mouse moves too slow, decrease if too fast"
		);

		_sensitivityMultiplier = CreateEntry(
			"SensitivityMultiplier", 1.0f,
			"Mouse sensitivity mulitplier, increase if mouse accelerates too little, or decrease if too much"
		);

		_invertHorizontalLook = CreateEntry(
			"InvertHorizontalLook", false,
			"* false: mouse left => look left, mouse right => look right\n" +
			"* true : mouse left => look right, mouse right => look left"
		);

		_invertVerticalLook = CreateEntry(
			"InvertVerticalLook", false,
			"* false: mouse up => look up, mouse down => look down\n" +
			"* true : mouse up => look down, mouse down => look up"
		);
	}


	public MelonPreferences_Entry<float> SensitivityHorizontal
	{
		get { return _sensitivityHorizontal; }
	}

	public MelonPreferences_Entry<float> SensitivityVertical
	{
		get { return _sensitivityVertical; }
	}

	public MelonPreferences_Entry<float> SensitivityMultiplier
	{
		get { return _sensitivityMultiplier; }
	}

	public MelonPreferences_Entry<bool> InvertHorizontalLook
	{
		get { return _invertHorizontalLook; }
	}

	public MelonPreferences_Entry<bool> InvertVerticalLook
	{
		get { return _invertVerticalLook; }
	}
}
