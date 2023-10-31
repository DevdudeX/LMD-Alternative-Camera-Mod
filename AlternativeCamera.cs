using UnityEngine;
using MelonLoader;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System;
using Il2CppMegagon.Downhill.Cameras;
using AlternativeCameraMod;

[assembly: MelonInfo(typeof(AlternativeCamera), "Alternative Camera", "1.0.4", "DevdudeX")]
[assembly: MelonGame()]
namespace AlternativeCameraMod
{
	/// <summary>
	/// Manages the alternative camera.
	/// </summary>
	public class AlternativeCamera : MelonMod
	{
		// Keep this updated!
		private const string MOD_VERSION = "1.0.4";
		public static AlternativeCamera instance;
		private static bool hasStartedOnce = false;
		private static bool cameraModEnabled = true;
		private static bool forceDisable = false;

		private MelonPreferences_Category mouseSettingsCat;
		private MelonPreferences_Entry<float> cfg_mSensitivityHorizontal;
		private MelonPreferences_Entry<float> cfg_mSensitivityVertical;
		private MelonPreferences_Entry<float> cfg_mSensitivityMultiplier;
		private MelonPreferences_Entry<bool> cfg_mInvertHorizontal;

		private MelonPreferences_Category gamepadSettingsCat;
		private MelonPreferences_Entry<float> cfg_gamepadStickDeadzoneR;
		private MelonPreferences_Entry<float> cfg_gamepadSensHorizontal;
		private MelonPreferences_Entry<float> cfg_gamepadSensVertical;
		private MelonPreferences_Entry<float> cfg_gamepadSensMultiplier;
		private MelonPreferences_Entry<bool> cfg_gamepadInvertHorizontal;
		private MelonPreferences_Entry<bool> cfg_gamepadInvertVertical;

		private MelonPreferences_Category cameraSettingsCat;
		private MelonPreferences_Entry<float> cfgCameraCollisionPadding;
		private MelonPreferences_Entry<float> cfgZoomLerpOutSpeed;
		private MelonPreferences_Entry<float> cfgZoomLerpInSpeed;
		private MelonPreferences_Entry<bool> cfgDefaultCameraOnPause;
		private MelonPreferences_Entry<bool> cfgCameraAutoAlign;
		private MelonPreferences_Entry<float> cfgAutoAlignSpeed;
		private MelonPreferences_Entry<float> cfgZoomStepIncrement;
		private MelonPreferences_Entry<float> cfgStandardFoV;
		private MelonPreferences_Entry<float> cfgFirstPersonFoV;

		private MelonPreferences_Category otherSettingsCat;
		private MelonPreferences_Entry<bool> cfgAutoEnableOnLevelLoad;

		private static KeyCode modToggleKey = KeyCode.Alpha0;
		private static KeyCode modStartupKey = KeyCode.Alpha9;
		private static KeyCode uiToggleKey = KeyCode.Keypad7;
		private static KeyCode grabGOsKey = KeyCode.Keypad9;
		private static KeyCode camPresetStandardKey = KeyCode.Keypad1;
		private static KeyCode camPresetFirstPersonKey = KeyCode.Keypad2;
		private static KeyCode camToggleInvertHorizontalKey = KeyCode.Keypad3;
		private static KeyCode camToggleAutoAlignKey = KeyCode.Keypad4;

		// Transforms and GameObjects
		/// <summary>The name of the gameobject that will act as the cameras target.</summary>
		private static string targetName = "Bike(Clone)";
		private static Transform playerBikeParentTransform;
		private static Transform playerBikeTransform;
		private static Transform camTransform;

		/// <summary>The main camera itself. Used to set the field of view.</summary>
		private static Camera mainCameraComponent;
		/// <summary>The ui camera. Used to toggle hud rendering.</summary>
		private static Camera uiRendererCamera;
		private static PlayCamera defaultCameraScript;

		// UI GameObjects
		private static Transform ui_mainUIParent;
		private static GameObject ui_pauseMenuUI;
		private static GameObject ui_settingsUI;
		private static GameObject ui_controlsUI;
		private static GameObject ui_resultScreenUI;
		private static GameObject ui_highscoreStandaloneUI;
		private static GameObject ui_dailyChallengeStandaloneUI;
		private static GameObject ui_cutsceneUI;
		private static GameObject ui_cutsceneLocationUI;

