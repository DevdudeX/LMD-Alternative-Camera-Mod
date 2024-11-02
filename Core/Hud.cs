using System.Text;
using LMSR_AlternativeCameraMod.Config;
using LMSR_AlternativeCameraMod.Language;
using MelonLoader;
using UnityEngine;


namespace LMSR_AlternativeCameraMod;

internal class Hud
{
	const string InstructionPrefix = " ► ";
	const string InstructionSeparator = " ├  ";
	const string InstructionSeparatorLast = " └  ";

	private readonly State _state;
	private readonly CameraControl _camera;
	private readonly InputHandler _input;
	private readonly Configuration _cfg;
	private readonly LanguageConfig _lang;
	private readonly Logger _logger;
	private bool _hudInfoVisible = true;
	private readonly List<ModHudInfoPart> _hudInfoParts = new();
	private int _fpsDisplayThreshold;
	private int _displayedFps;
	private bool _intialized;
	private PlayModeHudInstructions _playModeInstr;
	private PhotoModeHudInstructions _photoModeInstr;


	public Hud(State state, CameraControl camera, InputHandler input, Configuration cfg, LanguageConfig lang, Logger logger)
	{
		_state = state;
		_camera = camera;
		_input = input;
		_cfg = cfg;
		_lang = lang;
		_logger = logger;
	}


	public void Initialize()
	{
		if (_intialized) return;

		PreBuildPlayModeInstructions();
		PreBuildPhotoModeInstructions();

		ParseHudInfoDisplayFlags(_cfg.PlayMode.ModHudInfoDisplay.Value);
		MelonEvents.OnGUI.Subscribe(DrawInfoOnHud, 100);
		_intialized = true;
	}


	public void Close()
	{
		MelonEvents.OnGUI.Unsubscribe(DrawInfoOnHud);
		_intialized = false;
	}


	private void DrawInfoOnHud()
	{
		if (_state.Suspended)
		{
			// when disabled, only show text on menu screen, do not show others
			if (_state.CurrentScreen != Screen.MainMenuScreen)
			{
				return;
			}
		}

		StringBuilder text = new();
		int x = 0, y = 0, size = 10;
		string color = "#FFFFFF";
		string shadowColor = "#000000";
		int shadowOffsetX = 1;
		int shadowOffsetY = 1;
		bool addShadow = false;
		bool addBox = false;
		int boxPadding = 10;
		int textWidth = 800;
		int textHeight = 120;

		_logger.LogVerbose("Screen: {0}", _state.CurrentScreen);
		switch (_state.CurrentScreen)
		{
			case Screen.None:
				return;

			case Screen.LoadingScreen:
				text.AppendFormat(_lang.GetText(
					"Mod",
					"Title_{Version}",
					"Alternative Camera with Photo Mode {0}",
					AlternativeCamera.MOD_VERSION)
				);
				x = UnityEngine.Screen.currentResolution.width / 2 - 200;
				y = UnityEngine.Screen.currentResolution.height - 50;
				size = 20;
				color = Configuration.GameColor;
				break;

			case Screen.SplashScreen:
				// nothing
				return;

			case Screen.MainMenuScreen:
				text.AppendFormat(_lang.GetText(
					"Mod",
					"Title_{Version}",
					"Alternative Camera with Photo Mode {0}",
					AlternativeCamera.MOD_VERSION).ToUpper()
				);
#if DEBUG
				text.Append(" (DBG)");
#endif
				x = 130;
				y = UnityEngine.Screen.currentResolution.height - 250;
				size = 20;
				shadowOffsetX = 1;
				shadowOffsetY = 2;
				color = Configuration.GameColor2; // matches LMD text color

				if (!string.IsNullOrEmpty(_state.ErrorMessage))
				{
					color = nameof(Color.red);
					addBox = true;
					text.AppendLine();
					text.AppendFormat(_lang.GetText("Mod", "ErrorOutputLine_{Msg}", "[ERR] {0}", _state.ErrorMessage));
				}

				addShadow = true;
				break;

			case Screen.PlayScreen:
				if (!IsHudVisible()) return;

				if (ShowHudInfo())
				{
					x = 20;
					y = UnityEngine.Screen.currentResolution.height - 28;
					size = Math.Max(5, _cfg.PlayMode.ModHudTextSize.Value);
					color = "#FFFFFF";
					BuildHudInfoText(text);
				}

				break;

			case Screen.PhotoScreen:
				if (_state.PhotoModeInstructionsVisible)
				{
					DrawPhotoModeInstructionsOnHud();
				}

				break;

			case Screen.PauseScreen:
				if (_state.IsPausedInPhotoMode) return;

				if (_cfg.PlayMode.ShowCamInstructionsInPauseMenu.Value)
				{
					DrawPlayModeInstructionsOnHud();
				}
				else
				{
					x = 1300;
					y = UnityEngine.Screen.currentResolution.height - 120;
					size = Math.Max(5, _cfg.PlayMode.ModHudTextSize.Value);
					color = "#FFFFFF";
					text.Append(_lang.GetText("PlayMode", "PressKeyForInstructions_{key}", "(press {0} for instructions)", "'I' / 'R-Stick'"));
				}
				break;
		}

		if (text.Length > 0)
		{
			if (addBox)
			{
				GUI.Box(new Rect(x - boxPadding, y - boxPadding, textWidth + boxPadding * 2, textHeight + boxPadding * 2), "");
			}
			if (addShadow)
			{
				GUI.Label(
					new Rect(x + shadowOffsetX, y + shadowOffsetY, textWidth, textHeight),
					FormatLabel(text.ToString(), size, shadowColor, true)
				);
			}

			GUI.Label(
				new Rect(x, y, textWidth, textHeight),
				FormatLabel(text.ToString(), size, color, true)
			);
		}
	}


