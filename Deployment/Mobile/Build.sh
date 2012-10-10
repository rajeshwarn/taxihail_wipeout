#!/bin/bash	
CLIENT=CheckerYellowCabPalmetto

echo Applying Config Tool for $CLIENT
mono apcurium.MK.Booking.ConfigTool.exe $CLIENT

CONFIGIOS="Debug|iPhone"
echo Building iOS App for $CLIENT with $CONFIGIOS configuration
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Newtonsoft_Json_MonoTouch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Touch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Binding.Touch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Dialog.Touch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:SocialNetworks.Services.MonoTouch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Common.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Contract.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Client.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.Client.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.All.sln"

CONFIGANDROID="Debug"
TARGET="SignAndroidPackage"
echo Building Android App for $CLIENT with $CONFIGANDROID configuration
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Newtonsoft.Json.MonoDroid" "--configuration:$CONFIGANDROID" "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Binding.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Android.Maps" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Common.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Contract.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Client.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.Client.Android" "--configuration:$CONFIGANDROID" "--target:$TARGET"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"	