#!/bin/bash
set -e

if ! [ -d "ffmpeg/win64" ]; then
  mkdir -p ffmpeg/win64
  pushd ffmpeg/win64
  curl https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z -L > ffmpeg-release-full.7z
  #sudo apt-get install p7zip-full
  7z x ffmpeg-release-full.7z
  popd
fi
if ! [ -d "ffmpeg/linux64" ]; then
  mkdir -p ffmpeg/linux64
  pushd ffmpeg/linux64
  curl https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz -L > ffmpeg-release-amd64-static.tar.xz
  tar -xf ffmpeg-release-amd64-static.tar.xz
  popd
fi
