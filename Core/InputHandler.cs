using AlternativeCameraMod.Config;
using AlternativeCameraMod.Language;
using UnityEngine;


namespace AlternativeCameraMod;

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
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
   }


   public void ShowMouseCursor()
   {
      // Lock and hide the cursor
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = false;
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

      _moveHorizontal =
         Input.GetAxisRaw(
            "Horizontal"); // combines all input devices, arrows and stick, whatever does horizontal movement
      _moveVertical =
         Input.GetAxisRaw("Vertical"); // combines all input devices, arrows and stick, whatever does vertical movement
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
      private readonly InputHandler _ih;
      private readonly Configuration _cfg;
      private readonly LanguageConfig _lang;


      public PlayModeInput(InputHandler ih, Configuration cfg, LanguageConfig lang)
      {
         _ih = ih;
         _cfg = cfg;
         _lang = lang;
      }


      public bool ShowInstructions
      {
         get { return _ih.KeyDown(KeyCode.I) || _ih.ButtonDown(ControllerButton.RStick); }
      }


      public bool SelectOriginalCamera()
      {
         return _ih.KeyDown(_cfg.Keyboard.OriginalCamKey.Value);
      }


      public bool SelectThirdPerson()
      {
         return _ih.KeyDown(_cfg.Keyboard.ThirdPersonCamKey.Value);
      }


      public bool SelectFirstPerson()
      {
         return _ih.KeyDown(_cfg.Keyboard.FirstPersonCamKey.Value);
      }


      public bool InvertAlignmentMode()
      {
         return _ih._buttonHold5 || _ih.KeyHold(_cfg.Keyboard.InvertCamAlignModeKey.Value);
      }


      public bool ToggleCamState()
      {
         return _ih.ButtonDown(_cfg.Controller.ToggleCamStateButton.Value)
                || _ih.KeyDown(_cfg.Keyboard.ToggleCamStateKey.Value);
      }


      public bool InvertHorizontalLook()
      {
         return _ih.KeyDown(_cfg.Keyboard.InvertHorizontalLookKey.Value);
      }


      public bool ToggleCamAlignMode()
      {
         return _ih.KeyDown(_cfg.Keyboard.ToggleCamAlignModeKey.Value);
      }


      public bool SnapBehindBike()
      {
         return _ih.KeyDown(_cfg.Keyboard.SnapAlignCamKey.Value)
                || _ih.ButtonDown(_cfg.Controller.SnapAlignCamButton.Value);
      }


      public bool IncreaseFieldOfView()
      {
         return _ih.KeyDown(_cfg.Keyboard.FovDecreaseKey.Value) || _ih._dpadLeft;
      }


      public bool DecreaseFieldOfView()
      {
         return _ih.KeyDown(_cfg.Keyboard.FovIncreaseKey.Value) || _ih._dpadRight;
      }


      public bool ResetFieldOfView()
      {
         return _ih.KeyDown(_cfg.Keyboard.FovResetKey.Value);
      }


      public bool EnterPhotoMode()
      {
         return _ih.KeyDown(KeyCode.P) || _ih._buttonDown3;
      }


      public bool ToggleFieldOfViewIncrement()
      {
         return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
      }


      public bool ToggleHud()
      {
         return _ih.KeyDown(_cfg.Keyboard.HudToggleKey.Value);
      }


      public bool ToggleModHud()
      {
         return _ih.KeyDown(_cfg.Keyboard.HudInfoTextToggleKey.Value);
      }


      public bool ZoomIn()
      {
         return Input.GetAxis("Mouse ScrollWheel") > 0f
                || _ih._dpadUp;
      }


      public bool ZoomOut()
      {
         return Input.GetAxis("Mouse ScrollWheel") < 0f
                || _ih._dpadDown;
      }


      public bool AdjustFocalLength()
      {
         return _ih.KeyHold(_cfg.Keyboard.AdjustFocalLengthKey.Value);
      }


      public bool AdjustFocusDistance()
      {
         return _ih.KeyHold(_cfg.Keyboard.AdjustFocusDistanceKey.Value);
      }


      public string GetKeyText(PlayModeAction action)
      {
         switch (action)
         {
            default: return "";
            case PlayModeAction.CameraModeOriginal: return _ih.ConvertToText(_cfg.Keyboard.OriginalCamKey.Value);
            case PlayModeAction.CameraModeFirstPerson: return _ih.ConvertToText(_cfg.Keyboard.FirstPersonCamKey.Value);
            case PlayModeAction.CameraModeThirdPerson: return _ih.ConvertToText(_cfg.Keyboard.ThirdPersonCamKey.Value);
            case PlayModeAction.ToggleCameraAutoAlignMode:
               return _ih.ConvertToText(_cfg.Keyboard.ToggleCamAlignModeKey.Value);
            case PlayModeAction.ToggleCameraState: return _ih.ConvertToText(_cfg.Keyboard.ToggleCamStateKey.Value);
            case PlayModeAction.ToggleInvertLookHorizontal:
               return _ih.ConvertToText(_cfg.Keyboard.InvertHorizontalLookKey.Value);
            case PlayModeAction.ToggleGameHud: return _ih.ConvertToText(_cfg.Keyboard.HudToggleKey.Value);
            case PlayModeAction.ToggleModHudDisplays:
               return _ih.ConvertToText(_cfg.Keyboard.HudInfoTextToggleKey.Value);
            case PlayModeAction.LookAround: return _ih.ConvertToText("Mouse", "Mouse");
            case PlayModeAction.SnapCameraBehindBike: return _ih.ConvertToText(_cfg.Keyboard.SnapAlignCamKey.Value);
            case PlayModeAction.InvertCameraAutoAlignMode:
               return _ih.ConvertToText(_cfg.Keyboard.InvertCamAlignModeKey.Value);
            case PlayModeAction.ZoomInOut: return _ih.ConvertToText("MouseWheel", "Mouse Scroll");
            case PlayModeAction.ChangeDoFFocalLength:
               return _ih.ConvertToText(_cfg.Keyboard.AdjustFocalLengthKey.Value);
            case PlayModeAction.ChangeDoFFocusDistanceOffset:
               return _ih.ConvertToText(_cfg.Keyboard.AdjustFocusDistanceKey.Value);
            case PlayModeAction.IncreaseFoV: return _ih.ConvertToText(_cfg.Keyboard.FovIncreaseKey.Value);
            case PlayModeAction.DecreaseFoV: return _ih.ConvertToText(_cfg.Keyboard.FovDecreaseKey.Value);
            case PlayModeAction.ResetFoV: return _ih.ConvertToText(_cfg.Keyboard.FovResetKey.Value);
         }
      }


      public string GetButtonText(PlayModeAction action)
      {
         switch (action)
         {
            default: return "";
            case PlayModeAction.CameraModeOriginal: return "-";
            case PlayModeAction.CameraModeFirstPerson: return "-";
            case PlayModeAction.CameraModeThirdPerson: return "-";
            case PlayModeAction.ToggleCameraAutoAlignMode: return "-";
            case PlayModeAction.ToggleCameraState: return _ih.ConvertToText(_cfg.Controller.ToggleCamStateButton.Value);
            case PlayModeAction.ToggleInvertLookHorizontal: return "-";
            case PlayModeAction.ToggleGameHud: return "-";
            case PlayModeAction.ToggleModHudDisplays: return "-";
            case PlayModeAction.LookAround: return _ih.ConvertToText("RStick");
            case PlayModeAction.SnapCameraBehindBike: return _ih.ConvertToText("LButton");
            case PlayModeAction.InvertCameraAutoAlignMode: return _ih.ConvertToText("RButton");
            case PlayModeAction.ZoomInOut:
               return string.Format("{0} {1} / {2}",
                  _ih.ConvertToText("Dpad"),
                  _ih.ConvertToText("DpadUp", "up"),
                  _ih.ConvertToText("DpadDown", "down"));
            case PlayModeAction.ChangeDoFFocalLength: return "-";
            case PlayModeAction.ChangeDoFFocusDistanceOffset: return "-";
            case PlayModeAction.IncreaseFoV:
               return string.Format("{0} {1}", _ih.ConvertToText("Dpad"), _ih.ConvertToText("DpadLeft", "left"));
            case PlayModeAction.DecreaseFoV:
               return string.Format("{0} {1}", _ih.ConvertToText("Dpad"), _ih.ConvertToText("DpadRight", "right"));
            case PlayModeAction.ResetFoV: return "-";
         }
      }


      public string GetActionText(PlayModeAction action)
      {
         const string secId = "PlayMode";
         switch (action)
         {
            default: return "";
            case PlayModeAction.CameraModeOriginal:
               return _lang.GetText(secId, "ActionCameraOriginal", "Original Camera (isometric)");
            case PlayModeAction.CameraModeFirstPerson: 
               return _lang.GetText(secId, "ActionCameraFirst", "Alternative Camera: First Person");
            case PlayModeAction.CameraModeThirdPerson: 
               return _lang.GetText(secId, "ActionCameraThird", "Alternative Camera: Third person");
            case PlayModeAction.ToggleCameraAutoAlignMode: 
               return _lang.GetText(secId, "ActionCameraAutoAlign", "Toggle align mode Auto <-> Manual");
            case PlayModeAction.ToggleCameraState: 
               return _lang.GetText(secId, "ActionToggleCamState", "Toggle camera: Original <-> Alternative");
            case PlayModeAction.ToggleInvertLookHorizontal:
               return _lang.GetText(secId, "ActionInvertLookHorizontal", "Toggle invert look horizontal");
            case PlayModeAction.ToggleGameHud: 
               return _lang.GetText(secId, "ActionToggleGameHud", "Toggle Game HUD");
            case PlayModeAction.ToggleModHudDisplays:
               return _lang.GetText(secId, "ActionToggleModHud", "Toggle Mod HUD Displays");
            case PlayModeAction.LookAround: 
               return _lang.GetText(secId, "ActionLookAround", "Look around");
            case PlayModeAction.SnapCameraBehindBike: 
               return _lang.GetText(secId, "ActionSnapBehindBike", "Snap camera behind the bike");
            case PlayModeAction.InvertCameraAutoAlignMode:
               return _lang.GetText(secId, "ActionInvertCamAlignMode", "Invert camera align mode (hold)");
            case PlayModeAction.ZoomInOut:
               return _lang.GetText(secId, "ActionZoom", "Zoom camera in and out");
            case PlayModeAction.ChangeDoFFocalLength: 
               return _lang.GetText(secId, "ActionAdjustFocalLength", "Adjust DoF (hold+Zoom)");
            case PlayModeAction.ChangeDoFFocusDistanceOffset:
               return _lang.GetText(secId, "ActionAdjustFocusDistance", "Adjust DoF focus (hold+Zoom)");
            case PlayModeAction.IncreaseFoV: 
               return _lang.GetText(secId, "ActionIncreaseFov", "Increase FoV by 10 (hold Alt for 5)");
            case PlayModeAction.DecreaseFoV: 
               return _lang.GetText(secId, "ActionDecreaseFoV", "Decrease FoV by 10 (hold Alt for 5)");
            case PlayModeAction.ResetFoV:
               return _lang.GetText(secId, "ActionResetFoV", "Reset FoV to default/preset");
         }
      }
   }


   internal class PhotoModeInput
   {
      private readonly InputHandler _ih;
      private readonly Configuration _cfg;
      private readonly LanguageConfig _lang;


      public PhotoModeInput(InputHandler ih, Configuration cfg, LanguageConfig lang)
      {
         _ih = ih;
         _cfg = cfg;
         _lang = lang;
      }


      public bool Exit()
      {
         return _ih.KeyDown(KeyCode.P) || _ih.KeyDown(KeyCode.Backspace) || _ih._buttonDown3;
      }


      public bool ToggleHud()
      {
         return _ih.KeyDown(KeyCode.H) || _ih._buttonDown9;
      }


      public bool ToggleInstructions()
      {
         return _ih.KeyDown(KeyCode.I) || _ih._buttonDown8;
      }


      public bool ResetCamera()
      {
         return _ih.KeyDown(KeyCode.K) || _ih._dpadVertical > 0;
      }


      public bool TakePhoto()
      {
         return _ih.KeyDown(KeyCode.Space) || _ih._buttonDown2;
      }


      public bool ToggleDoFFocusMode()
      {
         return _ih.KeyDown(KeyCode.V) || _ih._dpadDown;
      }


      public bool AccelerateMovement()
      {
         return _ih.KeyHold(KeyCode.LeftShift) || _ih.KeyHold(KeyCode.RightShift) || _ih._buttonHold0;
      }


      public bool ZoomIn()
      {
         return Input.GetAxis("Mouse ScrollWheel") > 0f || _ih._buttonDown5;
      }


      public bool ZoomOut()
      {
         return Input.GetAxis("Mouse ScrollWheel") < 0f || _ih._buttonDown4;
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
         switch (action)
         {
            default: return "";
            case PhotoModeAction.Exit:
               return "P";
            case PhotoModeAction.ShootPhoto:
               return _lang.GetText("Input", "Space", "Space");
            case PhotoModeAction.ToggleInstructions:
               return "I";
            case PhotoModeAction.ToggleHud:
               return "H";
            case PhotoModeAction.MovePan:
               return "W A S D + " + _lang.GetText("Input", "Mouse", "Mouse");
            case PhotoModeAction.UpDown:
               return "R / F";
            case PhotoModeAction.Tilt:
               return "Q / E";
            case PhotoModeAction.SpeedUp:
               return "Shift";
            case PhotoModeAction.Reset:
               return "K";
            case PhotoModeAction.ToggleFoVDoF:
               return "V";
            case PhotoModeAction.ChangeFoVDoF:
               return _lang.GetText("Input", "MouseWheel", "Mouse Scroll");
         }
      }


      public string GetButtonText(PhotoModeAction action)
      {
         switch (action)
         {
            default: return "";
            case PhotoModeAction.Exit:
               return "Y";
            case PhotoModeAction.ShootPhoto:
               return "X";
            case PhotoModeAction.ToggleInstructions:
               return "L-Stick Click";
            case PhotoModeAction.ToggleHud:
               return "R-Stick Click";
            case PhotoModeAction.MovePan:
               return "L-Stick / R-Stick";
            case PhotoModeAction.UpDown:
               return "L-Trig / R-Trig";
            case PhotoModeAction.Tilt:
               return _lang.GetText("Input", "Dpad", "Dpad") + " ◄ / ►";
            case PhotoModeAction.SpeedUp:
               return "A";
            case PhotoModeAction.Reset:
               return _lang.GetText("Input", "Dpad", "Dpad") + " ▲";
            case PhotoModeAction.ToggleFoVDoF:
               return _lang.GetText("Input", "Dpad", "Dpad") + " ▼";
            case PhotoModeAction.ChangeFoVDoF:
               return "LB / RB";
         }
      }


      public string GetActionText(PhotoModeAction action)
      {
         const string secId = "PhotoMode";
         switch (action)
         {
            default: return "";
            case PhotoModeAction.Exit:
               return _lang.GetText(secId, "ActionExit", "Exit Photo Mode");
            case PhotoModeAction.ShootPhoto:
               return _lang.GetText(secId, "ActionShoot", "Take Photo");
            case PhotoModeAction.ToggleInstructions:
               return _lang.GetText(secId, "ActionToggleInstruct", "Toggle Instructions");
            case PhotoModeAction.ToggleHud:
               return _lang.GetText(secId, "ActionToggleHud", "Toggle HUD");
            case PhotoModeAction.MovePan:
               return _lang.GetText(secId, "ActionMovePan", "Move / Pan");
            case PhotoModeAction.UpDown:
               return _lang.GetText(secId, "ActionUpDown", "Up / Down");
            case PhotoModeAction.Tilt:
               return _lang.GetText(secId, "ActionTilt", "Tilt Left / Right");
            case PhotoModeAction.SpeedUp:
               return _lang.GetText(secId, "ActionSpeedUp", "Speed up movement");
            case PhotoModeAction.Reset:
               return _lang.GetText(secId, "ActionReset", "Reset rotation / FoV");
            case PhotoModeAction.ToggleFoVDoF:
               return _lang.GetText(secId, "ActionToggleFoVDoF", "Toggle FoV / DoF mode");
            case PhotoModeAction.ChangeFoVDoF:
               return _lang.GetText(secId, "ActionChangeFoVDoF", "Change FoV / DoF");
         }
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
}
