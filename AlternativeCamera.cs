// Mod

using System.Collections;
using System.Text;
using MelonLoader;
using AlternativeCameraMod;
// Unity
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
// Megagon
using Il2CppMegagon.Downhill.Cameras;
using Il2CppSystem.Diagnostics;
using AlternativeCameraMod.Config;
using AlternativeCameraMod.Language;


[assembly: MelonInfo(typeof(AlternativeCamera), "Alternative Camera with Photo Mode", AlternativeCamera.MOD_VERSION, "DevdudeX")]
[assembly: MelonGame()]


namespace AlternativeCameraMod;

/// <summary>
/// Manages the alternative camera system and the photo mode.
/// </summary>
public class AlternativeCamera : MelonMod
{
   public const string MOD_VERSION = "2.0.0"; // also update in project build properties

   private bool _disableModProcessing;
   private bool _modInitialized;
   private bool _modInitializing;
   private Configuration? _cfg;
   private ScreenMode _currentScreenState;
   private Logger? _logger;
   private string? _errorMessage;

   // Transforms and GameObjects
   /// <summary>The name of the gameobject that will act as the cameras target.</summary>
   private string _targetName = "Bike(Clone)";

   private Transform? _playerBikeParentTransform;
   private Transform? _playerBikeTransform;
   private Transform? _camTransform;
   private GameObject? _postProcessingObject;
   private DepthOfField? _depthOfFieldSettings;

   // The main camera itself. Used to set the field of view
   private Camera? _mainCamera;
   private PlayCamera? _defaultPlayCamera;

   private static readonly LayerMask __cameraCollisionLayers =
      LayerMask.GetMask("Ground", "Obstacle", "EnvironmentOther", "Terrain", "Lava");

   // UI GameObjects
   private readonly Dictionary<string, GameObject> _menuObjects = new();

   // Camera variables
   private bool _isMenuOpen = true;
   private bool _isMenuLastOpen;
   private bool _hasDepthOfFieldSetting;
   private float _wantedZoom = 8f;
   private float _targetZoomAmount;
   private Quaternion _rotation;
   private float _baseFocalLength;
   private float _photoModeBaseFoV;
   private float _wantedFocalLength;
   private Vector3 _targetOffset = new Vector3(0f, 2.4f, 0f);

   private readonly Dictionary<CameraMode, Tuple<float, float>> _fovLimitMap = new()
   {
      { CameraMode.Original, new Tuple<float, float>(30f, 120f) },
      { CameraMode.ThirdPerson, new Tuple<float, float>(40f, 120f) },
      { CameraMode.FirstPerson, new Tuple<float, float>(70f, 120f) }
   };
   private readonly Dictionary<CameraMode, Tuple<float, float>> _verticalClampAnglesMap = new()
   {
      // max -82, 82
      { CameraMode.ThirdPerson, new Tuple<float, float>(-70f, 30f) },
      { CameraMode.FirstPerson, new Tuple<float, float>(-80f, 50f) }
   };

   private const float DefaultIsometricFoV = 38f;
   private float _isometricFoV;
   private float _thirdPersonFoV;
   private float _firstPersonFoV;

   /// <summary>The distance from the bike to any world-collision between it and the camera.</summary>
   private float _projectedDistance = 200f;

   /// <summary>Camera rotation around vertical y-axis (left-right)</summary>
   private float _rotationHorizontal;
   /// <summary>Camera rotation around x-axis (ear-to-ear or up-down)</summary>
   private float _rotationVertical;
   private Vector3 _dirToCam;

   // Gamepad Inputs
   private float _anyGamepadDpadHorizontal;
   private float _anyGamepadDpadVertical;
   private float _anyGamepadTriggerInputL;
   private float _anyGamepadTriggerInputR;
   private float _anyGamepadStickHorizontalR;
   private float _anyGamepadStickVerticalR;

   private bool _anyGamepadBtn0; // A
   private bool _anyGamepadBtn5; // right bumper
   private bool _anyGamepadBtnDown0; // A
   private bool _anyGamepadBtnDown1; // B
   private bool _anyGamepadBtnDown2; // X
   private bool _anyGamepadBtnDown3; // Y
   private bool _anyGamepadBtnDown4; // left bumper
   private bool _anyGamepadBtnDown5; // right bumper
   private bool _anyGamepadBtnDown6; // back/select
   private bool _anyGamepadBtnDown7; // start
   private bool _anyGamepadBtnDown8; // left stick click
   private bool _anyGamepadBtnDown9; // right stick click
   private int _anyGamepadDpadUpCnt;
   private int _anyGamepadDpadDownCnt;
   private int _anyGamepadDpadLeftCnt;
   private int _anyGamepadDpadRightCnt;
   private bool _anyGamepadDpadUp;
   private bool _anyGamepadDpadDown;
   private bool _anyGamepadDpadLeft;
   private bool _anyGamepadDpadRight;
   private float _dpadPressDetectThreshold; // Used to treat dpad as a button
   private bool _dpadSingleClickDetect;
   private bool _holdingInvertManualAlignMode;

   private bool _escapeKeyDown;
   private bool _menuWasOpenedWhileInPhotoMode;

   private int _fps;
   private bool _firstInit;
   private bool _hudInfoVisible = true;
   private List<ModHudInfoPart> _hudInfoParts = new();
   private CameraMode _currentCamMode;
   private CameraMode _altCamStateMode;
   private bool _camAutoAlign;
   private readonly CamPos _camPosBike = new();

   private ModMode _mode;
   private float _photoModeBaseTimeScale;
   private float _photoModeBaseFocusDistanceDoF;
   private float _rotationRoll;
   private bool _photoModeFocus;
   private bool _photoModeInstructionsVisible = true;
   private bool _photoModeHudRestoreState;
   private string? _lastScreenshotInfo;
   private int _screenshotCounter;
   private readonly CamPos _camPosPhoto = new();
   private readonly CamPos _camPosPhotoShoot = new();

   private LanguageConfig _lang;


   class CamPos
   {
      public float RotationVertical;
      public float RotationHorizontal;
      public float RotationRoll;
      public Vector3 Position;
      public Vector3 EulerAngles;
      public Vector3 LocalEulerAngles;


      private static bool EqualsFloat(float a, float b)
      {
         return (b > a - 1E-12) && (b < a + 1E-12);
      }

      public bool Matches(CamPos otherPos)
      {
         if (!EqualsFloat(RotationHorizontal, otherPos.RotationHorizontal)) return false;
         if (!EqualsFloat(RotationVertical, otherPos.RotationVertical)) return false;
         if (!EqualsFloat(RotationRoll, otherPos.RotationRoll)) return false;
         if (!Position.Equals(otherPos.Position)) return false;
         if (!EulerAngles.Equals(otherPos.EulerAngles)) return false;
         if (!LocalEulerAngles.Equals(otherPos.LocalEulerAngles)) return false;
         return true;
      }
   }


   private bool CheckExistanceOfKnownConflictingMods()
   {
      var folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      var myName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

      var files = Directory.GetFiles(folder, "*.dll");
      List<string> incompatMods = new List<string>();
      foreach (var file in files)
      {
         var filename = Path.GetFileName(file);
         switch (filename.ToLower())
         {
            case "alternativecamera.dll":
            case "photomode.dll":
               // check if this DLL has been renamed to one of the other mods DLL and be tolerant about it
               if (!myName.Equals(filename, StringComparison.OrdinalIgnoreCase))
               {
                  incompatMods.Add(filename);
               }
               break;
         }
      }

      if (incompatMods.Count > 0)
      {
         HandleWarningState(
         _lang.GetText("Mod", "ErrIncompatibleWithMods_{mods}", "Incompatible with these mods: {0}", String.Join(", ", incompatMods))
                                                  + Environment.NewLine +
                                                  _lang.GetText("Mod", "ErrDisabledMod", "Mod disabled"),
         mustDisable: true);
         return true;
      }

      return false;
   }


   public override void OnEarlyInitializeMelon()
   {
      Cursor.lockState = CursorLockMode.None;
      MelonEvents.OnGUI.Subscribe(DrawHudText, 100);
   }


