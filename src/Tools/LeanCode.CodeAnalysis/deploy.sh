#!/bin sh

VERSION=1.0.0
dotnet pack
cp bin/Debug/LeanCode.CodeAnalysis.1.0.0.nupkg ~/.local-nugets
rm -rf ~/.nuget/packages/leancode.codeanalysis