	internal static string FormatLabel(string text, int size, string color, bool bold = false, bool italic = false)
	{
		string preFmt = "";
		string postFmt = "";
		if (bold)
		{
			preFmt += "<b>";
			postFmt += "</b>";
		}
		if (italic)
		{
			preFmt += "<i>";
			postFmt += "</i>";
		}

		var str = String.Format("{3}<color={2}><size={1}>{0}</size></color>{4}", text, size, color, preFmt, postFmt);
		return str;
	}


	private void PreBuildPlayModeInstructions()
	{
		List<PlayModeAction> actionListInOrderToShow = new()
		{
			PlayModeAction.CameraModeOriginal,
			PlayModeAction.CameraModeThirdPerson,
			PlayModeAction.CameraModeFirstPerson,
			PlayModeAction.ToggleCameraState,
			PlayModeAction.ToggleCameraAutoAlignMode,
			PlayModeAction.InvertCameraAutoAlignMode,
			PlayModeAction.ToggleInvertLookHorizontal,
			PlayModeAction.ToggleGameHud,
			PlayModeAction.ToggleModHudDisplays,
			PlayModeAction.LookAround,
			PlayModeAction.SnapCameraBehindBike,
			PlayModeAction.ZoomInOut,
			PlayModeAction.ChangeDoFFocalLength,
			PlayModeAction.ChangeDoFFocusDistanceOffset,
			PlayModeAction.IncreaseFoV,
			PlayModeAction.DecreaseFoV,
			PlayModeAction.ResetFoV
		};

		StringBuilder actions = new();

		actions.AppendLine(_lang.GetText("Input", "ActionHeader", "ACTION"));
		for (int index = 0; index < actionListInOrderToShow.Count; index++)
		{
			PlayModeAction action = actionListInOrderToShow[index];
			actions.AppendLine(InstructionPrefix + _input.PlayMode.GetActionText(action));
		}

		StringBuilder keys = new();
		keys.AppendLine(_lang.GetText("Input", "KeyMouseHeader", "KEYBOARD/MOUSE"));
		for (int index = 0; index < actionListInOrderToShow.Count; index++)
		{
			PlayModeAction action = actionListInOrderToShow[index];
			keys.AppendLine((index < actionListInOrderToShow.Count - 1 ? InstructionSeparator : InstructionSeparatorLast) + _input.PlayMode.GetKeyText(action));
		}

		StringBuilder btns = new();
		btns.AppendLine(_lang.GetText("Input", "ControllerHeader", "CONTROLLER"));
		for (int index = 0; index < actionListInOrderToShow.Count; index++)
		{
			PlayModeAction action = actionListInOrderToShow[index];
			btns.AppendLine((index < actionListInOrderToShow.Count - 1 ? InstructionSeparator : InstructionSeparatorLast) + _input.PlayMode.GetButtonText(action));
		}

		string pmLabel = _lang.GetText("PlayMode", "Title", "CONTROLS");

		_playModeInstr = new PlayModeHudInstructions();
		_playModeInstr.Build(
			pmLabel,
			actions.ToString(),
			keys.ToString(),
			btns.ToString(),
			titleSize: 30,
			titleColor: Configuration.GameColor2,
			textSize: 20,
			textColor: nameof(Color.white),
			shadowColor: nameof(Color.black),
			shadowOffset: 2
		);
	}


