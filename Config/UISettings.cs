using AlternativeCameraMod.Language;
using Il2CppSystem.Text;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class UISettings : ModSettingsCategory
{
   private MelonPreferences_Entry<bool> _gameHudVisible;
   private MelonPreferences_Entry<string> _modHudInfoDisplay;


   public UISettings(string filePath, LanguageConfig lng) 
      : base("UI", filePath, lng)
   {
      _gameHudVisible = CreateEntry("GameHudVisible", true,
         "Initial visibility of the game hud with timer and track info");
      _modHudInfoDisplay = CreateEntry("ModHudInfoDisplay", 
         String.Join(",", ModHudInfoPart.CamMode, ModHudInfoPart.CamAlign, ModHudInfoPart.FoV, ModHudInfoPart.FPS),
         "Tells which content to present in the info text at the left bottom location\n" +
         "CamMode, CamAlign, FoV, FPS; enter comma-separated in desired order or leave empty to show nothing");
   }

   
   public MelonPreferences_Entry<bool> GameHudVisible
   {
      get { return _gameHudVisible; }
   }


   public MelonPreferences_Entry<string> ModHudInfoDisplay
   {
      get { return _modHudInfoDisplay; }
   }
}
