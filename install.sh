#!bin/bash

INSTALL_PATH=$1/Assets/Scripts/
echo "Installing to $INSTALL_PATH"
mkdir -p $INSTALL_PATH
cp ./source/Assets/Scripts/*.cs $INSTALL_PATH
