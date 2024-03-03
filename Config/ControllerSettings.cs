using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class ControllerSettings : ModSettingsCategory
{
   private MelonPreferences_Entry<float> _rightStickDeadzone;
   private MelonPreferences_Entry<float> _leftStickDeadzone;
   private MelonPreferences_Entry<float> _leftTriggerDeadzone;
   private MelonPreferences_Entry<float> _rightTriggerDeadzone;
   private MelonPreferences_Entry<float> _sensitivityHorizontal;
   private MelonPreferences_Entry<float> _sensitivityVertical;
   private MelonPreferences_Entry<float> _sensitivityMultiplier;
   private MelonPreferences_Entry<bool> _invertHorizontalLook;
   private MelonPreferences_Entry<bool> _invertVerticalLook;

   private MelonPreferences_Entry<ControllerButton> _toggleCamStateButton;
   private MelonPreferences_Entry<ControllerButton> _snapAlignCamButton;


   public ControllerSettings(string filePath, LanguageConfig lng) 
      : base("Controller", filePath, lng)
   {
      _rightStickDeadzone = CreateEntry("RStickDeadzone", 0.15f, 
         "Accepts Right Stick input only when its value is higher than this percentage of the range");
      _leftStickDeadzone = CreateEntry("LStickDeadzone", 0.15f, 
         "Accepts Left Stick input only when its value is higher than this percentage of the range");
      _leftTriggerDeadzone = CreateEntry("LTriggerDeadzone", 0.1f, 
         "Accepts Left Trigger input only when its value is higher than this percentage of the range");
      _rightTriggerDeadzone = CreateEntry("RTriggerDeadzone", 0.1f, 
         "Accepts Right Trigger input only when its value is higher than this percentage of the range");
      _sensitivityHorizontal = CreateEntry("HorizontalSensitivity", 1.0f, 
         "Accepts Left Stick input only when its value is higher than this percentage of the range");
      _sensitivityVertical = CreateEntry("VerticalSensitivity", 1.0f,
         "Stick vertical sensitivity, increase if stick movement is too slow, decrease if too fast");
      _sensitivityMultiplier = CreateEntry("SensitivityMultiplier", 1.0f,
         "Stick sensitivity multiplier, increase if stick acceleration is too slow, decrease if too fast");
      _invertHorizontalLook = CreateEntry("InvertHorizontalLook", false,
         "* false: stick left => look left, stick right => look right\n" +
         "* true : stick left => look right, stick right => look left");
      _invertVerticalLook = CreateEntry("InvertVerticalLook", false,
         "* false: stick up => look up, stick down => look down\n" +
         "* true : stick up => look down, stick down => look up");

      _toggleCamStateButton = CreateEntry("ToggleCamState", ControllerButton.LB,
            "Tells which button to use to toggle between the alternative camera mode and the original camera mode\n" + 
            "Controller: None (disable), X, Y, LB, RB, LStick, RStick");
      _snapAlignCamButton = CreateEntry("SnapAlignCam", ControllerButton.RStick,
            "Tells which button to use to align the camera behind the bike on demand\n" + 
            "Controller: None (disable), X, Y, LB, RB, LStick, RStick");
   }
   

   public override string? Validate(LanguageConfig language)
   {
      if (!DoCheckButtonBindingsUnique())
      {
         return language.GetText("Config/Controller/Validation", "ErrDuplicateButtonBinds", "Duplicate controller button assignment");
      }

      return base.Validate(language);
   }

   
   private bool DoCheckButtonBindingsUnique()
   {
      HashSet<ControllerButton> keys = new HashSet<ControllerButton>();
      bool ok = keys.Add(_snapAlignCamButton.Value);
      ok &= keys.Add(_toggleCamStateButton.Value);
      return ok;
   }


   public MelonPreferences_Entry<float> RightStickDeadzone
   {
      get { return _rightStickDeadzone; }
   }


   public MelonPreferences_Entry<float> LeftStickDeadzone
   {
      get { return _leftStickDeadzone; }
   }


   public MelonPreferences_Entry<float> LeftTriggerDeadzone
   {
      get { return _leftTriggerDeadzone; }
   }


   public MelonPreferences_Entry<float> RightTriggerDeadzone
   {
      get { return _rightTriggerDeadzone; }
   }


   public MelonPreferences_Entry<float> SensitivityHorizontal
   {
      get { return _sensitivityHorizontal; }
   }


   public MelonPreferences_Entry<float> SensitivityVertical
   {
      get { return _sensitivityVertical; }
   }


   public MelonPreferences_Entry<float> SensitivityMultiplier
   {
      get { return _sensitivityMultiplier; }
   }


   public MelonPreferences_Entry<bool> InvertHorizontalLook
   {
      get { return _invertHorizontalLook; }
   }


   public MelonPreferences_Entry<bool> InvertVerticalLook
   {
      get { return _invertVerticalLook; }
   }

   
   public MelonPreferences_Entry<ControllerButton> ToggleCamStateButton
   {
      get { return _toggleCamStateButton; }
   }


   public MelonPreferences_Entry<ControllerButton> SnapAlignCamButton
   {
      get { return _snapAlignCamButton; }
   }


}
