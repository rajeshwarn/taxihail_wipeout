using System;
using ObjCRuntime;

[assembly: LinkWith ("libGoogleConversionTracking.a", LinkTarget.ArmV7 | LinkTarget.ArmV7s | LinkTarget.Simulator, "-ObjC", ForceLoad = true, WeakFrameworks = "AdSupport")]
