
[![en](https://img.shields.io/badge/lang-en-yellow.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.md)
[![de](https://img.shields.io/badge/lang-de-blue.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.de.md)


# DevdudeX's Alternative Kamera mit Fotomodus für Lonely Mountains: Downhill 
Dies ist eine Mod für *Lonely Mountains: Downhill*, die die standardmäßige "isometrische" Kamerasicht durch ein Third-/First-Person-System mit automatischer und manueller Kameraführung erweitert. Es unterstützt weiterhin die Originalkamera und ermöglicht den Wechsel zwischen diesen beiden Modi während der Fahrt. 
Zusätzlich enthält diese Mod einen Fotomodus, mit dem du die Zeit anhalten und die Kamera frei positionieren kannst. Perfekt für Screenshots!  

## Credits
[DevdudeX](https://github.com/DevdudeX) hat beide ursprünglichen Mods erstellt, `AlternativeCamera` und `PhotoMode`. Kudos an ihn für die geleistete Arbeit 
und Erstellung der Mods und insbesondere der Eingabe- und Kameralogik.

Unterstütze ihn hier:  
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L5S9BK3)

Die Mod basiert auf LavaGangs [Melon Loader](https://github.com/LavaGang/MelonLoader), einer Modding-Umgebung für in Unity erstellte Spiele. 

## Screenshots 
Aufgenommen mit dem Mod Fotomodus: 
![BikingPicTP1](/images/LMD_screenshot_01.png?raw=true) 
![BikingPicTP2](/images/LMD_screenshot_02.png?raw=true)
![BikingPicTP3](/images/LMD_screenshot_03.png?raw=true) 
![BikingPicTP4](/images/LMD_screenshot_04.png?raw=true)
![BikingPicFP1](/images/LMD_screenshot_05.png?raw=true)
![FreePhotoPos1](/images/LMD_screenshot_pm01.png?raw=true)
![FreePhotoPos2](/images/LMD_screenshot_pm02.png?raw=true)  

Screenshot des Fotomodus
 ![PhotoMode01](/images/LMD_photomode_01.png?raw=true)    

## Mod-Kompatibilitätswarnung
 Wenn du bereits die Original-Mod [AlternativeCamera](https://github.com/DevdudeX/LMD-Alternative-Camera-Mod) verwendest, deaktiviere diese bitte, während du diese Mod verwenden. 
 Einfach die Datei im Mods-Ordner umbenennen, bevor du das Spiel startest:    `AlternativeCameraMod.dll` --> `AlternativeCameraMod.dll.disabled`  
 
 Wenn du bereits die Mod [PhotoMode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod) verwendest, deaktiviere diese bitte, während du diese Mod verwendest. 
 Einfach die Datei im Mods-Ordner umbenennen, bevor du das Spiel startest:  `PhotoMode.dll` -> `PhotoMode.dll.disabled`.  

## Features
- Alternative Kameramodi für Third- und First-Person basierend auf [AlternativeCamera](https://github.com/DevdudeX/LMD-Alternative-Camera-Mod) 
- Wechsele während dem Spielen zwischen der Alternativkamera und dem Originalkameramodus
- Tastatur- und Controllereingaben sind anpassbar
- Erweiterter Fotomodus basierend auf [PhotoMode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod)  
- Lokalisierbar mit Sprachdateien

## Installation

### Schritt 1: Spielordner suchen
Du findest den Spielordner, indem du in Steam mit der rechten Maustaste auf das Spiel klickst und dann „Verwalten -> Lokale Dateien durchsuchen“ wählst.  (z. B. `D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`)

### Schritt 2: Lade den Melon Mod Loader herunter
Stand 3. März '24 ist die aktuelle Version v0.6.2: [Installer Download Direktlink](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe)

- Gehe auf https://melonwiki.xyz/#/  
- Schaue unter [Automatisierte Installation](https://melonwiki.xyz/#/?id=automated-installation)
- Beachte die [Vorraussetzungen](https://melonwiki.xyz/#/?id=requirements)  (.NET4.8 + .NET6)
 
### Schritt 3: Installiere den Melon Mod Loader
Installiere den Melon Loader in deinem LMD-Spielinstallationsordner. Führe das Spiel dann einmal aus und wenn beim Start der Melon Loader-Startbildschirm erscheint, ist alles in Ordnung. Beende das Spiel wieder. 

Sollte LMD beim Beenden einfrieren, füge `--quitfix` zu den Startoptionen hinzu. Dazu in Steam mit der rechten Maustaste auf das Spiel klicken und dann `Eigenschaften -> Allgemein -> Startoptionen`

### Schritt 4:  Lade diese Mod herunter und installiere sie
Lade `AlternativeCameraWithPhotoMode_v<X.Y.Z>.zip` unter den Releases herunter und entpacke es in den `Mods`-Ordner im LMD Spieleordner.
Optional lade eine der vorbereiteten Konfigurationdateien `AlternativeCameraWithPhotoMode.ini.<lang>.zip` herunter und entpacke sie in den `UserData`-Ordner im LMD Spieleordner. Die Dateien unterscheiden sich in den lokalisierten Kommentaren zu den Einträgen. Technisch sind sie natürlich gleich.

 
### Schritt 5: Starte das Spiel
Beim Start des Spiels wird die Mod automatisch initialisiert. Beim ersten Start wird automatisch die `AlternativeCameraWithPhotoMode.ini` erstellt, falls diese fehlt. Die Ini kannst du aber auch vorher hier herunterladen. 
Wenn die Mod aktiv ist, zeigt sie im Hauptmenü den Namen mit Version an. Fehlt dieser Text, ist die Mod nicht geladen.


## Wichtige Dateien und Ordner
Nehmen wir an, deine Steam-Bibliothek befindet sich hier: `D:\SteamLibrary\`.

### LMD Spielordner
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`

### Mod Basisordner
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods`
Das ist der Ort für die Mod Datei selbst: `AlternativeCameraWithPhotoMode.dll` 

### Mod Sprachdateien
Sprachdateien befinden sich hier:  `D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods\AlternativeCameraWithPhotoMode-assets\Languages`

Derzeit verfügbare Sprachdateien:  
- Englische Sprache: `lang.en.ini`  
- Deutsche Sprache: `lang.de.ini`

### Konfigurationsdatei
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\UserData\AlternativeCameraWithPhotoMode.ini`

## Konfiguration
Die Konfigurationsdatei `AlternativeCameraWithPhotoMode.ini` kann mit jedem Texteditor bearbeitet werden und enthält alle Einstellungen der Mod mit Beschreibung.

### Tastatur und Controller-Belegung im Spielmodus
Diese Tasten sind Voreinstellungen und können in der Konfigurationsdatei angepasst werden. Die Controller-Belegung ist fest definiert.

| Aktion                                             | Tastatur/Maus             | Controller                  |
| ---                                                | ---                       | ---                         |
| Kameramodus: Original,"isometrisch"                | F1                        | -                           |
| Kameramodus: Third Person                          | F2                        | -                           |
| Kameramodus: First Person                          | F3                        | -                           |
| Kamera umschalten Original -> Alternative          | Leertaste                 | Linke Schultertaste *)      |
| Auto/Manual-Positionierung umschalten              | F5                        | -                           |
| Invertiert horizontale Blickausrichtung            | F6                        | -                           |
| Game HUD an-/abschalten                            | H                         | -                           |
| Mod HUD Anzeige an/-abschalten                     | J                         | -                           |
| Kamera schwenken                                   | Mouse *)                  | Rechter Stick *)            |
| Kamera hinter das Fahrrad bewegen                  | Right Control             | Rechter Stick Klick *)      |
| Taste halten -> Pos.Modus temp. umschalten         | Mouse Right Button *)     | Rechte Schultertaste *)     |
| Kamera Zoom                                        | Mouse Scroll *)           | Steuerkreuz hoch/runter *)  |
| Tiefenschärfe anpassen                             | Hold L + Mouse Scroll *)  | -                           |
| Tiefenschärfe Fokus anpassen                       | Hold K + Mouse Scroll *)  | -                           |
| Sichtfeld (FoV) erhöhen um 10 (halte Alt für 5)    | 8                         | Steuerkreuz rechts *)       |
| Sichtfeld (FoV) verringern um 10 (halte Alt für 5) | 9                         | Steuerkreuz links *)        |
| Sichtefeld (FoV) zurücksetzen                      | 0                         |                             |

