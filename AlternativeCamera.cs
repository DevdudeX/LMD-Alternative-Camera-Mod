// Mod
using MelonLoader;
using AlternativeCameraMod;
using LMD_ModMenu;
// Unity
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
// Megagon
using Il2CppMegagon.Downhill.Cameras;

[assembly: MelonInfo(typeof(AlternativeCamera), "Alternative Camera", "1.0.7", "DevdudeX")]
[assembly: MelonGame()]
namespace AlternativeCameraMod
{
	/// <summary>
	/// Manages the alternative camera system.
	/// </summary>
	public class AlternativeCamera : MelonMod
	{
		// Keep this updated!
		private const string MOD_VERSION = "1.0.7";
		public static AlternativeCamera instance;
		private static bool hasStartedOnce = false;
		private static bool cameraModEnabled = false;
		private static bool forceDisable = false;

		#region Configuration Settings
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
		private MelonPreferences_Entry<bool> cfgCameraAlwaysAutoAlign;
		private MelonPreferences_Entry<float> cfgAutoAlignSpeed;
		private MelonPreferences_Entry<float> cfgZoomStepIncrement;
		private MelonPreferences_Entry<float> cfgStandardFoV;
		private MelonPreferences_Entry<float> cfgFirstPersonFoV;

		private MelonPreferences_Category otherSettingsCat;
		private MelonPreferences_Entry<bool> cfgAutoEnableOnLevelLoad;
		private MelonPreferences_Entry<float> cfgFocalLength;
		private MelonPreferences_Entry<float> cfgFocusDistanceOffset;

		private static KeyCode modToggleKey = KeyCode.Alpha0;
		private static KeyCode modStartupKey = KeyCode.Alpha9;
		private static KeyCode uiToggleKey = KeyCode.Keypad7;
		private static KeyCode grabGOsKey = KeyCode.Keypad9;
		private static KeyCode camPresetStandardKey = KeyCode.Keypad1;
		private static KeyCode camPresetFirstPersonKey = KeyCode.Keypad2;
		private static KeyCode camToggleInvertHorizontalKey = KeyCode.Keypad3;
		private static KeyCode camToggleAutoAlignKey = KeyCode.Keypad4;
		private static KeyCode focalLegthModeKey = KeyCode.L;
		private static KeyCode focusDistanceModeKey = KeyCode.K;

		// Gameplay Settings
		private static Vector3 targetOffset = new Vector3(0f, 2.4f, 0f);
		private static LayerMask cameraCollisionLayers = LayerMask.GetMask("Ground","Obstacle","EnvironmentOther","Terrain","Lava");

		// Camera angle limits
		private static readonly int xMinLimit = -82;
		private static readonly int xMaxLimit = 82;

		#endregion

		// Transforms and GameObjects
		/// <summary>The name of the gameobject that will act as the cameras target.</summary>
		private static string targetName = "Bike(Clone)";
		private static Transform playerBikeParentTransform;
		private static Transform playerBikeTransform;
		private static Transform camTransform;
		private GameObject postProcessingObject;
		private DepthOfField m_dofSettings;

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


		// Active variables
		private static bool hasMenuOpen = true;
		private static bool hasDOFSettings;
		private static float wantedZoom = 8f;
		private static float targetZoomAmount;
		private static Quaternion rotation;
		private static float baseFocalLength;
		private static float baseFoV;
		private static float wantedFocalLength;

		/// <summary>The distance from the bike to any world-collision between it and the camera.</summary>
		private static float projectedDistance = 200f;
		/// <summary>Camera rotation around vertical y-axis (left-right)</summary>
		private static float rotHorizontal;
		/// <summary>Camera rotation around x-axis (ear-to-ear or up-down)</summary>
		private static float rotVertical;
		private static Vector3 dirToCam;


