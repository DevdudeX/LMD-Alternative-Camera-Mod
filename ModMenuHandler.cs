using LMD_ModMenu;

namespace AlternativeCameraMod
{
	/// <summary>
	/// Takes care of integration with the LMD Mod Menu plugin.
	/// </summary>
	internal class ModMenuHandler
	{
		AlternativeCamera altCamScript;
		string InfoName = "Alternative Camera";

		public void HandleOnInitializeMelon(AlternativeCamera alternativeCamera)
		{
			altCamScript = alternativeCamera;

			// Mod Menu
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Mod Active", 0, MenuToggleCameraMod);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Game Hud", 1, MenuToggleHud);

			MenuManager.Instance.RegisterActionBtn(InfoName, "Load Preset: Third person", 2, MenuLoadPresetDefault);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Load Preset: First person", 3, MenuLoadPresetFirstPerson);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Invert Look Horizontal", 4, MenuToggleInvertLookHorizontal);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Auto Align", 5, MenuToggleAutoAlign);
			MenuManager.Instance.RegisterActionBtn(InfoName, "DEBUG: Try Fix References", 6, MenuFindReferences);


			//MenuManager.Instance.RegisterInfoItem(InfoName, "State: ", 6, GetReplayState);
		}


		// MOD MENU CONTROLS ==============
		void MenuToggleCameraMod(int callbackID)
		{
			altCamScript.ToggleModState();
		}

		void MenuToggleHud(int callbackID)
		{
			altCamScript.ToggleGameHUD();
		}

		// Camera presets
		void MenuLoadPresetDefault(int callbackID)
		{
			altCamScript.LoadPresetDefault();
		}
		void MenuLoadPresetFirstPerson(int callbackID)
		{
			altCamScript.LoadPresetFirstPerson();
		}

		// Settings
		void MenuToggleInvertLookHorizontal(int callbackID)
		{
			altCamScript.ToggleInvertedHorizontalMode();
		}
		void MenuToggleAutoAlign(int callbackID)
		{
			altCamScript.ToggleAutoAlignMode();
		}


		void MenuFindReferences(int callbackID)
		{
			altCamScript.GetAllRequiredReferences();
		}
	}
}
