# Multilanguage README Pattern
[![en](https://img.shields.io/badge/lang-en-blue.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.md)
[![de](https://img.shields.io/badge/lang-de-yellow.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.de.md)


# DevdudeX's Alternative Camera with Photo Mode for Lonely Mountains: Downhill

This is a mod for *Lonely Mountains: Downhill* that replaces the default distant "isometric-like" camera with a third/first person system with 
automatic and manual camera work. It still supports the original camera and allows to switch between them while biking. 
Additionally this mod contains a photo mode, that lets you pause time and position the camera freely. Perfect for taking screenshots!


## Credits
[DevdudeX](https://github.com/DevdudeX) created both mods, the AlternativeCamera and the PhotoMode. Kudos to him for doing the heavy lifting and the initial hard work on the mods and especially the core loop with the input and camera logic. 
Support him here: 
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L5S9BK3)

The mod is based on LavaGang's [Melon Loader](https://github.com/LavaGang/MelonLoader), a modding environment for games made in Unity. 

## Screenshots
Taken with the Mods Photo Mode:
![BikingPicTP1](/images/LMD_screenshot_01.png?raw=true)
![BikingPicTP2](/images/LMD_screenshot_02.png?raw=true)
![BikingPicTP3](/images/LMD_screenshot_03.png?raw=true)
![BikingPicTP4](/images/LMD_screenshot_04.png?raw=true)
![BikingPicFP1](/images/LMD_screenshot_05.png?raw=true)

![FreePhotoPos1](/images/LMD_screenshot_pm01.png?raw=true)
![FreePhotoPos2](/images/LMD_screenshot_pm02.png?raw=true)

Screenshot of the Photo Mode
![PhotoMode01](/images/LMD_photomode_02.png?raw=true)

## Mod Compatibility Warning
If you are already using the original [Alternative Camera](https://github.com/DevdudeX/LMD-Alternative-Camera-Mod) mod please disable it while using this mod.  You can do this by simply renaming the file in your mods folder before launching the game.  
`AlternativeCameraMod.dll` --> `AlternativeCameraMod.dll.disabled`

If you are already using the [Photo Mode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod) mod please disable it while using this mod.  You can do this by simply renaming the file in your mods folder before launching the game.  
`PhotoMode.dll` --> `PhotoMode.dll.disabled`

## Features
- Alternative Camera Modes for Third Person and First Person based on [Alternative Camera](https://github.com/DevdudeX/LMD-Alternative-Camera-Mod)
- Toggle between the alternative camera and the original camera mode while playing
- Controls rebindable for keyboard and controller (see keybindings below)
- Enhanced Photo Mode based on [Photo Mode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod)
- Localizable by language file

## Setup Instructions

#### Step 1: Locating the Game Folder
Your game folder can be found by right-clicking on the game in Steam and going 'Manage -> Browse local files'
(e.g. `D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`)

#### Step 2: Download the Melon Mod Loader
Go to https://melonwiki.xyz/#/  
Look under 'Automated Installation':
v0.6.2 is the current version on March 3, 2024


#### Step 3: Install the Melon Mod Loader
Install Melon Loader to your LMD game install folder.  
Run the game once and if the Melon Loader splash screen appears on launch, everything is fine. Then exit the game. 
Sollte es beim Beenden hängen, füge `--quitfix` zu den Startoptionen hinzu.


If it freezes on exit, add '--quitfix' to the Steam Launch Options.
Go to Steam, right click the game, then „Properties -> General -> Launch Options“

#### Step 4: Download and install this mod's release 
Download `AlternativeCameraWithPhotoMode.dll` from the releases and add it to the `Mods` folder in your LMD game folder.   
The mod is automatically initialized. 

#### Step 5: Start the game
When starting the game the mod is automatically initialized.
On the first start it automatically creates the `AlternativeCameraWithPhotoMode.ini` if it is missing.
The mod is showing its name and version in the main menu below the game version. If this text is missing, the mod is not loaded.


## Essential Files and Folders
Let's assume your Steam Library is located here: `D:\SteamLibrary\`

### Game Folder
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`

### Mod Base Folder
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods`

### Mod Assets/Languages Folder
Language files are located here:
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods\AlternativeCameraWithPhotoMode-assets\Languages`

Currently available language files:
- English Language: `lang.en.ini`
- German Language: `lang.de.ini`

### Config File
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\UserData\AlternativeCameraWithPhotoMode.ini`

## Configuration
A configuration file is generated in `[LMD folder]/UserData/AlternativeCameraWithPhotoMode.ini`.  
This file can be edited with any text editor and contains all the mods settings with description.  

### Bike Mode Keys
These are presets, keys rebindable.

| Action                                  | Keyboard/Mouse            | Controller                  |
| ---                                     | ---                       | ---                         |
| Camera Mode: Original Isometric         | F1                        | -                           |
| Camera Mode: Third Person               | F2                        | -                           |
| Camera Mode: First person               | F3                        | -                           |
| Toggle camera original<->alternative    | Space                     | Left Bumper *)              |
| Toggle camera auto-align mode           | F5                        | -                           |
| Toggle invert look horizontal           | F6                        | -                           |
| Toggle Game HUD                         | H                         | -                           |
| Toggle Mod HUD Displays                 | J                         | -                           |
| Look around                             | Mouse *)                  | Right Stick *)              |
| Snap camera to behind the player        | Right Control             | Right Stick Click           |
| Hold to invert camera auto-align mode   | Mouse Right Button *)     | Right Bumper *)             |
| Move camera in and out                  | Mouse Scroll *)           | Dpad Up / Down *)           |
| Change DoF focal length                 | Hold L + Mouse Scroll *)  | -                           |
| Change DoF focus distance offset        | Hold K + Mouse Scroll *)  | -                           |
| Increase FoV by 10 (hold Alt for 5)     | 8                         | Dpad Right *)               |
| Decrease FoV by 10 (hold Alt for 5)     | 9                         | Dpad Left *)                |
| Reset FoV to default/preset             | 0                         |                             |

*) fixed, not rebindable

### Photo Mode Keys
These keys are not rebindable.

| Action                  | Keyboard/Mouse   | Controller           |
| ---                     | ---              | ---                  |
| Enter/Exit Photo Mode   | P                | Y                    |
| Take Photo              | Space            | X                    |
| Toggle Instructions     | I                | L-Stick Click        |
| Toggle HUD              | H                | R-Stick Click        |
| Move / Pan              | W A S D + Mouse  | L-Stick / R-Stick    |
| Up / Down               | R / F            | L-Trig / R-Trig      |
| Tilt Left / Right       | Q / E            | Dpad left / right    |
| Speed up movement       | Shift            | A                    |
| Reset rotation / FoV    | K                | Dpad up              |
| Change FoV              | Mouse Scroll     | LB / RB              |
| Toggle DoF mode         | V                | Dpad down            |


## Info for keyboard users
- The game works best by setting mouse to `Steering Only` in options and using the keyboard for forward and brake 
- It is recommended to turn off camera `auto-align` 

## Known Issues & Fixes
- Game may freeze on quitting: Add the `--quitfix` to Steam Launch Options ([MelonLoader launch option](https://github.com/LavaGang/MelonLoader#launch-options))  
- Bike sometimes gets stuck and does not move anymore after switching between biking and photo mode; intentionally falling of the bike usually fixes this (use controller `B`)
- Low and mid depth of field setting in LMD blurs the bike in third person mode


