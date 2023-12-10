#!/bin/bash
set -e

if ! [ -d "puppeteer-stream" ]; then
  git clone https://github.com/SamuelScheit/puppeteer-stream.git puppeteer-stream
fi
pushd puppeteer-stream
git reset --hard 1204241b
git apply ../puppeteer-stream-1204241b.patch
