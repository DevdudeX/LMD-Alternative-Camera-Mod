using AlternativeCameraMod.Language;
using MelonLoader;
using UnityEngine;


namespace AlternativeCameraMod.Config;

internal class KeyboardSettings : ModSettingsCategory
{
   private MelonPreferences_Entry<KeyCode> _originalCamKey;
   private MelonPreferences_Entry<KeyCode> _thirdPersonCamKey;
   private MelonPreferences_Entry<KeyCode> _firstPersonCamKey;
   private MelonPreferences_Entry<KeyCode> _hudToggleKey;
   private MelonPreferences_Entry<KeyCode> _invertHorizontalLookKey;
   private MelonPreferences_Entry<KeyCode> _toggleCamAlignModeKey;
   private MelonPreferences_Entry<KeyCode> _snapAlignCamKey;
   private MelonPreferences_Entry<KeyCode> _invertCamAlignModeKey;
   private MelonPreferences_Entry<KeyCode> _hudInfoTextToggleKey;
   private MelonPreferences_Entry<KeyCode> _adjustFocalLengthKey;
   private MelonPreferences_Entry<KeyCode> _adjustFocusDistanceKey;
   private MelonPreferences_Entry<KeyCode> _toggleCamStateKey;
   private MelonPreferences_Entry<KeyCode> _fovIncreaseKey;
   private MelonPreferences_Entry<KeyCode> _fovDecreaseKey;
   private MelonPreferences_Entry<KeyCode> _fovResetKey;
   
   
   public KeyboardSettings(string filePath, LanguageConfig lng) 
      : base("Keyboard", filePath, lng)
   {
      _originalCamKey = CreateEntry("OriginalCam", KeyCode.F1,
         "Key to set camera to original camera work");
      _thirdPersonCamKey = CreateEntry("ThirdPersonCam", KeyCode.F2,
         "Key to set camera to third person view");
      _firstPersonCamKey = CreateEntry("FirstPersonCam", KeyCode.F3,
         "Key to set camera to first person view");
      _toggleCamStateKey = CreateEntry("ToggleCamState", KeyCode.Space, 
         "Key to toggle between the alternative camera mode and the original camera mode\n" + 
         "None to disable, otherwise any valid keybord key (see also Controller)");
      _toggleCamAlignModeKey = CreateEntry("ToggleCamAlignMode", KeyCode.F5,
         "Key to switch between Auto and Manual alignment, see Camera/CamAlignMode)");
      _snapAlignCamKey = CreateEntry("SnapAlignCam", KeyCode.RightControl,
         "Key to align the camera behind the bike on demand\n" + 
         "None to disable, otherwise any valid keyboard key (see also Controller)");
      _invertCamAlignModeKey = CreateEntry("InvertCamAlignMode", KeyCode.Mouse1,
         "Key to invert the alignment mode of the camera, so if this key is pressed\n" + 
         "a preset of auto alignment is toggled to manual alignment and a preset of\n" +
         "manual alignment is toggled to auto alignment");
      _invertHorizontalLookKey = CreateEntry("InvertHorizontalLook", KeyCode.F6, 
         "Key to change mouse horizontal look, see Mouse/InvertHorizontalLook");
      _hudToggleKey = CreateEntry("HudToggle", KeyCode.H,
         "Key to toggle the game hud with timer and track infos");
      _hudInfoTextToggleKey = CreateEntry("HudInfoTextToggle", KeyCode.J,
         "Key to toggle the info text about the cam state in the left bottom corner");
      _adjustFocalLengthKey = CreateEntry("AdjustFocalLength", KeyCode.L,
         "Hold this key to change focal length with Mouse Wheel Scroll");
      _adjustFocusDistanceKey = CreateEntry("AdjustFocusDistance", KeyCode.K,
         "Hold this key to change focus distance with Mouse Wheel Scroll");
      _fovIncreaseKey = CreateEntry("FieldOfViewIncrease", KeyCode.Alpha8,
         "Key to increase Field of View; FoV is set for each camera mode separately");
      _fovDecreaseKey = CreateEntry("FieldOfViewDecrease", KeyCode.Alpha9,
         "Key to decrease Field of View; FoV is set for each camera mode separately");
      _fovResetKey = CreateEntry("FieldOfViewReset", KeyCode.Alpha0,
         "Key to reset Field of View of the camera mode to the respective default");
      
   }


