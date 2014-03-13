#!/bin/bash
# Auto-config with username and bundle ID

clear
bundle_id=$(grep 'CFBundleIdentifier' -A1  Info.plist | tail -1 | cut -d ">" -f 2 | cut -d "<" -f 1)
configCucumberDotYml="APP=\"${PWD}/$(find bin/iPhoneSimulator/Debug -type d -name "*.app")\" BUNDLE_ID=\"$bundle_id\" SDK_VERSION=6.1 "
sudo killall "iPhone Simulator"
$altDir cucumber
