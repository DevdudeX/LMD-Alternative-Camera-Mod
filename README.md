# DevdudeX's Alternative Camera for Lonely Mountains: Downhill
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L5S9BK3)

This is a mod made for Melon Loader that replaces LMD's default distant "isometric-like" camera with a third person system.  

![Camera preview](/images/banner.png?raw=true)

#### Mod Compatibility Warning
If you are already using the [Photo Mode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod) mod please disable it while using this mod.  
You can do this by simply renaming the file in your mods folder before launching.  
`PhotoMode.dll` --> `PhotoMode.dll.disabled`

## Setup Instructions
#### Preparing
Your game folder can be found by right-clicking on the game in steam and going 'Manage -> Browse local files'  

**This mod is broken on ML v0.6.2+ at the time of writing!**  

Install Melon Loader **v0.6.1** to your LMD game install folder.  
Look under 'Automated Installation':  
https://melonwiki.xyz/#/  


If successful the Melon Loader splash screen should appear on launch.  
You may need to run the game once without the mod then exit.  
(See *Known Issues & Fixes* if your game freezes on quit)  

Download `AlternativeCameraMod.dll` from the releases and add it to the `Mods` folder in your LMD game folder.   

#### Loading The Mod In-Game
After loading into a level wait to be able to move the bike then hit `Keyboard [9]`.  
A few camera presets exist and can be activated as seen in the keybind section.  
##### Keyboard users:
- The game works best by setting mouse to `Steering Only` in options and using the keyboard for forward and brake.  
- It is recommended you turn off camera `auto-align`.  

#### Tweaking values
A config file is generated in `[LMD folder]/UserData/AlternativeCameraSettings.cfg`.  
This file can be edited with any text editor and contains all the mods settings.  


#### Keybinds
| Action                                  | Gamepad                   | Keyboard & Mouse      |
| ---                                     | ---                       | ---                   |
| Look around                             | Right Stick               | Mouse                 |
| Hold to invert camera auto-align mode   | Right Bumper              | Right Click           |
| Toggle the camera mod                   | -                         | 0                     |
| Toggle HUD & UI rendering               | -                         | H                     |
| Snap camera to behind the player        | -                         | Enter / Space         |
| Move camera in and out                  | -                         | Mouse Scroll          |
| Change DoF focal length                 | -                         | Hold L + Mouse Scroll |
| Change DoF focus distance offset        | -                         | Hold K + Mouse Scroll |
| Recommended camera preset               | -                         | Keypad 1              |
| First person ( *Experimental* )         | -                         | Keypad 2              |
| Toggle invert look horizontal           | -                         | Keypad 3              |
| Toggle camera auto-align mode           | -                         | Keypad 4              |
| Find and update gameobjects references  | -                         | Keypad 9              |



#### Known Issues & Fixes
- Controls are currently not rebindable.  
- Movement isn't working: Go to the last checkpoint / restart run.  
- Camera doesn't work: Try pressing `Keyboard [9]`, `[Keypad 9]`, and `Keyboard [0]` on your keyboard.  
- Game freezes on quitting: Add the `--quitfix` [MelonLoader launch option](https://github.com/LavaGang/MelonLoader#launch-options).  
On Steam: right-click on LMD ► Properties ► Launch Options ► Paste the command (with `--` infront!).
