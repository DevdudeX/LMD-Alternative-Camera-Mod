using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMSR_AlternativeCameraMod
{
	/// <summary>
	/// Holds unchanging hardcoded values like specific GameObject names, prefixes and paths.
	/// </summary>
	internal static class Constants
	{
		// Targets
		internal const string PLAY_CAMERA_NAME = "PlayCamera(Clone)";

		internal const string TRACKING_FIRSTPERSON_NAME = "neck_Anim_JNT";
		internal const string TRACKING_THIRDPERSON_PREFIX = "player00_SkiPlayerSimulation";

		internal const string PLAYER_VISUALS_HOLDER_PREFIX = "player00_SkiPlayerDisplay";
		internal const string PLAYER_AUDIO_HOLDER_NAME = "Audio";
		internal const string POST_PROCESSING_HOLDER_NAME = "DefaultPostProcessing";

		// UI --------------------
		// NAMES
		internal const string UI_ROOT_NAME = "UI_Root(Clone)";
		internal const string UI_CAMERA_NAMEPATH = "UI_Root(Clone)/UI_Camera";
		internal const string UI_CANVAS_NAME = "UI_Canvas";
		internal const string UI_SCREEN_PARENT_NAMEPATH = "SafetyZone/Screen_Parent";
		internal const string UI_START_MENU_NAME = "UI_Menu_StartMenu(Clone)";
		

		internal const string UI_PAUSE_SCREEN_NAME = "UI_Game_PauseScreen(Clone)";
		internal const string UI_HUD_NAME = "UI_HUD(Clone)";
		internal const string UI_LOADING_SCREEN_NAMEPATH = "UI_Root(Clone)/UI_HighPriority(Clone)/Loading";

		// PREFIXES
		internal const string UI_MENU_PREFIX = "UI_Menu";
		internal const string UI_HUD_PREFIX = "UI_HUD";


	}
}
