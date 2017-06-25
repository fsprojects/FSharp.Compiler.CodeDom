(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
### Installation

FSharp.Compiler.CodeDom supports installation via both NuGet and system-wide.
NuGet installation allows for basic use of the library within your application
or library.  A system-wide installation enables any CodeDom-capable program or
library to make use of the library, but requires administrator access (on
Windows) or write permission to the Mono GAC and `machine.config` file (on
Unix).  A system-wide install can coexist with individual NuGet installs.

To perform a system-wide install on Windows, build the library from the command
line with `build Release --build-installer`.  A MSI package will be produced in
`Installer\Installer\bin\Release`.  After installation, the library will be
available for use.

To perform a system-wide install on Unix, build the library from the command
line with `build Release`.  Navigate to `SimpleInstaller/bin/Release`, and
then, as a user with permission to write to the Mono GAC directory (e.g.
`/usr/lib/mono/gac`) and `machine.config` file (e.g.
`/etc/mono/4.5/machine.config`), run:

    [lang=sh]
    $ gacutil -i FSharp.Compiler.CodeDom.dll
    $ mono /usr/lib/mono/4.5/installutil.exe SimpleInstaller.dll

### Uninstallation

On Windows, uninstallation can be performed through Add/Remove Programs,
PowerShell, etc.

On Unix, as a user with the aforementioned privileges:

    [lang=sh]
    $ gacutil -u FSharp.Compiler.CodeDom # from any directory
    $ mono /usr/lib/mono/4.5/installutil.exe /uninstall SimpleInstaller.dll

As an alternative to using SimpleInstaller, the FSharp.Compiler.CodeDom line
under `<system.codedom>` can be manually deleted from `machine.config`.

### A note on CodeDom providers

CodeDom provides a mechanism that allows for programs which use CodeDom to
generate code for any language for which a provider is installed--the program
need not have any knowledge of that language or the existence of a provider.
Unfortunately, many programs (including ones from Microsoft!) are poorly written
and hard-code a list of supported providers, usually to only C# and VB,
destroying much of CodeDom's utility.  Properly written programs should always
allow the user to specify an arbitrary string (corresponding to a language with
a system-registered provider) to be passed to
`System.CodeDom.Compiler.CodeDomProvider.CreateProvider`.  This way, there is no
need to directly reference assemblies or hard-code providers, as any provider
available to the system can be used.

### Basic use of the provider (system-wide install)
*)

open System.IO
open System.CodeDom

let prov = Compiler.CodeDomProvider.CreateProvider "fs"

let ccu = CodeCompileUnit()
let nsp = CodeNamespace("MyNamespace")
let ty = CodeTypeDeclaration("MyClass")
let meth = CodeEntryPointMethod(Name="Main")
ty.Members.Add meth
nsp.Types.Add(ty) 
ccu.Namespaces.Add(nsp)

let options = Compiler.CodeGeneratorOptions()

let sw = new StringWriter()
prov.GenerateCodeFromCompileUnit(ccu,sw,options)
let code = sw.ToString()

(**

### Tutorial

First, reference the assembly (not needed if a system-wide install was done):
*)

#r "FSharp.Compiler.CodeDom.dll"

(**

Next, create an instance of the provider:
*)

open System.CodeDom

// if system-wide:
let provider = Compiler.CodeDomProvider.CreateProvider "fs"

// OR, if NuGet:
open FSharp.Compiler.CodeDom
let provider = new FSharpCodeProvider()

// OR, if the clean code provider is desired, regardless of installation type:
open FSharp.Compiler.CodeDom
let provider = new FSharpCleanCodeProvider()

(**

Now, add some Code DOM constructs to a code unit:
*)

let ccu = CodeCompileUnit()
let nsp = CodeNamespace("MyNamespace")
let ty = CodeTypeDeclaration("MyClass")
let meth = CodeEntryPointMethod(Name="Main")
ty.Members.Add meth
nsp.Types.Add(ty) 
ccu.Namespaces.Add(nsp)

(**

Generate the code:
*)

let options = Compiler.CodeGeneratorOptions()

let sw = new System.IO.StringWriter()
provider.GenerateCodeFromCompileUnit(ccu,sw,options)
let code = sw.ToString()
