using ObjCRuntime;

[assembly: LinkWith ("libDeviceCollectorLibrary.a", LinkTarget.ArmV7 | LinkTarget.Arm64 | LinkTarget.Simulator | LinkTarget.Simulator64, SmartLink = true, ForceLoad = true,
                     Frameworks="CoreLocation SystemConfiguration UIKit")]
