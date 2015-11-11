#!/bin/bash
clear
rvm use 1.9.2
cd Calabash
sudo cp ../bin/Debug/com.apcurium.MK.TaxiHailDemo-Signed.apk .
sudo calabash-android resign com.apcurium.MK.TaxiHailDemo-Signed.apk
sudo calabash-android run com.apcurium.MK.TaxiHailDemo-Signed.apk
