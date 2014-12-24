
all : fetchandroid


fetchandroid :
	mkdir -p android
	curl https://dl.google.com/dl/geosdk/googlemaps-android-m4b-2.9.0.rc1.zip -o android/googlemaps-android-m4b.zip
	unzip android/googlemaps-android-m4b.zip -d android/

clean :
	rm -rf android 

remove-extras :
	rm -rf android/googlemaps-android-m4b.zip