   public override void OnInitializeMelon()
   {
      _firstInit = true;
      
      // cfg for first creation and init
      var initLang = LanguageConfig.Load();
      _cfg = new Configuration(initLang); // OS lang
      _cfg.Save();

      _logger = new Logger(LoggerInstance);
      _logger.Level = _cfg.Common.LogLevel.Value;
#if DEBUG
      _logger.Level = LogLevel.Debug;
#endif

      _lang = LanguageConfig.Load(_cfg.Common.Language.Value);
      _logger.LogInfo("Used Mod Language: {0}", _lang.LanguageCode);

      if (CheckExistanceOfKnownConflictingMods())
      {
         return;
      }

      ValidateConfig();

      ParseHudInfoDisplayFlags(_cfg.UI.ModHudInfoDisplay.Value);
      _currentCamMode = _cfg.Camera.InitialMode.Value;
      _altCamStateMode = _cfg.Camera.InitialMode.Value;
      _camAutoAlign = _cfg.Camera.AlignmentMode.Value == CameraAlignmentMode.Auto;

      _isometricFoV = DefaultIsometricFoV;
      _thirdPersonFoV = LimitFoV(_cfg.Camera.ThirdPersonFoV.Value, CameraMode.ThirdPerson);
      _firstPersonFoV = LimitFoV(_cfg.Camera.FirstPersonFoV.Value, CameraMode.FirstPerson);
   }


   private void ValidateConfig()
   {
      var cfgErr = _cfg.Validate(_lang);

      if (!String.IsNullOrEmpty(cfgErr))
      {
         var str = cfgErr
                   + Environment.NewLine +
                   _lang.GetText("Mod", "ErrDisabledMod", "Mod disabled")
                   + Environment.NewLine +
                   _lang.GetText("Mod", "ErrCheckConfig_{CfgFile}", "check {0}", Configuration.ConfigFilePath);

         HandleErrorState(str);
      }
   }


   public override void OnDeinitializeMelon()
   {
      _disableModProcessing = true;
      MelonEvents.OnGUI.Unsubscribe(DrawHudText);
   }


   private float LimitFoV(float preset, CameraMode cm)
   {
      return Math.Max(_fovLimitMap[cm].Item1, Math.Min(_fovLimitMap[cm].Item2, preset));
   }


   private void HandleErrorState(string errMsg)
   {
      _disableModProcessing = true;
      _errorMessage = errMsg;
      _logger.LogError(errMsg);
   }


   private void HandleWarningState(string warnMsg, bool mustDisable = false)
   {
      _disableModProcessing = mustDisable;
      _errorMessage = warnMsg;
      _logger.LogError(warnMsg);
   }


