#!/bin/bash
set -e

RID="$1" # linux-x64 / win-x64

if ! [ -d "ffmpeg/win64" ] && [ "$RID" == "win-x64" ]; then
  mkdir -p ffmpeg/win64
  pushd ffmpeg/win64
  curl https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z -L > ffmpeg-release-full.7z
  #sudo apt-get install p7zip-full
  7z x ffmpeg-release-full.7z
  popd
elif ! [ -d "ffmpeg/linux64" ] && [ "$RID" == "linux-x64" ]; then
  mkdir -p ffmpeg/linux64
  pushd ffmpeg/linux64
  curl https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz -L > ffmpeg-release-amd64-static.tar.xz
  tar -xf ffmpeg-release-amd64-static.tar.xz
  popd
fi
