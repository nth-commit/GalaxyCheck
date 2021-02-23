#!/bin/bash

for replacers in "GalaxyCheck;NebulaCheck" "GalaxyCheck.Xunit;NebulaCheck.Xunit"
do
  arr=(${replacers//;/ })
  srcProjectName=${arr[0]}
  destProjectName=${arr[1]}

  find ./src/${srcProjectName} -type d -print0 | while read -d $'\0' currFilename; do
    newFilename=$(echo "$currFilename" | sed "s/GalaxyCheck/NebulaCheck/g")
    mkdir $newFilename
  done

  find ./src/${srcProjectName} -type f -print0 | while read -d $'\0' currFilename; do
    newFilename=$(echo $currFilename | sed "s/GalaxyCheck/NebulaCheck/g")
    cat $currFilename | sed "s/GalaxyCheck/NebulaCheck/g" > $newFilename
  done

  sed -i -e "s|https://github.com/nth-commit/NebulaCheck|https://github.com/nth-commit/GalaxyCheck|g" ./src/${destProjectName}/${destProjectName}.csproj
done