	private void PreBuildPhotoModeInstructions()
	{
		List<PhotoModeAction> actionListInOrderToShow = new()
		{
			PhotoModeAction.Exit,
			PhotoModeAction.TakePhoto,
			PhotoModeAction.ToggleInstructions,
			PhotoModeAction.ToggleHud,
			PhotoModeAction.MovePan,
			PhotoModeAction.UpDown,
			PhotoModeAction.Tilt,
			PhotoModeAction.SpeedUp,
			PhotoModeAction.Reset,
			PhotoModeAction.ToggleFoVDoF,
			PhotoModeAction.ChangeFoVDoF
		};

		StringBuilder actions = new();
		actions.AppendLine(_lang.GetText("Input", "ActionHeader", "ACTION"));
		for (int index = 0; index < actionListInOrderToShow.Count; index++)
		{
			PhotoModeAction action = actionListInOrderToShow[index];
			actions.AppendLine(InstructionPrefix + _input.PhotoMode.GetActionText(action));
		}

		StringBuilder keys = new();
		keys.AppendLine(_lang.GetText("Input", "KeyMouseHeader", "KEYBOARD/MOUSE"));
		for (int index = 0; index < actionListInOrderToShow.Count; index++)
		{
			PhotoModeAction action = actionListInOrderToShow[index];
			keys.AppendLine((index < actionListInOrderToShow.Count - 1 ? InstructionSeparator : InstructionSeparatorLast) + _input.PhotoMode.GetKeyText(action));
		}

		StringBuilder btns = new();
		btns.AppendLine(_lang.GetText("Input", "ControllerHeader", "CONTROLLER"));
		for (int index = 0; index < actionListInOrderToShow.Count; index++)
		{
			PhotoModeAction action = actionListInOrderToShow[index];
			btns.AppendLine((index < actionListInOrderToShow.Count - 1 ? InstructionSeparator : InstructionSeparatorLast) + _input.PhotoMode.GetButtonText(action));
		}

		string pmLabel = _lang.GetText("PhotoMode", "Title", "PHOTO MODE");

		_photoModeInstr = new PhotoModeHudInstructions();
		_photoModeInstr.Build(
			pmLabel,
			actions.ToString(),
			keys.ToString(),
			btns.ToString(),
			titleSize: 30,
			titleColor: Configuration.GameColor2,
			textSize: 20,
			textColor: nameof(Color.white),
			shadowColor: nameof(Color.black),
			shadowOffset: 2
		);

		_photoModeInstr.BuildNoteLabel(
			_lang.GetText("PhotoMode", "Note", "(This instructions box is not part of the photo)"),
			15,
			"#CCCCCC",
			nameof(Color.black)
		);
	}


	private void WriteLabel(string text, string shadowText, float x, float y, float w, float h, float shadowOffset = 2)
	{
		GUI.Label(new Rect(x + shadowOffset, y + shadowOffset, w, h), shadowText);
		GUI.Label(new Rect(x, y, w, h), text);
	}


