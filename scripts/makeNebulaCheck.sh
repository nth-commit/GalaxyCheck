#!/bin/bash

find ./src/GalaxyCheck -type d -print0 | while read -d $'\0' currFilename; do
  newFilename=$(echo "$currFilename" | sed "s/GalaxyCheck/NebulaCheck/g")
  mkdir $newFilename
done

find ./src/GalaxyCheck -type f -print0 | while read -d $'\0' currFilename; do
  newFilename=$(echo $currFilename | sed "s/GalaxyCheck/NebulaCheck/g")
  cat $currFilename | sed "s/GalaxyCheck/NebulaCheck/g" > $newFilename
done

sed -i -e "s|https://github.com/nth-commit/NebulaCheck|https://github.com/nth-commit/GalaxyCheck|g" ./src/NebulaCheck/NebulaCheck.csproj