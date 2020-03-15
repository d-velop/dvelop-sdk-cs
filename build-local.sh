#!/bin/bash

set -euo pipefail
# http://redsymbol.net/articles/unofficial-bash-strict-mode/
dotnet clean
dotnet buil -c Debug
dotnet pack --version-suffix alpha --output dist