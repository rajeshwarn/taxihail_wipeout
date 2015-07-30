using System;
using ObjCRuntime;

[assembly: LinkWith ("libPayPalMobile.a", LinkTarget.ArmV7 | LinkTarget.Arm64 | LinkTarget.Simulator | LinkTarget.Simulator64, SmartLink=true, ForceLoad = true, 
                    Frameworks="AVFoundation AudioToolbox CoreLocation CoreMedia CoreVideo SystemConfiguration Security MessageUI OpenGLES MobileCoreServices",
                    LinkerFlags="-lz -lxml2 -lc++ -lstdc++")]
