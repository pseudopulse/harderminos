rm -rf bin/Debug/netstandard2.0
dotnet restore
dotnet build
rm -rf ~/.steam/steam/steamapps/common/ULTRAKILL/BepInEx/plugins/netstandard2.0
cp -r bin/Debug/netstandard2.0 ~/.steam/steam/steamapps/common/ULTRAKILL/BepInEx/plugins/
