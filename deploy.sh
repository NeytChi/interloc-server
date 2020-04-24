#!/bin/bash

cd ./bin/

rm -r ./release/

cd ../

dotnet build --configuration release --runtime ubuntu.18.04-x64

dotnet publish -c release -r ubuntu.18.04-x64 --self-contained true

cd ./bin/release/netcoreapp2.2/ubuntu.18.04-x64/

zip -r r.zip ./publish/


cd ../../../../