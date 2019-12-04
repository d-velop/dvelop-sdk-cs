#!/bin/bash

set -euo pipefail
# http://redsymbol.net/articles/unofficial-bash-strict-mode/

export VERSION=$(<./version.txt)

echo ${TRAVIS_BUILD_NUMBER}

echo "clean"
dotnet clean

echo "build"
dotnet build -c Release

if [[ ${TRAVIS_PULL_REQUEST} == 'false' ]]; then

    if [[ ! -z ${TRAVIS_TAG} ]]; then
        echo "pack release"
        dotnet pack -c Release /p:Version=${VERSION}.${TRAVIS_BUILD_NUMBER} /p:InformationalVersion="$VERSION+$TRAVIS_BRANCH/$TRAVIS_COMMIT"
        #echo "push"
        #dotnet nuget push ./dvelop.NugetPushTest/bin/Release/dvelop.NugetPushTest.$VERSION.$TRAVIS_BUILD_NUMBER.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY
    else
        echo "pack prerelease"
        dotnet pack -c Release /p:Version=${VERSION}.${TRAVIS_BUILD_NUMBER}-prerelease /p:InformationalVersion="$VERSION+$TRAVIS_BRANCH/$TRAVIS_COMMIT"
        #echo "push"
        #dotnet nuget push ./**/bin/Release/*.$VERSION.$TRAVIS_BUILD_NUMBER-prerelease.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY
    fi
fi
echo "return code: "
echo $?