		// Gamepad Inputs
		private float anyGamepadDpadHorizontal;
		private float anyGamepadDpadVertical;
		private float anyGamepadTriggerInputL;
		private float anyGamepadTriggerInputR;
		private float anyGamepadStickHorizontalR;
		private float anyGamepadStickVerticalR;
		/// <summary>Gamepad [A] held state</summary>
		private bool anyGamepadBtn0;
		/// <summary>Gamepad [Right Bumper] held state</summary>
		private bool anyGamepadBtn5;
		/// <summary>Gamepad [B] pressed state</summary>
		private bool anyGamepadBtnDown1;
		/// <summary>Gamepad [X] pressed state</summary>
		private bool anyGamepadBtnDown2;
		/// <summary>Gamepad [Y] pressed state</summary>
		private bool anyGamepadBtnDown3;
		/// <summary>Left Bumper pressed state</summary>
		private bool anyGamepadBtnDown4;
		/// <summary>Right Bumper pressed state</summary>
		private bool anyGamepadBtnDown5;
		/// <summary>Start Button pressed state</summary>
		private bool anyGamepadBtnDown7;

		public override void OnEarlyInitializeMelon()
		{
			instance = this;
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
			cfg_mSensitivityHorizontal = mouseSettingsCat.CreateEntry<float>("HorizontalSensitivity", 0.8f);
			cfg_mSensitivityVertical = mouseSettingsCat.CreateEntry<float>("VerticalSensitivity", 0.8f);
			cfg_mSensitivityMultiplier = mouseSettingsCat.CreateEntry<float>("SensitivityMultiplier", 1);
			cfg_mInvertHorizontal = mouseSettingsCat.CreateEntry<bool>("InvertHorizontal", false);
			cfgZoomStepIncrement = mouseSettingsCat.CreateEntry<float>("ZoomStepIncrement", 0.20f, description:"How much one scroll zooms the camera.");

			// Gamepad Settings
			cfg_gamepadStickDeadzoneR = gamepadSettingsCat.CreateEntry<float>("GamepadStickDeadzoneR", 0.1f);
			cfg_gamepadSensHorizontal = gamepadSettingsCat.CreateEntry<float>("GamepadHorizontalSensitivity", 1);
			cfg_gamepadSensVertical = gamepadSettingsCat.CreateEntry<float>("GamepadVerticalSensitivity", 1);
			cfg_gamepadSensMultiplier = gamepadSettingsCat.CreateEntry<float>("GamepadSensitivityMultiplier", 1);
			cfg_gamepadInvertHorizontal = gamepadSettingsCat.CreateEntry<bool>("GamepadInvertHorizontal", false);
			cfg_gamepadInvertVertical = gamepadSettingsCat.CreateEntry<bool>("GamepadInvertVertical", false);

			// Camera Settings
			cfgZoomLerpOutSpeed = cameraSettingsCat.CreateEntry<float>("ZoomOutLerpSpeed", 1);
			cfgZoomLerpInSpeed = cameraSettingsCat.CreateEntry<float>("ZoomInLerpSpeed", 0.0880f);
			cfgCameraCollisionPadding = cameraSettingsCat.CreateEntry<float>("CameraCollisionPadding", 0.20f, description:"Distance the camera is pushed away from terrain.");
			cfgDefaultCameraOnPause = cameraSettingsCat.CreateEntry<bool>("DefaultCameraOnPause", true);
			cfgCameraAlwaysAutoAlign = cameraSettingsCat.CreateEntry<bool>("CameraAutoAlign", true);
			cfgAutoAlignSpeed = cameraSettingsCat.CreateEntry<float>("AutoAlignSpeed", 1.80f, description:"How quickly the camera moves behind the player.");
			cfgStandardFoV = cameraSettingsCat.CreateEntry<float>("StandardFoV", 70);
			cfgFirstPersonFoV = cameraSettingsCat.CreateEntry<float>("FirstPersonFoV", 98);


			// Other Settings
			cfgAutoEnableOnLevelLoad = otherSettingsCat.CreateEntry<bool>("EnableAltCameraOnLevelLoad", true);
			cfgFocalLength = otherSettingsCat.CreateEntry<float>("AltFocalLength", 66);
			cfgFocusDistanceOffset = otherSettingsCat.CreateEntry<float>("FocusDistanceOffset", 7);

			mouseSettingsCat.SaveToFile();
			gamepadSettingsCat.SaveToFile();
			cameraSettingsCat.SaveToFile();
			otherSettingsCat.SaveToFile();
		}

