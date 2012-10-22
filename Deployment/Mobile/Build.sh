#!/bin/bash	
echo Building Android app? Y / N
read BUILDANDROID

echo Building iOS app? Y / N
read BUILDIOS

echo Publish iOS app on diawi? Y / N
read PUBLISHIOS

echo Enter client name i.e. folder name in Config or empty for all configuration
read CLIENTNAME

if [[ -z "$CLIENTNAME" ]]; then
	LISTCLIENT=$(ls -l ../../Config | egrep '^d' | awk '{print $9}')
else
	LISTCLIENT=$CLIENTNAME
fi

for CLIENT in ${LISTCLIENT[@]}
do
	echo $CLIENT
	echo "-------------------------------"
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
		cp ../../Src/Mobile/iOS/bin/iPhone/Release/*.ipa /Volumes/WwwMobileApps/$CLIENT

		if [ "$PUBLISHIOS" = "Y" ]; then

			TEMPNAME=$(LC_CTYPE=C tr -dc "[:alpha:]" < /dev/urandom | head -c 10)".ipa"
			cp  ../../Src/Mobile/iOS/bin/iPhone/Release/*.ipa .
			FILE=$(find . -name \*.ipa)
			echo ${FILE:2} $TEMPNAME
			curl -F file=@${FILE:2} -F filename=blob -F name=$TEMPNAME http://www.diawi.com/upload.php
			curl -F uploader_0_tmpname=$TEMPNAME -F uploader_0_name=$FILE -F uploader_0_status=done -F uploader_count=1 -F email=matthieu.duluc@apcurium.com -F comment=$CLIENT http://www.diawi.com/result.php
			rm -rf $FILE
			rm -rf ../../Src/Mobile/iOS/bin/iPhone/Release/*.*
		fi
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