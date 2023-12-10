#!/bin/bash
set -e

RID="$1" # linux-x64 / win-x64

if ! [ -d "chrome/win64-120.0.6099.71" && "$RID" == "win-x64" ]; then
  npx @puppeteer/browsers install chrome@120.0.6099.71 --platform win64
elif ! [ -d "chrome/linux-120.0.6099.71" && "$RID" == "linux-x64" ]; then
  npx @puppeteer/browsers install chrome@120.0.6099.71 --platform linux
else
  echo unsupported rid
  exit 1
fi