		public override void OnUpdate()
		{
			UpdateGamepadInputs();
		}

		public override void OnLateUpdate()
		{
			if (forceDisable) {
				// Used for OnDeinitializeMelon()
				return;
			}

			// Here to allow disabling HUD while using the normal camera.
			if (Input.GetKeyDown(uiToggleKey))
			{
				uiRendererCamera = GameObject.Find("UICam").GetComponent<Camera>();
				ToggleGameHUD();
			}

			if (Input.GetKeyDown(modToggleKey))
			{
				if (!hasStartedOnce) {
					StartUpMod();
				}

				cameraModEnabled = !cameraModEnabled;
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
					ApplyCameraSettings(5.4f, new Vector3(0f, 2.4f, 0f), cfgStandardFoV.Value, 0.3f, "Bike(Clone)");
					AlignViewWithBike();
					// Adjust DoF
					if (hasDOFSettings) {
						m_dofSettings.focalLength.value = cfgFocalLength.Value;
					}
				}
			}

			// FIRST CHECKPOINT: Mod not enabled or hasn't grabbed references yet
			if (cameraModEnabled == false || hasStartedOnce == false) {
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

			// Calculating and setting the position/rotation of the camera
			CameraLogic();
		}

		private void UpdateGamepadInputs()
		{
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
			anyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") + Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
			anyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

			anyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") + Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
			anyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") + Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");
			anyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
			anyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

			anyGamepadBtn0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
			anyGamepadBtn5 = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

			anyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick2Button1) || Input.GetKeyDown(KeyCode.Joystick3Button1) || Input.GetKeyDown(KeyCode.Joystick4Button1);
			anyGamepadBtnDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick3Button2) || Input.GetKeyDown(KeyCode.Joystick4Button2);
			anyGamepadBtnDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Joystick2Button3) || Input.GetKeyDown(KeyCode.Joystick3Button3) || Input.GetKeyDown(KeyCode.Joystick4Button3);
			anyGamepadBtnDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick2Button4) || Input.GetKeyDown(KeyCode.Joystick3Button4) || Input.GetKeyDown(KeyCode.Joystick4Button4);
			anyGamepadBtnDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick2Button5) || Input.GetKeyDown(KeyCode.Joystick3Button5) || Input.GetKeyDown(KeyCode.Joystick4Button5);
			anyGamepadBtnDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick2Button7) || Input.GetKeyDown(KeyCode.Joystick3Button7) || Input.GetKeyDown(KeyCode.Joystick4Button7);
		}


		/// <summary>
		/// Handles the processing of the position and rotation of the camera.
		/// </summary>
		private void CameraLogic()
		{
			if (playerBikeTransform == null) {
				return;
			}
			dirToCam = camTransform.position - playerBikeTransform.TransformPoint(targetOffset);

			// Paused game check; only run when playing
			if (!hasMenuOpen)
			{
				// Double check that the default camera is disabled
				if (defaultCameraScript.enabled == true)
				{
					defaultCameraScript.enabled = false;
				}

				// Lock and hide the cursor
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				if (Input.GetAxis("Mouse ScrollWheel") > 0f)
				{
					// Scrolling forward; zoom in
					if (Input.GetKey(focalLegthModeKey) && wantedFocalLength > 0)
					{
						wantedFocalLength--;
						Debug.Log("FocalLength " + wantedFocalLength);
					}
					else if (Input.GetKey(focusDistanceModeKey))
					{
						cfgFocusDistanceOffset.Value++;
						Debug.Log("FocusDistanceOffset " + cfgFocusDistanceOffset.Value);
					}
					else {
						wantedZoom -= cfgZoomStepIncrement.Value;
					}
				}
				else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
				{
					// Scrolling backwards; zoom out
					if (Input.GetKey(focalLegthModeKey))
					{
						wantedFocalLength++;
						Debug.Log("FocalLength " + wantedFocalLength);
					}
					else if (Input.GetKey(focusDistanceModeKey) && cfgFocusDistanceOffset.Value > 0)
					{
						cfgFocusDistanceOffset.Value--;
						Debug.Log("FocusDistanceOffset " + cfgFocusDistanceOffset.Value);
					}
					else {
						wantedZoom += cfgZoomStepIncrement.Value;
					}
				}
				if (wantedZoom < 0.0f) {
					wantedZoom = 0.0f;
				}


				// Horizontal mouse movement will make camera rotate around vertical y-axis
				// Vertical mouse movement will make camera rotate along x-axis (your ear-to-ear axis)
				rotHorizontal += Input.GetAxisRaw("Mouse X") * cfg_mSensitivityHorizontal.Value * cfg_mSensitivityMultiplier.Value;
				rotVertical += Input.GetAxisRaw("Mouse Y") * cfg_mSensitivityVertical.Value * cfg_mSensitivityMultiplier.Value;

				// Also apply controller input
				rotHorizontal += ApplyInnerDeadzone(anyGamepadStickHorizontalR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensHorizontal.Value * cfg_gamepadSensMultiplier.Value;
				rotVertical -= ApplyInnerDeadzone(anyGamepadStickVerticalR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensVertical.Value * cfg_gamepadSensMultiplier.Value;
				rotVertical = ClampAngle(rotVertical, xMinLimit, xMaxLimit);  // Clamp the up-down rotation



				// Always auto-align is true or invert button is enabling it
				bool holdingInvertAutoAlignMode = anyGamepadBtn5 || Input.GetKey(KeyCode.Mouse1);
				bool autoAligningIsActive = (cfgCameraAlwaysAutoAlign.Value && !holdingInvertAutoAlignMode) || (cfgCameraAlwaysAutoAlign.Value == false && holdingInvertAutoAlignMode);
				if (autoAligningIsActive)
				{
					// Lerp the horizontal rotation relative to the player
					if (cfg_mInvertHorizontal.Value == true) {
						rotHorizontal = Mathf.LerpAngle(rotHorizontal, -playerBikeParentTransform.localRotation.eulerAngles.y, cfgAutoAlignSpeed.Value * Time.deltaTime);
					}
					else {
						rotHorizontal = Mathf.LerpAngle(rotHorizontal, playerBikeParentTransform.localRotation.eulerAngles.y, cfgAutoAlignSpeed.Value * Time.deltaTime);
					}
				}
				// Keep angles reasonable
				rotHorizontal = ClampAngle(rotHorizontal, -360, 360);
				if (cfg_mInvertHorizontal.Value == true) {
					rotation = Quaternion.Euler(-rotVertical, -rotHorizontal, 0f);
				}
				else {
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
				if (targetZoomAmount < 0.0f) {
					targetZoomAmount = 0.0f;
				}

				Vector3 finalPosition = rotation * new Vector3(0f, 0f, -targetZoomAmount) + playerBikeTransform.TransformPoint(targetOffset);

				// Apply values
				camTransform.position = finalPosition;
				camTransform.rotation = rotation;

				// Adjust DoF
				if (hasDOFSettings) {
					m_dofSettings.focusDistance.value = cfgFocusDistanceOffset.Value + Vector3.Distance(camTransform.position, playerBikeTransform.position);
					m_dofSettings.focalLength.value = wantedFocalLength;
				}
			}
			else	// The menu is open; game is paused
			{
				// While paused show the cursor
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				// and optionally show the default camera
				if (defaultCameraScript.enabled == false && cfgDefaultCameraOnPause.Value)
				{
					// Adjust DoF
					if (hasDOFSettings) {
						m_dofSettings.focalLength.value = baseFocalLength;
					}
					//mainCameraComponent.fieldOfView = baseFoV;
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
			//ApplyCameraSettings(5.4f, new Vector3(0f, 2.4f, 0f), cfgStandardFoV.Value, 0.28f, "Bike(Clone)");

			rotHorizontal = camTransform.eulerAngles.y;
			rotVertical = camTransform.eulerAngles.x;

			AlignViewWithBike();

			if (hasDOFSettings)
			{
				baseFocalLength = m_dofSettings.focalLength.GetValue<float>();
				wantedFocalLength = cfgFocalLength.Value;
			}
			baseFoV = mainCameraComponent.fieldOfView;

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
				ApplyCameraSettings(0f, new Vector3(0.0f, 0.3f, 0f), cfgFirstPersonFoV.Value, 0.6f, "neck_BindJNT");

				// Navigate to bike mesh renderer to prevent it from vanishing in first person
				SkinnedMeshRenderer bikeMeshRenderer = playerBikeParentTransform.GetChild(7).transform.GetChild(1).gameObject.GetComponent<SkinnedMeshRenderer>();
				bikeMeshRenderer.updateWhenOffscreen = true;
			}

			// Mouse inverting
			if (Input.GetKeyDown(camToggleInvertHorizontalKey))
			{
				cfg_mInvertHorizontal.Value = !cfg_mInvertHorizontal.Value;
				AlignViewWithBike();
			}

			// Camera auto align
			if (Input.GetKeyDown(camToggleAutoAlignKey))
			{
				cfgCameraAlwaysAutoAlign.Value = !cfgCameraAlwaysAutoAlign.Value;
			}
		}

		/// <summary>
		/// Finds and assigns important GameObjects and Transforms.
		/// </summary>
		private void GetTargetGameObjects()
		{
			playerBikeParentTransform = GameObject.Find("Bike(Clone)").GetComponent<Transform>();
			playerBikeTransform = GameObject.Find(targetName).GetComponent<Transform>();
			camTransform = GameObject.Find("PlayCamera(Clone)").GetComponent<Transform>();
			mainCameraComponent = camTransform.gameObject.GetComponent<Camera>();
			defaultCameraScript = camTransform.gameObject.GetComponent<PlayCamera>();
			postProcessingObject = camTransform.Find("DefaultPostProcessing").gameObject;

			hasDOFSettings = postProcessingObject.GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings<DepthOfField>(out m_dofSettings);
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
		/// Resets the camera settings to default values. FoV, nearClipPlane, focalLength.
		/// </summary>
		private void ApplyDefaultCameraSettings()
		{
			mainCameraComponent.fieldOfView = 38f;	// or 34?
			mainCameraComponent.nearClipPlane = 0.3f;
			if (hasDOFSettings) {
				m_dofSettings.focalLength.value = baseFocalLength;
			}
		}

		/// <summary>
		/// Allows applying multiple camera settings quickly.
		/// </summary>
		private void ApplyCameraSettings(float followDistance, Vector3 followTargetOffset, float cameraFov, float nearClipPlane, string followTargetName)
		{
			targetName = followTargetName;
			// Update reference
			playerBikeTransform = GameObject.Find(targetName).GetComponent<Transform>();

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
			if (axis > deadzone || axis < -deadzone) {
				return axis;
			}
			return 0;
		}

		public static void DrawDemoText()
		{
			GUI.Label(new Rect(20, 8, 1000, 200), "<b><color=white><size=15>DevdudeX's Alt Camera v"+ MOD_VERSION +"</size></color></b>");
		}
		public override void OnDeinitializeMelon()
		{
			// In case the melon gets unregistered
			forceDisable = true;
			MelonEvents.OnGUI.Unsubscribe(DrawDemoText);
		}
	}
}
