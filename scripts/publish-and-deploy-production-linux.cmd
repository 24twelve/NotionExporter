@echo off
set PUBLISH_PATH=deploy

echo Delete old publishes (%PUBLISH_PATH%/)
del "%PUBLISH_PATH%" /f /q

echo Begin publish
dotnet publish NotionExporter.sln -c Release --self-contained -r linux-x64 -o "%PUBLISH_PATH%" --nologo
cd "%PUBLISH_PATH%\secrets"
del runtime-config.json
ren runtime-config-production-linux.json runtime-config.json

cd ../..

wsl ansible-playbook deploy-production-linux.yml -i deploy/secrets/hosts.yml