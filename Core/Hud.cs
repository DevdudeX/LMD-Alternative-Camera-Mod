using System.Text;
using AlternativeCameraMod.Config;
using AlternativeCameraMod.Language;
using MelonLoader;
using UnityEngine;


namespace AlternativeCameraMod;

internal class Hud
{
   private readonly State _state;
   private readonly CameraControl _camera;
   private readonly InputHandler _input;
   private readonly Configuration _cfg;
   private readonly LanguageConfig _lang;
   private readonly Logger _logger;
   private bool _hudInfoVisible = true;
   private readonly List<ModHudInfoPart> _hudInfoParts = new();


   public Hud(State state, CameraControl camera, InputHandler input, Configuration cfg,
                     LanguageConfig lang, Logger logger)
   {
      _state = state;
      _camera = camera;
      _input = input;
      _cfg = cfg;
      _lang = lang;
      _logger = logger;
      ParseHudInfoDisplayFlags(_cfg.UI.ModHudInfoDisplay.Value);
      Initialize();
   }


   private void Initialize()
   {
      MelonEvents.OnGUI.Subscribe(DrawInfoOnHud, 100);
   }

   
   public void Close()
   {
      MelonEvents.OnGUI.Unsubscribe(DrawInfoOnHud);
   }


   private void DrawInfoOnHud()
   {
      if (_state.Suspended)
      {
         // when disabled, only show text on menu screen, do not show others
         if (_state.CurrentScreen != Screen.MenuScreen)
         {
            return;
         }
      }

      StringBuilder text = new StringBuilder();
      int x = 0, y = 0, size = 10;
      string color = "#FFFFFF";
      string shadowColor = "#000000";
      int shadowOffsetX = 1;
      int shadowOffsetY = 1;
      bool shadow = false;
      _logger.LogVerbose("Screen: {0}", _state.CurrentScreen);
      switch (_state.CurrentScreen)
      {
         case Screen.None:
            return;

         case Screen.LoadingScreen:
            text.AppendFormat(_lang.GetText("Mod",
               "Title_{Version}",
               "Alternative Camera with Photo Mode {0}",
               AlternativeCamera.MOD_VERSION));
            x = UnityEngine.Screen.currentResolution.width / 2 - 200;
            y = UnityEngine.Screen.currentResolution.height - 50;
            size = 20;
            color = Configuration.GameColor;
            break;

         case Screen.SplashScreen:
            // nothing
            return;

         case Screen.MenuScreen:
            text.AppendFormat(_lang.GetText("Mod",
               "Title_{Version}",
               "Alternative Camera with Photo Mode {0}",
               AlternativeCamera.MOD_VERSION).ToUpper());
#if DEBUG
            text.Append(" (DBG)");
#endif
            x = 130;
            y = UnityEngine.Screen.currentResolution.height - 200;
            size = 20;
            shadowOffsetX = 1;
            shadowOffsetY = 2;
            color = Configuration.GameColor; // matches LMD text color

            if (!string.IsNullOrEmpty(_state.ErrorMessage))
            {
               text.AppendLine();
               text.AppendFormat(_lang.GetText("Mod", "ErrorOutputLine_{Msg}", "[ERR] {0}", _state.ErrorMessage));
            }

            shadow = true;
            break;

         case Screen.PlayScreen:
            if (!IsHudVisible()) return;

            if (ShowHudInfo())
            {
               x = 20;
               y = UnityEngine.Screen.currentResolution.height - 28;
               size = 15;
               color = "#FFFFFF";
               BuildHudInfoText(text);
            }

            break;

         case Screen.PhotoScreen:
            if (_state.PhotoModeInstructionsVisible)
            {
               DrawPhotoModeInstructionsOnHud();
            }

            break;
      }

      if (text.Length > 0)
      {
         if (shadow)
         {
            GUI.Label(new Rect(x + shadowOffsetX, y + shadowOffsetY, 2000, 200),
               string.Format("<b><color={0}><size={1}>{2}</size></color></b>",
                  shadowColor,
                  size,
                  text));
         }

         GUI.Label(new Rect(x, y, 2000, 200),
            string.Format("<b><color={0}><size={1}>{2}</size></color></b>",
               color,
               size,
               text));
      }
   }