		// Gameplay Settings
		private static Vector3 targetOffset = new Vector3(0f, 2.4f, 0f);
		private static LayerMask cameraCollisionLayers = LayerMask.GetMask("Ground","Obstacle","EnvironmentOther","Terrain","Lava");

		// Camera angle limits
		private static int xMinLimit = -82;
		private static int xMaxLimit = 82;

		// Active variables
		private static bool hasMenuOpen = true;
		//static bool invertCamVertical = false;	// WIP
		private static float wantedZoom = 8f;
		private static float targetZoomAmount;
		private static Quaternion rotation;

		/// <summary>The distance from the bike to any world-collision between it and the camera.</summary>
		private static float projectedDistance = 200f;
		/// <summary>Camera rotation around vertical y-axis (left-right)</summary>
		private static float rotHorizontal;
		/// <summary>Camera rotation around x-axis (ear-to-ear or up-down)</summary>
		private static float rotVertical;
		private static Vector3 dirToCam;

		public override void OnEarlyInitializeMelon()
		{
			instance = this;
			MelonEvents.OnGUI.Subscribe(DrawDemoText, 100);
		}

		public override void OnInitializeMelon()
		{
			mouseSettingsCat = MelonPreferences.CreateCategory("Mouse Settings");
			mouseSettingsCat.SetFilePath("UserData/AlternativeCameraSettings.cfg");

			gamepadSettingsCat = MelonPreferences.CreateCategory("Gamepad Settings");
			gamepadSettingsCat.SetFilePath("UserData/AlternativeCameraSettings.cfg");

			cameraSettingsCat = MelonPreferences.CreateCategory("Camera Settings");
			cameraSettingsCat.SetFilePath("UserData/AlternativeCameraSettings.cfg");

			otherSettingsCat = MelonPreferences.CreateCategory("Other Settings");
			otherSettingsCat.SetFilePath("UserData/AlternativeCameraSettings.cfg");

			// Mouse Settings
			cfg_mSensitivityHorizontal = mouseSettingsCat.CreateEntry<float>("HorizontalSensitivity", 1.4f);
			cfg_mSensitivityVertical = mouseSettingsCat.CreateEntry<float>("VerticalSensitivity", 1.4f);
			cfg_mSensitivityMultiplier = mouseSettingsCat.CreateEntry<float>("SensitivityMultiplier", 1f);
			cfg_mInvertHorizontal = mouseSettingsCat.CreateEntry<bool>("InvertHorizontal", false);
			cfgZoomStepIncrement = mouseSettingsCat.CreateEntry<float>("ZoomStepIncrement", 0.20f, description:"How much one scroll zooms the camera.");

			// Gamepad Settings
			cfg_gamepadStickDeadzoneR = gamepadSettingsCat.CreateEntry<float>("GamepadStickDeadzoneR", 0.1f);
			cfg_gamepadSensHorizontal = gamepadSettingsCat.CreateEntry<float>("GamepadHorizontalSensitivity", 1f);
			cfg_gamepadSensVertical = gamepadSettingsCat.CreateEntry<float>("GamepadVerticalSensitivity", 1f);
			cfg_gamepadSensMultiplier = gamepadSettingsCat.CreateEntry<float>("GamepadSensitivityMultiplier", 1f);
			cfg_gamepadInvertHorizontal = gamepadSettingsCat.CreateEntry<bool>("GamepadInvertHorizontal", false);
			cfg_gamepadInvertVertical = gamepadSettingsCat.CreateEntry<bool>("GamepadInvertVertical", false);

			// Camera Settings
			cfgZoomLerpOutSpeed = cameraSettingsCat.CreateEntry<float>("ZoomOutLerpSpeed", 1.0f);
			cfgZoomLerpInSpeed = cameraSettingsCat.CreateEntry<float>("ZoomInLerpSpeed", 0.0880f);
			cfgCameraCollisionPadding = cameraSettingsCat.CreateEntry<float>("CameraCollisionPadding", 0.20f, description:"Distance the camera is pushed away from terrain.");
			cfgDefaultCameraOnPause = cameraSettingsCat.CreateEntry<bool>("DefaultCameraOnPause", true);
			cfgCameraAutoAlign = cameraSettingsCat.CreateEntry<bool>("CameraAutoAlign", true);
			cfgAutoAlignSpeed = cameraSettingsCat.CreateEntry<float>("AutoAlignSpeed", 1.80f, description:"How quickly the camera moves behind the player.");
			cfgStandardFoV = cameraSettingsCat.CreateEntry<float>("StandardFoV", 70f);
			cfgFirstPersonFoV = cameraSettingsCat.CreateEntry<float>("FirstPersonFoV", 98f);

			// Other Settings
			cfgAutoEnableOnLevelLoad = otherSettingsCat.CreateEntry<bool>("EnableAltCameraOnLevelLoad", true);

			mouseSettingsCat.SaveToFile();
			gamepadSettingsCat.SaveToFile();
			cameraSettingsCat.SaveToFile();
			otherSettingsCat.SaveToFile();
		}