   public override string? Validate(LanguageConfig language)
   {
      if (!DoCheckKeybindingsUnique())
      {
         return language.GetText("Config/Keyboard/Validation", "ErrDuplicateKeyBinds", "Duplicate key bindings");
      }

      return base.Validate(language);
   }

   
   private bool DoCheckKeybindingsUnique()
   {
      HashSet<KeyCode> keys = new HashSet<KeyCode>();
      bool ok = keys.Add(_originalCamKey.Value);
      ok &= keys.Add(_thirdPersonCamKey.Value);
      ok &= keys.Add(_firstPersonCamKey.Value);
      ok &= keys.Add(_toggleCamAlignModeKey.Value);
      ok &= keys.Add(_snapAlignCamKey.Value);
      ok &= keys.Add(_invertHorizontalLookKey.Value);
      ok &= keys.Add(_hudToggleKey.Value);
      ok &= keys.Add(_hudInfoTextToggleKey.Value);
      ok &= keys.Add(_adjustFocalLengthKey.Value);
      ok &= keys.Add(_adjustFocusDistanceKey.Value);
      ok &= keys.Add(_fovIncreaseKey.Value);
      ok &= keys.Add(_fovDecreaseKey.Value);
      ok &= keys.Add(_fovResetKey.Value);
      ok &= keys.Add(_toggleCamStateKey.Value);
      return ok;
   }

   
   public MelonPreferences_Entry<KeyCode> OriginalCamKey
   {
      get { return _originalCamKey; }
   }


   public MelonPreferences_Entry<KeyCode> ThirdPersonCamKey
   {
      get { return _thirdPersonCamKey; }
   }


   public MelonPreferences_Entry<KeyCode> FirstPersonCamKey
   {
      get { return _firstPersonCamKey; }
   }


   public MelonPreferences_Entry<KeyCode> HudToggleKey
   {
      get { return _hudToggleKey; }
   }


   public MelonPreferences_Entry<KeyCode> InvertHorizontalLookKey
   {
      get { return _invertHorizontalLookKey; }
   }


   public MelonPreferences_Entry<KeyCode> ToggleCamAlignModeKey
   {
      get { return _toggleCamAlignModeKey; }
   }


   public MelonPreferences_Entry<KeyCode> SnapAlignCamKey
   {
      get { return _snapAlignCamKey; }
   }
   
   
   public MelonPreferences_Entry<KeyCode> InvertCamAlignModeKey
   {
      get { return _invertCamAlignModeKey; }
   }


   public MelonPreferences_Entry<KeyCode> HudInfoTextToggleKey
   {
      get { return _hudInfoTextToggleKey; }
   }


   public MelonPreferences_Entry<KeyCode> AdjustFocalLengthKey
   {
      get { return _adjustFocalLengthKey; }
   }


   public MelonPreferences_Entry<KeyCode> AdjustFocusDistanceKey
   {
      get { return _adjustFocusDistanceKey; }
   }


   public MelonPreferences_Entry<KeyCode> ToggleCamStateKey
   {
      get { return _toggleCamStateKey; }
   }


   public MelonPreferences_Entry<KeyCode> FovIncreaseKey
   {
      get { return _fovIncreaseKey; }
   }


   public MelonPreferences_Entry<KeyCode> FovDecreaseKey
   {
      get { return _fovDecreaseKey; }
   }


   public MelonPreferences_Entry<KeyCode> FovResetKey
   {
      get { return _fovResetKey; }
   }
}
