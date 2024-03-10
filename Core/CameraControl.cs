using System.Collections;
using AlternativeCameraMod.Config;
using Il2CppMegagon.Downhill.Audio;
using Il2CppMegagon.Downhill.Cameras;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


namespace AlternativeCameraMod;

internal class CameraControl
{
   private readonly State _state;
   private readonly InputHandler _input;
   private readonly Logger _logger;
   private readonly Configuration _cfg;

   // Transforms and GameObjects
   /// <summary>The name of the gameobject that will act as the cameras target.</summary>
   private string _targetName = "Bike(Clone)";

   private Transform _bikeParentTransform = null!;
   private Transform _bikeTransform = null!;
   private Transform _camTransform = null!;
   private DepthOfField _depthOfFieldSettings = null!;
   private BikeSound _bikeSound;

   // The main camera itself. Used to set the field of view
   private Camera _mainCamera = null!;
   private PlayCamera _defaultPlayCamera = null!;

   private static readonly LayerMask __cameraCollisionLayers =
      LayerMask.GetMask("Ground", "Obstacle", "EnvironmentOther", "Terrain", "Lava");

   private bool _hasDepthOfFieldSetting;
   private float _appliedZoom = 8f;
   private float _targetZoomAmount;
   private Quaternion _rotation;
   
   private float _baseFoV;
   private float _baseFocusDistance;
   private float _baseFocalLength;
   private float _appliedFocalLength;

   private Vector3 _targetOffset = new Vector3(0f, 2.4f, 0f);

   private readonly Dictionary<CameraView, Tuple<float, float>> _fovLimitMap = new()
   {
      { CameraView.Original, new Tuple<float, float>(30f, 120f) },
      { CameraView.ThirdPerson, new Tuple<float, float>(40f, 120f) },
      { CameraView.FirstPerson, new Tuple<float, float>(70f, 120f) }
   };

   private readonly Dictionary<CameraView, Tuple<float, float>> _verticalClampAnglesMap = new()
   {
      // max -82, 82
      { CameraView.ThirdPerson, new Tuple<float, float>(-70f, 30f) },
      { CameraView.FirstPerson, new Tuple<float, float>(-80f, 50f) }
   };

   private const float DefaultIsometricFoV = 38f;
   private float _isometricFoV;
   private float _thirdPersonFoV;
   private float _firstPersonFoV;

   /// <summary>The distance from the bike to any world-collision between it and the camera.</summary>
   private float _projectedDistance = 200f;

   private float _rotationHorizontal;
   private float _rotationVertical;
   private float _rotationRoll;
   private Vector3 _dirToCam;

   private bool _camAutoAlign;
   private CameraView _currentCamView; // this is any
   private CameraView _altCamStateView; // this is Third/First only

   private readonly CamPos _camPosBike = new();
   private readonly CamPos _camPosPhoto = new();
   private readonly CamPos _camPosPhotoShoot = new();
   private float _baseFoVPhotoMode;
   private CameraFocusAdjustMode _focusAdjustMode;
   private float _photoModeBaseTimeScale;
   private float _baseFocusDistancePhotoMode;
   private bool _photoModeHudRestoreState;
   private int _screenshotCounter;
   private bool _initialized;


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


   public CameraControl(State state, InputHandler input, Configuration cfg, Logger logger)
   {
      _state = state;
      _input = input;
      _logger = logger;
      _cfg = cfg;

      _camAutoAlign = _cfg.Camera.AlignmentMode.Value == CameraAlignmentMode.Auto;

      _isometricFoV = DefaultIsometricFoV;
      _thirdPersonFoV = LimitBikeCamFoV(_cfg.Camera.ThirdPersonFoV.Value, CameraView.ThirdPerson);
      _firstPersonFoV = LimitBikeCamFoV(_cfg.Camera.FirstPersonFoV.Value, CameraView.FirstPerson);
   }


   public CameraView CurrentCamView
   {
      get { return _currentCamView; }
   }


   public bool AutoAlign
   {
      get { return _camAutoAlign; }
   }


   public float IsometricFoV
   {
      get { return _isometricFoV; }
   }


   public float ThirdPersonFoV
   {
      get { return _thirdPersonFoV; }
   }


