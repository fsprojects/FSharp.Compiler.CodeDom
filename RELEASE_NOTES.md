#### 1.0.0.1 - March 28 2016
* Full support for system-wide installation on Mono.  As on Windows, this allows
  for "soft-referencing" FSCC via Compiler.CodeDomProvider.CreateProvider,
  enabling any CodeDom-based program to generate F# code (apart from
  poorly-written programs which hard-code the list of providers they support).
  Invoking the compiler via CodeDom now also works.
* The MSI installer now supports upgrades.
* The build system has been updated to mirror the most recent ProjectScaffold.
  IDE builds should still work, but it is recommended to run "build Debug" or
  "build Release" instead (or "build BuildPackage" to generate a NuGet package).
* Source code is now available during debugging or upon jumping to a symbol's
  definition, courtesy of SourceLink.

#### 0.9.4 - January 18 2016
* Change MSI installer to also install the provider assembly to the Program
  Files directory and add an AssemblyFolders registry key so that the provider
  will show up in the VS "add reference" window
* Escape reserved F# name/keyword identifiers
* Change CreateEscapedIdentifier and CreateValidIdentifier to behave in
  accordance with CodeDom recommendations

#### 0.9.3 - September 18 2015
* Add ability to specify path to fsc via FSHARPINSTALLPATH env variable

#### 0.9.2 - March 23 2015
* Update FSharp.Core version to 4.3.1.0, this helps in situations where assembly redirects cannot be applied, and most people are using F# 3.1.x

#### 0.9.1 - September 9 2014
* Add clean code generator from https://github.com/fsharp/fsharpbinding/tree/master/monodevelop/MonoDevelop.FSharpBinding/PowerPack

#### 0.9.0 - September 9 2014
* Initial import of code from fsharppowerpack.codeplex.com libs\Dec2012\src
* Adjust namespace names
