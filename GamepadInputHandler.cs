using UnityEngine;

namespace AlternativeCameraMod
{
	/// <summary>
	/// What input layout should be targeted.
	/// </summary>
	public enum GamepadInputMode
	{
		Standard,
		DualShock
	}

	/// <summary>
	/// Intended to act as an interface for gamepad inputs even if they are mapped differently.
	/// </summary>
	internal class GamepadInputHandler
	{
		public float AnyGamepadDpadHorizontal { get; private set; }
		public float AnyGamepadDpadVertical { get; private set; }

		public float AnyGamepadTriggerInputL { get; private set; }
		public float AnyGamepadTriggerInputR { get; private set; }


		//public float AnyGamepadStickHorizontalL { get; private set; }
		//public float AnyGamepadStickVerticalL { get; private set; }
		public float AnyGamepadStickHorizontalR { get; private set; }
		public float AnyGamepadStickVerticalR { get; private set; }

		// Held state
		public bool AnyGamepadFaceBtnLeft { get; private set; }
		public bool AnyGamepadFaceBtnRight { get; private set; }
		public bool AnyGamepadFaceBtnTop { get; private set; }
		public bool AnyGamepadFaceBtnBottom { get; private set; }
		

		
		public bool AnyGamepadLeftBumper { get; private set; }
		public bool AnyGamepadRightBumper { get; private set; }

		// Down this frame state
		public bool AnyGamepadBtnDown1 { get; private set; }
		
		public bool AnyGamepadBtnDown3 { get; private set; }
		public bool AnyGamepadBtnDown4 { get; private set; }
		public bool AnyGamepadBtnDown5 { get; private set; }
		public bool AnyGamepadBtnDown7 { get; private set; }

		public void StartHandler()
		{
			// Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
			player = ReInput.players.GetPlayer(playerId);
		}

		public void UpdateGamepadInputs(GamepadInputMode inputMode)
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

			if (inputMode == GamepadInputMode.Standard)
			{
				PollInputsStandard();
			}
			else if (inputMode == GamepadInputMode.DualShock)
			{
				PollInputsDualShock();
			}
		}

		void PollInputsStandard()
		{
			AnyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") + Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
			AnyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

			AnyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") + Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
			AnyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") + Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");

			//AnyGamepadStickHorizontalL = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
			//AnyGamepadStickVerticalL = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");
			AnyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
			AnyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

			AnyGamepadFaceBtnBottom = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
			AnyGamepadFaceBtnRight = Input.GetKey(KeyCode.Joystick1Button1) || Input.GetKey(KeyCode.Joystick2Button1) || Input.GetKey(KeyCode.Joystick3Button1) || Input.GetKey(KeyCode.Joystick4Button1);
			AnyGamepadFaceBtnLeft = Input.GetKey(KeyCode.Joystick1Button2) || Input.GetKey(KeyCode.Joystick2Button2) || Input.GetKey(KeyCode.Joystick3Button2) || Input.GetKey(KeyCode.Joystick4Button2);
			AnyGamepadFaceBtnTop = Input.GetKey(KeyCode.Joystick1Button3) || Input.GetKey(KeyCode.Joystick2Button3) || Input.GetKey(KeyCode.Joystick3Button3) || Input.GetKey(KeyCode.Joystick4Button3);


