# Alternative Camera for Lonely Mountains: Downhill
This is a mod made for Melon Loader that replaces LMD's default distant "isometric-like" camera with a third person system.
 

## Setup Instructions
#### Preparing
Install MelonLoader to your LMD game folder.  
Look under 'Automated Installation':
https://melonwiki.xyz/#/  
(v0.6.1 is the current version at time of writing)  

Add 'AlternativeCameraMod.dll' to the Mods folder.  
If successful the Melon Loader loading screen should appear on launch.  


#### Loading The Mod In-Game
After loading into a level wait to be able to move the bike then hit [9] on your keyboard.

The game works best by setting mouse to steering only in options and using the keyboard for forward and brake.  
For use with a controller activate camera auto align.  
A few camera presets exist and can be activated as seen in the keybind section.  


#### Tweaking values
A config file is generated in '[LMD folder]/UserData/AlternativeCameraSettings.cfg'.  
This file can be edited with any text editor and contains all the mods settings.  


#### Keybinds
- [9] Start the camera  
- [0] Toggle the camera mod (might be buggy)  
- [Enter, Space] Reset camera rotation to behind the player  
- [Mouse Scroll] Zoom camera in and out  
- [Right Click, Right Bumper] Hold to invert camera auto-align  
- [Keypad 1] Recommended settings; 70 FoV and nice follow distance, works well for steering  
- [Keypad 3] First person (Experimental); 80 FoV and the camera is attached to the neck, character model might clip weirdly  
- [Keypad 3] Toggle invert mouse horizontal  
- [Keypad 4] Toggle camera auto-align (for use with a controller)  
- [Keypad 7] Toggle HUD rendering  
- [Keypad 9] Find and update gameobjects references  


#### Known Issues & Fixes
- Movement isn't working: Go to the last checkpoint / restart run.  
- Camera doesn't work: Try pressing [9], [Keypad 9], and [0] on your keyboard.  
- Game freezes on quitting: Use [Alt+Tab] to select the commandline window and then close it.
