﻿using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class UISettings : ModSettingsCategory
{
   private readonly MelonPreferences_Entry<bool> _gameHudVisible;
   private readonly MelonPreferences_Entry<string> _modHudInfoDisplay;
   private readonly MelonPreferences_Entry<int> _modHudTextSize;
   private readonly MelonPreferences_Entry<bool> _showCamInstructionsInPauseMenu;


   public UISettings(string filePath, LanguageConfig lng) 
      : base("UI", filePath, lng)
   {
      _gameHudVisible = CreateEntry("GameHudVisible", true, "Initial visibility of the game hud with timer and track info");
      _modHudInfoDisplay = CreateEntry("ModHudInfoDisplay", 
         String.Join(",", ModHudInfoPart.CamMode, ModHudInfoPart.CamAlign, ModHudInfoPart.FoV, ModHudInfoPart.FPS),
         "Tells which content to present in the info text at the left bottom location\n" +
         "CamMode, CamAlign, FoV, FPS; enter comma-separated in desired order or leave empty to show nothing");
      _modHudTextSize = CreateEntry("ModHudTextSize", 20, "Size of the mod hud text");
      _showCamInstructionsInPauseMenu = CreateEntry("ShowCamInstructionsInPauseMenu", true, "Visibility of the camera instructions in pause menu");
   }

   
   public MelonPreferences_Entry<bool> GameHudVisible
   {
      get { return _gameHudVisible; }
   }


   public MelonPreferences_Entry<string> ModHudInfoDisplay
   {
      get { return _modHudInfoDisplay; }
   }


   public MelonPreferences_Entry<int> ModHudTextSize
   {
      get { return _modHudTextSize; }
   }


   public MelonPreferences_Entry<bool> ShowCamInstructionsInPauseMenu
   {
      get { return _showCamInstructionsInPauseMenu; }
   }
}
