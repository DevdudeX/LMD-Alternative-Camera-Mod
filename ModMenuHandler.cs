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
			// For debugging
			MenuManager.Instance.RegisterInfoItem(InfoName, "State: ", 8, MenuGetGamepadState);

			// Action buttons
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Mod Active", 0, MenuToggleCameraMod);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Game Hud", 1, MenuToggleHud);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Load Preset: Third person", 2, MenuLoadPresetDefault);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Load Preset: First person", 3, MenuLoadPresetFirstPerson);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Invert Look Horizontal", 4, MenuToggleInvertLookHorizontal);
			MenuManager.Instance.RegisterActionBtn(InfoName, "Toggle Auto Align", 5, MenuToggleAutoAlign);
			MenuManager.Instance.RegisterActionBtn(InfoName, "DEBUG: Try Fix References", 6, MenuFindReferences);
			MenuManager.Instance.RegisterActionBtn(InfoName, "DEBUG: Toggle Alt Input Mapping", 7, MenuToggleGamepadAltInputMapping);
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
		void MenuToggleGamepadAltInputMapping(int callbackID)
		{
			altCamScript.ToggleGamepadAltMapping();
		}


		void MenuFindReferences(int callbackID)
		{
			altCamScript.GetAllRequiredReferences();
		}


		string MenuGetGamepadState()
		{
			//anyGamepadDpadHorizontal                   anyGamepadDpadVertical
			//anyGamepadTriggerInputL                    anyGamepadTriggerInputR
			//anyGamepadStickHorizontalR                 anyGamepadStickVerticalR
			//anyGamepadBtn0(Gamepad [A] held state)     anyGamepadBtn5 (Gamepad [Right Bumper] held state)
			//anyGamepadBtnDown1
			//anyGamepadBtnDown2
			//anyGamepadBtnDown3
			//anyGamepadBtnDown4
			//anyGamepadBtnDown5
			//anyGamepadBtnDown7
			string[] gamepadDataArray = altCamScript.GetGamepadState();
			string allData = "";

			foreach (string dataEntry in gamepadDataArray)
			{
				if (string.IsNullOrEmpty(dataEntry))
				{
					continue;
				}
				allData += $"{dataEntry} | ";
			}

			return allData;
		}
	}
}
