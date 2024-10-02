using UnityEngine;

namespace AlternativeCameraMod
{
	/// <summary>
	/// Intended to act as an interface for gamepad inputs even if they are mapped differently.
	/// </summary>
	internal class GamepadInputHandler
	{
		public float AnyGamepadDpadHorizontal { get; private set; }
		public float AnyGamepadDpadVertical { get; private set; }

		public float AnyGamepadTriggerInputL { get; private set; }
		public float AnyGamepadTriggerInputR { get; private set; }

		public float AnyGamepadStickHorizontalR { get; private set; }
		public float AnyGamepadStickVerticalR { get; private set; }

		// Held state
		public bool AnyGamepadBtn0 { get; private set; }

		/// <summary>Gamepad [Right Bumper] held state.</summary>
		public bool AnyGamepadRightBumper { get; private set; }

		// Down this frame state
		public bool AnyGamepadBtnDown1 { get; private set; }
		public bool AnyGamepadBtnDown2 { get; private set; }
		public bool AnyGamepadBtnDown3 { get; private set; }
		public bool AnyGamepadBtnDown4 { get; private set; }
		public bool AnyGamepadBtnDown5 { get; private set; }
		public bool AnyGamepadBtnDown7 { get; private set; }

		public void UpdateGamepadInputs(bool alternativeMapping)
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

			if (alternativeMapping)
			{
				PollAlternativeInputs();
			}
			else
			{
				PollStandardInputs();
			}
		}

		void PollStandardInputs()
		{
			/*
				anyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis6")
				anyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis7")
	 
				anyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis9")
				anyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis10")
				anyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis4")
				anyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5")
			 */
			AnyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") + Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
			AnyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

			AnyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") + Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
			AnyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") + Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");
			AnyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
			AnyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

			AnyGamepadBtn0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
			AnyGamepadRightBumper = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

			AnyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick2Button1) || Input.GetKeyDown(KeyCode.Joystick3Button1) || Input.GetKeyDown(KeyCode.Joystick4Button1);
			AnyGamepadBtnDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick3Button2) || Input.GetKeyDown(KeyCode.Joystick4Button2);
			AnyGamepadBtnDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Joystick2Button3) || Input.GetKeyDown(KeyCode.Joystick3Button3) || Input.GetKeyDown(KeyCode.Joystick4Button3);
			AnyGamepadBtnDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick2Button4) || Input.GetKeyDown(KeyCode.Joystick3Button4) || Input.GetKeyDown(KeyCode.Joystick4Button4);
			AnyGamepadBtnDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick2Button5) || Input.GetKeyDown(KeyCode.Joystick3Button5) || Input.GetKeyDown(KeyCode.Joystick4Button5);
			AnyGamepadBtnDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick2Button7) || Input.GetKeyDown(KeyCode.Joystick3Button7) || Input.GetKeyDown(KeyCode.Joystick4Button7);
		}

		void PollAlternativeInputs()
		{
			/* PS4 Mapping
				DpadHorizontal = Joy1Axis7
				DpadVertical = Joy1Axis8

				TriggerL = Joy1Axis4
				TriggerR = Joy1Axis5

				StickHorizontalR = Joy1Axis3
				StickVerticalR = Joy1Axis6
			 */
			AnyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");
			AnyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis8") + Input.GetAxisRaw("Joy2Axis8") + Input.GetAxisRaw("Joy3Axis8") + Input.GetAxisRaw("Joy4Axis8");

			// FIXME:
			//AnyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");
			AnyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") + Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
			
			AnyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis3") + Input.GetAxisRaw("Joy2Axis3") + Input.GetAxisRaw("Joy3Axis3") + Input.GetAxisRaw("Joy4Axis3");
			AnyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

			AnyGamepadBtn0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
			//AnyGamepadLeftBumper = Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick2Button4) || Input.GetKey(KeyCode.Joystick3Button4) || Input.GetKey(KeyCode.Joystick4Button4);
			AnyGamepadRightBumper = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);

			AnyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick2Button1) || Input.GetKeyDown(KeyCode.Joystick3Button1) || Input.GetKeyDown(KeyCode.Joystick4Button1);
			AnyGamepadBtnDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick3Button2) || Input.GetKeyDown(KeyCode.Joystick4Button2);
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
		public string[] GetGamepadState()
		{
			//anyGamepadDpadHorizontal
			//anyGamepadDpadVertical
			//anyGamepadTriggerInputL
			//anyGamepadTriggerInputR
			//anyGamepadStickHorizontalR
			//anyGamepadStickVerticalR
			//anyGamepadBtn0
			//anyGamepadBtn5
			//anyGamepadBtnDown1
			//anyGamepadBtnDown2
			//anyGamepadBtnDown3
			//anyGamepadBtnDown4
			//anyGamepadBtnDown5
			//anyGamepadBtnDown7

			string[] joyData = new string[16];
			joyData[0] = AnyGamepadDpadHorizontal.ToString();
			joyData[1] = AnyGamepadDpadVertical.ToString();
			joyData[3] = AnyGamepadTriggerInputL.ToString();
			joyData[4] = AnyGamepadTriggerInputR.ToString();
			joyData[5] = AnyGamepadStickHorizontalR.ToString();
			joyData[6] = AnyGamepadStickVerticalR.ToString();

			joyData[7] = AnyGamepadBtn0.ToString();
			joyData[8] = AnyGamepadRightBumper.ToString();
			joyData[9] = AnyGamepadBtnDown1.ToString();
			joyData[10] = AnyGamepadBtnDown2.ToString();
			joyData[11] = AnyGamepadBtnDown3.ToString();
			joyData[12] = AnyGamepadBtnDown4.ToString();
			joyData[13] = AnyGamepadBtnDown5.ToString();
			joyData[14] = AnyGamepadBtnDown7.ToString();

			return joyData;
		}


	}
}
