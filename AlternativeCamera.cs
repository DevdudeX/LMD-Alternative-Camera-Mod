// Mod
using MelonLoader;
using AlternativeCameraMod;
// Megagon
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
   public const string MOD_VERSION = "2.1.0"; // also update in project build properties

   private bool _modInitialized;
   private bool _modInitializing;
   private bool _firstFrame;
   private Configuration _cfg = null!;
   private LanguageConfig _lang = null!;
   private InputHandler _input = null!;
   private Logger _logger = null!;
   private CameraControl _camera = null!;
   private Hud _hud = null!;
   private State _state = null!;
   

   public override void OnEarlyInitializeMelon()
   {
   }


   public override void OnInitializeMelon()
   {
      _firstFrame = true;

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

      _state = new State(_logger);
      ValidateConfig();

      _input = new InputHandler(_cfg, _lang, _logger);
      _camera = new CameraControl(_state, _input, _cfg, _logger);
      _hud = new Hud(_state, _camera, _input, _cfg, _lang, _logger);
   }


   public override void OnDeinitializeMelon()
   {
      _state.SuspendOperation();
      _hud.Close();
   }


   public override void OnUpdate()
   {
      if (_state.Suspended) return;
      _input.OnUpdate();
   }


   public override void OnLateUpdate()
   {
      _state.TrackScreenState();
      _hud.InitializeOnce();

      if (_state.Suspended)
      {
         return;
      }

      InitializeOnce();
      if (!_modInitialized)
      {
         return;
      }

      _state.CheckMenuOpen();
      
      if (SpecialTreatmentForUsingMenuDuringPhotoMode())
      {
         return;
      }

      if (_state.IsMenuOpenChanged())
      {
         _camera.OnMenuOpen();
         return;
      }

      if (_state.IsMenuOpen)
      {
         _input.EnableMouseCursorOnDemand(_state.Fps);
         if (_state.CurrentScreen == Screen.PauseScreen)
         {
            if (_input.PlayMode.ShowInstructions)
            {
               _cfg.PlayMode.ShowCamInstructionsInPauseMenu.Value = !_cfg.PlayMode.ShowCamInstructionsInPauseMenu.Value;
            }
         }

         return;
      }
      
      if (_state.NeedCameraReset)
      {
         _logger.LogDebug("Resetting camera ...");
         _camera.ApplyCameraModeOnEnterPlay();
         _state.ClearNeedCameraReset();
      }

      if (_state.CameraMode == CameraMode.BikeCam)
      {
         if (HandleBikeModeUserInputs())
         {
            _camera.ProcessBikeMode();
         }
      }
      else if (_state.CameraMode == CameraMode.PhotoCam)
      {
         if (HandlePhotoModeUserInputs())
         {
            _camera.ProcessPhotoMode();
         }
      }

      if (_firstFrame)
      {
         _hud.ToggleHudVisiblity(_cfg.PlayMode.GameHudVisible.Value);
      }

      HandleCommonUserInputs();

      _firstFrame = false;
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


   private void ValidateConfig()
   {
      var cfgErr = _cfg.Validate(_lang);

      if (!String.IsNullOrEmpty(cfgErr))
      {
         var str = cfgErr
                   + Environment.NewLine + "! " + 
                   _lang.GetText("Mod", "ErrDisabledMod", "Mod disabled")
                   + Environment.NewLine + "! " + 
                   _lang.GetText("Mod", "ErrCheckConfig_{CfgFile}", "check {0}", Configuration.ConfigFilePath);

         HandleErrorState(str);
      }
   }


   private void InitializeOnce()
   {
      // Initialize once
      if (!_modInitialized && !_modInitializing)
      {
         _modInitializing = true;
         _modInitialized = InitializeSubsystems();
         _modInitializing = false;
      }
   }


   private bool InitializeSubsystems()
   {
      if (!_camera.Initialize())
      {
         return false;
      }

      if (!_state.Initialize())
      {
         return false;
      }
      
      return true;
   }


   private void HandleErrorState(string errMsg)
   {
      _state.SuspendOperation();
      _state.ErrorMessage = errMsg;
      _logger.LogError(errMsg);
   }


   private void HandleWarningState(string warnMsg, bool mustDisable = false)
   {
      if (mustDisable)
      {
         _state.SuspendOperation();
      }

      _state.ErrorMessage = warnMsg;
      _logger.LogError(warnMsg);
   }


   private bool SpecialTreatmentForUsingMenuDuringPhotoMode()
   {
      if (_input.OpenMenu())
      {
         if (_camera.Mode == CameraMode.PhotoCam)
         {
            if (_state.LastScreen == Screen.PhotoScreen)
            {
               _state.TrackPausedInPhotoMode();
               _camera.TogglePhotoMode(_hud, false);
               _camera.OnMenuOpen();
               return true; // do not apply further camera logic
            }
         }
      }
      else if (_state.ShouldReturnToPhotoModeFromPauseMenu())
      {
         _camera.TogglePhotoMode(_hud, true, true);
         return true; // do not apply further camera logic
      }
      
      return false;
   }


   private void HandleCommonUserInputs()
   {
      if (_camera.Mode == CameraMode.BikeCam)
      {
         if (_input.PlayMode.ToggleHud())
         {
            _hud.ToggleHudVisiblity();
         }

         if (_input.PlayMode.ToggleModHud())
         {
            _hud.ToggleModHudInfoVisibility();
         }
      }
      else if (_camera.Mode == CameraMode.PhotoCam)
      {
         if (_input.PhotoMode.ToggleHud())
         {
            _hud.ToggleHudVisiblity();
         }
      }
   }


   private bool HandleBikeModeUserInputs()
   {
      if (_input.PlayMode.SelectOriginalCamera())
      {
         if (_cfg.PlayMode.EnableToggleCamStateByOriginalCamKey.Value)
         {
            _camera.ToggleCamState();
         }
         else
         {
            _camera.SelectCameraMode(CameraView.Original);
         }
      }

      if (_input.PlayMode.SelectThirdPerson()
          || (_firstFrame && _cfg.Camera.InitialMode.Value == CameraView.ThirdPerson))
      {
         _camera.SelectCameraMode(CameraView.ThirdPerson);
      }

      if (_input.PlayMode.SelectFirstPerson()
          || (_firstFrame && _cfg.Camera.InitialMode.Value == CameraView.FirstPerson))
      {
         _camera.SelectCameraMode(CameraView.FirstPerson);
      }

      if (_input.PlayMode.ToggleCamState())
      {
         _camera.ToggleCamState();
      }

      // Mouse inverting
      if (_input.PlayMode.InvertHorizontalLook())
      {
         _cfg.Mouse.InvertHorizontalLook.Value = !_cfg.Mouse.InvertHorizontalLook.Value;
         _camera.AlignViewWithBike();
      }

      // Camera auto align
      if (_input.PlayMode.ToggleCamAlignMode())
      {
         _camera.ToggleAutoAlignment();
      }

      // Camera 'snap back' alignment
      if (_input.PlayMode.SnapBehindBike())
      {
         _camera.SnapBehindBike();
      }

      // Field of view
      var alt = _input.PlayMode.ToggleFieldOfViewIncrement();
      var incr = alt ? 5 : 10;
      if (_input.PlayMode.IncreaseFieldOfView())
      {
         _camera.ChangeFieldOfView(-incr);
      }
      if (_input.PlayMode.DecreaseFieldOfView())
      {
         _camera.ChangeFieldOfView(incr);
      }
      if (_input.PlayMode.ResetFieldOfView())
      {
         _camera.ChangeFieldOfView(0);
      }

      // Photo Mode
      if (_input.PlayMode.EnterPhotoMode())
      {
         _camera.TogglePhotoMode(_hud, true);
      }

      return _state.CameraMode == CameraMode.BikeCam && _camera.CurrentCamView != CameraView.Original;
   }


   private bool HandlePhotoModeUserInputs()
   {
      if (_input.Restart())
      {
         _camera.TogglePhotoMode(_hud, false);
      }

      if (_input.PhotoMode.Exit())
      {
         _camera.TogglePhotoMode(_hud, false);
      }

      if (_input.PhotoMode.ToggleInstructions())
      {
         _state.TogglePhotoModeInstructions();
      }

      if (_input.PhotoMode.ResetCamera())
      {
         _camera.ResetCamera();
      }

      if (_input.PhotoMode.TakePhoto())
      {
         var e = _camera.TakeScreenshot();
         while (e.MoveNext()) { }

         if (_camera.ScreenshotResult != null)
         {
            if (_camera.ScreenshotResult.FilePath != null)
            {
               _state.LastScreenshotInfo = _lang.GetText("PhotoMode", "ScreenshotSaved_{file}", "Photo saved to {0}", _camera.ScreenshotResult.FilePath);
            }
            else if (_camera.ScreenshotResult.ErrorMessage != null)
            {
               _state.LastScreenshotInfo = _lang.GetText("PhotoMode", "ScreenshotSaveErr_{msg}", "Photo save error: {0}", _camera.ScreenshotResult.ErrorMessage);

            }
         }
      }

      if (_input.PhotoMode.ToggleDoFFocusMode())
      {
         _camera.ToggleFocusAdjustMode();
      }

      return _state.CameraMode == CameraMode.PhotoCam; // unchanged in phote mode
   }
}
