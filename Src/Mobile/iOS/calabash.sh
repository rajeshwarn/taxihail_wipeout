#!/bin/bash
# Auto-config with username and bundle ID

clear
bundle_id=$(grep 'CFBundleIdentifier' -A1  Info.plist | tail -1 | cut -d ">" -f 2 | cut -d "<" -f 1)
echo $bundle_id
configCucumberDotYml="APP=${PWD}/bin/iPhoneSimulator/Debug/TaxiHail.app BUNDLE_ID=\"$bundle_id\" SDK_VERSION=7.1 "
killall "iPhone Simulator"
echo $configCucumberDotYml
$altDir cucumber $configCucumberDotYml