		public override void OnLateUpdate()
		{
			if (forceDisable) {
				// Used for OnDeinitializeMelon()
				return;
			}

			if (Input.GetKeyDown(modStartupKey))
			{
				StartUpMod();
			}

			// FIRST CHECKPOINT: Mod not ready
			if (!hasStartedOnce) {
				return;
			}

			// Here to allow disabling HUD while using the normal camera.
			if (Input.GetKeyDown(uiToggleKey))
			{
				ToggleGameHUD();
			}

			if (Input.GetKeyDown(modToggleKey))
			{
				cameraModEnabled = !cameraModEnabled;
				if (!hasStartedOnce) {
					return;
				}

				if (cameraModEnabled == false && defaultCameraScript.enabled == false)
				{
					// Turn the mod OFF
					defaultCameraScript.enabled = true;
					ApplyDefaultCameraSettings();
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				else
				{
					// Turn the mod ON
					defaultCameraScript.enabled = false;
					ApplyCameraSettings(6f, new Vector3(0f, 2.4f, 0f), 70f, 0.28f, "Bike(Clone)");
					AlignViewWithBike();
				}
			}

			// SECOND CHECKPOINT: Mod not enabled
			if (cameraModEnabled == false) {
				return;
			}


			// ==================== MAIN MOD METHODS ====================
			hasMenuOpen = GameMenuUiActive();

			// If the menu is open and default camera on pause is set don't run any functions
			if (hasMenuOpen && defaultCameraScript.enabled == false && cfgDefaultCameraOnPause.Value)
			{
				defaultCameraScript.enabled = true;
				return;
			}

			// All keybind handling
			HandleSettingsInputs();

			if (playerBikeTransform == null)
			{
				return;
			}

			CameraLogic();
		}

		/// <summary>
		/// Handles the processing of the position and rotation of the camera.
		/// </summary>
		private void CameraLogic()
		{
			dirToCam = camTransform.position - playerBikeTransform.TransformPoint(targetOffset);

			// Clamp distance at 0
			if (wantedZoom < 0.0f) {
				wantedZoom = 0.0f;
			}
			if (targetZoomAmount < 0.0f) {
				targetZoomAmount = 0.0f;
			}

			// Paused game check; only run when playing
			if (!hasMenuOpen)
			{
				// Double check that the default camera is disabled
				if (defaultCameraScript.enabled == true)
				{
					defaultCameraScript.enabled = false;
				}

				if(Input.anyKeyDown)
				{
					//LoggerInstance.Msg("Input ==> ["+ Input.inputString +"]");
				}

				// Lock and hide the cursor
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				if (Input.GetAxis("Mouse ScrollWheel") > 0f)
				{
					// Scrolling forward; zoom in
					wantedZoom -= cfgZoomStepIncrement.Value;
				}
				else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
				{
					// Scrolling backwards; zoom out
					wantedZoom += cfgZoomStepIncrement.Value;
				}

				float gamepadHorizontalInputRStick = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
				float gamepadVerticalInputRStick = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

				//float totalHorizontalInputValue = Input.GetAxisRaw("Mouse X") + gamepadHorizontalInput;
				//float totalVerticalInputValue = Input.GetAxisRaw("Mouse Y") + gamepadVerticalInput;

				bool anyJoystickButton5 = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);
				bool holdingInvertAutoAlign = anyJoystickButton5 || Input.GetKey(KeyCode.Mouse1);

				// Horizontal mouse movement will make camera rotate around vertical y-axis
				// Vertical mouse movement will make camera rotate along x-axis (your ear-to-ear axis)
				rotHorizontal += Input.GetAxisRaw("Mouse X") * cfg_mSensitivityHorizontal.Value * cfg_mSensitivityMultiplier.Value;
				rotVertical += Input.GetAxisRaw("Mouse Y") * cfg_mSensitivityVertical.Value * cfg_mSensitivityMultiplier.Value;

				// Also take controller input
				rotHorizontal += ApplyInnerDeadzone(gamepadHorizontalInputRStick, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensHorizontal.Value * cfg_gamepadSensMultiplier.Value;
				rotVertical -= ApplyInnerDeadzone(gamepadVerticalInputRStick, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensVertical.Value * cfg_gamepadSensMultiplier.Value;
				rotVertical = ClampAngle(rotVertical, (float)xMinLimit, (float)xMaxLimit);  // Clamp the up-down rotation

				/*
					JoystickButton0 - X
					JoystickButton1 - A
					JoystickButton2 - B
					JoystickButton3 - Y
					JoystickButton4 - LB
					JoystickButton5 - RB
					JoystickButton6 - LT
					JoystickButton7 - RT
					JoystickButton8 - back
					JoystickButton9 - start
					JoystickButton10 - left stick[not direction, button]
					JoystickButton11 - right stick[not direction, button]
				*/

				if (cfg_mInvertHorizontal.Value == true)
				{
					// Auto align is on and no input is being given
					if (cfgCameraAutoAlign.Value == true && !holdingInvertAutoAlign)
					{
						// Lerp the horizontal rotation relative to the player
						rotHorizontal = Mathf.LerpAngle(rotHorizontal, -playerBikeParentTransform.localRotation.eulerAngles.y, cfgAutoAlignSpeed.Value * Time.deltaTime);
						rotHorizontal = ClampAngle(rotHorizontal, -360, 360);
					}
					rotation = Quaternion.Euler(-rotVertical, -rotHorizontal, 0f);
				}
				else
				{
					if (cfgCameraAutoAlign.Value == true && !holdingInvertAutoAlign)
					{
						// Lerp the horizontal rotation relative to the player
						rotHorizontal = Mathf.LerpAngle(rotHorizontal, playerBikeParentTransform.localRotation.eulerAngles.y, cfgAutoAlignSpeed.Value * Time.deltaTime);
						rotHorizontal = ClampAngle(rotHorizontal, -360, 360);
					}
					rotation = Quaternion.Euler(-rotVertical, rotHorizontal, 0f);
				}

				RaycastHit hitInfo;
				// Raycast from the target towards the camera
				if (Physics.Raycast(playerBikeTransform.TransformPoint(targetOffset), dirToCam.normalized, out hitInfo, wantedZoom + 0.2f, cameraCollisionLayers))
				{
					projectedDistance = Vector3.Distance(hitInfo.point, playerBikeTransform.TransformPoint(targetOffset));
				} else
				{
					projectedDistance = 900;
				}

				if (projectedDistance < wantedZoom)
				{
					// Desired camera distance is greater than the collision distance so zoom in to prevent clipping
					// b=bike, c=camera, *=collision
					// b-------*---c
					// b------c*
					float newTargetZoom = projectedDistance - cfgCameraCollisionPadding.Value;
					targetZoomAmount = Mathf.Lerp(targetZoomAmount, newTargetZoom, cfgZoomLerpInSpeed.Value);
				} else
				{
					// Zoom the camera back out to wanted distance over time
					targetZoomAmount = Mathf.Lerp(targetZoomAmount, wantedZoom, Time.deltaTime * cfgZoomLerpOutSpeed.Value);
				}

				Vector3 finalPosition = rotation * new Vector3(0f, 0f, -targetZoomAmount) + playerBikeTransform.TransformPoint(targetOffset);

				// Apply values
				camTransform.position = finalPosition;
				camTransform.rotation = rotation;
			}
			else	// The menu is open; game is paused
			{
				// While paused show the cursor
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				// and optionally show the default camera
				if (defaultCameraScript.enabled == false && cfgDefaultCameraOnPause.Value)
				{
					defaultCameraScript.enabled = true;
				}
			}
		}

		/// <summary>
		/// Grabs required objects, applies standard camera settings and enables the hasStartedOnce bool.
		/// </summary>
		private void StartUpMod()
		{
			LoggerInstance.Msg("Starting alternative camera system!");

			// Assigning GO's
			GetTargetGameObjects();

			// Very hacky way of testing if paused
			GetUiObjects();

			// Apply some starting camera settings
			ApplyCameraSettings(5.4f, new Vector3(0f, 2.4f, 0f), cfgStandardFoV.Value, 0.28f, "Bike(Clone)");

			Vector3 eulerAngles = camTransform.eulerAngles;
			rotHorizontal = eulerAngles.y;
			rotVertical = eulerAngles.x;

			AlignViewWithBike();

			hasStartedOnce = true;
		}

		/// <summary>
		/// Handles all settings keybinds.
		/// </summary>
		private void HandleSettingsInputs()
		{
			if (Input.GetKeyDown(grabGOsKey))
			{
				// Find gameobjects again/update references on level load
				GetTargetGameObjects();
				GetUiObjects();
				AlignViewWithBike();
			}
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
			{
				// On checkpoint restarts || On track restarts
				AlignViewWithBike();
			}

			if (Input.GetKeyDown(camPresetStandardKey))
			{
				// Standard
				ApplyCameraSettings(5.4f, new Vector3(0f, 2.4f, 0f), cfgStandardFoV.Value, 0.28f, "Bike(Clone)");
			}
			if (Input.GetKeyDown(camPresetFirstPersonKey))
			{
				// First Person
				ApplyCameraSettings(0f, (new Vector3(0.0f, 0.3f, 0f)), cfgFirstPersonFoV.Value, 0.6f, "neck_BindJNT");

				// Navigate to bike mesh renderer to prevent it from vanishing in first person
				SkinnedMeshRenderer bikeMeshRenderer = playerBikeParentTransform.GetChild(7).transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
				bikeMeshRenderer.updateWhenOffscreen = true;
			}

			// Mouse inverting
			if (Input.GetKeyDown(camToggleInvertHorizontalKey))
			{
				cfg_mInvertHorizontal.Value = !cfg_mInvertHorizontal.Value;
				LoggerInstance.Msg("Toggled invert camera horizontal ==> ["+ cfg_mInvertHorizontal.Value +"]");
				AlignViewWithBike();
			}

			// Camera auto align
			if (Input.GetKeyDown(camToggleAutoAlignKey))
			{
				cfgCameraAutoAlign.Value = !cfgCameraAutoAlign.Value;
				LoggerInstance.Msg("Toggled auto-align ==> ["+ cfgCameraAutoAlign.Value +"]");
			}
		}

		/// <summary>
		/// Finds and assigns important GameObjects and Transforms.
		/// </summary>
		private static void GetTargetGameObjects()
		{
			playerBikeParentTransform = GameObject.Find("Bike(Clone)").GetComponent<Transform>();
			playerBikeTransform = GameObject.Find(targetName).GetComponent<Transform>();
			camTransform = GameObject.Find("PlayCamera(Clone)").GetComponent<Transform>();
			mainCameraComponent = camTransform.gameObject.GetComponent<Camera>();
			defaultCameraScript = camTransform.gameObject.GetComponent<PlayCamera>();
		}

		/// <summary>
		/// Finds and assigns all relevant UI GameObjects.
		/// </summary>
		private static void GetUiObjects()
		{
			uiRendererCamera = GameObject.Find("UICam").GetComponent<Camera>();
			ui_mainUIParent = GameObject.Find("Wrapper").GetComponent<Transform>();

			ui_pauseMenuUI = ui_mainUIParent.Find("PauseScreen(Clone)").gameObject;
			ui_settingsUI = ui_mainUIParent.Find("SettingsScreen(Clone)").gameObject;
			ui_controlsUI = ui_mainUIParent.Find("TutorialScreen(Clone)").gameObject;
			ui_resultScreenUI = ui_mainUIParent.Find("ResultScreen(Clone)").gameObject;
			ui_highscoreStandaloneUI = ui_mainUIParent.Find("HighscoreStandalone(Clone)").gameObject;
			ui_dailyChallengeStandaloneUI = ui_mainUIParent.Find("DailyChallengesStandalone(Clone)").gameObject;
			ui_cutsceneUI = ui_mainUIParent.Find("CutsceneScreen(Clone)").gameObject;

			GameObject playscreenParentUI = ui_mainUIParent.Find("PlayScreen(Clone)").gameObject;
			if (playscreenParentUI != null)
			{
				GameObject playscreenGroupUI = playscreenParentUI.transform.Find("PlayScreen_Group").gameObject;
				if (playscreenGroupUI != null)
				{
					ui_cutsceneLocationUI = playscreenGroupUI.transform.Find("HiddenLocationPanel").gameObject;
				}
			}

			Debug.Log("[AltCameraMod]Debug: Assigned UI GameObjects");
		}

		/// <summary>
		/// Resets the camera settings to default values (34 FoV?).
		/// </summary>
		private void ApplyDefaultCameraSettings()
		{
			mainCameraComponent.fieldOfView = 38f;	// or 34?
			mainCameraComponent.nearClipPlane = 0.3f;
		}
		/// <summary>
		/// Allows applying multiple camera settings quickly.
		/// </summary>
		private void ApplyCameraSettings(float followDistance, Vector3 followTargetOffset, float cameraFov, float nearClipPlane, string followTargetName)
		{
			targetName = followTargetName;
			// Update references
			GetTargetGameObjects();

			wantedZoom = followDistance;
			targetOffset = followTargetOffset;

			mainCameraComponent.fieldOfView = cameraFov;	// Default: 34
			mainCameraComponent.nearClipPlane = nearClipPlane;	// Default: 0.3

			LoggerInstance.Msg("Applied a camera preset!");
		}

		/// <summary>
		/// Checks if any menu gui is currently on screen.
		/// </summary>
		private static bool GameMenuUiActive()
		{
			if (ui_pauseMenuUI.active == true ||
				ui_settingsUI.active == true ||
				ui_controlsUI.active == true ||
				ui_resultScreenUI.active == true ||
				ui_highscoreStandaloneUI.active == true ||
				ui_dailyChallengeStandaloneUI.active == true ||
				ui_cutsceneUI.active == true
			)
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// Checks if the secret area gui is currently on screen.
		/// </summary>
		private static bool CutsceneSecretUiActive()
		{
			if (ui_cutsceneLocationUI != null && ui_cutsceneLocationUI.active == true) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Makes the camera move to directly behind the player.
		/// Useful for restarting at checkpoints.
		/// </summary>
		private void AlignViewWithBike()
		{
			if (playerBikeParentTransform.gameObject == null) {
				return;
			}
			Vector3 bikeRotation = playerBikeParentTransform.localRotation.eulerAngles;
			if (cfg_mInvertHorizontal.Value == true)
			{
				rotHorizontal = -bikeRotation.y;
			}
			else
			{
				rotHorizontal = bikeRotation.y;
			}
		}

		/// <summary>
		/// Toggles the rendering of the game HUD.
		/// </summary>
		private void ToggleGameHUD()
		{
			if (uiRendererCamera == null) {
				return;
			}
			ToggleGameHUD(!uiRendererCamera.enabled);
		}
		/// <summary>
		/// Enables or disables rendering of the game HUD.
		/// </summary>
		/// <param name="visible">Should the HUD be rendered.</param>
		private void ToggleGameHUD(bool visible)
		{
			if (uiRendererCamera == null) {
				return;
			}
			// Toggle UI camera rendering
			uiRendererCamera.enabled = visible;
		}

		/// <summary>
		/// Tries to clamp the angle to values between 360 and -360.
		/// </summary>
		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f) {
				angle += 360f;
			}
			if (angle > 360f) {
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}

		/// <summary>
		/// Snaps the given value to 0 if it falls within the deadzone radius in either direction.
		/// </summary>
		/// <returns>The axis if outside the deadzone, otherwise returns 0.</returns>
		private static float ApplyInnerDeadzone(float axis, float deadzone)
		{
			if (axis > deadzone) {
				return axis;
			}
			if (axis < -deadzone) {
				return axis;
			}
			return 0;
		}


		public static void DrawDemoText()
		{
			GUI.Label(new Rect(20, 20, 1000, 200), "<b><color=white><size=16>DevdudeX's Camera Mod Demo v"+ MOD_VERSION +"</size></color></b>");
		}
		public override void OnDeinitializeMelon()
		{
			// In case the melon gets unregistered
			forceDisable = true;
			MelonEvents.OnGUI.Unsubscribe(DrawDemoText);
		}
	}
}
