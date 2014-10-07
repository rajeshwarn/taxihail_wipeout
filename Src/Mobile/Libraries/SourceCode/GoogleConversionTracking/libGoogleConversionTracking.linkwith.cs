using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libGoogleConversionTracking.a", LinkTarget.Simulator | LinkTarget.ArmV7, "-ObjC", ForceLoad = true, WeakFrameworks = "AdSupport")]
