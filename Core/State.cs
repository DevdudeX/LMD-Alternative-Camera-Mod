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
	private bool _needCameraReset;
	private bool _initialized;


	public State(Logger logger)
	{
		_logger = logger;
	}


	public bool Initialize()
	{
		if (_initialized) return true;
		_initialized = GatherMenuRelatedGameObjects();
		return _initialized;
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
		bool mainMenuActive = false;
		bool gameMenuActive = false;
		bool playActive = false;
		bool pauseActive = false;
		Transform uiMainParent = wrapper.GetComponent<Transform>();
		for (int i = 0; i < uiMainParent.childCount; i++)
		{
			Transform child = uiMainParent.GetChild(i);
			GameObject go = child.gameObject;
			if (go.name.StartsWith("BlackBorder"))
			{
				blackActive = go.active;
			}
			else if (go.name.StartsWith("SplashScreen"))
			{
				splashActive = go.active;
			}
			else if (go.name.StartsWith("MainMenu"))
			{
				mainMenuActive = go.active;
			}
			else if (go.name.StartsWith("GameMenu"))
			{
				gameMenuActive = go.active;
			}
			else if (go.name.StartsWith("PlayScreen"))
			{
				playActive = go.active;
			}
			else if (go.name.StartsWith("PauseScreen"))
			{
				pauseActive = go.active;
			}
		}

		if (blackActive && !splashActive && !mainMenuActive) _currentScreen = Screen.LoadingScreen;
		else if (splashActive && !mainMenuActive) _currentScreen = Screen.SplashScreen;
		else if (mainMenuActive) _currentScreen = Screen.MainMenuScreen;
		else if (gameMenuActive) _currentScreen = Screen.GameMenuScreen;
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


	public bool IsPausedInPhotoMode
	{
		get { return _menuWasOpenedWhileInPhotoMode; }
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
		_needCameraReset = true;
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
		if (!_needCameraReset)
		{
			_needCameraReset = _isMenuLastOpen && !_isMenuOpen;
		}
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

	public bool IsMenuOpenChanged()
	{
		return _isMenuOpen && !_isMenuLastOpen;
	}

	public int Fps
	{
		get { return _fps; }
	}

	public bool NeedCameraReset
	{
		get { return _needCameraReset; }
	}

	public void ClearNeedCameraReset()
	{
		_needCameraReset = false;
	}

	public string ErrorMessage { get; set; }

	public bool PhotoModeInstructionsVisible { get; private set; } = true;

	public string LastScreenshotInfo { get; set; }


	private bool GatherMenuRelatedGameObjects()
	{
		GameObject wrapper = GameObject.Find("Wrapper");
		if (wrapper == null)
		{
			return false;
		}

		Transform uiMainParent = GameObject.Find("Wrapper").GetComponent<Transform>();
		for (int i = 0; i < uiMainParent.childCount; i++)
		{
			Transform child = uiMainParent.GetChild(i);
			GameObject go = child.gameObject;
			if (IsMenuObject(go.name))
			{
				_menuObjects[go.name] = go;
				_logger.LogVerbose("Game Object: {0}", go.name);
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
