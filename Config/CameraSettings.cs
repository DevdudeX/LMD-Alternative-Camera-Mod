using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class CameraSettings : ModSettingsCategory
{
   private MelonPreferences_Entry<CameraMode> _initialMode;
   private MelonPreferences_Entry<float> _collisionPadding;
   private MelonPreferences_Entry<float> _zoomLerpOutSpeed;
   private MelonPreferences_Entry<float> _zoomLerpInSpeed;
   private MelonPreferences_Entry<bool> _defaultCameraOnPause;
   private MelonPreferences_Entry<CameraAlignmentMode> _alignmentMode;
   private MelonPreferences_Entry<CameraManualAlignmentInput> _manualAlignmentInput;
   private MelonPreferences_Entry<float> _autoAlignSpeed;
   private MelonPreferences_Entry<float> _zoomStepIncrement;
   private MelonPreferences_Entry<float> _thirdPersonFoV;
   private MelonPreferences_Entry<float> _firstPersonFoV;
   private MelonPreferences_Entry<float> _focalLength;
   private MelonPreferences_Entry<float> _focusDistanceOffset;
   private MelonPreferences_Entry<float> _thirdPersionRotationVert;
   private MelonPreferences_Entry<float> _thirdPersionInitialFollowDistance;


   public CameraSettings(string filePath, LanguageConfig lng) 
      : base("Camera", filePath, lng)
   {
      _initialMode = CreateEntry("InitialMode", CameraMode.ThirdPerson, 
         "Camera mode to begin with: Original, ThirdPerson, FirstPerson");

      _alignmentMode = CreateEntry("AlignMode", CameraAlignmentMode.Auto, 
         "* Auto  : camera follows the bike automatically\n" +
         "* Manual: camera can be aligned respective to the manual align mode");

      _manualAlignmentInput = CreateEntry("ManualAlign", CameraManualAlignmentInput.MouseOrRStick, 
         "* KeyOrButton  : pressing the configured key or button aligns the camera behind the bike for the moment\n" +
         "* MouseOrRStick: align the camera freely around the bike");

      _autoAlignSpeed = CreateEntry("AutoAlignSpeed", 1.80f, 
         "How quickly the camera moves behind the bike");

      _zoomLerpOutSpeed = CreateEntry("ZoomOutLerpSpeed", 1.0f);

      _zoomLerpInSpeed = CreateEntry("ZoomInLerpSpeed", 0.0880f);

      _zoomStepIncrement = CreateEntry("ZoomStepIncrement", 0.20f, 
         "How much one scroll/dpad zooms the camera");

      _collisionPadding = CreateEntry("CollisionPadding", 0.20f, 
         "Distance the camera is pushed away from terrain");

      _defaultCameraOnPause = CreateEntry("DefaultCameraOnPause", true,
         "* true : when pausing the game the camera switches to original view\n" +
         "* false: stays in the selected camera mode (false)");

      _thirdPersonFoV = CreateEntry("ThirdPersonFoV", 70f,
         "Initial field of view when activating third person cam");

      _firstPersonFoV = CreateEntry("FirstPersonFoV", 100f,
         "Initial field of view when activating first person cam");

      _focalLength = CreateEntry("FocalLength", 66f);

      _focusDistanceOffset = CreateEntry("FocusDistanceOffset", 7f);

      _thirdPersionRotationVert = CreateEntry("ThirdPersonCamPosVertical", -20f, 
         "Default vertical position of the camera above behind the bike");

      _thirdPersionInitialFollowDistance = CreateEntry("ThirdPersonInitialFollowDistance", 6f,
         "Initial distance of the camera to the bike when using third person cam\n" +
         "Can be adjusted while playing by zoom functionality");
   }

   
   public MelonPreferences_Entry<CameraMode> InitialMode
   {
      get { return _initialMode; }
   }

   
   public MelonPreferences_Entry<float> CollisionPadding
   {
      get { return _collisionPadding; }
   }


   public MelonPreferences_Entry<float> ZoomLerpOutSpeed
   {
      get { return _zoomLerpOutSpeed; }
   }


   public MelonPreferences_Entry<float> ZoomLerpInSpeed
   {
      get { return _zoomLerpInSpeed; }
   }


   public MelonPreferences_Entry<bool> DefaultCameraOnPause
   {
      get { return _defaultCameraOnPause; }
   }


   public MelonPreferences_Entry<CameraAlignmentMode> AlignmentMode
   {
      get { return _alignmentMode; }
   }


   public MelonPreferences_Entry<CameraManualAlignmentInput> ManualAlignmentInput
   {
      get { return _manualAlignmentInput; }
   }


   public MelonPreferences_Entry<float> AutoAlignSpeed
   {
      get { return _autoAlignSpeed; }
   }


   public MelonPreferences_Entry<float> ZoomStepIncrement
   {
      get { return _zoomStepIncrement; }
   }


   public MelonPreferences_Entry<float> ThirdPersonFoV
   {
      get { return _thirdPersonFoV; }
   }


   public MelonPreferences_Entry<float> FirstPersonFoV
   {
      get { return _firstPersonFoV; }
   }


   public MelonPreferences_Entry<float> FocalLength
   {
      get { return _focalLength; }
   }


   public MelonPreferences_Entry<float> FocusDistanceOffset
   {
      get { return _focusDistanceOffset; }
   }

   
   public MelonPreferences_Entry<float> ThirdPersionRotationVert
   {
      get { return _thirdPersionRotationVert; }
   }


   public MelonPreferences_Entry<float> ThirdPersionInitialFollowDistance
   {
      get { return _thirdPersionInitialFollowDistance; }
   }
}