   private void DrawPhotoModeInstructionsOnHud()
   {
      string format = @"<b><color={0}><size={1}>{2}</size></color></b>";

      var actions = new StringBuilder();
      string pre = " * ";
      actions.AppendLine(_lang.GetText("PhotoMode", "ActionHeader", "ACTION"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionExit", "Exit Photo Mode"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionShoot", "Take Photo"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionToggleInstruct", "Toggle Instructions"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionToggleHud", "Toggle HUD"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionMovePan", "Move / Pan"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionUpDown", "Up / Down"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionTilt", "Tilt Left / Right"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionSpeedUp", "Speed up movement"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionReset", "Reset rotation / FoV"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionChangeFoV", "Change FoV"));
      actions.AppendLine(pre + _lang.GetText("PhotoMode", "ActionToggleDoF", "Toggle DoF mode"));

      string sep = "  ";
      var keys = new StringBuilder();
      keys.AppendLine(_lang.GetText("PhotoMode", "KeyMouseHeader", "CONTROLLER"));
      keys.AppendLine(sep + "P");
      keys.AppendLine(sep + _lang.GetText("PhotoMode", "InputSpaceKey", "Space"));
      keys.AppendLine(sep + "I");
      keys.AppendLine(sep + "H");
      keys.AppendLine(sep + "W A S D + " + _lang.GetText("PhotoMode", "InputMouse", "Mouse"));
      keys.AppendLine(sep + "R / F");
      keys.AppendLine(sep + "Q / E");
      keys.AppendLine(sep + "Shift");
      keys.AppendLine(sep + "K");
      keys.AppendLine(sep + _lang.GetText("PhotoMode", "InputMouseWheel", "Mouse Scroll"));
      keys.AppendLine(sep + "V");

      var btns = new StringBuilder();
      var dpad = _lang.GetText("PhotoMode", "InputDpad", "Dpad");
      btns.AppendLine(_lang.GetText("PhotoMode", "ControllerHeader", "CONTROLLER"));
      btns.AppendLine(sep + "Y");
      btns.AppendLine(sep + "X");
      btns.AppendLine(sep + "L-Stick Click");
      btns.AppendLine(sep + "L-Stick Click");
      btns.AppendLine(sep + "L-Stick / R-Stick");
      btns.AppendLine(sep + "L-Trig / R-Trig");
      btns.AppendLine(sep + dpad + " ◄ / ►");
      btns.AppendLine(sep + "A");
      btns.AppendLine(sep + dpad + " ▲");
      btns.AppendLine(sep + "LB / RB");
      btns.AppendLine(sep + dpad + " ▼");


      var foreColor = Configuration.GameColor;
      var shadowColor = "black";
      var shadowOff = 2;

      var fmtBindDescr = string.Format(format, foreColor, 20, actions);
      var fmtKeyDescr = string.Format(format, foreColor, 20, keys);
      var fmtBtnDescr = string.Format(format, foreColor, 20, btns);

      var fmtBindDescrShadow = string.Format(format, shadowColor, 20, actions);
      var fmtKeyDescrShadow = string.Format(format, shadowColor, 20, keys);
      var fmtBtnDescrShadow = string.Format(format, shadowColor, 20, btns);

      var pmLabel = _lang.GetText("PhotoMode", "Title", "PHOTO MODE");
      var onLabel = _lang.GetText("PhotoMode", "On", "on");
      var offLabel = _lang.GetText("PhotoMode", "Off", "off");
      var dofModeLabel = _lang.GetText("PhotoMode",
         "DoFModeLabel_{state}",
         "DoF focus mode: {0}",
         _camera.PhotoFocusMode ? onLabel : offLabel);
      var note = _lang.GetText("PhotoMode", "Note", "(This instructions box is not part of the photo)");

      float xOffset = 50;
      float xOffset2 = 400;
      float xOffset3 = 600;
      float yPosTitle = 200;
      float yPosInstr = 250;
      float yPosNote = 540;
      float yPosState = 600;
      float yPosSaveInfo = 680;

      GUI.Box(new Rect(xOffset - 20, yPosTitle - 20, 800, 470), "");

      GUI.Label(new Rect(xOffset + shadowOff, yPosTitle + shadowOff, 1000, 200),
         $"<b><color=black><size=30>{pmLabel}</size></color></b>");
      GUI.Label(new Rect(xOffset, yPosTitle, 1000, 200), $"<b><color=white><size=30>{pmLabel}</size></color></b>");

      GUI.Label(new Rect(xOffset + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBindDescrShadow);
      GUI.Label(new Rect(xOffset, yPosInstr, 2000, 2000), fmtBindDescr);

      GUI.Label(new Rect(xOffset2 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBtnDescrShadow);
      GUI.Label(new Rect(xOffset2, yPosInstr, 2000, 2000), fmtBtnDescr);

      GUI.Label(new Rect(xOffset3 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtKeyDescrShadow);
      GUI.Label(new Rect(xOffset3, yPosInstr, 2000, 2000), fmtKeyDescr);

      GUI.Label(new Rect(xOffset + shadowOff, yPosState + shadowOff, 1000, 200),
         $"<b><color={shadowColor}><size=20>{dofModeLabel}</size></color></b>");
      GUI.Label(new Rect(xOffset, yPosState, 1000, 200),
         $"<b><color={(_camera.PhotoFocusMode ? "lime" : foreColor)}><size=20>{dofModeLabel}</size></color></b>");

      GUI.Label(new Rect(xOffset + 1, yPosNote + 1, 1000, 200), $"<b><color=black><size=15>{note}</size></color></b>");
      GUI.Label(new Rect(xOffset, yPosNote, 1000, 200), $"<b><color=#CCCCCC><size=15>{note}</size></color></b>");

      if (!string.IsNullOrEmpty(_state.LastScreenshotInfo))
      {
         GUI.Box(new Rect(xOffset - 20, yPosSaveInfo - 10, 800, 90), "");

         GUI.Label(new Rect(xOffset + shadowOff, yPosSaveInfo + shadowOff, 1200, 200),
            $"<b><color={shadowColor}><size=20>{_state.LastScreenshotInfo}</size></color></b>");
         GUI.Label(new Rect(xOffset, yPosSaveInfo, 1200, 200),
            $"<b><color=yellow><size=20>{_state.LastScreenshotInfo}</size></color></b>");
      }
   }


   public static Camera? GetHudCam()
   {
      var hudCamObj = GameObject.Find("UICam");
      if (hudCamObj == null)
      {
         return null;
      }

      var hudCam = hudCamObj.GetComponent<Camera>();
      return hudCam;
   }


   /// <summary>
   /// Toggles the rendering of the game HUD.
   /// </summary>
   public void ToggleHudVisiblity(bool? visible = null)
   {
      var hud = GetHudCam();
      if (hud == null)
      {
         return;
      }

      if (visible.HasValue)
      {
         _logger.LogDebug("Init Game Hud: {0}", visible.Value);
         hud.enabled = visible.Value;
      }
      else
      {
         hud.enabled = !hud.enabled;
         _logger.LogDebug("Toggle Game Hud: {0}", hud.enabled);
      }
   }


   public void ToggleModHudInfoVisibility()
   {
      _hudInfoVisible = !_hudInfoVisible;
   }


   public bool IsHudVisible()
   {
      var hudCam = GetHudCam();
      return hudCam != null && hudCam.enabled;
   }


   private bool ShowHudInfo()
   {
      return _hudInfoParts.Count > 0 && _hudInfoVisible && IsHudVisible();
   }


   internal void ParseHudInfoDisplayFlags(string text)
   {
      if (string.IsNullOrWhiteSpace(text))
      {
         return;
      }

      var tokens = text.Split(',').Select(t => t.Trim());
      foreach (var token in tokens)
      {
         if (Enum.TryParse<ModHudInfoPart>(token, true, out var flag))
         {
            _hudInfoParts.Add(flag);
         }
      }
   }


   private void BuildHudInfoText(StringBuilder target)
   {
      foreach (var infoPart in _hudInfoParts)
      {
         if (target.Length > 0)
         {
            target.Append("  ");
         }

         switch (infoPart)
         {
            case ModHudInfoPart.CamMode:
               string mode = "";
               switch (_camera.CurrentCamView)
               {
                  case CameraView.Original:
                     mode = _lang.GetText("Mod", "CamOriginal", "Original");
                     break;
                  case CameraView.ThirdPerson:
                     mode = _lang.GetText("Mod", "CamThirdPerson", "Third Person");
                     break;
                  case CameraView.FirstPerson:
                     mode = _lang.GetText("Mod", "CamFirstPerson", "First Person");
                     break;
               }

               target.Append(_lang.GetText("Mod", "CamLabel_{Mode}", "Cam: {0}", mode));
               break;

            case ModHudInfoPart.FoV:
               float val = 0;
               switch (_camera.CurrentCamView)
               {
                  case CameraView.Original:
                     val = _camera.IsometricFoV;
                     break;
                  case CameraView.ThirdPerson:
                     val = _camera.ThirdPersonFoV;
                     break;
                  case CameraView.FirstPerson:
                     val = _camera.FirstPersonFoV;
                     break;
               }

               target.Append(_lang.GetText("Mod", "FoVInfoLabel_{fov}", "FoV: {0}", val));
               break;

            case ModHudInfoPart.FPS:
               target.Append(_lang.GetText("Mod", "FPSLabel_{fps}", "FPS: {0}", _state.Fps));
               break;

            case ModHudInfoPart.CamAlign:
               string align = _camera.AutoAlign
                  ? (_input.PlayMode.InvertAlignmentMode() ? "Manual" : "Auto")
                  : (_input.PlayMode.InvertAlignmentMode() ? "Auto" : "Manual");

               target.Append(_lang.GetText("Mod", "CamAlignLabel_{align}", "({0})", align));
               break;
         }
      }
   }
}
