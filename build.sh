#!/usr/bin/env bash

if test "$OS" = "Windows_NT"
then
  # use .Net

  .paket/paket.bootstrapper.exe
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi

  .paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi

  [ ! -e build.fsx ] && .paket/paket.exe update
  [ ! -e build.fsx ] && packages/FAKE/tools/FAKE.exe init.fsx
  packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
else
  # Paket is compilet with .NET Core now!
  ./.paket/paket.bootstrapper.exe
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi

  ./.paket/paket.exe restore
  exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi

  [ ! -e build.fsx ] && ./.paket/paket.exe update
  [ ! -e build.fsx ] && mono packages/FAKE/tools/FAKE.exe init.fsx
  mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi
