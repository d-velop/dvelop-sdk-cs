#!/bin/bash

set -euo pipefail
# http://redsymbol.net/articles/unofficial-bash-strict-mode/

export VERSION=$(grep -oPm1 "(?<=<VersionPrefix>)[^<]+" Directory.Build.props)

echo ${TRAVIS_BUILD_NUMBER}

echo "clean"
dotnet clean

echo "test"
dotnet test --logger:"console;verbosity=normal"

if [[ ${TRAVIS_PULL_REQUEST} == 'false' ]]; then

echo "build"


    if [[ ! -z ${TRAVIS_TAG} ]]; then
        echo "pack release"
		dotnet build -c Release
        dotnet pack -c Release /p:Version=${VERSION}.${TRAVIS_BUILD_NUMBER} /p:InformationalVersion="$VERSION+$TRAVIS_BRANCH/$TRAVIS_COMMIT"
        echo "push"
        find . -name "*.$VERSION.$TRAVIS_BUILD_NUMBER.nupkg" | xargs -i dotnet nuget push {} -s https://api.nuget.org/v3/index.json -k ${NUGET_API_KEY}
    else
        echo "pack prerelease"
        dotnet build -c Debug
		if [[ ${TRAVIS_BRANCH} == 'main' ]]; then
            dotnet pack -c Debug /p:Version=${VERSION}.${TRAVIS_BUILD_NUMBER}-prerelease /p:InformationalVersion="$VERSION+$TRAVIS_BRANCH/$TRAVIS_COMMIT"
            echo "push"
            find . -name "*.$VERSION.$TRAVIS_BUILD_NUMBER-prerelease.nupkg" | xargs -i dotnet nuget push {} -s https://api.nuget.org/v3/index.json -k ${NUGET_API_KEY}
        fi
    fi
fi
echo "return code: $?"