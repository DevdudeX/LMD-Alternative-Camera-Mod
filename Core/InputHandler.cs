using LMSR_AlternativeCameraMod.Config;
using LMSR_AlternativeCameraMod.Language;
using UnityEngine;


namespace LMSR_AlternativeCameraMod;

internal class InputHandler
{
	private readonly Configuration _cfg;
	private readonly LanguageConfig _lang;
	private readonly Logger _logger;

	// General
	private float _moveHorizontal;
	private float _moveVertical;

	// Controller Inputs
	private float _dpadHorizontal;
	private float _dpadVertical;
	private float _triggerInputL;
	private float _triggerInputR;
	private float _leftStickHorizontal;
	private float _leftStickVertical;
	private float _rightStickHorizontal;
	private float _rightStickVertical;

	private bool _buttonHold0; // A
	private bool _buttonHold5; // right bumper

	private bool _buttonDown0; // A
	private bool _buttonDown1; // B
	private bool _buttonDown2; // X
	private bool _buttonDown3; // Y
	private bool _buttonDown4; // left bumper
	private bool _buttonDown5; // right bumper
	private bool _buttonDown6; // back/select
	private bool _buttonDown7; // start
	private bool _buttonDown8; // left stick click
	private bool _buttonDown9; // right stick click

	private bool _dpadUp;
	private bool _dpadDown;
	private bool _dpadLeft;
	private bool _dpadRight;
	private float _dpadPressDetectThreshold; // Used to treat dpad as a button
	private bool _dpadSingleClickDetect;

	// Mouse
	private Vector3 _lastMousePos;
	private bool _mouseMoved;
	private int _mouseVisibleThreshold;

	// Keyboard
	private bool _escapeKeyDown;


	public InputHandler(Configuration cfg, LanguageConfig lang, Logger logger)
	{
		_cfg = cfg;
		_lang = lang;
		_logger = logger;
		Cursor.lockState = CursorLockMode.None;
		PlayMode = new PlayModeInput(this, cfg, lang);
		PhotoMode = new PhotoModeInput(this, cfg, _lang);
	}


	public void OnUpdate()
	{
		DetermineStickInput();
		DetermineTriggerInput();
		DetermineButtonInput();
		DetermineDpadInput();

		DetectMouseMovement();
		_escapeKeyDown = KeyDown(KeyCode.Escape); // Escape key down can safely detected only in OnUpdate, never later
	}


	public void HideMouseCursor()
	{
		// Lock and hide the cursor 
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}