*) nicht anpassbar

### Tastatur- und Controller-Belegung im Fotomodus
Die Tastenbelegung ist wie folgt (nicht anpassbar):

| Aktion                          | Tastatur/Maus        | Controller               |
| ---                             | ---                  | ---                      |
| Fotomodus an/aus                | P                    | Y                        |
| Foto aufnehmen (in Datei)       | Leertaste            | X                        |
| Anleitung an/aus                | I                    | L-Stick Klick            |
| HUD an/aus                      | H                    | R-Stick Klick            |
| Bewegen/Schwenken               | W A S D + Maus       | L-Stick / R-Stick        |
| Hoch/Runter                     | R / F                | L-Trig / R-Trig          |
| Drehen Links/Rechts             | Q / E                | Steuerkreuz links/rechts |
| Bewegung beschleunigen          | Shift                | A                        |
| Rotation/Sichtfeld zurücksetzen |                      | Steuerkreuz oben         |
| Sichtfeld (FoV) anpassen        | Mausrad scrollen     | LB / RB                  |
| Tiefenschärfe-Modus umschalten  | V                    | Steuerkreuz unten        |

## Info zu Tastatur/Maus
- Wenn du aktiv mit Maus spielen möchtest, muss die automatische Ausrichtung der Kamera durch die Mod ausgeschaltet sein, da sonst die Mauseingabe nichts bewirkt (`AlignMode=Manual`)
- Das Spielen klappt mit Maus und Tastatur am besten, wenn du die Maus in den Optionen auf `Nur Lenken` einstellst und die Tastatur für Fahren und Bremsen verwendest


## Bekannte Probleme und Korrekturen
- Das Spiel kann beim Beenden einfrieren: Füge `--quitfix` zu den Steam-Startoptionen hinzu (siehe [Melon Loader Startoptionen](https://github.com/LavaGang/MelonLoader#launch-options))
- Das Fahrrad steckt manchmal fest und fährt nicht mehr los, nachdem zwischen Fahrrad- und Fotomodus gewechselt wurde; das passiert meistens, wenn das Fahrrad noch am Startpunkt steht; egal wo, nur ein Neustart des Levels oder des Checkpoints löst das; manchmal funktioniert auch ein absichtliches Herunterfallen vom Fahrrad klappt auch (Controller `B`).  
- Hin und wieder verstellt sich auch bei automatischer Kamerapositionierung die vertikale Ausrichtung ein klein wenig und muss einmalig manuell nachkorrigiert werden
- Bei niedriger und mittlerer Tiefeschärfe-Einstellung in LMD ist das Fahrrad im Third-Person-Modus unscharf
