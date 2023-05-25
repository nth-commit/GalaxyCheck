#!/bin/bash

for replacers in "GalaxyCheck;GalaxyCheck.Stable" "GalaxyCheck.Xunit;GalaxyCheck.Stable.Xunit" "GalaxyCheck.Xunit.CodeAnalysis;GalaxyCheck.Stable.Xunit.CodeAnalysis"
do
  arr=(${replacers//;/ })
  srcProjectName=${arr[0]}
  destProjectName=${arr[1]}

  find ./src/${srcProjectName} -type d -print0 | while read -d $'\0' currFilename; do
    newFilename=$(echo "$currFilename" | sed "s/GalaxyCheck/GalaxyCheck.Stable/g")
    mkdir $newFilename
  done

  find ./src/${srcProjectName} -type f -print0 | while read -d $'\0' currFilename; do
    newFilename=$(echo $currFilename | sed "s/GalaxyCheck/GalaxyCheck.Stable/g")
    cat $currFilename | sed "s/GalaxyCheck/GalaxyCheck.Stable/g" > $newFilename
  done

  sed -i -e "s|https://github.com/nth-commit/GalaxyCheck.Stable|https://github.com/nth-commit/GalaxyCheck|g" ./src/${destProjectName}/${destProjectName}.csproj
done