   public float FirstPersonFoV
   {
      get { return _firstPersonFoV; }
   }


   public CameraMode Mode
   {
      get { return _state.CameraMode; }
      private set { _state.CameraMode = value; }
   }


   public bool InitializeOnce()
   {
      if (_initialized)
      {
         return true;
      }

      if (!GatherCameraRelatedGameObjects())
      {
         return false;
      }

      _logger.LogInfo("Starting alternative camera system ...");

      // Get intial cam position
      _logger.LogInfo("MainCam, FoV {0}, NCP {1}", _mainCamera.fieldOfView, _mainCamera.nearClipPlane);

      _currentCamView = _cfg.Camera.InitialMode.Value;
      _altCamStateView = _cfg.Camera.InitialMode.Value;

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
         _depthOfFieldSettings.focusDistance.Override((float)Math.Round(_depthOfFieldSettings.focusDistance.GetValue<float>())); // round it initially
         _baseFocusDistance = _depthOfFieldSettings.focusDistance.GetValue<float>();
         _depthOfFieldSettings.focalLength.Override((float)Math.Round(_depthOfFieldSettings.focalLength.GetValue<float>())); // round it initially
         _baseFocalLength = _depthOfFieldSettings.focalLength.GetValue<float>();
         _logger.LogInfo("DoF: " + _baseFocalLength);
         _appliedFocalLength = _cfg.Camera.FocalLength.Value;
      }

      _baseFoV = _mainCamera.fieldOfView;

      _initialized = true;
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

      var audio = bikeClone.transform.Find("Audio");
      if (audio != null)
      {
         _bikeSound = audio.GetComponent<BikeSound>();
      }

      _bikeParentTransform = bikeClone.GetComponent<Transform>();

      var target = GameObject.Find(_targetName);
      if (target == null)
      {
         return false;
      }

      _bikeTransform = target.GetComponent<Transform>();

      var cam = GameObject.Find("PlayCamera(Clone)");
      if (cam == null)
      {
         return false;
      }

      _camTransform = cam.GetComponent<Transform>();
      _mainCamera = _camTransform.gameObject.GetComponent<Camera>();
      _defaultPlayCamera = _camTransform.gameObject.GetComponent<PlayCamera>();
      
      var postProcessingObject = _camTransform.Find("DefaultPostProcessing").gameObject;

      _depthOfFieldSettings = postProcessingObject?.GetComponent<PostProcessVolume>()?.sharedProfile?.GetSetting<DepthOfField>();
      _hasDepthOfFieldSetting = _depthOfFieldSettings != null;
      
