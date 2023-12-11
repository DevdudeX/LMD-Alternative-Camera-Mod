# DevdudeX's Alternative Camera for Lonely Mountains: Downhill
This is a mod made for Melon Loader that replaces LMD's default distant "isometric-like" camera with a third person system.  

![Camera preview](/images/banner.png?raw=true)

#### Mod Compatibility Warning
If you are already using the [Photo Mode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod) mod please disable it while using this mod.  
You can do this by simply renaming the file in your mods folder before launching.  
`PhotoMode.dll` --> `PhotoMode.dll.disabled`

## Setup Instructions
#### Preparing
Your game folder can be found by right-clicking on the game in steam and going 'Manage -> Browse local files'  

Install Melon Loader to your LMD game install folder.  
Look under 'Automated Installation':  
https://melonwiki.xyz/#/  
(v0.6.1 is the current version at time of writing)  

Run the game once then exit. (See **Known Issues & Fixes** if your game freezes on quit)  
If successful the Melon Loader splash screen should appear on launch. 

Download `AlternativeCameraMod.dll` from the releases and add it to the `Mods` folder in your LMD game folder.   

#### Loading The Mod In-Game
After loading into a level wait to be able to move the bike then hit `Keyboard [9]`.  
A few camera presets exist and can be activated as seen in the keybind section.  
#####Keyboard users:
- The game works best by setting mouse to `Steering Only` in options and using the keyboard for forward and brake.  
- It is recommended you turn off camera `auto-align`.  

#### Tweaking values
A config file is generated in `[LMD folder]/UserData/AlternativeCameraSettings.cfg`.  
This file can be edited with any text editor and contains all the mods settings.  


#### Keybinds
| Keyboard & Mouse      | Gamepad                   | Action                                  |
| ---                   | ---                       | ---                                     |
| Mouse                 | Right Stick               | Look around                             |
| Right Click           | Right Bumper              | Hold to disable camera auto-align       |
| 9                     | -                         | Start the camera                        |
| 0                     | -                         | Toggle the camera mod (might be buggy)  |
| Enter / Space         | -                         | Snap camera to behind the player        |
| Mouse Scroll          | -                         | Move camera in and out                  |
| H                     | -                         | Toggle HUD & UI rendering               |
| Keypad 1              | -                         | Recommended camera preset               |
| Keypad 2              | -                         | First person ( *Experimental* )         |
| Keypad 3              | -                         | Toggle invert mouse horizontal          |
| Keypad 4              | -                         | Toggle camera auto-align                |
| Keypad 9              | -                         | Find and update gameobjects references  |


#### Known Issues & Fixes
- Movement isn't working: Go to the last checkpoint / restart run.  
- Camera doesn't work: Try pressing `Keyboard [9]`, `[Keypad 9]`, and `Keyboard [0]` on your keyboard.  
- Game freezes on quitting: Use `[Alt + Tab]` to select the commandline window and then close it.
