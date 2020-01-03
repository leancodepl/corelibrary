#!/bin/sh

export VERSION=5.0.0-pre

dotnet pack
cp bin/Debug/LeanCode.CodeAnalysis.$VERSION.nupkg ~/.local-nugets
rm -rf ~/.nuget/packages/leancode.codeanalysis
