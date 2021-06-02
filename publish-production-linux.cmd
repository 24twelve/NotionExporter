
@echo off
del commit_hash.txt
git rev-parse HEAD > commit_hash.txt
set /p COMMIT_HASH=<commit_hash.txt
set APP_PATH=/opt/NotionExporter
set CURRENT_APP_PATH=%APP_PATH%/current
set TEMP_PATH=%APP_PATH%/temp
set PUBLISH_PATH=deploy
set /p TARGET=<"NotionExporterWebApi\secrets\deploy-target"
set SYSTEMD_DST_PATH=/etc/systemd/system/notion-exporter.service
set SYSTEMD_SERVICE_NAME=notion-exporter.service
set DEPLOY_PATH=%APP_PATH%/deploy/%COMMIT_HASH%

echo Delete old publishes (%PUBLISH_PATH%/)
del "%PUBLISH_PATH%" /f /q

echo Begin publish for commit hash %COMMIT_HASH%
dotnet publish NotionExporter.sln -c Release --self-contained -r linux-x64 -o "%PUBLISH_PATH%" --nologo
cd "%PUBLISH_PATH%\secrets"
del config.json
ren config-production-linux.json config.json

echo Begin deploy to %TARGET%:%DEPLOY_PATH%
cd ../..
ssh -t %TARGET% "mkdir -p %DEPLOY_PATH% && rm -r %DEPLOY_PATH%/*"
scp -pr "%PUBLISH_PATH%/*" %TARGET%:%DEPLOY_PATH%

echo Copy systemd service config from to %SYSTEMln D_DST_PATH%
ssh -t %TARGET% "mkdir -p %TEMP_PATH% && rm -r %TEMP_PATH%/*"
scp %PUBLISH_PATH%/secrets/systemd.config %TARGET%:%TEMP_PATH%

echo Shut down systemd service
ssh -t %TARGET% "sudo mv %TEMP_PATH%/systemd.config %SYSTEMD_DST_PATH% && sudo systemctl disable %SYSTEMD_SERVICE_NAME% && sudo systemctl stop %SYSTEMD_SERVICE_NAME%"

echo Switch symlink and switch on systemd service
ssh -t %TARGET% "sudo rm %CURRENT_APP_PATH% && sudo ln -sf %DEPLOY_PATH% %CURRENT_APP_PATH% && sudo chmod +x %CURRENT_APP_PATH%/NotionExporterWebApi && sudo systemctl daemon-reload && sudo systemctl enable %SYSTEMD_SERVICE_NAME% && sudo systemctl start %SYSTEMD_SERVICE_NAME%"
timeout 5
ssh -t %TARGET% "sudo systemctl status %SYSTEMD_SERVICE_NAME% | grep  Active: && echo 'Running ' && curl http://localhost:5000/api/version"

echo Checking whether the current version is running (should be %COMMIT_HASH%)
del running_version.txt
ssh -t %TARGET% "curl http://localhost:5000/api/version">running_version.txt
findstr %COMMIT_HASH% running_version.txt
IF not %ERRORLEVEL%==1 (echo Versions match) ELSE (echo Running version differs from required && EXIT /B %ERRORLEVEL%)
