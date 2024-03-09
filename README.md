
[![en](https://img.shields.io/badge/lang-en-blue.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.md)
[![de](https://img.shields.io/badge/lang-de-yellow.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.de.md)


# DevdudeX's Alternative Camera with Photo Mode for Lonely Mountains: Downhill

This is a mod for *Lonely Mountains: Downhill* that extends the default distant "isometric-like" camera with a third/first person system with automatic and manual camera work. It still supports the original camera and allows to switch between them while riding the bike. 
Additionally this mod contains a photo mode, that lets you pause time and position the camera freely. Perfect for taking screenshots!


## Credits
[DevdudeX](https://github.com/DevdudeX) created both original mods, `AlternativeCamera` and `PhotoMode`. 
Kudos to him for doing the heavy lifting and the initial hard work on the mods and especially the core loop with the input and camera logic.

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

### Step 1: Locating the Game Folder
Your game folder can be found by right-clicking on the game in Steam and going 'Manage -> Browse local files'
(e.g. `D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`)

### Step 2: Download the Melon Mod Loader
As of March 3, 2024 the current version is v0.6.2: [Installer Direct Download Link](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe)

- Go to https://melonwiki.xyz/#/  
- Look under [Automated Installation](https://melonwiki.xyz/#/?id=automated-installation)
- [Check requirements](https://melonwiki.xyz/#/?id=requirements)  (.NET4.8 + .NET6)

### Step 3: Install the Melon Mod Loader
Install Melon Loader to your LMD game install folder.  
Run the game once and if the Melon Loader splash screen appears on launch, everything is fine. Then exit the game. 
If LMD freezes on exit, add `--quitfix` to the Steam Launch Options.
Go to Steam, right click the game, then `Properties -> General -> Launch Options`

### Step 4: Download and install this mod's release 
Download `AlternativeCameraWithPhotoMode_v<X.Y.Z>.zip` from the releases and unpack it to the `Mods` folder in your LMD game folder.
Optionally download any of the `AlternativeCameraWithPhotoMode.ini.<lang>.zip` and unpack it to the `UserData` folder in your LMD game folder. The configuration files are different in the localized comments about the entries. Technically they are same, of course.

### Step 5: Start the game
When starting the game the mod is automatically initialized.
On the first start it automatically creates the `AlternativeCameraWithPhotoMode.ini` if it is missing.
The mod is showing its name and version in the main menu. If this text is missing, the mod is not loaded.


## Essential Files and Folders
Let's assume your Steam Library is located here: `D:\SteamLibrary\`

### Game Folder
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`

### Mod Base Folder
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods`
This is the location for the mod file itself: `AlternativeCameraWithPhotoMode.dll` 

### Mod Assets/Languages Folder
Language files are located here:
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods\AlternativeCameraWithPhotoMode-assets\Languages`

Currently available language files:
- English Language: `lang.en.ini`
- German Language: `lang.de.ini`

### Configuration File
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\UserData\AlternativeCameraWithPhotoMode.ini`

## Configuration
The configuration file  `AlternativeCameraWithPhotoMode.ini` can be edited with any text editor and contains all the mods settings with description.  

### Bike Mode Keys
The keys are presets and can be customized. Controller inputs are fixed.

| Action                                  | Keyboard/Mouse            | Controller                  |
| ---                                     | ---                       | ---                         |
| Original camera (isometric)             | F1                        | -                           |
| Alternative camera: Third Person        | F2                        | -                           |
| Alternative camera: First Person        | F3                        | -                           |
| Toggle camera: Original <-> Alternative | Space                     | Left Bumper *)              |
| Toggle camera auto-align mode           | F5                        | -                           |
| Toggle invert look horizontal           | F6                        | -                           |
| Toggle Game HUD                         | H                         | -                           |
| Toggle Mod HUD Displays                 | J                         | -                           |
| Look around                             | Mouse *)                  | Right Stick *)              |
| Snap camera to behind the bike          | Right Control             | Right Stick Click *)        |
| Invert camera auto-align mode (hold)    | Mouse Right Button *)     | Right Bumper *)             |
| Zoom camera in and out                  | Mouse Scroll *)           | Dpad Up / Down *)           |
| Adjust DoF focal length                 | Hold L + Mouse Scroll *)  | -                           |
| Adjust DoF focus distance offset        | Hold K + Mouse Scroll *)  | -                           |
| Increase FoV by 10 (hold Alt for 5)     | 8                         | Dpad Right *)               |
| Decrease FoV by 10 (hold Alt for 5)     | 9                         | Dpad Left *)                |
| Reset FoV to default/preset             | 0                         |                             |

*) not customizable

### Photo Mode Keys
These input assignment is as follows (not customizable):

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
| Toggle FoV / DoF mode   | V                | Dpad down            |
| Change FoV / DoF        | Mouse Scroll     | LB / RB              |


## Info for keyboard/mouse
- If you want to actively play with mouse, the automatic alignment of the camera by the mod must be switched off, otherwise mouse input has no effect (`AlignMode=Manual`)
- Playing with keyboard and mouse works best by setting mouse to `Steering Only` in LMD options and using the keyboard for forward and brake

## Known Issues & Fixes
- Game may freeze on quitting: Add the `--quitfix` to Steam Launch Options (see [Melon Loader launch option](https://github.com/LavaGang/MelonLoader#launch-options))  
- Bike sometimes gets stuck and does not move anymore after switching between bike and photo mode; this mostly happens when the bike is still at level start or a checkpoint; only restart of level or checkpoint resolves such situations; sometimes it works to intentionally fall of the bike (controller `B`)
- Every now and then, even with automatic camera positioning, the vertical alignment changes  slightly and has to be corrected manually once
- Low and mid depth of field setting in LMD blurs the bike in third person mode
