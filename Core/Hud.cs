﻿using System.Text;
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
   private int _fpsDisplayThreshold;
   private int _displayedFps;
   private bool _intialized;


   public Hud(State state, CameraControl camera, InputHandler input, Configuration cfg,
                     LanguageConfig lang, Logger logger)
   {
      _state = state;
      _camera = camera;
      _input = input;
      _cfg = cfg;
      _lang = lang;
      _logger = logger;
   }


   public void InitializeOnce()
   {
      if (_intialized) return;
     
      ParseHudInfoDisplayFlags(_cfg.PlayMode.ModHudInfoDisplay.Value);

      MelonEvents.OnGUI.Subscribe(DrawInfoOnHud, 100);
      _intialized = true;
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
      bool addShadow = false;
      bool addBox = false;
      int boxPadding = 10;
      int textWidth = 800;
      int textHeight = 120;
      
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
            y = UnityEngine.Screen.currentResolution.height - 250;
            size = 20;
            shadowOffsetX = 1;
            shadowOffsetY = 2;
            color = Configuration.GameColor2; // matches LMD text color

            if (!string.IsNullOrEmpty(_state.ErrorMessage))
            {
               color = nameof(Color.red);
               addBox = true;
               text.AppendLine();
               text.AppendFormat(_lang.GetText("Mod", "ErrorOutputLine_{Msg}", "[ERR] {0}", _state.ErrorMessage));
            }

            addShadow = true;
            break;

         case Screen.PlayScreen:
            if (!IsHudVisible()) return;

            if (ShowHudInfo())
            {
               x = 20;
               y = UnityEngine.Screen.currentResolution.height - 28;
               size = Math.Max(5, _cfg.PlayMode.ModHudTextSize.Value);
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
         
         case Screen.PauseScreen:
            if (_state.IsPausedInPhotoMode) return;

            if (_cfg.PlayMode.ShowCamInstructionsInPauseMenu.Value)
            {
               DrawPlayModeInstructionsOnHud();
            }
            else
            {  
               x = 1300;
               y = UnityEngine.Screen.currentResolution.height - 120;
               size = Math.Max(5, _cfg.PlayMode.ModHudTextSize.Value);
               color = "#FFFFFF";
               text.Append(_lang.GetText("PlayMode", "PressKeyForInstructions_{key}", "(press {0} for instructions)", "'I' / 'R-Stick'"));
            }
            break;
      }

      if (text.Length > 0)
      {
         if (addBox)
         {
            GUI.Box(new Rect(x - boxPadding, y - boxPadding, textWidth + boxPadding * 2, textHeight + boxPadding*2), "");
         }
         if (addShadow)
         {
            GUI.Label(new Rect(x + shadowOffsetX, y + shadowOffsetY, textWidth, textHeight), 
               FormatLabel(text.ToString(), size, shadowColor, true));
         }

         GUI.Label(new Rect(x, y, textWidth, textHeight), 
            FormatLabel(text.ToString(), size, color, true));
      }
   }


   private void DrawPlayModeInstructionsOnHud()
   {
      var actionListInOrderToShow = new List<PlayModeAction>
      {
         PlayModeAction.CameraModeOriginal,
         PlayModeAction.CameraModeThirdPerson,
         PlayModeAction.CameraModeFirstPerson,
         PlayModeAction.ToggleCameraState,
         PlayModeAction.ToggleCameraAutoAlignMode,
         PlayModeAction.InvertCameraAutoAlignMode,
         PlayModeAction.ToggleInvertLookHorizontal,
         PlayModeAction.ToggleGameHud,
         PlayModeAction.ToggleModHudDisplays,
         PlayModeAction.LookAround,
         PlayModeAction.SnapCameraBehindBike,
         PlayModeAction.ZoomInOut,
         PlayModeAction.ChangeDoFFocalLength,
         PlayModeAction.ChangeDoFFocusDistanceOffset,
         PlayModeAction.IncreaseFoV,
         PlayModeAction.DecreaseFoV,
         PlayModeAction.ResetFoV
      };

      var actions = new StringBuilder();
      string pre = " ► ";
      actions.AppendLine(_lang.GetText("Input", "ActionHeader", "ACTION"));
      for (var index = 0; index < actionListInOrderToShow.Count; index++)
      {
         var action = actionListInOrderToShow[index];
         actions.AppendLine(pre + _input.PlayMode.GetActionText(action));
      }

      const string sep = " ├  ";
      const string lastSep = " └  ";
      var keys = new StringBuilder();
      keys.AppendLine(_lang.GetText("Input", "KeyMouseHeader", "KEYBOARD/MOUSE"));
      for (var index = 0; index < actionListInOrderToShow.Count; index++)
      {
         var action = actionListInOrderToShow[index];
         keys.AppendLine((index < actionListInOrderToShow.Count - 1 ? sep : lastSep) + _input.PlayMode.GetKeyText(action));
      }
      
      var btns = new StringBuilder();
      btns.AppendLine(_lang.GetText("Input", "ControllerHeader", "CONTROLLER"));
      for (var index = 0; index < actionListInOrderToShow.Count; index++)
      {
         var action = actionListInOrderToShow[index];
         btns.AppendLine((index < actionListInOrderToShow.Count - 1 ? sep : lastSep) + _input.PlayMode.GetButtonText(action));
      }
      
      var titleForeColor = Configuration.GameColor2;
      var foreColor = Color.white;
      var shadowColor = "black";
      var shadowOff = 2;

      var textSize = 20;
      var fmtBindDescr = FormatLabel(actions.ToString(), textSize, foreColor);
      var fmtKeyDescr = FormatLabel(keys.ToString(), textSize, foreColor);
      var fmtBtnDescr = FormatLabel(btns.ToString(), textSize, foreColor);

      var fmtBindDescrShadow = FormatLabel(actions.ToString(), textSize, shadowColor);
      var fmtKeyDescrShadow = FormatLabel(keys.ToString(), textSize, shadowColor);
      var fmtBtnDescrShadow = FormatLabel(btns.ToString(), textSize, shadowColor);

      var pmLabel = _lang.GetText("PlayMode", "Title", "CONTROLS");
      
      var w1 = _lang.GetIntNum("PlayMode", "ColWidthAction", 450);
      var w2 = _lang.GetIntNum("PlayMode", "ColWidthController", 200);
      var w3 = _lang.GetIntNum("PlayMode", "ColWidthKeyMouse", 200);

      var boxWidth = w1 + w2 + w3 + 40;
      var boxHeight = 480;
      var boxPadding = 20;

      float xOffset = 850;
      float xOffset2 = xOffset + w1;
      float xOffset3 = xOffset2 + w2;

      float yPosOffset = 280;
      float yPosTitle = yPosOffset;
      float yPosInstr = yPosOffset + 50;
      
      GUI.Box(new Rect(xOffset - boxPadding, yPosOffset - boxPadding, boxWidth + boxPadding*2, boxHeight + boxPadding*2), "");

      GUI.Label(new Rect(xOffset + shadowOff, yPosTitle + shadowOff, 1000, 200), FormatLabel(pmLabel, 30, nameof(Color.black), bold: true));
      GUI.Label(new Rect(xOffset, yPosTitle, 1000, 200), FormatLabel(pmLabel, 30, titleForeColor, bold: true));

      GUI.Label(new Rect(xOffset + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBindDescrShadow);
      GUI.Label(new Rect(xOffset, yPosInstr, 2000, 2000), fmtBindDescr);

      GUI.Label(new Rect(xOffset2 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtKeyDescrShadow);
      GUI.Label(new Rect(xOffset2, yPosInstr, 2000, 2000), fmtKeyDescr);

      GUI.Label(new Rect(xOffset3 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBtnDescrShadow);
      GUI.Label(new Rect(xOffset3, yPosInstr, 2000, 2000), fmtBtnDescr);
   }

   
   private void DrawPhotoModeInstructionsOnHud()
   {
       var actionListInOrderToShow = new List<PhotoModeAction>
      {
         PhotoModeAction.Exit,
         PhotoModeAction.ShootPhoto,
         PhotoModeAction.ToggleInstructions,
         PhotoModeAction.ToggleHud,
         PhotoModeAction.MovePan,
         PhotoModeAction.UpDown,
         PhotoModeAction.Tilt,
         PhotoModeAction.SpeedUp,
         PhotoModeAction.Reset,
         PhotoModeAction.ToggleFoVDoF,
         PhotoModeAction.ChangeFoVDoF
      };

      var actions = new StringBuilder();
      string pre = " ► ";
      actions.AppendLine(_lang.GetText("Input", "ActionHeader", "ACTION"));
      for (var index = 0; index < actionListInOrderToShow.Count; index++)
      {
         var action = actionListInOrderToShow[index];
         actions.AppendLine(pre + _input.PhotoMode.GetActionText(action));
      }

      const string sep = " ├  ";
      const string lastSep = " └  ";
      var keys = new StringBuilder();
      keys.AppendLine(_lang.GetText("Input", "KeyMouseHeader", "KEYBOARD/MOUSE"));
      for (var index = 0; index < actionListInOrderToShow.Count; index++)
      {
         var action = actionListInOrderToShow[index];
         keys.AppendLine((index < actionListInOrderToShow.Count - 1 ? sep : lastSep) + _input.PhotoMode.GetKeyText(action));
      }
      
      var btns = new StringBuilder();
      btns.AppendLine(_lang.GetText("Input", "ControllerHeader", "CONTROLLER"));
      for (var index = 0; index < actionListInOrderToShow.Count; index++)
      {
         var action = actionListInOrderToShow[index];
         btns.AppendLine((index < actionListInOrderToShow.Count - 1 ? sep : lastSep) + _input.PhotoMode.GetButtonText(action));
      }
      
      var titleForeColor = Configuration.GameColor2;
      var foreColor = Color.white;
      var shadowColor = "black";
      var shadowOff = 2;

      var textSize = 20;
      var fmtBindDescr = FormatLabel(actions.ToString(), textSize, foreColor);
      var fmtKeyDescr = FormatLabel(keys.ToString(), textSize, foreColor);
      var fmtBtnDescr = FormatLabel(btns.ToString(), textSize, foreColor);

      var fmtBindDescrShadow = FormatLabel(actions.ToString(), textSize, shadowColor);
      var fmtKeyDescrShadow = FormatLabel(keys.ToString(), textSize, shadowColor);
      var fmtBtnDescrShadow = FormatLabel(btns.ToString(), textSize, shadowColor);

      var pmLabel = _lang.GetText("PhotoMode", "Title", "PHOTO MODE");
      var focusModeLabel = _lang.GetText("PhotoMode", "FocusAdjustModeLabel_{state}", "Focus mode: {0}", GetFocusModeText());
      var note = _lang.GetText("PhotoMode", "Note", "(This instructions box is not part of the photo)");

      var w1 = _lang.GetIntNum("PhotoMode", "ColWidthAction", 350);
      var w2 = _lang.GetIntNum("PhotoMode", "ColWidthController", 200);
      var w3 = _lang.GetIntNum("PhotoMode", "ColWidthKeyMouse", 200);

      var boxWidth = w1 + w2 + w3;
      var boxHeight = 450;
      var boxPadding = 20;

      float xOffset = 50;
      float xOffset2 = xOffset + w1;
      float xOffset3 = xOffset2 + w2;

      float yPosTitle = 200;
      float yPosInstr = 250;
      float yPosNote = 540;
      float yPosState = 600;
      float yPosSaveInfo = 680 + boxPadding;

      GUI.Box(new Rect(xOffset - boxPadding, yPosTitle - boxPadding, boxWidth + boxPadding*2, boxHeight + boxPadding*2), "");

      GUI.Label(new Rect(xOffset + shadowOff, yPosTitle + shadowOff, 1000, 200), FormatLabel(pmLabel, 30, nameof(Color.black), bold:true));
      GUI.Label(new Rect(xOffset, yPosTitle, 1000, 200), FormatLabel(pmLabel, 30, titleForeColor, bold:true));

      GUI.Label(new Rect(xOffset + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBindDescrShadow);
      GUI.Label(new Rect(xOffset, yPosInstr, 2000, 2000), fmtBindDescr);

      GUI.Label(new Rect(xOffset2 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtKeyDescrShadow);
      GUI.Label(new Rect(xOffset2, yPosInstr, 2000, 2000), fmtKeyDescr);

      GUI.Label(new Rect(xOffset3 + shadowOff, yPosInstr + shadowOff, 2000, 2000), fmtBtnDescrShadow);
      GUI.Label(new Rect(xOffset3, yPosInstr, 2000, 2000), fmtBtnDescr);

      GUI.Label(new Rect(xOffset + shadowOff, yPosState + shadowOff, 1000, 200), 
         FormatLabel(focusModeLabel, 20, shadowColor));
      GUI.Label(new Rect(xOffset, yPosState, 1000, 200), FormatLabel(focusModeLabel, 20, "lightblue"));

      GUI.Label(new Rect(xOffset + 1, yPosNote + 1, 1000, 200), FormatLabel(note, 15, nameof(Color.black)));
      GUI.Label(new Rect(xOffset, yPosNote, 1000, 200), FormatLabel(note, 15, "#CCCCCC"));

      if (!string.IsNullOrEmpty(_state.LastScreenshotInfo))
      {
         GUI.Box(new Rect(xOffset - boxPadding, yPosSaveInfo - boxPadding, boxWidth + boxPadding*2, 90), "");

         GUI.Label(new Rect(xOffset + shadowOff, yPosSaveInfo + shadowOff, boxWidth, 200), FormatLabel(_state.LastScreenshotInfo, 20, shadowColor));
         GUI.Label(new Rect(xOffset, yPosSaveInfo, boxWidth, 200), FormatLabel(_state.LastScreenshotInfo, 20, Configuration.GameColor2));
      }
   }


   private string GetFocusModeText()
   {
      switch (_camera.FocusAdjustMode)
      {
         case CameraFocusAdjustMode.DepthOfField :
            var dofLabel = _lang.GetText("PhotoMode", "DepthOfField_{dof}", "depth of field distance ({0})", _camera.DepthOfField);
            return dofLabel;
            
         default:
         case CameraFocusAdjustMode.FieldOfView:
            var fovLabel = _lang.GetText("PhotoMode", "FieldOfView_{fov}", "field of view ({0})", _camera.FieldOfView);
            return fovLabel;
      }
   }


   private string FormatLabel(string text, int size, Color color, bool bold = false, bool italic = false)
   {
      return FormatLabel(text, size, color.ToString(), bold, italic);
   }


   private string FormatLabel(string text, int size, string color, bool bold = false, bool italic = false)
   {
      string preFmt = "";
      string postFmt = "";
      if (bold)
      {
         preFmt += "<b>";
         postFmt += "</b>";
      }
      if (italic)
      {
         preFmt += "<i>";
         postFmt += "</i>";
      }
      
      var str = String.Format("{3}<color={2}><size={1}>{0}</size></color>{4}", text, size, color, preFmt, postFmt);
      return str;
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
               _fpsDisplayThreshold--;
               if (_fpsDisplayThreshold <= 0)
               {
                  _displayedFps = _state.Fps;
                  _fpsDisplayThreshold = _state.Fps;
               }
               target.Append(_lang.GetText("Mod", "FPSLabel_{fps}", "FPS: {0}", _displayedFps));
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