      return true;
   }


   /// <summary>
   /// Handles the processing of the position and rotation of the camera.
   /// </summary>
   public void ProcessBikeMode()
   {
      if (_currentCamView == CameraView.Original)
      {
         return;
      }

      if (_bikeTransform == null)
      {
         return;
      }

      _dirToCam = _camTransform.position - _bikeTransform.TransformPoint(_targetOffset);

      DisableDefaultCamera();
      _input.HideMouseCursor();

      bool zoomIn = _input.PlayMode.ZoomIn();
      bool zoomOut = _input.PlayMode.ZoomOut();

      if (zoomIn)
      {
         // Scrolling forward; zoom in
         if (_input.PlayMode.AdjustFocalLength() && _appliedFocalLength > 0)
         {
            _appliedFocalLength--;
            _logger.LogDebug("FocalLength " + _appliedFocalLength);
         }
         else if (_input.PlayMode.AdjustFocusDistance())
         {
            _cfg.Camera.FocusDistanceOffset.Value++;
            _logger.LogDebug("FocusDistanceOffset " + _cfg.Camera.FocusDistanceOffset.Value);
         }
         else
         {
            _appliedZoom -= _cfg.Camera.ZoomStepIncrement.Value;
         }
      }
      else if (zoomOut)
      {
         // Scrolling backwards; zoom out
         if (_input.PlayMode.AdjustFocalLength())
         {
            _appliedFocalLength++;
            _logger.LogDebug("FocalLength " + _appliedFocalLength);
         }
         else if (_input.PlayMode.AdjustFocusDistance() && _cfg.Camera.FocusDistanceOffset.Value > 0)
         {
            _cfg.Camera.FocusDistanceOffset.Value--;
            _logger.LogDebug("FocusDistanceOffset " + _cfg.Camera.FocusDistanceOffset.Value);
         }
         else
         {
            _appliedZoom += _cfg.Camera.ZoomStepIncrement.Value;
         }
      }

      if (_appliedZoom < 0.0f)
      {
         _appliedZoom = 0.0f;
      }

      // Horizontal mouse movement will make camera rotate around vertical y-axis
      // Vertical mouse movement will make camera rotate along x-axis (your ear-to-ear axis)
      _rotationHorizontal += _input.MouseHorizontal();
      _rotationVertical += _input.MouseVertical();

      // Also apply controller input
      _rotationHorizontal += _input.RightStickHorizontal();
      _rotationVertical -= _input.RightStickVertical();
      _rotationVertical = ClampAngle(_rotationVertical,
         _verticalClampAnglesMap[_currentCamView].Item1,
         _verticalClampAnglesMap[_currentCamView].Item2); // Clamp the up-down rotation to reasonable range


      // Handle alignment, either auto or manual by mouse/stick
      bool autoAligningIsActive = (_camAutoAlign && !_input.PlayMode.InvertAlignmentMode()) ||
                                  (!_camAutoAlign && _input.PlayMode.InvertAlignmentMode());

      if (autoAligningIsActive)
      {
         // auto align camera behind the bike
         if (_cfg.Mouse.InvertHorizontalLook.Value)
         {
            // Lerp the horizontal rotation relative to the player
            _rotationHorizontal = Mathf.LerpAngle(_rotationHorizontal,
               -_bikeParentTransform.localRotation.eulerAngles.y,
               _cfg.Camera.AutoAlignSpeed.Value * Time.deltaTime);
            _rotationHorizontal = ClampAngle(_rotationHorizontal, -360, 360);
            _rotation = Quaternion.Euler(-_rotationVertical, -_rotationHorizontal, 0f);
         }
         else
         {
            _rotationHorizontal = Mathf.LerpAngle(_rotationHorizontal,
               _bikeParentTransform.localRotation.eulerAngles.y,
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
      if (Physics.Raycast(_bikeTransform.TransformPoint(_targetOffset),
             _dirToCam.normalized,
             out var hitInfo,
             _appliedZoom + 0.2f,
             __cameraCollisionLayers))
      {
         _projectedDistance = Vector3.Distance(hitInfo.point, _bikeTransform.TransformPoint(_targetOffset));
      }
      else
      {
         _projectedDistance = 900;
      }

      if (_projectedDistance < _appliedZoom)
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
         _targetZoomAmount = Mathf.Lerp(_targetZoomAmount,
            _appliedZoom,
            Time.deltaTime * _cfg.Camera.ZoomLerpOutSpeed.Value);
      }

      if (_targetZoomAmount < 0.0f)
      {
         _targetZoomAmount = 0.0f;
      }

      Vector3 finalPosition = _rotation * new Vector3(0f, 0f, -_targetZoomAmount) +
                              _bikeTransform.TransformPoint(_targetOffset);

      // Apply values
      _camTransform.position = finalPosition;
      _camTransform.rotation = _rotation;

      // Adjust DoF
      if (_hasDepthOfFieldSetting)
      {
         _depthOfFieldSettings.focusDistance.Override(_cfg.Camera.FocusDistanceOffset.Value +
                                                     Vector3.Distance(_camTransform.position,
                                                        _bikeTransform.position));
         _depthOfFieldSettings.focalLength.Override(_appliedFocalLength);
      }
   }


   public void ProcessPhotoMode()
   {
      bool holdingSprint = _input.PhotoMode.AccelerateMovement();

      if (_input.PhotoMode.ZoomIn())
      {
         switch (_focusAdjustMode)
         {
            case CameraFocusAdjustMode.DepthOfField:
               _depthOfFieldSettings.focusDistance.Override(
                  Math.Min(18f, _depthOfFieldSettings.focusDistance.GetValue<float>() + (holdingSprint ? 1f : 0.5f)));
               break;
            case CameraFocusAdjustMode.FieldOfView:
               _mainCamera.fieldOfView = LimitPhotoCamFoV(_mainCamera.fieldOfView - (holdingSprint ? 10 : 5));
               break;
         }
      }
      else if (_input.PhotoMode.ZoomOut())
      {
         switch (_focusAdjustMode)
         {
            case CameraFocusAdjustMode.DepthOfField:
               _depthOfFieldSettings.focusDistance.Override(
                  Math.Max(0f, _depthOfFieldSettings.focusDistance.GetValue<float>() - (holdingSprint ? 1f : 0.5f)));
               break;
            case CameraFocusAdjustMode.FieldOfView:
               _mainCamera.fieldOfView = LimitPhotoCamFoV(_mainCamera.fieldOfView + (holdingSprint ? 10 : 5));
               break;
         }
      }

      // Moving the camera
      Vector3 moveVector = TranslateInputToCameraMovement() * _cfg.PhotoMode.CameraMovementSpeed.Value *
                           Time.fixedDeltaTime;

      // Speed up movement when shift key held
      if (holdingSprint)
      {
         moveVector *= _cfg.PhotoMode.CameraMovementSpeedMultiplier.Value;
      }

      // Horizontal movement will make camera rotate around vertical y-axis
      // Vertical movement will make camera rotate along x-axis (your ear-to-ear axis)

      // Mouse
      _rotationHorizontal += _input.MouseHorizontal() * (_cfg.Mouse.InvertHorizontalLook.Value ? -1 : 1);
      _rotationVertical += _input.MouseVertical() * (_cfg.Mouse.InvertVerticalLook.Value ? -1 : 1);
      // Controller
      _rotationHorizontal += _input.RightStickHorizontal() * (_cfg.Controller.InvertHorizontalLook.Value ? -1 : 1);
      _rotationVertical += _input.RightStickVertical() * (_cfg.Controller.InvertVerticalLook.Value ? 1 : -1);

      // Clamp the up-down rotation
      _rotationVertical = ClampAngle(_rotationVertical, -360, 360);

      if (_input.PhotoMode.RollLeft())
      {
         _rotationRoll += 0.1f;
         if (holdingSprint)
         {
            _rotationRoll += 0.7f;
         }
      }

      if (_input.PhotoMode.RollRight())
      {
         _rotationRoll -= 0.1f;
         if (holdingSprint)
         {
            _rotationRoll -= 0.7f;
         }
      }

      _rotationRoll += -_input.DpadHorizontal() * 4f * Time.fixedDeltaTime * (holdingSprint ? 7 : 1);

      _rotation = Quaternion.Euler(-_rotationVertical, _rotationHorizontal, _rotationRoll);
      Vector3 newPosition = _camTransform.position + moveVector;

      // Apply values
      _camTransform.position = newPosition;
      _camTransform.rotation = _rotation;

      ClearScreenshotInfoOnCamMove();
   }


   private void ClearScreenshotInfoOnCamMove()
   {
      var comparePos = new CamPos();
      SaveCamPos(comparePos);
      if (!comparePos.Matches(_camPosPhotoShoot))
      {
         _state.LastScreenshotInfo = null;
      }
   }


   private float LimitBikeCamFoV(float preset, CameraView cm)
   {
      return Math.Max(_fovLimitMap[cm].Item1, Math.Min(_fovLimitMap[cm].Item2, preset));
   }


   private float LimitPhotoCamFoV(float preset)
   {
      // 150 is a sensible limit
      return Math.Max(10, Math.Min(150, preset));
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


   private Vector3 TranslateInputToCameraMovement()
   {
      Vector3 direction = new Vector3();

      if (_input.PhotoMode.MoveUp())
      {
         direction += Vector3.up;
      }

      if (_input.PhotoMode.MoveDown())
      {
         direction += Vector3.down;
      }

      direction += Vector3.up * _input.RightTrigger();
      direction += Vector3.down * _input.LeftTrigger();

      direction += _camTransform.forward * _input.VerticalMovement();
      direction += _camTransform.right * _input.HorizontalMovement();

      return direction;
   }


   public void OnMenuOpen()
   {
      // and optionally show the default camera
      if (_cfg.Camera.DefaultCameraOnPause.Value)
      {
         ResetToDefaultCamera();
      }
   }


   public void ResetToDefaultCamera()
   {
      // Adjust DoF
      if (_hasDepthOfFieldSetting)
      {
         _depthOfFieldSettings.focalLength.Override(_baseFocalLength);
      }

      _mainCamera.fieldOfView = DefaultIsometricFoV;
      EnableDefaultCamera();
   }


   public void ToggleAutoAlignment()
   {
      _camAutoAlign = !_camAutoAlign;
      if (_camAutoAlign)
      {
         SnapBehindBike();
      }
   }


   public void ToggleCamState()
   {
      _logger.LogDebug("Toggle Cam State");
      if (_currentCamView == CameraView.Original)
      {
         ApplyCameraMode(_altCamStateView);
      }
      else
      {
         ApplyCameraMode(CameraView.Original);
      }
   }


   public void SnapBehindBike()
   {
      _rotationVertical = _cfg.Camera.ThirdPersionRotationVert.Value;
      AlignViewWithBike();
   }


   public void SelectCameraMode(CameraView camView)
   {
      if (GatherCameraRelatedGameObjects())
      {
         switch (camView)
         {
            case CameraView.ThirdPerson:
               SelectThirdPersonCam();
               break;
            case CameraView.FirstPerson:
               SelectFirstPersonCam();
               break;
            case CameraView.Original:
               SelectOriginalCam();
               break;
         }
      }
   }


   public void ChangeFieldOfView(int fovIncr)
   {
      switch (_currentCamView)
      {
         case CameraView.Original:
            if (fovIncr != 0)
            {
               _isometricFoV = LimitBikeCamFoV(_isometricFoV + fovIncr, CameraView.Original);
            }
            else
            {
               _isometricFoV = DefaultIsometricFoV;
            }

            ApplyIsometricCameraSettings();
            break;
         case CameraView.ThirdPerson:
            if (fovIncr != 0)
            {
               _thirdPersonFoV = LimitBikeCamFoV(_thirdPersonFoV + fovIncr, CameraView.ThirdPerson);
            }
            else
            {
               _thirdPersonFoV = LimitBikeCamFoV(_cfg.Camera.ThirdPersonFoV.Value, CameraView.ThirdPerson);
            }

            ApplyThirdPersonCam();
            break;
         case CameraView.FirstPerson:
            if (fovIncr != 0)
            {
               _firstPersonFoV = LimitBikeCamFoV(_firstPersonFoV + fovIncr, CameraView.FirstPerson);
            }
            else
            {
               _firstPersonFoV = LimitBikeCamFoV(_cfg.Camera.FirstPersonFoV.Value, CameraView.FirstPerson);
            }

            ApplyFirstPersonCam();
            break;
      }
   }


   /// <summary>
   /// Makes the camera move to directly behind the player.
   /// Useful for restarting at checkpoints.
   /// </summary>
   public void AlignViewWithBike()
   {
      if (_bikeParentTransform.gameObject == null)
      {
         return;
      }

      Vector3 bikeRotation = _bikeParentTransform.localRotation.eulerAngles;
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
   /// Toggles photo mode to the provided state.
   /// </summary>
   public void TogglePhotoMode(Hud hud, bool activate, bool applyPreviousCamPos = false)
   {
      if (activate)
      {
         _state.OnPhotoModeEnter();
         if (_bikeSound != null)
         {
            // avoid annoying repetitive sound sample which plays while bike is frozen
            _bikeSound.enabled = false;
         }

         SaveCamPos(_camPosBike);

         Mode = CameraMode.PhotoCam;
         _photoModeHudRestoreState = hud.IsHudVisible();
         if (_cfg.PhotoMode.AutoHideHud.Value)
         {
            hud.ToggleHudVisiblity(false);
         }

         _logger.LogInfo("Enter photo mode");
         _photoModeBaseTimeScale = Time.timeScale; // Save the original time scale before freezing
         Time.timeScale = 0;

         GatherCameraRelatedGameObjects();

         // Save the original FoV and DoF focus distance
         _baseFoVPhotoMode = _mainCamera.fieldOfView;
         _baseFocusDistancePhotoMode = _depthOfFieldSettings.focusDistance.GetValue<float>();

         if (applyPreviousCamPos)
         {
            ApplyCamPos(_camPosPhoto);
         }
         else
         {
            // use current state
            _rotationHorizontal = _camTransform.eulerAngles.y;
            _rotationVertical = -_camTransform.eulerAngles.x;
            _rotationRoll = 0;
         }

         DisableDefaultCamera();
         _input.HideMouseCursor();
      }
      else
      {
         SaveCamPos(_camPosPhoto);

         Mode = CameraMode.BikeCam;
         _logger.LogInfo("Exit photo mode");

         Time.timeScale = _photoModeBaseTimeScale; // Reset the time scale to what it was before we froze the time
         _mainCamera.fieldOfView = _baseFoVPhotoMode; // Restore the original FoV
         _depthOfFieldSettings.focusDistance.Override(_baseFocusDistancePhotoMode); // Restore the original focus distance for DoF

         ApplyCameraMode(_currentCamView);
         ApplyCamPos(_camPosBike);

         hud.ToggleHudVisiblity(_photoModeHudRestoreState);
         _state.OnPhotoModeExit();
         if (_bikeSound != null)
         {
            _bikeSound.enabled = true;
         }
      }

      Thread.Sleep(200);
   }


   private void DisableDefaultCamera()
   {
      _defaultPlayCamera.enabled = false;
   }


   private void EnableDefaultCamera()
   {
      _defaultPlayCamera.enabled = true;
   }


   public void ApplyCameraModeOnEnterPlay()
   {
      if (Mode == CameraMode.BikeCam)
      {
         SelectCameraMode(_currentCamView);
      }
   }


   public void ResetCamera()
   {
      if (Mode == CameraMode.BikeCam)
      {
         _depthOfFieldSettings.focusDistance.Override(_baseFocusDistance);
         _mainCamera.fieldOfView = _baseFoV;
         _rotationRoll = 0;
      }
      else if (Mode == CameraMode.PhotoCam)
      {
         _depthOfFieldSettings.focusDistance.Override(_baseFocusDistancePhotoMode);
         _mainCamera.fieldOfView = _baseFoVPhotoMode;
         _rotationRoll = 0;
      }
   }


   public void ToggleFocusAdjustMode()
   {
      if (_hasDepthOfFieldSetting)
      {
         switch (_focusAdjustMode)
         {
            case CameraFocusAdjustMode.DepthOfField:
               _focusAdjustMode = CameraFocusAdjustMode.FieldOfView;
               break;
            case CameraFocusAdjustMode.FieldOfView:
               _focusAdjustMode = CameraFocusAdjustMode.DepthOfField;
               break;
         }

         _logger.LogDebug("Focus Adjust Mode: " + _focusAdjustMode);
      }
   }


   public IEnumerator TakeScreenshot()
   {
      SaveCamPos(_camPosPhotoShoot);
      yield return new WaitForEndOfFrame();

      try
      {
         Camera gameCam = _mainCamera;
         var hudCam = Hud.GetHudCam();

         int captureWidth = UnityEngine.Screen.width;
         int captureHeight = UnityEngine.Screen.height;

         var currentRt = RenderTexture.active;
         // var currentGameTt = gameCam.targetTexture;
         // var currentHudTt = hudCam.targetTexture;

         // creates off-screen render texture that can rendered into

         gameCam.targetTexture = new RenderTexture(captureWidth, captureHeight, 24);
         RenderTexture.active = gameCam.targetTexture;
         gameCam.Render();
         if (hudCam != null && hudCam.enabled)
         {
            hudCam.targetTexture = gameCam.targetTexture;
            hudCam.Render();
         }

         Texture2D imageOverview = new Texture2D(gameCam.targetTexture.width,
            gameCam.targetTexture.height,
            TextureFormat.RGB24,
            false);
         imageOverview.ReadPixels(new Rect(0, 0, gameCam.targetTexture.width, gameCam.targetTexture.height), 0, 0);
         imageOverview.Apply();

         RenderTexture.active = currentRt;
         gameCam.targetTexture = null;
         if (hudCam != null)
         {
            hudCam.targetTexture = null;
         }

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
               newPath = Path.Combine(Path.GetDirectoryName(path),
                  Path.GetFileNameWithoutExtension(path) + sepChar + cnt.ToString("D3") + Path.GetExtension(path));
            }
            while (File.Exists(newPath));

            path = newPath;
         }

         File.WriteAllBytes(path, bytes);
         ScreenshotResult = new ScreenshotResult(path, null);

      }
      catch (Exception ex)
      {
         ScreenshotResult = new ScreenshotResult(null, ex.Message);
         _logger.LogError("Screenshot save error: {0}", ex.Message);
      }
   }


   public CameraFocusAdjustMode FocusAdjustMode
   {
      get { return _focusAdjustMode; }
   }


   public ScreenshotResult ScreenshotResult { get; private set; }


   public float DepthOfField
   {
      get { return (float)Math.Round(_depthOfFieldSettings.focusDistance.GetValue<float>(), 1); }
   }
   
   
   public int FieldOfView
   {
      get { return (int)_mainCamera.fieldOfView; }
   }

   
   private void ApplyCameraMode(CameraView camView)
   {
      if (GatherCameraRelatedGameObjects())
      {
         switch (camView)
         {
            case CameraView.ThirdPerson:
               ApplyThirdPersonCam();
               break;
            case CameraView.FirstPerson:
               ApplyFirstPersonCam();
               break;
            case CameraView.Original:
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
      _logger.LogInfo("Original camera");
      _currentCamView = CameraView.Original;
      EnableDefaultCamera();
      ApplyIsometricCameraSettings();
   }


   private void SelectFirstPersonCam()
   {
      _altCamStateView = CameraView.FirstPerson;
      ApplyFirstPersonCam();
   }


   private void ApplyFirstPersonCam()
   {
      _currentCamView = CameraView.FirstPerson;
      ApplyCameraSettings(0f, new Vector3(0.0f, 0.3f, 0.0f), _firstPersonFoV, 0.6f, "neck_BindJNT");
      // Navigate to bike mesh renderer to prevent it from vanishing in first person
      SkinnedMeshRenderer bikeMeshRenderer = _bikeParentTransform.GetChild(7).transform.GetChild(1)
         .gameObject.GetComponent<SkinnedMeshRenderer>();
      bikeMeshRenderer.updateWhenOffscreen = true;
      ApplyCommonCameraSettings();
      _logger.LogInfo("First person camera");
      AlignViewWithBike();
   }


   private void SelectThirdPersonCam()
   {
      _altCamStateView = CameraView.ThirdPerson;
      ApplyThirdPersonCam();
   }


   private void ApplyThirdPersonCam()
   {
      _logger.LogInfo("Third person camera");
      _currentCamView = CameraView.ThirdPerson;
      ApplyCameraSettings(_cfg.Camera.ThirdPersionInitialFollowDistance.Value,
         new Vector3(0f, 2.4f, 0f),
         _thirdPersonFoV,
         0.28f,
         "Bike(Clone)");
      ApplyCommonCameraSettings();
      AlignViewWithBike();
   }


   private void ApplyCommonCameraSettings()
   {
      // Adjust DoF
      if (_hasDepthOfFieldSetting)
      {
         _depthOfFieldSettings.focalLength.Override(_cfg.Camera.FocalLength.Value);
      }

      DisableDefaultCamera();
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
         _depthOfFieldSettings.focalLength.Override(_baseFocalLength);
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
      _bikeTransform = GameObject.Find(_targetName).GetComponent<Transform>();

      _appliedZoom = followDistance;
      _targetOffset = followTargetOffset;

      _mainCamera.fieldOfView = cameraFov; // Default: 34
      _mainCamera.nearClipPlane = nearClipPlane; // Default: 0.3
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


   private bool EqualsZero(float floatVal)
   {
      return Math.Abs(floatVal) < 1E-12;
   }


   private string BuildFileName(int width, int height)
   {
      string fmt = _cfg.PhotoMode.ScreenshotFilenameFormat.Value;
      if (string.IsNullOrEmpty(fmt))
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
}
