namespace LMSR_AlternativeCameraMod;

internal abstract class HudInstructions
{
	private string _title;
	private string _titleShadow;
	private string _actions;
	private string _keys;
	private string _buttons;
	private string _actionsShadow;
	private string _keysShadow;
	private string _buttonsShadow;


	public void Build(string title, string actions, string keys, string buttons, int titleSize, string titleColor, int textSize, string textColor, string shadowColor, int shadowOffset)
	{
		_title = Hud.FormatLabel(title, titleSize, titleColor, true);
		_titleShadow = Hud.FormatLabel(title, titleSize, shadowColor, true);

		_actions = Hud.FormatLabel(actions, textSize, textColor);
		_keys = Hud.FormatLabel(keys, textSize, textColor);
		_buttons = Hud.FormatLabel(buttons, textSize, textColor);

		_actionsShadow = Hud.FormatLabel(actions, textSize, shadowColor);
		_keysShadow = Hud.FormatLabel(keys, textSize, shadowColor);
		_buttonsShadow = Hud.FormatLabel(buttons, textSize, shadowColor);
	}


	public string Title
	{
		get { return _title; }
	}

	public string TitleShadow
	{
		get { return _titleShadow; }
	}

	public string Actions
	{
		get { return _actions; }
	}

	public string Keys
	{
		get { return _keys; }
	}

	public string Buttons
	{
		get { return _buttons; }
	}

	public string ActionsShadow
	{
		get { return _actionsShadow; }
	}

	public string KeysShadow
	{
		get { return _keysShadow; }
	}

	public string ButtonsShadow
	{
		get { return _buttonsShadow; }
	}
}



internal class PlayModeHudInstructions : HudInstructions
{
}



internal class PhotoModeHudInstructions : HudInstructions
{
	private string _note;
	private string _noteShadow;


	public void BuildNoteLabel(string text, int size, string textColor, string shadowColor)
	{
		_note = Hud.FormatLabel(text, size, textColor);
		_noteShadow = Hud.FormatLabel(text, size, shadowColor);
	}


	public string Note
	{
		get { return _note; }
	}

	public string NoteShadow
	{
		get { return _noteShadow; }
	}
}