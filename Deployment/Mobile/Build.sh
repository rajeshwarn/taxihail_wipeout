#!/bin/bash	
echo Building Android app? Y / N
read BUILDANDROID

echo Building iOS app? Y / N
read BUILDIOS

echo Enter client name i.e. folder name in Config or empty for all configuration
read CLIENTNAME

if [[ -z "$CLIENTNAME" ]]; then
	LISTCLIENT=$(ls -l ../../Config | egrep '^d' | awk '{print $9}')
else
	LISTCLIENT=$CLIENTNAME
fi

for CLIENT in "${LISTCLIENT[@]}"
do
	echo $CLIENT
	echo Applying Config Tool for $CLIENT
	mono apcurium.MK.Booking.ConfigTool.exe $CLIENT

	if [ "$BUILDIOS" = "Y" ]; then
		CONFIGIOS="Release|iPhone"
		echo Building iOS App for $CLIENT with $CONFIGIOS configuration
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Newtonsoft_Json_MonoTouch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Touch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Binding.Touch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Dialog.Touch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:SocialNetworks.Services.MonoTouch"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Common.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Google.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Maps.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Contract.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Client.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.Client.iOS"   "--configuration:$CONFIGIOS"  "../../Src/Mobile/MK.Booking.Mobile.Solution.iOS.sln"
		mv ../../Src/Mobile/iOS/bin/iPhone/Release/*.ipa /Volumes/WwwMobileApps/$CLIENT
	fi	

	if [ "$BUILDANDROID" = "Y" ]; then

		CONFIGANDROID="Release"
		TARGET="SignAndroidPackage"
		echo Building Android App for $CLIENT with $CONFIGANDROID configuration
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Newtonsoft.Json.MonoDroid" "--configuration:$CONFIGANDROID" "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Binding.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:Cirrious.MvvmCross.Android.Maps" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Common.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Google.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Maps.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Contract.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Api.Client.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.Android" "--configuration:$CONFIGANDROID"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"
		/Applications/MonoDevelop.app/Contents/MacOS/mdtool build "--project:MK.Booking.Mobile.Client.Android" "--configuration:$CONFIGANDROID" "--target:$TARGET"  "../../Src/Mobile/MK.Booking.Mobile.Solution.Android.sln"	
		mv ../../Src/Mobile/Android/bin/Release/*Signed.apk /Volumes/WwwMobileApps/$CLIENT
	fi
done

echo Rollback changes in source directory ? Y / N
read ROLLBACK
if [ "$ROLLBACK" = "Y" ]; then
	hg update -r default -C
	hg purge
fi