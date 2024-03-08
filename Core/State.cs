using UnityEngine;


namespace AlternativeCameraMod;

internal class State
{
   private readonly Logger _logger;
   private readonly Dictionary<string, GameObject> _menuObjects = new();
   private bool _suspended;
   private Screen _lastScreenState;
   private Screen _currentScreen;
   private bool _isMenuOpen = true;
   private bool _isMenuLastOpen;
   private bool _menuWasOpenedWhileInPhotoMode;
   private int _fps;
   private bool _resetCameraOnLevelStart;


   public State(Logger logger)
   {
      _logger = logger;
   }


   public bool Initialize()
   {
      return GatherMenuRelatedGameObjects();
   }


   public void TrackScreenState()
   {
      _fps = (int)(1 / Math.Max(Time.deltaTime, 0.001));

      _lastScreenState = _currentScreen;
      var wrapper = GameObject.Find("Wrapper");
      if (wrapper == null)
      {
         _currentScreen = Screen.LoadingScreen;
         return;
      }

      bool blackActive = false;
      bool splashActive = false;
      bool menuActive = false;
      bool playActive = false;
      bool pauseActive = false;
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
         if (g.name.StartsWith("PauseScreen"))
         {
            pauseActive = g.active;
         }
      }

      if (blackActive && !splashActive && !menuActive) _currentScreen = Screen.LoadingScreen;
      else if (splashActive && !menuActive) _currentScreen = Screen.SplashScreen;
      else if (menuActive) _currentScreen = Screen.MenuScreen;
      else if (pauseActive) _currentScreen = Screen.PauseScreen;
      else if (playActive)
      {
         if (CameraMode == CameraMode.PhotoCam)
         {
            _currentScreen = Screen.PhotoScreen;
         }
         else
         {
            _currentScreen = Screen.PlayScreen;
         }
      }
      else _currentScreen = Screen.None;
   }


   public void TrackPausedInPhotoMode()
   {
      _menuWasOpenedWhileInPhotoMode = true;
      _logger.LogDebug("Open menu in photo mode");
   }


   public void TogglePhotoModeInstructions()
   {
      PhotoModeInstructionsVisible = !PhotoModeInstructionsVisible;
   }


   public bool ShouldReturnToPhotoModeFromPauseMenu()
   {
      if (_lastScreenState == Screen.PauseScreen 
          && _currentScreen == Screen.PlayScreen 
          && _menuWasOpenedWhileInPhotoMode)
      {
         _isMenuOpen = false;
         _menuWasOpenedWhileInPhotoMode = false;
         return true;
      }

      return false;
   }


   public void OnPhotoModeEnter()
   {
      _logger.LogDebug("Enter photomode: {0} / {1} / {2}", _lastScreenState, _currentScreen, _menuWasOpenedWhileInPhotoMode);
      LastScreenshotInfo = null;
   }


   public void OnPhotoModeExit()
   {
      _logger.LogDebug("Exit photomode: {0} / {1} / {2}", _lastScreenState, _currentScreen, _menuWasOpenedWhileInPhotoMode);
      PhotoModeInstructionsVisible = true; // next time show instruction again
   }
   

   public void SuspendOperation()
   {
      _suspended = true;
   }
   
   
   public void ResumeOperation()
   {
      _suspended = false;
   }

   
   public void CheckMenuOpen()
   {
      _isMenuOpen = _menuObjects.Values.Any(g => g.active);
      _logger.LogDebug(_isMenuOpen && _isMenuOpen != _isMenuLastOpen, "Menu opened");
      _resetCameraOnLevelStart = _isMenuLastOpen && !_isMenuOpen;
      _isMenuLastOpen = _isMenuOpen;
   }

   
   public bool Suspended
   {
      get { return _suspended; }
   }


   public Screen CurrentScreen
   {
      get { return _currentScreen; }
   }
   
   
   public Screen LastScreen
   {
      get { return _lastScreenState; }
   }


   public CameraMode CameraMode { get; set; }


   public bool IsMenuOpen
   {
      get { return _isMenuOpen; }
   }


   public int Fps
   {
      get { return _fps; }
   }


   public bool ResetCameraOnLevelStart
   {
      get { return _resetCameraOnLevelStart; }
   }


   public string ErrorMessage { get; set; }
   public bool PhotoModeInstructionsVisible { get; private set; } = true;


   public string LastScreenshotInfo { get; set; }


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


   private bool IsMenuObject(string name)
   {
      if (name == null) return false;
      if (name.StartsWith("BlackBorder")) return false;
      if (name.StartsWith("SplashScreen")) return false;
      if (name.StartsWith("PlayScreen")) return false;
      return true;
   }
}
