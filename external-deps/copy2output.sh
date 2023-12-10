#!/bin/bash
set -e

RID="$1" # linux-x64 / win-x64
DST="$2" # ../src/out/files

if [ "$RID" == "linux-x64" ]; then
  mkdir -p $DST/chrome
  cp -r ./chrome/linux-120.0.6099.71/chrome-linux64/. $DST/chrome

  mkdir -p $DST/ffmpeg
  cp $(find ffmpeg/linux64 -type f -name ffmpeg) $DST/ffmpeg
elif [ "$RID" == "win-x64" ]; then
  mkdir -p $DST/chrome
  cp -r ./chrome/win64-120.0.6099.71/chrome-win64/. $DST/chrome

  mkdir -p $DST/ffmpeg
  cp $(find ffmpeg/win64 -type f -name ffmpeg.exe) $DST/ffmpeg
else
  echo unsupported rid
  exit 1
fi

mkdir -p $DST/puppeteer-stream
cp -r ./puppeteer-stream/extension $DST/puppeteer-stream

mkdir -p $DST/reaggie-frontend
cp -r ./reaggie-frontend/room $DST/reaggie-frontend
cp -r ./reaggie-frontend/home $DST/reaggie-frontend
