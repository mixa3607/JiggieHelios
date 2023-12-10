#!/bin/bash
set -e

if ! [ -d "chrome/win64-120.0.6099.71" ]; then
  npx @puppeteer/browsers install chrome@120.0.6099.71 --platform win64
fi
if ! [ -d "chrome/linux-120.0.6099.71" ]; then
  npx @puppeteer/browsers install chrome@120.0.6099.71 --platform linux
fi
