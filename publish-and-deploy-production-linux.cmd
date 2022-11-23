echo Delete old publishes (deploy/)
del "deploy" /f /q

echo Begin publish
dotnet publish NotionExporter.sln -c Release --self-contained -r linux-x64 -o "deploy" --nologo

wsl ansible-playbook ./scripts/deploy/deploy-production-linux.yml -i deploy/secrets/inventory.yml