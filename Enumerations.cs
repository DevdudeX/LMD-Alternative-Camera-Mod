namespace AlternativeCameraMod;

internal enum ModMode
{
   BikeCam,
   PhotoCam
}


internal enum CameraMode
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


[Flags]
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

