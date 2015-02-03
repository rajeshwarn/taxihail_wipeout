using ObjCRuntime;

[assembly: LinkWith ("libGoogleConversionTracking.a", LinkTarget.ArmV7 | LinkTarget.Arm64 | LinkTarget.Simulator | LinkTarget.Simulator64, "-ObjC", ForceLoad = true, WeakFrameworks = "AdSupport")]