   private void DrawHudText()
   {
      if (_disableModProcessing)
      {
         // when disabled, only show text on menu screen, do not show others
         if (_currentScreenState != ScreenMode.MenuScreen)
         {
            return;
         }
      }

      StringBuilder text = new StringBuilder();
      int x = 0, y = 0, size = 10;
      string color = "#FFFFFF";
      string shadowColor = "#000000";
      int shadowOffsetX = 1;
      int shadowOffsetY = 1;
      bool shadow = false;
      _logger.LogVerbose("Screen: {0}", _currentScreenState);
      switch (_currentScreenState)
      {
         case ScreenMode.None:
            return;

         case ScreenMode.LoadingScreen:
            text.AppendFormat(_lang.GetText("Mod", "Title_{Version}", "Alternative Camera with Photo Mode {0}", MOD_VERSION));
            x = Screen.currentResolution.width / 2 - 200;
            y = Screen.currentResolution.height - 50;
            size = 20;
            color = "#E9AC4F";
            break;

         case ScreenMode.SplashScreen:
            // nothing
            return;

         case ScreenMode.MenuScreen:
            text.AppendFormat(_lang.GetText("Mod", "Title_{Version}", "Alternative Camera with Photo Mode {0}", MOD_VERSION).ToUpper());
#if DEBUG
            text.Append(" (DBG)");
#endif
            x = 130;
            y = Screen.currentResolution.height - 200;
            size = 20;
            shadowOffsetX = 1;
            shadowOffsetY = 2;
            color = "#E9AC4F"; // matches LMD text color

            if (!String.IsNullOrEmpty(_errorMessage))
            {
               text.AppendLine();
               text.AppendFormat(_lang.GetText("Mod", "ErrorOutputLine_{Msg}", "[ERR] {0}", _errorMessage));
            }

            shadow = true;
            break;

         case ScreenMode.PlayScreen:
            if (!IsHudVisible()) return;
            
            if (_hudInfoParts.Count > 0 && _hudInfoVisible && IsHudVisible())
            {
               x = 20;
               y = Screen.currentResolution.height - 28;
               size = 15;
               color = "#FFFFFF";
               BuildHudInfoText(text);
            }

            break;

         case ScreenMode.PhotoScreen:
            if (_photoModeInstructionsVisible)
            {
               DrawPhotoModeInstructionsOnHud();
            }
            break;
      }

      if (text.Length > 0)
      {
         if (shadow)
         {
            GUI.Label(new Rect(x + shadowOffsetX, y + shadowOffsetY, 2000, 200),
               String.Format("<b><color={0}><size={1}>{2}</size></color></b>",
                  shadowColor, size, text));
         }
         GUI.Label(new Rect(x, y, 2000, 200),
         String.Format("<b><color={0}><size={1}>{2}</size></color></b>",
            color, size, text));
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
               switch (_currentCamMode)
               {
                  case CameraMode.Original:
                     mode = _lang.GetText("Mod", "CamOriginal", "Original");
                     break;
                  case CameraMode.ThirdPerson:
                     mode = _lang.GetText("Mod", "CamThirdPerson", "Third Person");
                     break;
                  case CameraMode.FirstPerson:
                     mode = _lang.GetText("Mod", "CamFirstPerson", "First Person");
                     break;
               }

               target.Append(_lang.GetText("Mod", "CamLabel_{Mode}", "Cam: {0}", mode));
               break;

            case ModHudInfoPart.FoV:
               float val = 0;
               switch (_currentCamMode)
               {
                  case CameraMode.Original:
                     val = _isometricFoV;
                     break;
                  case CameraMode.ThirdPerson:
                     val = _thirdPersonFoV;
                     break;
                  case CameraMode.FirstPerson:
                     val = _firstPersonFoV;
                     break;
               }

               target.Append(_lang.GetText("Mod", "FoVInfoLabel_{fov}", "FoV: {0}", val));
               break;

            case ModHudInfoPart.FPS:
               target.Append(_lang.GetText("Mod", "FPSLabel_{fps}", "FPS: {0}", _fps));
               break;
            
            case ModHudInfoPart.CamAlign:
               string align = _camAutoAlign
                  ? (_holdingInvertManualAlignMode ? "Manual" : "Auto")
                  : (_holdingInvertManualAlignMode ? "Auto" : "Manual");
               
               target.Append(_lang.GetText("Mod", "CamAlignLabel_{align}", "({0})", align));
               break;
         }
      }
   }


   public override void OnUpdate()
   {
      if (_disableModProcessing) return;

      UpdateGamepadInputs();
      _escapeKeyDown = CheckKeyPressed(KeyCode.Escape);
   }


   public override void OnLateUpdate()
   {
      _fps = (int)(1 / Math.Max(Time.deltaTime, 0.001));
      //_logger.LogDebug("FPS: {0}", _fps.ToString());

      TrackScreenState();

      if (_disableModProcessing)
      {
         return;
      }

      if (StartUpMod())
      {
         _isMenuOpen = IsGameMenuActive();
         _logger.LogDebug(_isMenuOpen && _isMenuOpen != _isMenuLastOpen, "Menu opened");
         bool initCamModeOnLevelStart = _isMenuLastOpen && !_isMenuOpen;
         _isMenuLastOpen = _isMenuOpen;

         if (SpecialTreatmentForUsingMenuDuringPhotoMode())
         {
            return;
         }

         if (_isMenuOpen)
         {
            // If the menu is open and default camera on pause is set don't run any functions
            if (_cfg.Camera.DefaultCameraOnPause.Value)
            {
               Cursor.visible = true;
               Cursor.lockState = CursorLockMode.None;
               _defaultPlayCamera.enabled = true;
            }
            return;
         }

         if (initCamModeOnLevelStart && _mode == ModMode.BikeCam)
         {
            SelectCameraMode(_currentCamMode);
         }

         if (_mode == ModMode.BikeCam)
         {
            HandleBikeModeUserInputs();
            if (_currentCamMode != CameraMode.Original)
            {
               ProcessBikeModeCamera();
            }
         }
         else if (_mode == ModMode.PhotoCam)
         {
            HandlePhotoModeUserInputs();
            ProcessPhotoModeCamera();
         }
      }

      if (_firstInit)
      {
         ToggleHudVisiblity(_cfg.UI.GameHudVisible.Value);
      }

      HandleCommonUserInputs();

      _firstInit = false;
   }


   private bool SpecialTreatmentForUsingMenuDuringPhotoMode()
   {
      // Escape key needs special handling as this toggles the game pause menu
      // which interferes with photo mode
      if (_escapeKeyDown || _anyGamepadBtnDown7/*start*/)
      {
         if (_mode == ModMode.PhotoCam)
         {
            // track that the (pause) menu should be opened while in photo mode
            // and close photo mode to allow the menu to come up
            _menuWasOpenedWhileInPhotoMode = _isMenuOpen;
            TogglePhotoMode(false);
            return true;
         }
         else if (_mode == ModMode.BikeCam)
         {
            // check if previously the (pause) menu was be opened while in photo mode
            // and return to photo mode when menu is closed by escape key
            if (_menuWasOpenedWhileInPhotoMode)
            {
               _isMenuOpen = false;
               _menuWasOpenedWhileInPhotoMode = false;
               TogglePhotoMode(true, true);
               return true; // do not apply further camera logic
            }
         }
      }

      return false; // continue with further logic
   }


   private void HandleCommonUserInputs()
   {
      switch (_mode)
      {
         case ModMode.BikeCam:
            if (CheckKeyPressed(_cfg.Keyboard.HudToggleKey.Value))
            {
               ToggleHudVisiblity();
            }

            // Mod Hud Text Visibility
            if (CheckKeyPressed(_cfg.Keyboard.HudInfoTextToggleKey.Value))
            {
               ToggleModHudInfoVisibility();
            }
            break;

         case ModMode.PhotoCam:
            if (CheckKeyPressed(KeyCode.H) || _anyGamepadBtnDown9)
            {
               ToggleHudVisiblity();
            }
            break;
      }
   }


   /// <summary>
   /// Checks if any menu gui is currently on screen.
   /// </summary>
   private bool IsGameMenuActive()
   {
      return _menuObjects.Values.Any(g => g.active);
   }


   /// <summary>
   /// Toggles the rendering of the game HUD.
   /// </summary>
   private void ToggleHudVisiblity(bool? visible = null)
   {
      var hud = GetHudObject();
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


   private bool IsHudVisible()
   {
      var hudCam = GetHudObject();
      return hudCam != null && hudCam.enabled;
   }


   private Camera GetHudObject()
   {
      var hudCamObj = GameObject.Find("UICam");
      if (hudCamObj == null)
      {
         return null;
      }

      var hudCam = hudCamObj.GetComponent<Camera>();
      return hudCam;
   }


   private void ToggleModHudInfoVisibility()
   {
      _hudInfoVisible = !_hudInfoVisible;
   }


   private bool StartUpMod()
   {
      // Initialize once
      if (!_modInitialized && !_modInitializing)
      {
         _modInitializing = true;
         _modInitialized = InitializeMod();
         _modInitializing = false;
      }

      return _modInitialized;
   }


   /// <summary>
   /// Grabs required objects and inits standard camera settings
   /// </summary>
   private bool InitializeMod()
   {
      if (!GatherCameraRelatedGameObjects())
      {
         return false;
      }

      _logger.LogInfo("Starting alternative camera system ...");
      _logger.LogInfo("MainCam, FoV {0}, NCP {1}", _mainCamera.fieldOfView, _mainCamera.nearClipPlane);

      if (!GatherMenuRelatedGameObjects())
      {
         return false;
      }

      // Get intial cam position
      _rotationHorizontal = 0;
      _rotationVertical = _cfg.Camera.ThirdPersionRotationVert.Value;

      if (EqualsZero(_rotationHorizontal))
      {
         _rotationHorizontal = _camTransform.eulerAngles.y;
      }
      if (EqualsZero(_rotationVertical))
      {
         _rotationVertical = _camTransform.eulerAngles.x;
      }

      if (_hasDepthOfFieldSetting)
      {
         _baseFocalLength = _depthOfFieldSettings.focalLength.GetValue<float>();
         _logger.LogInfo("DoF: " + _baseFocalLength);
         _wantedFocalLength = _cfg.Camera.FocalLength.Value;
      }

      _photoModeBaseFoV = _mainCamera.fieldOfView;

      return true;
   }


   private bool EqualsZero(float floatVal)
   {
      return Math.Abs(floatVal) < 1E-12;
   }


   private enum ScreenMode
   {
      None,
      LoadingScreen,
      SplashScreen,
      MenuScreen,
      PlayScreen,
      PhotoScreen
   }


   private void TrackScreenState()
   {
      var wrapper = GameObject.Find("Wrapper");
      if (wrapper == null)
      {
         _currentScreenState = ScreenMode.LoadingScreen;
         return;
      }

      bool blackActive = false;
      bool splashActive = false;
      bool menuActive = false;
      bool playActive = false;
      var uiMainParent = GameObject.Find("Wrapper").GetComponent<Transform>();
      for (int i = 0; i < uiMainParent.childCount; i++)
      {
         var ch = uiMainParent.GetChild(i);
         var g = ch.gameObject;
         if (g.name.StartsWith("BlackBorder"))
         {
            blackActive = g.active;
         }
         if (g.name.StartsWith("SplashScreen"))
         {
            splashActive = g.active;
         }
         if (g.name.StartsWith("MainMenu"))
         {
            menuActive = g.active;
         }
         if (g.name.StartsWith("PlayScreen"))
         {
            playActive = g.active;
         }
      }

      if (blackActive && !splashActive && !menuActive) _currentScreenState = ScreenMode.LoadingScreen;
      else if (splashActive && !menuActive) _currentScreenState = ScreenMode.SplashScreen;
      else if (menuActive) _currentScreenState = ScreenMode.MenuScreen;
      else if (playActive)
      {
         if (_mode == ModMode.PhotoCam)
         {
            _currentScreenState = ScreenMode.PhotoScreen;
         }
         else
         {
            _currentScreenState = ScreenMode.PlayScreen;
         }
      }
      else _currentScreenState = ScreenMode.None;
   }


   /// <summary>
   /// Finds and assigns all relevant UI GameObjects.
   /// </summary>
   private bool GatherMenuRelatedGameObjects()
   {
      var wrapper = GameObject.Find("Wrapper");
      if (wrapper == null)
      {
         return false;
      }

      var uiMainParent = GameObject.Find("Wrapper").GetComponent<Transform>();
      for (int i = 0; i < uiMainParent.childCount; i++)
      {
         var ch = uiMainParent.GetChild(i);
         var g = ch.gameObject;
         if (IsMenuObject(g.name))
         {
            _menuObjects[g.name] = g;
            _logger.LogVerbose("Game Object: {0}", g.name);
         }
      }

      return true;
   }


   /// <summary>
   /// Finds and assigns important GameObjects and Transforms.
   /// </summary>
   private bool GatherCameraRelatedGameObjects()
   {
      var bikeClone = GameObject.Find("Bike(Clone)");
      if (bikeClone == null)
      {
         return false;
      }

      _playerBikeParentTransform = bikeClone.GetComponent<Transform>();

      var target = GameObject.Find(_targetName);
      if (target == null)
      {
         return false;
      }

      _playerBikeTransform = target.GetComponent<Transform>();

      var cam = GameObject.Find("PlayCamera(Clone)");
      if (cam == null)
      {
         return false;
      }

      _camTransform = cam.GetComponent<Transform>();
      _mainCamera = _camTransform.gameObject.GetComponent<Camera>();
      _defaultPlayCamera = _camTransform.gameObject.GetComponent<PlayCamera>();
      _postProcessingObject = _camTransform.Find("DefaultPostProcessing").gameObject;

      // Note: changed to GetSettings here as TryGetSettings is not working for me
      _depthOfFieldSettings = _postProcessingObject.GetComponent<PostProcessVolume>().sharedProfile.GetSetting<DepthOfField>();
      _hasDepthOfFieldSetting = _depthOfFieldSettings != null;

      return true;
   }


   private bool IsMenuObject(string name)
   {
      if (name == null) return false;
      if (name.StartsWith("BlackBorder")) return false;
      if (name.StartsWith("SplashScreen")) return false;
      if (name.StartsWith("PlayScreen")) return false;
      return true;
   }


   private void UpdateGamepadInputs()
   {
      UpdateDpadInputs();
      UpdateControllerTriggerAndStickInputs();
      UpdateControllerButtonsDown();
      _holdingInvertManualAlignMode = _anyGamepadBtn5 || Input.GetKey(KeyCode.Mouse1);
   }


   private void UpdateControllerTriggerAndStickInputs()
   {
      _anyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") +
                                 Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
      _anyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") +
                                 Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");

      _anyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") +
                                    Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
      _anyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") +
                                  Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");
   }


   private void UpdateDpadInputs()
   {
      _anyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") +
                                  Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
      _anyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") +
                                Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

      if (!_dpadSingleClickDetect && _dpadPressDetectThreshold <= 0)
      {
         _anyGamepadDpadLeft = _anyGamepadDpadHorizontal < 0f;
         _anyGamepadDpadRight = _anyGamepadDpadHorizontal > 0f;
         _anyGamepadDpadUp = _anyGamepadDpadVertical > 0f;
         _anyGamepadDpadDown = _anyGamepadDpadVertical < 0f;

         _dpadSingleClickDetect = _anyGamepadDpadLeft | _anyGamepadDpadRight | _anyGamepadDpadUp | _anyGamepadDpadDown;
         if (_dpadSingleClickDetect)
         {
            _dpadPressDetectThreshold = 0.15f;
         }
      }
      else
      {
         _anyGamepadDpadUp = false;
         _anyGamepadDpadDown = false;
         _anyGamepadDpadLeft = false;
         _anyGamepadDpadRight = false;

         float dt = Time.deltaTime <= 0 ? 0.005f : Time.deltaTime;
         _dpadPressDetectThreshold -= dt;
         if (_dpadPressDetectThreshold <= 0)
         {
            _dpadSingleClickDetect = false;
            _dpadPressDetectThreshold = 0;
         }
      }
   }


   private void UpdateControllerButtonsDown()
   {
      _anyGamepadBtn0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) ||
                        Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
      _anyGamepadBtn5 = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) ||
                        Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

      _anyGamepadBtnDown0 = Input.GetKeyDown(KeyCode.Joystick1Button0) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button0) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button0) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button0);
      _anyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button1) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button1) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button1);
      _anyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button1) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button1) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button1);
      _anyGamepadBtnDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button2) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button2) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button2);
      _anyGamepadBtnDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button3) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button3) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button3);
      _anyGamepadBtnDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button4) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button4) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button4);
      _anyGamepadBtnDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button5) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button5) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button5);
      _anyGamepadBtnDown6 = Input.GetKeyDown(KeyCode.Joystick1Button6) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button6) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button6) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button6);
      _anyGamepadBtnDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button7) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button7) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button7);
      _anyGamepadBtnDown8 = Input.GetKeyDown(KeyCode.Joystick1Button8) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button8) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button8) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button8);
      _anyGamepadBtnDown9 = Input.GetKeyDown(KeyCode.Joystick1Button9) ||
                            Input.GetKeyDown(KeyCode.Joystick2Button9) ||
                            Input.GetKeyDown(KeyCode.Joystick3Button9) ||
                            Input.GetKeyDown(KeyCode.Joystick4Button9);

      // e.g. Xbox Controller
      if (_anyGamepadBtnDown0) _logger.LogDebug("Button 0 down (A)"); // A
      if (_anyGamepadBtnDown1) _logger.LogDebug("Button 1 down (B)"); // B
      if (_anyGamepadBtnDown2) _logger.LogDebug("Button 2 down (X)"); // X
      if (_anyGamepadBtnDown3) _logger.LogDebug("Button 3 down (Y)"); // Y
      if (_anyGamepadBtnDown4) _logger.LogDebug("Button 4 down (LB)"); // left shoulder
      if (_anyGamepadBtnDown5) _logger.LogDebug("Button 5 down (RB)"); // right shoulder
      if (_anyGamepadBtnDown6) _logger.LogDebug("Button 6 down (SEL)"); // back/select
      if (_anyGamepadBtnDown7) _logger.LogDebug("Button 7 down (START)"); // start
      if (_anyGamepadBtnDown8) _logger.LogDebug("Button 8 down (LS)"); // left stick click
      if (_anyGamepadBtnDown9) _logger.LogDebug("Button 9 down (RS)"); // right stick click
   }


   /// <summary>
   /// Handles the processing of the position and rotation of the camera.
   /// </summary>
   private void ProcessBikeModeCamera()
   {
      if (_playerBikeTransform == null)
      {
         return;
      }

      _dirToCam = _camTransform.position - _playerBikeTransform.TransformPoint(_targetOffset);

      // Paused game check; only run when playing
      if (!_isMenuOpen)
      {
         // Double check that the default camera is disabled
         _defaultPlayCamera.enabled = false;

         // Lock and hide the cursor
         if (!Debugger.IsAttached)
         {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
         }

         bool zoomIn = Input.GetAxis("Mouse ScrollWheel") > 0f
                        || _anyGamepadDpadUp;
         bool zoomOut = Input.GetAxis("Mouse ScrollWheel") < 0f
                        || _anyGamepadDpadDown;

         if (zoomIn)
         {
            // Scrolling forward; zoom in
            if (Input.GetKey(_cfg.Keyboard.AdjustFocalLengthKey.Value) && _wantedFocalLength > 0)
            {
               _wantedFocalLength--;
               _logger.LogDebug("FocalLength " + _wantedFocalLength);
            }
            else if (Input.GetKey(_cfg.Keyboard.AdjustFocusDistanceKey.Value))
            {
               _cfg.Camera.FocusDistanceOffset.Value++;
               _logger.LogDebug("FocusDistanceOffset " + _cfg.Camera.FocusDistanceOffset.Value);
            }
            else
            {
               _wantedZoom -= _cfg.Camera.ZoomStepIncrement.Value;
            }
         }
         else if (zoomOut)
         {
            // Scrolling backwards; zoom out
            if (Input.GetKey(_cfg.Keyboard.AdjustFocalLengthKey.Value))
            {
               _wantedFocalLength++;
               _logger.LogDebug("FocalLength " + _wantedFocalLength);
            }
            else if (Input.GetKey(_cfg.Keyboard.AdjustFocusDistanceKey.Value) && _cfg.Camera.FocusDistanceOffset.Value > 0)
            {
               _cfg.Camera.FocusDistanceOffset.Value--;
               _logger.LogDebug("FocusDistanceOffset " + _cfg.Camera.FocusDistanceOffset.Value);
            }
            else
            {
               _wantedZoom += _cfg.Camera.ZoomStepIncrement.Value;
            }
         }

         if (_wantedZoom < 0.0f)
         {
            _wantedZoom = 0.0f;
         }

         // Horizontal mouse movement will make camera rotate around vertical y-axis
         // Vertical mouse movement will make camera rotate along x-axis (your ear-to-ear axis)
         _rotationHorizontal += Input.GetAxisRaw("Mouse X") * _cfg.Mouse.SensitivityHorizontal.Value *
                           _cfg.Mouse.SensitivityMultiplier.Value;
         _rotationVertical += Input.GetAxisRaw("Mouse Y") * _cfg.Mouse.SensitivityVertical.Value *
                         _cfg.Mouse.SensitivityMultiplier.Value;

         // Also apply controller input
         _rotationHorizontal += ApplyInnerDeadzone(_anyGamepadStickHorizontalR, _cfg.Controller.RightStickDeadzone.Value) *
                           _cfg.Controller.SensitivityHorizontal.Value * _cfg.Controller.SensitivityMultiplier.Value;
         _rotationVertical -= ApplyInnerDeadzone(_anyGamepadStickVerticalR, _cfg.Controller.RightStickDeadzone.Value) *
                         _cfg.Controller.SensitivityVertical.Value * _cfg.Controller.SensitivityMultiplier.Value;
         _rotationVertical = ClampAngle(_rotationVertical, -50, 30); // Clamp the up-down rotation

         // Handle alignment, either auto or manual by mouse/stick
         bool autoAligningIsActive = (_camAutoAlign && !_holdingInvertManualAlignMode) ||
                                     (!_camAutoAlign && _holdingInvertManualAlignMode);

         if (autoAligningIsActive)
         {
            // auto align camera behind the bike
            if (_cfg.Mouse.InvertHorizontalLook.Value)
            {
               // Lerp the horizontal rotation relative to the player
               _rotationHorizontal = Mathf.LerpAngle(_rotationHorizontal,
                  -_playerBikeParentTransform.localRotation.eulerAngles.y,
                  _cfg.Camera.AutoAlignSpeed.Value * Time.deltaTime);
               _rotationHorizontal = ClampAngle(_rotationHorizontal, -360, 360);
               _rotation = Quaternion.Euler(-_rotationVertical, -_rotationHorizontal, 0f);
            }
            else
            {
               _rotationHorizontal = Mathf.LerpAngle(_rotationHorizontal,
                  _playerBikeParentTransform.localRotation.eulerAngles.y,
                  _cfg.Camera.AutoAlignSpeed.Value * Time.deltaTime);
               _rotationHorizontal = ClampAngle(_rotationHorizontal, -360, 360);
               _rotation = Quaternion.Euler(-_rotationVertical, _rotationHorizontal, 0f);
            }
         }
         else if (!_camAutoAlign && _cfg.Camera.ManualAlignmentInput.Value == CameraManualAlignmentInput.MouseOrRStick)
         {
            // move the camera to where the Mouse/RStick moved it
            _rotationHorizontal = ClampAngle(_rotationHorizontal, -360, 360);
            if (_cfg.Mouse.InvertHorizontalLook.Value)
            {
               _rotation = Quaternion.Euler(-_rotationVertical, -_rotationHorizontal, 0f);
            }
            else
            {
               _rotation = Quaternion.Euler(-_rotationVertical, _rotationHorizontal, 0f);
            }
         }

         // Raycast from the target towards the camera
         if (Physics.Raycast(_playerBikeTransform.TransformPoint(_targetOffset),
                _dirToCam.normalized,
                out var hitInfo,
                _wantedZoom + 0.2f,
                __cameraCollisionLayers))
         {
            _projectedDistance = Vector3.Distance(hitInfo.point, _playerBikeTransform.TransformPoint(_targetOffset));
         }
         else
         {
            _projectedDistance = 900;
         }

         if (_projectedDistance < _wantedZoom)
         {
            // Desired camera distance is greater than the collision distance so zoom in to prevent clipping
            // b=bike, c=camera, *=collision
            // b-------*---c
            // b------c*
            float newTargetZoom = _projectedDistance - _cfg.Camera.CollisionPadding.Value;
            _targetZoomAmount = Mathf.Lerp(_targetZoomAmount, newTargetZoom, _cfg.Camera.ZoomLerpInSpeed.Value);
         }
         else
         {
            // Zoom the camera back out to wanted distance over time
            _targetZoomAmount = Mathf.Lerp(_targetZoomAmount, _wantedZoom, Time.deltaTime * _cfg.Camera.ZoomLerpOutSpeed.Value);
         }

         if (_targetZoomAmount < 0.0f)
         {
            _targetZoomAmount = 0.0f;
         }

         Vector3 finalPosition = _rotation * new Vector3(0f, 0f, -_targetZoomAmount) +
                                 _playerBikeTransform.TransformPoint(_targetOffset);

         // Apply values
         _camTransform.position = finalPosition;
         _camTransform.rotation = _rotation;

         // Adjust DoF
         if (_hasDepthOfFieldSetting)
         {
            _depthOfFieldSettings.focusDistance.value = _cfg.Camera.FocusDistanceOffset.Value +
                                                        Vector3.Distance(_camTransform.position,
                                                           _playerBikeTransform.position);
            _depthOfFieldSettings.focalLength.value = _wantedFocalLength;
         }
      }
      else // The menu is open; game is paused
      {
         // While paused show the cursor
         Cursor.lockState = CursorLockMode.None;
         Cursor.visible = true;

         // and optionally show the default camera
         if (!_defaultPlayCamera.enabled && _cfg.Camera.DefaultCameraOnPause.Value)
         {
            // Adjust DoF
            if (_hasDepthOfFieldSetting)
            {
               _depthOfFieldSettings.focalLength.value = _baseFocalLength;
            }

            //mainCameraComponent.fieldOfView = baseFoV;
            _defaultPlayCamera.enabled = true;
         }
      }
   }


   private void HandleBikeModeUserInputs()
   {
      if (CheckKeyPressed(_cfg.Keyboard.OriginalCamKey.Value))
      {
         SelectCameraMode(CameraMode.Original);
      }

      if (CheckKeyPressed(_cfg.Keyboard.ThirdPersonCamKey.Value)
          || (_firstInit && _cfg.Camera.InitialMode.Value == CameraMode.ThirdPerson))
      {
         SelectCameraMode(CameraMode.ThirdPerson);
      }

      if (CheckKeyPressed(_cfg.Keyboard.FirstPersonCamKey.Value)
          || (_firstInit && _cfg.Camera.InitialMode.Value == CameraMode.FirstPerson))
      {
         SelectCameraMode(CameraMode.FirstPerson);
      }

      if (CheckGamepadButtonPressed(_cfg.Controller.ToggleCamStateButton.Value)
          || CheckKeyPressed(_cfg.Keyboard.ToggleCamStateKey.Value))
      {
         ToggleCamState();
      }

      // Mouse inverting
      if (CheckKeyPressed(_cfg.Keyboard.InvertHorizontalLookKey.Value))
      {
         _cfg.Mouse.InvertHorizontalLook.Value = !_cfg.Mouse.InvertHorizontalLook.Value;
         AlignViewWithBike();
      }

      // Camera auto align
      if (CheckKeyPressed(_cfg.Keyboard.ToggleCamAlignModeKey.Value))
      {
         _camAutoAlign = !_camAutoAlign;
         if (_camAutoAlign)
         {
            ResetCamPosition();
         }
      }

      // Camera 'snap back' alignment
      if (CheckKeyPressed(_cfg.Keyboard.SnapAlignCamKey.Value)
          || CheckGamepadButtonPressed(_cfg.Controller.SnapAlignCamButton.Value))
      {
         ResetCamPosition();
      }

      // Field of view
      var alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
      var incr = alt ? 5 : 10;
      if (CheckKeyPressed(_cfg.Keyboard.FovDecreaseKey.Value) || _anyGamepadDpadLeft)
      {
         ChangeFieldOfView(-incr);
      }
      if (CheckKeyPressed(_cfg.Keyboard.FovIncreaseKey.Value) || _anyGamepadDpadRight)
      {
         ChangeFieldOfView(incr);
      }
      if (CheckKeyPressed(_cfg.Keyboard.FovResetKey.Value))
      {
         ChangeFieldOfView(0);
      }

      // Photo Mode
      if (CheckKeyPressed(KeyCode.P) || _anyGamepadBtnDown3
          || (_escapeKeyDown && _menuWasOpenedWhileInPhotoMode))
      {
         _menuWasOpenedWhileInPhotoMode = false;
         TogglePhotoMode(true);
      }

   }


   private void ChangeFieldOfView(int fovIncr)
   {
      switch (_currentCamMode)
      {
         case CameraMode.Original:
            if (fovIncr != 0)
            {
               _isometricFoV = LimitFoV(_isometricFoV + fovIncr, CameraMode.Original);
            }
            else
            {
               _isometricFoV = DefaultIsometricFoV;
            }
            ApplyIsometricCameraSettings();
            break;
         case CameraMode.ThirdPerson:
            if (fovIncr != 0)
            {
               _thirdPersonFoV = LimitFoV(_thirdPersonFoV + fovIncr, CameraMode.ThirdPerson);
            }
            else
            {
               _thirdPersonFoV = LimitFoV(_cfg.Camera.ThirdPersonFoV.Value, CameraMode.ThirdPerson);
            }
            ApplyThirdPersonCam();
            break;
         case CameraMode.FirstPerson:
            if (fovIncr != 0)
            {
               _firstPersonFoV = LimitFoV(_firstPersonFoV + fovIncr, CameraMode.FirstPerson);
            }
            else
            {
               _firstPersonFoV = LimitFoV(_cfg.Camera.FirstPersonFoV.Value, CameraMode.FirstPerson);
            }
            ApplyFirstPersonCam();
            break;
      }
   }


   private void ToggleCamState()
   {
      _logger.LogDebug("Toggle Cam State");
      if (_currentCamMode == CameraMode.Original)
      {
         ApplyCameraMode(_altCamStateMode);
      }
      else
      {
         ApplyCameraMode(CameraMode.Original);
      }
   }


   private bool CheckKeyPressed(KeyCode kc)
   {
      if (kc == KeyCode.None) return false;
      bool pressed = Input.GetKeyDown(kc);
      return pressed;
   }


   private bool CheckGamepadButtonPressed(ControllerButton gpBtn)
   {
      bool pressed = false;
      switch (gpBtn)
      {
         case ControllerButton.X:
            pressed = _anyGamepadBtnDown2;
            _logger.LogDebug(pressed, "Gamepad X");
            break;
         case ControllerButton.Y:
            pressed = _anyGamepadBtnDown3;
            _logger.LogDebug(pressed, "Gamepad Y");
            break;
         case ControllerButton.LB:
            pressed = _anyGamepadBtnDown4;
            _logger.LogDebug(pressed, "Gamepad LB");
            break;
         case ControllerButton.RB:
            pressed = _anyGamepadBtnDown5;
            _logger.LogDebug(pressed, "Gamepad RB");
            break;
         case ControllerButton.LStick:
            pressed = _anyGamepadBtnDown8;
            _logger.LogDebug(pressed, "Gamepad LStick");
            break;
         case ControllerButton.RStick:
            pressed = _anyGamepadBtnDown9;
            _logger.LogDebug(pressed, "Gamepad RStick");
            break;
      }

      return pressed;
   }


   private void ResetCamPosition()
   {
      _rotationVertical = _cfg.Camera.ThirdPersionRotationVert.Value;
      AlignViewWithBike();
   }


   private void SelectCameraMode(CameraMode camMode)
   {
      if (GatherCameraRelatedGameObjects())
      {
         switch (camMode)
         {
            case CameraMode.ThirdPerson:
               SelectThirdPersonCam();
               break;
            case CameraMode.FirstPerson:
               SelectFirstPersonCam();
               break;
            case CameraMode.Original:
               SelectOriginalCam();
               break;
         }
      }
   }


   private void ApplyCameraMode(CameraMode camMode)
   {
      if (GatherCameraRelatedGameObjects())
      {
         switch (camMode)
         {
            case CameraMode.ThirdPerson:
               ApplyThirdPersonCam();
               break;
            case CameraMode.FirstPerson:
               ApplyFirstPersonCam();
               break;
            case CameraMode.Original:
               ApplyOriginalCam();
               break;
         }
      }
   }


   private void SelectOriginalCam()
   {
      ApplyOriginalCam();
   }


   private void ApplyOriginalCam()
   {
      _currentCamMode = CameraMode.Original;
      _defaultPlayCamera.enabled = true;
      ApplyIsometricCameraSettings();
      Cursor.lockState = CursorLockMode.None;
      //Cursor.visible = true;
      _logger.LogInfo("Original camera");
      AlignViewWithBike();
   }


   private void SelectFirstPersonCam()
   {
      _altCamStateMode = CameraMode.FirstPerson;
      ApplyFirstPersonCam();
   }


   private void ApplyFirstPersonCam()
   {
      _currentCamMode = CameraMode.FirstPerson;
      ApplyCameraSettings(0f, new Vector3(0.0f, 0.3f, 0.0f), _firstPersonFoV, 0.6f, "neck_BindJNT");
      // Navigate to bike mesh renderer to prevent it from vanishing in first person
      SkinnedMeshRenderer bikeMeshRenderer = _playerBikeParentTransform.GetChild(7).transform.GetChild(1)
         .gameObject.GetComponent<SkinnedMeshRenderer>();
      bikeMeshRenderer.updateWhenOffscreen = true;
      ApplyCommonCameraSettings();
      _logger.LogInfo("First person camera");
      AlignViewWithBike();
   }


   private void SelectThirdPersonCam()
   {
      _altCamStateMode = CameraMode.ThirdPerson;
      ApplyThirdPersonCam();
   }


   private void ApplyThirdPersonCam()
   {
      _currentCamMode = CameraMode.ThirdPerson;
      ApplyCameraSettings(_cfg.Camera.ThirdPersionInitialFollowDistance.Value, new Vector3(0f, 2.4f, 0f), _thirdPersonFoV, 0.28f, "Bike(Clone)");
      ApplyCommonCameraSettings();
      _logger.LogInfo("Third person camera");
      AlignViewWithBike();
   }


   private void ApplyCommonCameraSettings()
   {
      // Adjust DoF
      if (_hasDepthOfFieldSetting)
      {
         _depthOfFieldSettings.focalLength.value = _cfg.Camera.FocalLength.Value;
      }

      _defaultPlayCamera.enabled = false;
   }


   /// <summary>
   /// Resets the camera settings to default values. FoV, nearClipPlane, focalLength.
   /// </summary>
   private void ApplyIsometricCameraSettings()
   {
      _mainCamera.fieldOfView = _isometricFoV;
      _mainCamera.nearClipPlane = 0.3f;
      if (_hasDepthOfFieldSetting)
      {
         _depthOfFieldSettings.focalLength.value = _baseFocalLength;
      }
   }


   /// <summary>
   /// Allows applying multiple camera settings quickly.
   /// </summary>
   private void ApplyCameraSettings(float followDistance, Vector3 followTargetOffset, float cameraFov,
      float nearClipPlane, string followTargetName)
   {
      _targetName = followTargetName;
      // Update reference
      _playerBikeTransform = GameObject.Find(_targetName).GetComponent<Transform>();

      _wantedZoom = followDistance;
      _targetOffset = followTargetOffset;

      _mainCamera.fieldOfView = cameraFov; // Default: 34
      _mainCamera.nearClipPlane = nearClipPlane; // Default: 0.3
   }


   /// <summary>
   /// Makes the camera move to directly behind the player.
   /// Useful for restarting at checkpoints.
   /// </summary>
   private void AlignViewWithBike()
   {
      if (_playerBikeParentTransform.gameObject == null)
      {
         return;
      }

      Vector3 bikeRotation = _playerBikeParentTransform.localRotation.eulerAngles;
      if (_cfg.Mouse.InvertHorizontalLook.Value)
      {
         _rotationHorizontal = -bikeRotation.y;
      }
      else
      {
         _rotationHorizontal = bikeRotation.y;
      }
   }


   /// <summary>
   /// Tries to clamp the angle to values between 360 and -360.
   /// </summary>
   private float ClampAngle(float angle, float min, float max)
   {
      if (angle < -360f)
      {
         angle += 360f;
      }

      if (angle > 360f)
      {
         angle -= 360f;
      }

      return Mathf.Clamp(angle, min, max);
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


   #region Photo Mode

   /// <summary>
   /// Toggles photo mode to the provided state.
   /// </summary>
   private void TogglePhotoMode(bool activate, bool applyPreviousState = false)
   {
      _lastScreenshotInfo = null;

      if (activate)
      {
         SaveCamPos(_camPosBike);

         _mode = ModMode.PhotoCam;
         _photoModeHudRestoreState = IsHudVisible();
         if (_cfg.PhotoMode.AutoHideHud.Value)
         {
            ToggleHudVisiblity(false);
         }

         _logger.LogInfo("Enter photo mode");
         _photoModeBaseTimeScale = Time.timeScale; // Save the original time scale before freezing
         Time.timeScale = 0;

         GatherCameraRelatedGameObjects();

         // Save the original FoV and DoF focus distance
         _photoModeBaseFoV = _mainCamera.fieldOfView;
         _photoModeBaseFocusDistanceDoF = _depthOfFieldSettings.focusDistance.GetValue<float>();

         // Save camera rotation for later calculations
         if (applyPreviousState)
         {
            ApplyCamPos(_camPosPhoto);
         }
         else
         {
            _rotationHorizontal = _camTransform.eulerAngles.y;
            _rotationVertical = -_camTransform.eulerAngles.x;
            _rotationRoll = 0;
         }

         // Disable normal camera controller
         _defaultPlayCamera.enabled = false;

         // Lock and hide the cursor
         if (!Debugger.IsAttached)
         {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
         }
      }
      else
      {
         SaveCamPos(_camPosPhoto);

         _mode = ModMode.BikeCam;
         _logger.LogInfo("Exit photo mode");
         Time.timeScale = _photoModeBaseTimeScale; // Reset the time scale to what it was before we froze the time
         _mainCamera.fieldOfView = _photoModeBaseFoV;	// Restore the original FoV
         _depthOfFieldSettings.focusDistance.value = _photoModeBaseFocusDistanceDoF;   // Restore the original focus distance for DoF

         ApplyCameraMode(_currentCamMode);
         ApplyCamPos(_camPosBike);

         ToggleHudVisiblity(_photoModeHudRestoreState);
         _photoModeInstructionsVisible = true; // next time show instruction again
      }

      System.Threading.Thread.Sleep(200);
   }


   private void ApplyCamPos(CamPos cp)
   {
      _camTransform.eulerAngles = cp.EulerAngles;
      _camTransform.localEulerAngles = cp.LocalEulerAngles;
      _camTransform.position = cp.Position;
      _rotationHorizontal = cp.RotationHorizontal;
      _rotationVertical = cp.RotationVertical;
      _rotationRoll = cp.RotationRoll;
   }


   private void SaveCamPos(CamPos cp)
   {
      cp.EulerAngles = _camTransform.eulerAngles;
      cp.LocalEulerAngles = _camTransform.localEulerAngles;
      cp.Position = _camTransform.position;
      cp.RotationHorizontal = _rotationHorizontal;
      cp.RotationVertical = _rotationVertical;
      cp.RotationRoll = _rotationRoll;
   }


   private void HandlePhotoModeUserInputs()
   {
      if (CheckKeyPressed(KeyCode.P) || CheckKeyPressed(KeyCode.Backspace) || _anyGamepadBtnDown3)
      {
         TogglePhotoMode(false);
      }

      if (CheckKeyPressed(KeyCode.I) || _anyGamepadBtnDown8)
      {
         _photoModeInstructionsVisible = !_photoModeInstructionsVisible;
      }

      if (CheckKeyPressed(KeyCode.K) || _anyGamepadDpadVertical > 0)
      {
         _mainCamera.fieldOfView = _photoModeBaseFoV;
         _rotationRoll = 0;
      }

      if (CheckKeyPressed(KeyCode.Space) || _anyGamepadBtnDown2)
      {
         var e = TakeScreenshot();
         while (e.MoveNext()) { }
      }

      if (_hasDepthOfFieldSetting)
      {
         if (CheckKeyPressed(KeyCode.V) || _anyGamepadDpadDown)
         {
            _photoModeFocus = !_photoModeFocus;
            _logger.LogDebug("Focus Mode: " + _photoModeFocus + " dp:" + _anyGamepadDpadDownCnt + " i:" + _dpadPressDetectThreshold);
         }
      }
   }


   private IEnumerator TakeScreenshot()
   {
      SaveCamPos(_camPosPhotoShoot);
      yield return new WaitForEndOfFrame();

      try
      {
         Camera gameCam = _mainCamera;
         var hudCam = GetHudObject();

         int captureWidth = Screen.width;
         int captureHeight = Screen.height;

         var currentRt = RenderTexture.active;
         // var currentGameTt = gameCam.targetTexture;
         // var currentHudTt = hudCam.targetTexture;

         // creates off-screen render texture that can rendered into

         gameCam.targetTexture = new RenderTexture(captureWidth, captureHeight, 24);
         RenderTexture.active = gameCam.targetTexture;
         gameCam.Render();
         if (hudCam.enabled)
         {
            hudCam.targetTexture = gameCam.targetTexture;
            hudCam.Render();
         }

         Texture2D imageOverview = new Texture2D(gameCam.targetTexture.width, gameCam.targetTexture.height, TextureFormat.RGB24, false);
         imageOverview.ReadPixels(new Rect(0, 0, gameCam.targetTexture.width, gameCam.targetTexture.height), 0, 0);
         imageOverview.Apply();

         RenderTexture.active = currentRt;
         gameCam.targetTexture = null;
         hudCam.targetTexture = null;

         string filename = BuildFileName(Convert.ToInt32(imageOverview.width), Convert.ToInt32(imageOverview.height));

         // Encode texture into PNG
         byte[] bytes;
         if (filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
         {
            var jpgQuality = _cfg.PhotoMode.ScreenshotJpgQuality.Value;
            if (jpgQuality < 0 || jpgQuality > 100)
            {
               jpgQuality = 75;
            }
            bytes = imageOverview.EncodeToJPG(jpgQuality);
         }
         else // .png
         {
            if (!filename.EndsWith(".png"))
            {
               filename += ".png";
            }
            bytes = imageOverview.EncodeToPNG();
         }

         var dir = _cfg.PhotoMode.ScreenshotFolder.Value;
         var path = Path.Combine(dir, filename);
         var pathWithSubFolder = Path.GetDirectoryName(path); // the filename format may introduce a subfolder
         if (!Directory.Exists(pathWithSubFolder))
         {
            Directory.CreateDirectory(pathWithSubFolder);
         }
         
         if (File.Exists(path))
         {
            // try to anticipate what kind of format the user likes
            string sepChar = " ";
            if (filename.Contains("_"))
            {
               sepChar = "_";
            }
            int cnt = 0;
            string newPath;
            do
            {
               cnt++;
               newPath= Path.Combine(Path.GetDirectoryName(path),
                  Path.GetFileNameWithoutExtension(path) + sepChar + cnt.ToString("D3") + Path.GetExtension(path));
            }
            while (File.Exists(newPath));

            path = newPath;
         }

         File.WriteAllBytes(path, bytes);
         _lastScreenshotInfo = _lang.GetText("PhotoMode", "ScreenshotSaved_{file}", "Photo saved to {0}", path);
         _logger.LogInfo("Screenshot saved to {0}", path);
      }
      catch (Exception ex)
      {
         _lastScreenshotInfo = _lang.GetText("PhotoMode", "ScreenshotSaveErr_{msg}", "Photo save error: {0}", ex.Message);
         _logger.LogError("Screenshot save error: {0}", ex.Message);
      }
   }


   private string BuildFileName(int width, int height)
   {
      string fmt = _cfg.PhotoMode.ScreenshotFilenameFormat.Value;
      if (String.IsNullOrEmpty(fmt))
      {
         fmt = _cfg.PhotoMode.ScreenshotFilenameFormat.DefaultValue;
      }

      _screenshotCounter++;

      var dt = DateTime.Now;
      fmt = fmt.Replace("{w}", width.ToString());
      fmt = fmt.Replace("{h}", height.ToString());
      fmt = fmt.Replace("{d}", dt.ToString("yyyy-MM-dd"));
      fmt = fmt.Replace("{t}", dt.ToString("HH-mm-ss"));
      fmt = fmt.Replace("{d1}", dt.ToString("yyyy-MM-dd"));
      fmt = fmt.Replace("{t1}", dt.ToString("HH-mm-ss"));
      fmt = fmt.Replace("{d2}", dt.ToString("yyyyMMdd"));
      fmt = fmt.Replace("{t2}", dt.ToString("HHmmss"));
      fmt = fmt.Replace("{cnt2}", _screenshotCounter.ToString("D2"));
      fmt = fmt.Replace("{cnt3}", _screenshotCounter.ToString("D3"));
      fmt = fmt.Replace("{cnt4}", _screenshotCounter.ToString("D4"));
      fmt = fmt.Replace("{cnt5}", _screenshotCounter.ToString("D5"));

      return fmt.Trim();
   }


   private void DrawPhotoModeInstructionsOnHud()
   {
      string format = @"<b><color={0}><size={1}>{2}</size></color></b>";

      var actions = new StringBuilder();
      string pre = " * ";
      actions.AppendLine(_lang.GetText("PhotoMode", "ActionHeader", "ACTION"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionExit", "Exit Photo Mode"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionShoot", "Take Photo"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionToggleInstruct", "Toggle Instructions"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionToggleHud", "Toggle HUD"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionMovePan", "Move / Pan"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionUpDown", "Up / Down"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionTilt", "Tilt Left / Right"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionSpeedUp", "Speed up movement"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionReset", "Reset rotation / FoV"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionChangeFoV", "Change FoV"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionToggleDoF", "Toggle DoF mode"));
      
      string sep = "  ";
      var keys = new StringBuilder();
      keys.AppendLine(_lang.GetText("PhotoMode", "KeyMouseHeader", "CONTROLLER"));
      keys.AppendLine(sep + "P");
      keys.AppendLine(sep + _lang.GetText("PhotoMode", "InputSpaceKey", "Space"));
      keys.AppendLine(sep + "I");
      keys.AppendLine(sep + "H");
      keys.AppendLine(sep + "W A S D + " + _lang.GetText("PhotoMode", "InputMouse", "Mouse"));
      keys.AppendLine(sep + "R / F");
      keys.AppendLine(sep + "Q / E");
      keys.AppendLine(sep + "Shift");
      keys.AppendLine(sep + "K");
      keys.AppendLine(sep + _lang.GetText("PhotoMode", "InputMouseWheel", "Mouse Scroll"));
      keys.AppendLine(sep + "V");

      var btns = new StringBuilder();
      var dpad = _lang.GetText("PhotoMode", "InputDpad", "Dpad");
      btns.AppendLine(_lang.GetText("PhotoMode", "ControllerHeader", "CONTROLLER"));
      btns.AppendLine(sep + "Y");
      btns.AppendLine(sep + "X");
      btns.AppendLine(sep + "L-Stick Click");
      btns.AppendLine(sep + "L-Stick Click");
      btns.AppendLine(sep + "L-Stick / R-Stick");
      btns.AppendLine(sep + "L-Trig / R-Trig");
      btns.AppendLine(sep + dpad + " ◄ / ►");
      btns.AppendLine(sep + "A");
      btns.AppendLine(sep + dpad + " ▲");
      btns.AppendLine(sep + "LB / RB");
      btns.AppendLine(sep + dpad + " ▼");


      var foreColor = "#E9AC4F";
      var shadowColor = "black";
      var shadowOff = 2;

      var fmtBindDescr = String.Format(format, foreColor, 20, actions);
      var fmtKeyDescr = String.Format(format, foreColor, 20, keys);
      var fmtBtnDescr = String.Format(format, foreColor, 20, btns);

      var fmtBindDescrShadow = String.Format(format, shadowColor, 20, actions);
      var fmtKeyDescrShadow = String.Format(format, shadowColor, 20, keys);
      var fmtBtnDescrShadow = String.Format(format, shadowColor, 20, btns);

      var pmLabel = _lang.GetText("PhotoMode", "Title", "PHOTO MODE");
      var onLabel = _lang.GetText("PhotoMode", "On", "on");
      var offLabel = _lang.GetText("PhotoMode", "Off", "off");
      var dofModeLabel = _lang.GetText("PhotoMode", "DoFModeLabel_{state}", "DoF focus mode: {0}", _photoModeFocus ? onLabel : offLabel);
      var note = _lang.GetText("PhotoMode", "Note", "(This instructions box is not part of the photo)");
      
      float xOffset = 50;
      float xOffset2 = 400;
      float xOffset3 = 600;
      float yPosTitle = 200;
      float yPosInstr = 250;
      float yPosNote = 540;
      float yPosState = 600;
      float yPosSaveInfo = 680;
      
      GUI.Box(new Rect(xOffset - 20, yPosTitle - 20, 800, 470), "");

      GUI.Label(new Rect(xOffset + shadowOff, yPosTitle + shadowOff, 1000, 200), $"<b><color=black><size=30>{pmLabel}</size></color></b>");
      GUI.Label(new Rect(xOffset, yPosTitle, 1000, 200), $"<b><color=white><size=30>{pmLabel}</size></color></b>");

      GUI.Label(new Rect(xOffset + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBindDescrShadow);
      GUI.Label(new Rect(xOffset, yPosInstr, 2000, 2000), fmtBindDescr);

      GUI.Label(new Rect(xOffset2 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBtnDescrShadow);
      GUI.Label(new Rect(xOffset2, yPosInstr, 2000, 2000), fmtBtnDescr);

      GUI.Label(new Rect(xOffset3 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtKeyDescrShadow);
      GUI.Label(new Rect(xOffset3, yPosInstr, 2000, 2000), fmtKeyDescr);

      GUI.Label(new Rect(xOffset + shadowOff, yPosState + shadowOff, 1000, 200), $"<b><color={shadowColor}><size=20>{dofModeLabel}</size></color></b>");
      GUI.Label(new Rect(xOffset, yPosState, 1000, 200), $"<b><color={(_photoModeFocus ? "lime" : foreColor)}><size=20>{dofModeLabel}</size></color></b>");

      GUI.Label(new Rect(xOffset + 1, yPosNote + 1, 1000, 200), $"<b><color=black><size=15>{note}</size></color></b>");
      GUI.Label(new Rect(xOffset, yPosNote, 1000, 200), $"<b><color=#CCCCCC><size=15>{note}</size></color></b>");

      if (!String.IsNullOrEmpty(_lastScreenshotInfo))
      {
         GUI.Box(new Rect(xOffset - 20, yPosSaveInfo - 10, 800, 90), "");

         GUI.Label(new Rect(xOffset + shadowOff, yPosSaveInfo + shadowOff, 1200, 200),
            $"<b><color={shadowColor}><size=20>{_lastScreenshotInfo}</size></color></b>");
         GUI.Label(new Rect(xOffset, yPosSaveInfo, 1200, 200),
            $"<b><color=yellow><size=20>{_lastScreenshotInfo}</size></color></b>");
      }
   }


   private void ProcessPhotoModeCamera()
   {
      //inFocusMode = hasDOFSettings && (Input.GetKey(focusModeModifierKey) || anyGamepadDpadVertical < 0);
      bool holdingSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || _anyGamepadBtn0;

      if (Input.GetAxis("Mouse ScrollWheel") > 0f || _anyGamepadBtnDown5)
      {
         // Scrolling forward / right bumper
         if (_photoModeFocus)
         {
            // Move DoF focus further away
            _depthOfFieldSettings.focusDistance.value = _depthOfFieldSettings.focusDistance.GetValue<float>() + (holdingSprint ? 1f : 0.05f);
         }
         else
         {
            // FoV zoom in
            _mainCamera.fieldOfView -= 1;
         }
      }
      else if (Input.GetAxis("Mouse ScrollWheel") < 0f || _anyGamepadBtnDown4)
      {
         // Scrolling backwards / left bumper
         if (_photoModeFocus)
         {
            // Move DoF focus further away
            _depthOfFieldSettings.focusDistance.value = _depthOfFieldSettings.focusDistance.GetValue<float>() - (holdingSprint ? 1f : 0.05f);
         }
         else
         {
            // FoV zoom out
            _mainCamera.fieldOfView += 1;
         }
      }

      // Moving the camera
      Vector3 moveVector = TranlateInputToCameraMovement() * _cfg.PhotoMode.CameraMovementSpeed.Value * Time.fixedDeltaTime;

      // Speed up movement when shift key held
      if (holdingSprint)
      {
         moveVector *= _cfg.PhotoMode.CameraMovementSpeedMultiplier.Value;
      }

      // Horizontal movement will make camera rotate around vertical y-axis
      // Vertical movement will make camera rotate along x-axis (your ear-to-ear axis)

      // Mouse input
      if (_cfg.Mouse.InvertHorizontalLook.Value)
      {
         _rotationHorizontal -= Input.GetAxisRaw("Mouse X") * _cfg.Mouse.SensitivityHorizontal.Value * _cfg.Mouse.SensitivityMultiplier.Value;
      }
      else
      {
         _rotationHorizontal += Input.GetAxisRaw("Mouse X") * _cfg.Mouse.SensitivityHorizontal.Value * _cfg.Mouse.SensitivityMultiplier.Value;
      }
      if (_cfg.Mouse.InvertVerticalLook.Value)
      {
         _rotationVertical -= Input.GetAxisRaw("Mouse Y") * _cfg.Mouse.SensitivityVertical.Value * _cfg.Mouse.SensitivityMultiplier.Value;
      }
      else
      {
         _rotationVertical += Input.GetAxisRaw("Mouse Y") * _cfg.Mouse.SensitivityVertical.Value * _cfg.Mouse.SensitivityMultiplier.Value;
      }

      // Controller input
      if (_cfg.Controller.InvertHorizontalLook.Value)
      {
         _rotationHorizontal -= ApplyInnerDeadzone(_anyGamepadStickHorizontalR, _cfg.Controller.RightStickDeadzone.Value) * _cfg.Controller.SensitivityHorizontal.Value * _cfg.Controller.SensitivityMultiplier.Value;
      }
      else
      {
         _rotationHorizontal += ApplyInnerDeadzone(_anyGamepadStickHorizontalR, _cfg.Controller.RightStickDeadzone.Value) * _cfg.Controller.SensitivityHorizontal.Value * _cfg.Controller.SensitivityMultiplier.Value;
      }
      if (_cfg.Controller.InvertVerticalLook.Value)
      {
         _rotationVertical += ApplyInnerDeadzone(_anyGamepadStickVerticalR, _cfg.Controller.RightStickDeadzone.Value) * _cfg.Controller.SensitivityVertical.Value * _cfg.Controller.SensitivityMultiplier.Value;
      }
      else
      {
         _rotationVertical -= ApplyInnerDeadzone(_anyGamepadStickVerticalR, _cfg.Controller.RightStickDeadzone.Value) * _cfg.Controller.SensitivityVertical.Value * _cfg.Controller.SensitivityMultiplier.Value;
      }

      // Clamp the up-down rotation
      _rotationVertical = ClampAngle(_rotationVertical,
         _verticalClampAnglesMap[_currentCamMode].Item1,
         _verticalClampAnglesMap[_currentCamMode].Item2);

      if (Input.GetKey(KeyCode.Q))
      {
         _rotationRoll += 0.1f;
         if (holdingSprint)
         {
            _rotationRoll += 0.7f;
         }
      }
      if (Input.GetKey(KeyCode.E))
      {
         _rotationRoll -= 0.1f;
         if (holdingSprint)
         {
            _rotationRoll -= 0.7f;
         }
      }
      _rotationRoll += -_anyGamepadDpadHorizontal * 4f * Time.fixedDeltaTime * (holdingSprint ? 7 : 1);

      _rotation = Quaternion.Euler(-_rotationVertical, _rotationHorizontal, _rotationRoll);
      Vector3 newPosition = _camTransform.position + moveVector;

      // Apply values
      _camTransform.position = newPosition;
      _camTransform.rotation = _rotation;

      var comparePos = new CamPos();
      SaveCamPos(comparePos);
      if (!comparePos.Matches(_camPosPhotoShoot))
      {
         // clear screenshot info when camera is moved
         _lastScreenshotInfo = null;
      }
   }


   private Vector3 TranlateInputToCameraMovement()
   {
      Vector3 direction = new Vector3();

      if (Input.GetKey(KeyCode.R))
      {
         direction += Vector3.up;
      }
      if (Input.GetKey(KeyCode.F))
      {
         direction += Vector3.down;
      }

      direction += ApplyInnerDeadzone(_anyGamepadTriggerInputR, _cfg.Controller.RightTriggerDeadzone.Value) * Vector3.up;
      direction += ApplyInnerDeadzone(_anyGamepadTriggerInputL, _cfg.Controller.LeftTriggerDeadzone.Value) * Vector3.down;

      direction += ApplyInnerDeadzone(Input.GetAxisRaw("Vertical"), _cfg.Controller.LeftStickDeadzone.Value) * _camTransform.forward;
      direction += ApplyInnerDeadzone(Input.GetAxisRaw("Horizontal"), _cfg.Controller.LeftStickDeadzone.Value) * _camTransform.right;

      return direction;
   }

   #endregion


   internal void ParseHudInfoDisplayFlags(string text)
   {
      if (String.IsNullOrWhiteSpace(text))
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
}
