namespace AlternativeCameraMod;

internal enum Screen
{
   None,
   LoadingScreen,
   SplashScreen,
   MenuScreen,
   PlayScreen,
   PauseScreen,
   PhotoScreen
}


internal enum CameraMode
{
   BikeCam,
   PhotoCam
}


internal enum CameraView
{
   Original,
   ThirdPerson,
   FirstPerson
}


internal enum CameraAlignmentMode
{
   Auto,
   Manual
}


internal enum CameraFocusAdjustMode
{
   FieldOfView,
   DepthOfField
}


internal enum CameraManualAlignmentInput
{
   KeyOrButton,
   MouseOrRStick
}


internal enum ControllerButton
{
   None,
   X,
   Y,
   LB,
   RB,
   LStick,
   RStick
}


internal enum ModHudInfoPart
{
   CamMode,
   FoV,
   FPS,
   CamAlign
}


internal enum LogLevel
{
   Error,
   Warning,
   Info,
   Debug,
   Verbose
}


internal enum PlayModeAction
{
   CameraModeOriginal,
   CameraModeThirdPerson,
   CameraModeFirstPerson,
   ToggleCameraState,
   ToggleCameraAutoAlignMode,
   ToggleInvertLookHorizontal,
   ToggleGameHud,
   ToggleModHudDisplays,
   LookAround,
   SnapCameraToBehindTheBike,
   HoldToInvertCameraAutoAlignMode,
   ZoomInOut,
   ChangeDoFFocalLength,
   ChangeDoFFocusDistanceOffset,
   IncreaseFoV,
   DecreaseFoV,
   ResetFoV
}