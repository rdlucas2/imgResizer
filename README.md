```
dotnet publish -r win-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -p:PublishReadyToRun=true
```

```
dotnet publish -r linux-x64 -c LinuxRelease --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:PublishTrimmed=true -p:PublishReadyToRun=false
```