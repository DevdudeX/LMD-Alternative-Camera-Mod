[![en](https://img.shields.io/badge/lang-en-yellow.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.md)
[![de](https://img.shields.io/badge/lang-de-blue.svg)](https://github.com/faehromon/LMD-Alternative-Camera-Mod/blob/AltCamWithPhotoMode/README.de.md)


# DevdudeX's Alternative Kamera mit Fotomodus f�r Lonely Mountains: Downhill 
Dies ist eine Mod f�r *Lonely Mountains: Downhill*, die die standardm��ige "isometrische" Kamerasicht durch ein Third-/First-Person-System mit automatischer und manueller Kameraf�hrung ersetzt. Es unterst�tzt weiterhin die Originalkamera und erm�glicht den Wechsel zwischen diesen beiden Modi w�hrend dem Fahren. 
Zus�tzlich enth�lt diese Mod einen Fotomodus, mit dem du die Zeit anhalten und die Kamera frei positionieren kannst. Perfekt f�r Screenshots!  

## Credits
[DevdudeX](https://github.com/DevdudeX) hat beide urspr�nglichen Mods erstellt, `AlternativeCamera` und `PhotoMode`. Kudos an ihn f�r die geleistete Arbeit 
und Erstellung der Mods und insbesondere der Eingabe- und Kameralogik.
Unterst�tze ihn hier:  
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L5S9BK3)

Die Mod basiert auf LavaGangs [Melon Loader](https://github.com/LavaGang/MelonLoader), einer Modding-Umgebung f�r in Unity erstellte Spiele. 

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

## Mod-Kompatibilit�tswarnung
 Wenn du bereits die Original-Mod [AlternativeCamera](https://github.com/DevdudeX/LMD-Alternative-Camera-Mod) verwendest, deaktiviere diese bitte, w�hrend du diese Mod verwenden. 
 Einfach die Datei im Mods-Ordner umbenennen, bevor du das Spiel startest:    `AlternativeCameraMod.dll` --> `AlternativeCameraMod.dll.disabled`  
 
 Wenn du bereits die Mod [PhotoMode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod) verwendest, deaktiviere diese bitte, w�hrend du diese Mod verwendest. 
 Einfach die Datei im Mods-Ordner umbenennen, bevor du das Spiel startest:  `PhotoMode.dll` -> `PhotoMode.dll.disabled`.  

## Features
- Alternative Kameramodi f�r Third- und First-Person basierend auf [AlternativeCamera](https://github.com/DevdudeX/LMD-Alternative-Camera-Mod) 
- Wechsele w�hrend dem Spielen zwischen der Alternativkamera und dem Originalkameramodus
- Tastatur- und Controllereingaben sind anpassbar
- Erweiterter Fotomodus basierend auf [PhotoMode](https://github.com/DevdudeX/LMD-Photo-Mode-Mod)  
- Lokalisierbar mit Sprachdateien

## Installation

#### Schritt 1: Spielordner suchen
Du findest den Spielordner, indem du in Steam mit der rechten Maustaste auf das Spiel klickst und dann �Verwalten -> Lokale Dateien durchsuchen� w�hlst.  (z. B. `D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`)

#### Schritt 2: Lade den Melon Mod Loader herunter
Gehe auf https://melonwiki.xyz/#/  
Schaue dort unter �Automatisierte Installation�
(v0.6.2 ist die aktuelle Version Stand 3.M�rz'24)  

#### Schritt 3: Installiere den Melon Mod Loader
Installiere den Melon Loader in deinem LMD-Spielinstallationsordner.  F�hre das Spiel dann einmal aus und wenn beim Start der Melon Loader-Startbildschirm erscheint, ist alles in Ordnung. Beende das Spiel wieder. Sollte es beim Beenden h�ngen, f�ge `--quitfix` zu den Startoptionen hinzu.
Dazu in Steam mit der rechten Maustaste auf das Spiel klicken und dann �Eigenschaften -> Allgemein -> Startoptionen�

#### Schritt 4:  Lade diese Mod herunter und installiere sie
Lade `AlternativeCameraWithPhotoMode_v2.0.0.zip` aus den Ver�ffentlichungen herunter und entpacke es im Ordner `Mods` im LMD-Spielordner.  
 
#### Schritt 5: Starte das Spiel
Beim Start des Spiels wird die Mod automatisch initialisiert. Beim ersten Start wird automatisch die `AlternativeCameraWithPhotoMode.ini` erstellt, falls diese fehlt. Die Ini kannst du aber auch vorher hier herunterladen. 
Wenn die Mod aktiv ist, zeigt sie im Hauptmen� den Namen mit Version an. Fehlt dieser Text, ist die Mod nicht geladen.

## Wichtige Dateien und Ordner
Nehmen wir an, deine Steam-Bibliothek befindet sich hier: `D:\SteamLibrary\`.

### LMD Spielordner
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill`

### Mod Basisordner
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods`

### Mod Sprachdateien
Sprachdateien befinden sich hier:  `D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\Mods\AlternativeCameraWithPhotoMode-assets\Languages`
Derzeit verf�gbare Sprachdateien:  
- Englische Sprache: `lang.en.ini`  
- Deutsche Sprache: `lang.de.ini`

### Konfigurationsdatei
`D:\SteamLibrary\steamapps\common\Lonely Mountains - Downhill\UserData\AlternativeCameraWithPhotoMode.ini`

## Konfiguration
Eine Konfigurationsdatei wird in `[LMD-Ordner]\UserData\AlternativeCameraWithPhotoMode.ini` generiert.  Diese Datei kann mit jedem Texteditor bearbeitet werden und enth�lt alle Einstellungen der Mod mit Beschreibung.

### Tastatur und Controller-Belegung im Spielmodus
Diese Tasten sind Voreinstellungen und k�nnen in der Konfigurationsdatei angepasst werden.

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
| Kamera hinter das Fahrrad bewegen                  | Right Control             | Rechter Stick Klick         |
| Taste halten -> Pos.Modus temp. umschalten         | Mouse Right Button *)     | Rechte Schultertaste *)     |
| Kamera Zoom                                        | Mouse Scroll *)           | Steuerkreuz hoch/runter *)  |
| Tiefensch�rfe anpassen                             | Hold L + Mouse Scroll *)  | -                           |
| Tiefensch�rfe Fokus anpassen                       | Hold K + Mouse Scroll *)  | -                           |
| Sichtfeld (FoV) erh�hen um 10 (halte Alt f�r 5)    | 8                         | Steuerkreuz rechts *)       |
| Sichtfeld (FoV) verringern um 10 (halte Alt f�r 5) | 9                         | Steuerkreuz links *)        |
| Sichtefeld (FoV) zur�cksetzen                      | 0                         |                             |

*) fixed, not rebindable

### Tastatur- und Controller-Belegung im Fotomodus
Die Tastenbelegung ist wie folgt (nicht anpassbar)

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
| Rotation/Sichtfeld zur�cksetzen |                      | Steuerkreuz oben         |
| Sichtfeld (FoV) anpassen        | Mausrad scrollen     | LB / RB                  |
| Tiefensch�rfe-Modus umschalten  | V                    | Steuerkreuz unten        |

## Info f�r Tastaturbenutzer
- Das Spiel funktioniert mit Tastatur am besten, wenn du die Maus in den Optionen auf �Nur Lenken� einstellst und die Tastatur zum Vorw�rts- und Bremsen verwendest
- Es wird empfohlen, die automatische Ausrichtung der Kamera auszuschalten

## Bekannte Probleme und Korrekturen
- Das Spiel kann beim Beenden einfrieren: F�ge `--quitfix` zu den Steam-Startoptionen hinzu ([MelonLoader-Startoption](https://github.com/LavaGang/MelonLoader#launch-options))
- Das Fahrrad f�hrt manchmal nicht los und bewegt sich nicht mehr, nachdem zwischen Fahrrad- und Fotomodus gewechselt wurde. Durch absichtliches Herunterfallen vom Fahrrad behebt das Problem meist (Controller `B`).  
- Bei niedriger und mittlerer Tiefesch�rfe-Einstellung in LMD ist das Fahrrad im Third-Person-Modus unscharf