	public void ShowMouseCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}


	private void DetectMouseMovement()
	{
		var mpos = Input.mousePosition;
		_mouseMoved = !_lastMousePos.Equals(mpos);
		_lastMousePos = mpos;
	}


	public void EnableMouseCursorOnDemand(int fps)
	{
		if (_mouseMoved)
		{
			_mouseVisibleThreshold = fps * 3;
			ShowMouseCursor();
		}
		else if (_mouseVisibleThreshold > 0)
		{
			_mouseVisibleThreshold -= 1;
		}
		else if (_mouseVisibleThreshold <= 0)
		{
			HideMouseCursor();
		}
	}


	private void DetermineTriggerInput()
	{
		_triggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") +
						 Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
		_triggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") +
						 Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");
	}


	private void DetermineStickInput()
	{
		_leftStickHorizontal = Input.GetAxisRaw("Joy1Axis1") + Input.GetAxisRaw("Joy2Axis1") +
							   Input.GetAxisRaw("Joy3Axis1") + Input.GetAxisRaw("Joy4Axis1");
		_leftStickVertical = Input.GetAxisRaw("Joy1Axis2") + Input.GetAxisRaw("Joy2Axis2") +
							 Input.GetAxisRaw("Joy3Axis2") + Input.GetAxisRaw("Joy4Axis2");

		_rightStickHorizontal = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") +
								Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
		_rightStickVertical = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") +
							  Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

		// combines all input devices, arrows and stick, whatever does horizontal movement
		_moveHorizontal = Input.GetAxisRaw("Horizontal");

		// combines all input devices, arrows and stick, whatever does vertical movement
		_moveVertical = Input.GetAxisRaw("Vertical");
	}


	private void DetermineDpadInput()
	{
		_dpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") +
						  Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
		_dpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") +
						Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

		if (!_dpadSingleClickDetect && _dpadPressDetectThreshold <= 0)
		{
			_dpadLeft = _dpadHorizontal < 0f;
			_dpadRight = _dpadHorizontal > 0f;
			_dpadUp = _dpadVertical > 0f;
			_dpadDown = _dpadVertical < 0f;

			_dpadSingleClickDetect = _dpadLeft | _dpadRight | _dpadUp | _dpadDown;
			if (_dpadSingleClickDetect)
			{
				_dpadPressDetectThreshold = 0.15f;
			}
		}
		else
		{
			_dpadUp = false;
			_dpadDown = false;
			_dpadLeft = false;
			_dpadRight = false;

			float dt = Time.deltaTime <= 0 ? 0.005f : Time.deltaTime;
			_dpadPressDetectThreshold -= dt;
			if (_dpadPressDetectThreshold <= 0)
			{
				_dpadSingleClickDetect = false;
				_dpadPressDetectThreshold = 0;
			}
		}
	}


	private void DetermineButtonInput()
	{
		_buttonHold0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) ||
					   Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
		_buttonHold5 = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) ||
					   Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

		_buttonDown0 = Input.GetKeyDown(KeyCode.Joystick1Button0) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button0) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button0) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button0);
		_buttonDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button1) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button1) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button1);
		_buttonDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button1) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button1) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button1);
		_buttonDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button2) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button2) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button2);
		_buttonDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button3) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button3) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button3);
		_buttonDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button4) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button4) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button4);
		_buttonDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button5) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button5) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button5);
		_buttonDown6 = Input.GetKeyDown(KeyCode.Joystick1Button6) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button6) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button6) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button6);
		_buttonDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button7) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button7) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button7);
		_buttonDown8 = Input.GetKeyDown(KeyCode.Joystick1Button8) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button8) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button8) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button8);
		_buttonDown9 = Input.GetKeyDown(KeyCode.Joystick1Button9) ||
					   Input.GetKeyDown(KeyCode.Joystick2Button9) ||
					   Input.GetKeyDown(KeyCode.Joystick3Button9) ||
					   Input.GetKeyDown(KeyCode.Joystick4Button9);

		// e.g. Xbox Controller
		if (_buttonDown0) _logger.LogDebug("Button 0 down (A)"); // A
		if (_buttonDown1) _logger.LogDebug("Button 1 down (B)"); // B
		if (_buttonDown2) _logger.LogDebug("Button 2 down (X)"); // X
		if (_buttonDown3) _logger.LogDebug("Button 3 down (Y)"); // Y
		if (_buttonDown4) _logger.LogDebug("Button 4 down (LB)"); // left shoulder
		if (_buttonDown5) _logger.LogDebug("Button 5 down (RB)"); // right shoulder
		if (_buttonDown6) _logger.LogDebug("Button 6 down (SEL)"); // back/select
		if (_buttonDown7) _logger.LogDebug("Button 7 down (START)"); // start
		if (_buttonDown8) _logger.LogDebug("Button 8 down (LS)"); // left stick click
		if (_buttonDown9) _logger.LogDebug("Button 9 down (RS)"); // right stick click
	}


	private bool KeyDown(KeyCode kc)
	{
		if (kc == KeyCode.None) return false;
		bool pressed = Input.GetKeyDown(kc);
		return pressed;
	}


	private bool KeyHold(KeyCode kc)
	{
		if (kc == KeyCode.None) return false;
		bool pressed = Input.GetKey(kc);
		return pressed;
	}


	private bool ButtonDown(ControllerButton gpBtn)
	{
		bool pressed = false;
		switch (gpBtn)
		{
			case ControllerButton.X:
				pressed = _buttonDown2;
				_logger.LogDebug(pressed, "Controller X");
				break;
			case ControllerButton.Y:
				pressed = _buttonDown3;
				_logger.LogDebug(pressed, "Controller Y");
				break;
			case ControllerButton.LB:
				pressed = _buttonDown4;
				_logger.LogDebug(pressed, "Controller LB");
				break;
			case ControllerButton.RB:
				pressed = _buttonDown5;
				_logger.LogDebug(pressed, "Controller RB");
				break;
			case ControllerButton.LStick:
				pressed = _buttonDown8;
				_logger.LogDebug(pressed, "Controller LStick");
				break;
			case ControllerButton.RStick:
				pressed = _buttonDown9;
				_logger.LogDebug(pressed, "Controller RStick");
				break;
		}

		return pressed;
	}


	/// <summary>
	/// Snaps the given value to 0 if it falls within the deadzone radius in either direction.
	/// </summary>
	/// <returns>The axis if outside the deadzone, otherwise returns 0.</returns>
	private float ApplyInnerDeadzone(float axis, float deadzone)
	{
		if (axis > deadzone || axis < -deadzone)
		{
			return axis;
		}

		return 0;
	}


	private string ConvertToText(string value, string? fallbackText = null)
	{
		string keyTxtLang = _lang.GetText("Input", value, fallbackText ?? value);
		return keyTxtLang;
	}


	private string ConvertToText(KeyCode value)
	{
		switch (value)
		{
			case KeyCode.Alpha0:
			case KeyCode.Alpha1:
			case KeyCode.Alpha2:
			case KeyCode.Alpha3:
			case KeyCode.Alpha4:
			case KeyCode.Alpha5:
			case KeyCode.Alpha6:
			case KeyCode.Alpha7:
			case KeyCode.Alpha8:
			case KeyCode.Alpha9:
				return value.ToString().Substring(5, 1);
		}

		string keyTxt = value.ToString();
		string keyTxtLang = _lang.GetText("Input", keyTxt, "---");
		if (keyTxtLang != "---")
		{
			return keyTxtLang;
		}

		return keyTxt;
	}


	private string ConvertToText(ControllerButton btn)
	{
		string keyTxtLang = _lang.GetText("Input", btn.ToString(), btn.ToString());
		return keyTxtLang;
	}


	public bool OpenMenu()
	{
		return _escapeKeyDown || _buttonDown7 /*start*/;
	}


	public PlayModeInput PlayMode { get; }
	public PhotoModeInput PhotoMode { get; }


	internal class PlayModeInput
	{
		private readonly InputHandler _inputHandler;
		private readonly Configuration _cfg;
		private readonly LanguageConfig _lang;


		public PlayModeInput(InputHandler ih, Configuration cfg, LanguageConfig lang)
		{
			_inputHandler = ih;
			_cfg = cfg;
			_lang = lang;
		}


		public bool ShowInstructions
		{
			get { return _inputHandler.KeyDown(KeyCode.I) || _inputHandler.ButtonDown(ControllerButton.RStick); }
		}


		public bool SelectOriginalCamera()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.OriginalCamKey.Value);
		}

		public bool SelectThirdPerson()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.ThirdPersonCamKey.Value);
		}

		public bool SelectFirstPerson()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.FirstPersonCamKey.Value);
		}

		public bool InvertAlignmentMode()
		{
			return _inputHandler._buttonHold5 || _inputHandler.KeyHold(_cfg.Keyboard.InvertCamAlignModeKey.Value);
		}

		public bool ToggleCamState()
		{
			return _inputHandler.ButtonDown(_cfg.Controller.ToggleCamStateButton.Value)
				   || _inputHandler.KeyDown(_cfg.Keyboard.ToggleCamStateKey.Value);
		}

		public bool InvertHorizontalLook()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.InvertHorizontalLookKey.Value);
		}

		public bool ToggleCamAlignMode()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.ToggleCamAlignModeKey.Value);
		}

		public bool SnapBehindBike()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.SnapAlignCamKey.Value)
				   || _inputHandler.ButtonDown(_cfg.Controller.SnapAlignCamButton.Value);
		}

		public bool IncreaseFieldOfView()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.FovDecreaseKey.Value) || _inputHandler._dpadLeft;
		}

		public bool DecreaseFieldOfView()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.FovIncreaseKey.Value) || _inputHandler._dpadRight;
		}

		public bool ResetFieldOfView()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.FovResetKey.Value);
		}

		public bool EnterPhotoMode()
		{
			return _inputHandler.KeyDown(KeyCode.P) || _inputHandler._buttonDown3;
		}

		public bool ToggleFieldOfViewIncrement()
		{
			return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		}

		public bool ToggleHud()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.HudToggleKey.Value);
		}

		public bool ToggleModHud()
		{
			return _inputHandler.KeyDown(_cfg.Keyboard.HudInfoTextToggleKey.Value);
		}

		public bool ZoomIn()
		{
			return Input.GetAxis("Mouse ScrollWheel") > 0f || _inputHandler._dpadUp;
		}

		public bool ZoomOut()
		{
			return Input.GetAxis("Mouse ScrollWheel") < 0f || _inputHandler._dpadDown;
		}

		public bool AdjustFocalLength()
		{
			return _inputHandler.KeyHold(_cfg.Keyboard.AdjustFocalLengthKey.Value);
		}

		public bool AdjustFocusDistance()
		{
			return _inputHandler.KeyHold(_cfg.Keyboard.AdjustFocusDistanceKey.Value);
		}


		public string GetKeyText(PlayModeAction action)
		{
			return action switch
			{
				PlayModeAction.CameraModeOriginal           => _inputHandler.ConvertToText(_cfg.Keyboard.OriginalCamKey.Value),
				PlayModeAction.CameraModeFirstPerson        => _inputHandler.ConvertToText(_cfg.Keyboard.FirstPersonCamKey.Value),
				PlayModeAction.CameraModeThirdPerson        => _inputHandler.ConvertToText(_cfg.Keyboard.ThirdPersonCamKey.Value),
				PlayModeAction.ToggleCameraAutoAlignMode    => _inputHandler.ConvertToText(_cfg.Keyboard.ToggleCamAlignModeKey.Value),
				PlayModeAction.ToggleCameraState            => _inputHandler.ConvertToText(_cfg.Keyboard.ToggleCamStateKey.Value),
				PlayModeAction.ToggleInvertLookHorizontal   => _inputHandler.ConvertToText(_cfg.Keyboard.InvertHorizontalLookKey.Value),
				PlayModeAction.ToggleGameHud                => _inputHandler.ConvertToText(_cfg.Keyboard.HudToggleKey.Value),
				PlayModeAction.ToggleModHudDisplays         => _inputHandler.ConvertToText(_cfg.Keyboard.HudInfoTextToggleKey.Value),
				PlayModeAction.LookAround                   => _inputHandler.ConvertToText("Mouse", "Mouse"),
				PlayModeAction.SnapCameraBehindBike         => _inputHandler.ConvertToText(_cfg.Keyboard.SnapAlignCamKey.Value),
				PlayModeAction.InvertCameraAutoAlignMode    => _inputHandler.ConvertToText(_cfg.Keyboard.InvertCamAlignModeKey.Value),
				PlayModeAction.ZoomInOut                    => _inputHandler.ConvertToText("MouseWheel", "Mouse Scroll"),
				PlayModeAction.ChangeDoFFocalLength         => _inputHandler.ConvertToText(_cfg.Keyboard.AdjustFocalLengthKey.Value),
				PlayModeAction.ChangeDoFFocusDistanceOffset => _inputHandler.ConvertToText(_cfg.Keyboard.AdjustFocusDistanceKey.Value),
				PlayModeAction.IncreaseFoV                  => _inputHandler.ConvertToText(_cfg.Keyboard.FovIncreaseKey.Value),
				PlayModeAction.DecreaseFoV                  => _inputHandler.ConvertToText(_cfg.Keyboard.FovDecreaseKey.Value),
				PlayModeAction.ResetFoV                     => _inputHandler.ConvertToText(_cfg.Keyboard.FovResetKey.Value),
				_ => "",
			};
		}


		public string GetButtonText(PlayModeAction action)
		{
			return action switch
			{
				PlayModeAction.CameraModeOriginal           => "-",
				PlayModeAction.CameraModeFirstPerson        => "-",
				PlayModeAction.CameraModeThirdPerson        => "-",
				PlayModeAction.ToggleCameraAutoAlignMode    => "-",
				PlayModeAction.ToggleCameraState            => _inputHandler.ConvertToText(_cfg.Controller.ToggleCamStateButton.Value),
				PlayModeAction.ToggleInvertLookHorizontal   => "-",
				PlayModeAction.ToggleGameHud                => "-",
				PlayModeAction.ToggleModHudDisplays         => "-",
				PlayModeAction.LookAround                   => _inputHandler.ConvertToText("RStick"),
				PlayModeAction.SnapCameraBehindBike         => _inputHandler.ConvertToText("LButton"),
				PlayModeAction.InvertCameraAutoAlignMode    => _inputHandler.ConvertToText("RButton"),
				PlayModeAction.ZoomInOut => string.Format( "{0} {1} / {2}", 
															_inputHandler.ConvertToText("Dpad"),
															_inputHandler.ConvertToText("DpadUp", "▲"),
															_inputHandler.ConvertToText("DpadDown", "▼")
														),
				PlayModeAction.ChangeDoFFocalLength         => "-",
				PlayModeAction.ChangeDoFFocusDistanceOffset => "-",
				PlayModeAction.IncreaseFoV                  => string.Format("{0} {1}", _inputHandler.ConvertToText("Dpad"), _inputHandler.ConvertToText("DpadLeft", "◄")),
				PlayModeAction.DecreaseFoV                  => string.Format("{0} {1}", _inputHandler.ConvertToText("Dpad"), _inputHandler.ConvertToText("DpadRight", "►")),
				PlayModeAction.ResetFoV                     => "-",
				_ => "",
			};
		}


		public string GetActionText(PlayModeAction action)
		{
			const string secId = "PlayMode";
			return action switch
			{
				PlayModeAction.CameraModeOriginal           => _lang.GetText(secId, "ActionCameraOriginal", "Original Camera (isometric)"),
				PlayModeAction.CameraModeFirstPerson        => _lang.GetText(secId, "ActionCameraFirst", "Alternative Camera: First Person"),
				PlayModeAction.CameraModeThirdPerson        => _lang.GetText(secId, "ActionCameraThird", "Alternative Camera: Third person"),
				PlayModeAction.ToggleCameraAutoAlignMode    => _lang.GetText(secId, "ActionCameraAutoAlign", "Toggle align mode Auto ↔ Manual"),
				PlayModeAction.ToggleCameraState            => _lang.GetText(secId, "ActionToggleCamState", "Toggle camera: Original ↔ Alternative"),
				PlayModeAction.ToggleInvertLookHorizontal   => _lang.GetText(secId, "ActionInvertLookHorizontal", "Toggle invert look horizontal"),
				PlayModeAction.ToggleGameHud                => _lang.GetText(secId, "ActionToggleGameHud", "Toggle Game HUD"),
				PlayModeAction.ToggleModHudDisplays         => _lang.GetText(secId, "ActionToggleModHud", "Toggle Mod HUD Displays"),
				PlayModeAction.LookAround                   => _lang.GetText(secId, "ActionLookAround", "Look around"),
				PlayModeAction.SnapCameraBehindBike         => _lang.GetText(secId, "ActionSnapBehindBike", "Snap camera behind the bike"),
				PlayModeAction.InvertCameraAutoAlignMode    => _lang.GetText(secId, "ActionInvertCamAlignMode", "Invert camera align mode (hold)"),
				PlayModeAction.ZoomInOut                    => _lang.GetText(secId, "ActionZoom", "Zoom camera in and out"),
				PlayModeAction.ChangeDoFFocalLength         => _lang.GetText(secId, "ActionAdjustFocalLength", "Adjust DoF (hold+Zoom)"),
				PlayModeAction.ChangeDoFFocusDistanceOffset => _lang.GetText(secId, "ActionAdjustFocusDistance", "Adjust DoF focus (hold+Zoom)"),
				PlayModeAction.IncreaseFoV                  => _lang.GetText(secId, "ActionIncreaseFov", "Increase FoV by 10 (hold Alt for 5)"),
				PlayModeAction.DecreaseFoV                  => _lang.GetText(secId, "ActionDecreaseFoV", "Decrease FoV by 10 (hold Alt for 5)"),
				PlayModeAction.ResetFoV                     => _lang.GetText(secId, "ActionResetFoV", "Reset FoV to default/preset"),
				_ => "",
			};
		}
	}


	internal class PhotoModeInput
	{
		private readonly InputHandler _inputHandler;
		private readonly Configuration _cfg;
		private readonly LanguageConfig _lang;


		public PhotoModeInput(InputHandler ih, Configuration cfg, LanguageConfig lang)
		{
			_inputHandler = ih;
			_cfg = cfg;
			_lang = lang;
		}


		public bool Exit()
		{
			return _inputHandler.KeyDown(KeyCode.P) || _inputHandler.KeyDown(KeyCode.Backspace) || _inputHandler._buttonDown3;
		}

		public bool ToggleHud()
		{
			return _inputHandler.KeyDown(KeyCode.H) || _inputHandler._buttonDown9;
		}

		public bool ToggleInstructions()
		{
			return _inputHandler.KeyDown(KeyCode.I) || _inputHandler._buttonDown8;
		}

		public bool ResetCamera()
		{
			return _inputHandler.KeyDown(KeyCode.K) || _inputHandler._dpadVertical > 0;
		}

		public bool TakePhoto()
		{
			return _inputHandler.KeyDown(KeyCode.Space) || _inputHandler._buttonDown2;
		}

		public bool ToggleDoFFocusMode()
		{
			return _inputHandler.KeyDown(KeyCode.V) || _inputHandler._dpadDown;
		}

		public bool AccelerateMovement()
		{
			return _inputHandler.KeyHold(KeyCode.LeftShift) || _inputHandler.KeyHold(KeyCode.RightShift) || _inputHandler._buttonHold0;
		}

		public bool ZoomIn()
		{
			return Input.GetAxis("Mouse ScrollWheel") > 0f || _inputHandler._buttonDown5;
		}

		public bool ZoomOut()
		{
			return Input.GetAxis("Mouse ScrollWheel") < 0f || _inputHandler._buttonDown4;
		}

		public bool RollLeft()
		{
			return Input.GetKey(KeyCode.Q);
		}

		public bool RollRight()
		{
			return Input.GetKey(KeyCode.E);
		}

		public bool MoveUp()
		{
			return Input.GetKey(KeyCode.R);
		}

		public bool MoveDown()
		{
			return Input.GetKey(KeyCode.F);
		}


		public string GetKeyText(PhotoModeAction action)
		{
			return action switch
			{
				PhotoModeAction.Exit               => "P",
				PhotoModeAction.TakePhoto          => _lang.GetText("Input", "Space", "Space"),
				PhotoModeAction.ToggleInstructions => "I",
				PhotoModeAction.ToggleHud          => "H",
				PhotoModeAction.MovePan            => "W A S D + " + _lang.GetText("Input", "Mouse", "Mouse"),
				PhotoModeAction.UpDown             => "R / F",
				PhotoModeAction.Tilt               => "Q / E",
				PhotoModeAction.SpeedUp            => "Shift",
				PhotoModeAction.Reset              => "K",
				PhotoModeAction.ToggleFoVDoF       => "V",
				PhotoModeAction.ChangeFoVDoF       => _lang.GetText("Input", "MouseWheel", "Mouse Scroll"),
				_ => "",
			};
		}


		public string GetButtonText(PhotoModeAction action)
		{
			return action switch
			{
				PhotoModeAction.Exit               => "Y",
				PhotoModeAction.TakePhoto          => "X",
				PhotoModeAction.ToggleInstructions => "L-Stick Click",
				PhotoModeAction.ToggleHud          => "R-Stick Click",
				PhotoModeAction.MovePan            => "L-Stick / R-Stick",
				PhotoModeAction.UpDown             => "L-Trig / R-Trig",
				PhotoModeAction.Tilt               => _lang.GetText("Input", "Dpad", "Dpad") + " ◄ / ►",
				PhotoModeAction.SpeedUp            => "A",
				PhotoModeAction.Reset              => _lang.GetText("Input", "Dpad", "Dpad") + " ▲",
				PhotoModeAction.ToggleFoVDoF       => _lang.GetText("Input", "Dpad", "Dpad") + " ▼",
				PhotoModeAction.ChangeFoVDoF       => "LB / RB",
				_ => "",
			};
		}


		public string GetActionText(PhotoModeAction action)
		{
			const string secId = "PhotoMode";
			return action switch
			{
				PhotoModeAction.Exit               => _lang.GetText(secId, "ActionExit", "Exit Photo Mode"),
				PhotoModeAction.TakePhoto          => _lang.GetText(secId, "ActionTakePhoto", "Take Photo"),
				PhotoModeAction.ToggleInstructions => _lang.GetText(secId, "ActionToggleInstruct", "Toggle Instructions"),
				PhotoModeAction.ToggleHud          => _lang.GetText(secId, "ActionToggleHud", "Toggle HUD"),
				PhotoModeAction.MovePan            => _lang.GetText(secId, "ActionMovePan", "Move / Pan"),
				PhotoModeAction.UpDown             => _lang.GetText(secId, "ActionUpDown", "Up / Down"),
				PhotoModeAction.Tilt               => _lang.GetText(secId, "ActionTilt", "Tilt Left / Right"),
				PhotoModeAction.SpeedUp            => _lang.GetText(secId, "ActionSpeedUp", "Speed up movement"),
				PhotoModeAction.Reset              => _lang.GetText(secId, "ActionReset", "Reset rotation / FoV"),
				PhotoModeAction.ToggleFoVDoF       => _lang.GetText(secId, "ActionToggleFoVDoF", "Toggle FoV / DoF mode"),
				PhotoModeAction.ChangeFoVDoF       => _lang.GetText(secId, "ActionChangeFoVDoF", "Change FoV / DoF"),
				_ => "",
			};
		}
	}


	public float HorizontalMovement()
	{
		return ApplyInnerDeadzone(_moveHorizontal, _cfg.Controller.LeftStickDeadzone.Value);
	}


	public float VerticalMovement()
	{
		return ApplyInnerDeadzone(_moveVertical, _cfg.Controller.LeftStickDeadzone.Value);
	}


	public float LeftStickHorizontal()
	{
		return ApplyInnerDeadzone(_leftStickHorizontal, _cfg.Controller.LeftStickDeadzone.Value) *
			   _cfg.Controller.SensitivityHorizontal.Value *
			   _cfg.Controller.SensitivityMultiplier.Value;
	}


	public float LeftStickVertical()
	{
		return ApplyInnerDeadzone(_leftStickVertical, _cfg.Controller.LeftStickDeadzone.Value) *
			   _cfg.Controller.SensitivityHorizontal.Value *
			   _cfg.Controller.SensitivityMultiplier.Value;
	}


	public float RightStickHorizontal()
	{
		return ApplyInnerDeadzone(_rightStickHorizontal, _cfg.Controller.RightStickDeadzone.Value) *
			   _cfg.Controller.SensitivityHorizontal.Value *
			   _cfg.Controller.SensitivityMultiplier.Value;
	}


	public float RightStickVertical()
	{
		return ApplyInnerDeadzone(_rightStickVertical, _cfg.Controller.RightStickDeadzone.Value) *
			   _cfg.Controller.SensitivityVertical.Value *
			   _cfg.Controller.SensitivityMultiplier.Value;
	}


	public float DpadHorizontal()
	{
		return _dpadHorizontal;
	}


	public float RightTrigger()
	{
		return ApplyInnerDeadzone(_triggerInputR, _cfg.Controller.RightTriggerDeadzone.Value);
	}


	public float LeftTrigger()
	{
		return ApplyInnerDeadzone(_triggerInputL, _cfg.Controller.LeftTriggerDeadzone.Value);
	}


	public float MouseHorizontal()
	{
		return Input.GetAxisRaw("Mouse X") *
			   _cfg.Mouse.SensitivityHorizontal.Value *
			   _cfg.Mouse.SensitivityMultiplier.Value;
	}


	public float MouseVertical()
	{
		return Input.GetAxisRaw("Mouse Y") *
			   _cfg.Mouse.SensitivityVertical.Value *
			   _cfg.Mouse.SensitivityMultiplier.Value;
	}


	public bool Restart()
	{
		return _buttonDown1; // B
	}


	public bool DevKey(int fkey)
	{
		return Input.GetKeyDown(KeyCode.F1 + (fkey - 1));
	}
}