			AnyGamepadLeftBumper = Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick2Button4) || Input.GetKey(KeyCode.Joystick3Button4) || Input.GetKey(KeyCode.Joystick4Button4);
			AnyGamepadRightBumper = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

			AnyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick2Button1) || Input.GetKeyDown(KeyCode.Joystick3Button1) || Input.GetKeyDown(KeyCode.Joystick4Button1);
			
			AnyGamepadBtnDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Joystick2Button3) || Input.GetKeyDown(KeyCode.Joystick3Button3) || Input.GetKeyDown(KeyCode.Joystick4Button3);
			AnyGamepadBtnDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick2Button4) || Input.GetKeyDown(KeyCode.Joystick3Button4) || Input.GetKeyDown(KeyCode.Joystick4Button4);
			AnyGamepadBtnDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick2Button5) || Input.GetKeyDown(KeyCode.Joystick3Button5) || Input.GetKeyDown(KeyCode.Joystick4Button5);
			AnyGamepadBtnDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick2Button7) || Input.GetKeyDown(KeyCode.Joystick3Button7) || Input.GetKeyDown(KeyCode.Joystick4Button7);
		}

		void PollInputsDualShock()
		{
			// PS4 DualShock mapping is broken
			// for now simply ignore camera input
			AnyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");
			AnyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis8") + Input.GetAxisRaw("Joy2Axis8") + Input.GetAxisRaw("Joy3Axis8") + Input.GetAxisRaw("Joy4Axis8");

			AnyGamepadTriggerInputL = -1f;
			AnyGamepadTriggerInputR = -1f;
			
			AnyGamepadStickHorizontalR = 0;
			AnyGamepadStickVerticalR = 0;

			AnyGamepadFaceBtnBottom = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
			AnyGamepadLeftBumper = Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick2Button4) || Input.GetKey(KeyCode.Joystick3Button4) || Input.GetKey(KeyCode.Joystick4Button4);
			AnyGamepadRightBumper = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

			AnyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick2Button1) || Input.GetKeyDown(KeyCode.Joystick3Button1) || Input.GetKeyDown(KeyCode.Joystick4Button1);
			AnyGamepadFaceBtnLeft = Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick3Button2) || Input.GetKeyDown(KeyCode.Joystick4Button2);
			AnyGamepadBtnDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Joystick2Button3) || Input.GetKeyDown(KeyCode.Joystick3Button3) || Input.GetKeyDown(KeyCode.Joystick4Button3);
			AnyGamepadBtnDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick2Button4) || Input.GetKeyDown(KeyCode.Joystick3Button4) || Input.GetKeyDown(KeyCode.Joystick4Button4);
			AnyGamepadBtnDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick2Button5) || Input.GetKeyDown(KeyCode.Joystick3Button5) || Input.GetKeyDown(KeyCode.Joystick4Button5);
			AnyGamepadBtnDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick2Button7) || Input.GetKeyDown(KeyCode.Joystick3Button7) || Input.GetKeyDown(KeyCode.Joystick4Button7);
		}


		// MOD MENU
		/// <summary>
		/// Debug method for checking gamepad state.
		/// </summary>
		/// <returns>Array of gamepad data.</returns>
		public string GetGamepadState()
		{
			string formattedData = "\n";
			formattedData += $"DPad: {AnyGamepadDpadHorizontal} | {AnyGamepadDpadVertical}\n";
			formattedData += $"Trigger: {AnyGamepadTriggerInputL} | {AnyGamepadTriggerInputR}\n";
			formattedData += $"Stick R: {AnyGamepadStickHorizontalR} | {AnyGamepadStickVerticalR}\n";

			formattedData += $"Face Btn:<{AnyGamepadFaceBtnLeft} | >{AnyGamepadFaceBtnRight} | ^{AnyGamepadFaceBtnTop} | v{AnyGamepadFaceBtnBottom} | \n";
			formattedData += $"Bumpers: {AnyGamepadLeftBumper} | {AnyGamepadRightBumper} | \n";
			
			formattedData += $"{AnyGamepadBtnDown1} | ";
			formattedData += $"{AnyGamepadFaceBtnLeft} | ";
			formattedData += $"{AnyGamepadBtnDown3} | ";
			formattedData += $"{AnyGamepadBtnDown4} | ";
			formattedData += $"{AnyGamepadBtnDown5} | ";
			formattedData += $"{AnyGamepadBtnDown7} | ";


			return formattedData;
		}


	}
}
