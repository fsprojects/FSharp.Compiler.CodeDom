@if "%_echo%"=="" echo off

setlocal
if EXIST build.ok DEL /f /q build.ok

del /s /q cdtsuite.exe > NUL 2>&1
del /s /q test.out > NUL 2>&1
del /s /q test.err > NUL 2>&1

REM == Invoke resgen to create .cs files out of .resx
resgen.exe tests\Properties\Resources.resx    /str:cs,tests.Properties,Resources,tests\Properties\Resources.Designer.cs
resgen.exe CodeDomTest\Properties\Resources.resx /str:cs,CodeDomTest.Properties,Resources,CodeDomTest\Properties\Resources.Designer.cs

msbuild.exe CodeDomTest\CodeDOM.TestCore.csproj
msbuild.exe tests\CodeDOM.Tests.csproj
msbuild.exe CodeDOM.TestSuite.csproj

if not exist bin\Debug\CdtSuite.exe goto Error

bin\Debug\CdtSuite.exe /testcaselib:bin\Debug\tests.dll /codedomproviderlib:..\..\bin\FSharp.Compiler.CodeDom.dll /codedomprovider:FSharp.Compiler.CodeDom.FSharpCodeProvider > test.out

if not exist test.out goto Error
type test.out

type test.out | find /C "TEST FAILED" > test.err
for /f %%c IN (test.err) do (if NOT "%%c"=="0" (
   echo Error: CodeDom TEST FAILURES DETECTED IN TESTS PREVIOUSLY KNOWN TO PASS!
   type test.out | find "TEST FAILED"
   set NonexistentErrorLevel 2> nul
   goto Error)
)

echo Ran fsharp CodeDom tests OK (ignore "FAIED" above- some failures expected)
:Ok
echo. > build.ok
endlocal
exit /b 0

:Error
call %SCRIPT_ROOT%\ChompErr.bat %ERRORLEVEL% %~f0
endlocal
exit /b %ERRORLEVEL%