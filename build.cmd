@echo off

.paket\paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

IF NOT EXIST build.fsx (
  .paket\paket.exe update
  packages\build\FAKE\tools\FAKE.exe init.fsx
)

set fsiargs=--fsiargs -d:BUILD_INSTALLER

IF NOT "x%1" == "x--build-installer" (
  packages\build\FAKE\tools\FAKE.exe build.fsx %*
) ELSE (
  packages\build\FAKE\tools\FAKE.exe %2 %3 %4 %5 %6 %7 %8 %9 %fsiargs% build.fsx
)
