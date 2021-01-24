#!/bin/sh

ARCH=$(uname -m)"-unknown-linux-gnu"
PROG="BookParse.Shell"

if [ "$(uname -s)" = "Linux" ]; then
        if [ -e *.csproj ]; then
                LD_LIBRARY_PATH=../../../lib/$ARCH dotnet run
        else
                LD_LIBRARY_PATH=lib/$ARCH ./$PROG
        fi;
fi;