	private void DrawPlayModeInstructionsOnHud()
	{
		int wA = _lang.GetIntNum("PlayMode", "ColWidthAction", 450);
		int wC = _lang.GetIntNum("PlayMode", "ColWidthController", 200);
		int wK = _lang.GetIntNum("PlayMode", "ColWidthKeyMouse", 200);

		var boxPadding = 20;
		var boxWidth = wA + wK + wC + boxPadding * 2;
		var boxHeight = 480;

		float xPosA = 850;
		float xPosK = xPosA + wA;
		float xPosB = xPosK + wK;

		float yPosOffset = 280;
		float yPosTitle = yPosOffset;
		float yPosInstr = yPosOffset + 50;

		GUI.Box(new Rect(xPosA - boxPadding, yPosOffset - boxPadding, boxWidth + boxPadding * 2, boxHeight + boxPadding * 2), "");

		WriteLabel(_playModeInstr.Title, _playModeInstr.TitleShadow, xPosA, yPosTitle, 1000, 200);
		WriteLabel(_playModeInstr.Actions, _playModeInstr.ActionsShadow, xPosA, yPosInstr, 2000, 2000);
		WriteLabel(_playModeInstr.Keys, _playModeInstr.KeysShadow, xPosK, yPosInstr, 2000, 2000);
		WriteLabel(_playModeInstr.Buttons, _playModeInstr.ButtonsShadow, xPosB, yPosInstr, 2000, 2000);
	}


	private void DrawPhotoModeInstructionsOnHud()
	{
		string focusModeLabel = _lang.GetText("PhotoMode", "FocusAdjustModeLabel_{state}", "Focus mode: {0}", GetFocusModeText());
		int wA = _lang.GetIntNum("PhotoMode", "ColWidthAction", 350);
		int wC = _lang.GetIntNum("PhotoMode", "ColWidthController", 200);
		int wK = _lang.GetIntNum("PhotoMode", "ColWidthKeyMouse", 200);

		var boxPadding = 20;
		var boxWidth = wA + wK + wC + boxPadding * 2;
		var boxHeight = 480;

		float xPosA = 50;
		float xPosK = xPosA + wA;
		float xPosC = xPosK + wK;

		float yPosTitle = 200;
		float yPosInstr = 250;
		float yPosNote = 540;
		float yPosState = 600;
		float yPosSaveInfo = 680 + boxPadding;

		GUI.Box(new Rect(xPosA - boxPadding, yPosTitle - boxPadding, boxWidth + boxPadding * 2, boxHeight + boxPadding * 2), "");

		WriteLabel(_photoModeInstr.Title, _photoModeInstr.TitleShadow, xPosA, yPosTitle, 1000, 200);
		WriteLabel(_photoModeInstr.Actions, _photoModeInstr.ActionsShadow, xPosA, yPosInstr, 2000, 2000);
		WriteLabel(_photoModeInstr.Keys, _photoModeInstr.KeysShadow, xPosK, yPosInstr, 2000, 2000);
		WriteLabel(_photoModeInstr.Buttons, _photoModeInstr.ButtonsShadow, xPosC, yPosInstr, 2000, 2000);

		WriteLabel(_photoModeInstr.Note, _photoModeInstr.NoteShadow, xPosA, yPosNote, 2000, 2000, 1);
		WriteLabel(FormatLabel(focusModeLabel, 20, "lightblue"), FormatLabel(focusModeLabel, 20, "black"), xPosA, yPosState, 2000, 2000, 2);

		if (!string.IsNullOrEmpty(_state.LastScreenshotInfo))
		{
			GUI.Box(new Rect(xPosA - boxPadding, yPosSaveInfo - boxPadding, boxWidth + boxPadding * 2, 90), "");

			WriteLabel(
				FormatLabel(_state.LastScreenshotInfo, 20, Configuration.GameColor2),
				FormatLabel(_state.LastScreenshotInfo, 20, "black"),
				xPosA, yPosSaveInfo, boxWidth, 200
			);
		}
	}


	private string GetFocusModeText()
	{
		switch (_camera.FocusAdjustMode)
		{
			case CameraFocusAdjustMode.DepthOfField:
				string dofLabel = _lang.GetText("PhotoMode", "DepthOfField_{dof}", "depth of field distance ({0})", _camera.DepthOfField);
				return dofLabel;

			default:
			case CameraFocusAdjustMode.FieldOfView:
				string fovLabel = _lang.GetText("PhotoMode", "FieldOfView_{fov}", "field of view ({0})", _camera.FieldOfView);
				return fovLabel;
		}
	}


	public static Camera? GetHudCam()
	{
		GameObject hudCamObj = GameObject.Find("UI_Camera");
		if (hudCamObj == null)
		{
			return null;
		}

		Camera hudCam = hudCamObj.GetComponent<Camera>();
		return hudCam;
	}


	/// <summary>
	/// Toggles the rendering of the game HUD.
	/// </summary>
	public void ToggleHudVisiblity(bool? visible = null)
	{
		var hud = GetHudCam();
		if (hud == null)
		{
			return;
		}

		if (visible.HasValue)
		{
			_logger.LogDebug("Init Game Hud: {0}", visible.Value);
			hud.enabled = visible.Value;
		}
		else
		{
			hud.enabled = !hud.enabled;
			_logger.LogDebug("Toggle Game Hud: {0}", hud.enabled);
		}
	}


	public void ToggleModHudInfoVisibility()
	{
		_hudInfoVisible = !_hudInfoVisible;
	}


	public bool IsHudVisible()
	{
		var hudCam = GetHudCam();
		return hudCam != null && hudCam.enabled;
	}


	private bool ShowHudInfo()
	{
		return _hudInfoParts.Count > 0 && _hudInfoVisible && IsHudVisible();
	}


	internal void ParseHudInfoDisplayFlags(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}

		var tokens = text.Split(',').Select(t => t.Trim());
		foreach (var token in tokens)
		{
			if (Enum.TryParse<ModHudInfoPart>(token, true, out var flag))
			{
				_hudInfoParts.Add(flag);
			}
		}
	}


	private void BuildHudInfoText(StringBuilder target)
	{
		foreach (var infoPart in _hudInfoParts)
		{
			if (target.Length > 0)
			{
				target.Append("  ");
			}

			switch (infoPart)
			{
				case ModHudInfoPart.CamMode:
					string mode = "";
					switch (_camera.CurrentCamView)
					{
						case CameraView.Original:
							mode = _lang.GetText("Mod", "CamOriginal", "Original");
							break;
						case CameraView.ThirdPerson:
							mode = _lang.GetText("Mod", "CamThirdPerson", "Third Person");
							break;
						case CameraView.FirstPerson:
							mode = _lang.GetText("Mod", "CamFirstPerson", "First Person");
							break;
					}

					target.Append(_lang.GetText("Mod", "CamLabel_{Mode}", "Cam: {0}", mode));
					break;

				case ModHudInfoPart.FoV:
					float val = 0;
					switch (_camera.CurrentCamView)
					{
						case CameraView.Original:
							val = _camera.IsometricFoV;
							break;
						case CameraView.ThirdPerson:
							val = _camera.ThirdPersonFoV;
							break;
						case CameraView.FirstPerson:
							val = _camera.FirstPersonFoV;
							break;
					}

					target.Append(_lang.GetText("Mod", "FoVInfoLabel_{fov}", "FoV: {0}", val));
					break;

				case ModHudInfoPart.FPS:
					_fpsDisplayThreshold--;
					if (_fpsDisplayThreshold <= 0)
					{
						_displayedFps = _state.Fps;
						_fpsDisplayThreshold = _state.Fps;
					}
					target.Append(_lang.GetText("Mod", "FPSLabel_{fps}", "FPS: {0}", _displayedFps));
					break;

				case ModHudInfoPart.CamAlign:
					string align = _camera.AutoAlign
					   ? (_input.PlayMode.InvertAlignmentMode() ? "Manual" : "Auto")
					   : (_input.PlayMode.InvertAlignmentMode() ? "Auto" : "Manual");

					target.Append(_lang.GetText("Mod", "CamAlignLabel_{align}", "({0})", align));
					break;
			}
		}
	}
}