#!/bin/bash
set -e

if ! [ -d "reaggie-frontend" ]; then
  git clone https://git.coom.tech/coomdev/reaggie-frontend.git reaggie-frontend
fi
if ! [ -d "reaggie" ]; then
  git clone https://git.coom.tech/coomdev/reaggie.git reaggie
fi
pushd reaggie-frontend
git reset --hard 2dfc1199
git apply ../reaggie-frontend-2dfc1199.patch
npm i
npm run build-release
