using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

// This installer is intended for Mono users.  While this will work on Windows,
// the MSI installer is more convenient, as it handles the entire installation
// process and also enables easy upgrades of FSharp.Compiler.CodeDom.

// Run InstallUtil.exe on this dll to register the provider in the GAC.  Note
// that this installer does not install the provider into the GAC; you must do
// it yourself using gacutil.exe.  On Mono, installutil is in /usr/lib/mono/4.5
// or a similar directory.

namespace FSharp.Compiler.CodeDom
{
[RunInstaller(true)]
public class SimpleInstaller : Installer
{
    override public void Install(IDictionary savedState)
    {
        base.Install(savedState);
        InstallLib.addEntry(typeof(FSharpCodeProvider).AssemblyQualifiedName);
    }

    override public void Uninstall(IDictionary savedState)
    {
        base.Uninstall(savedState);
        InstallLib.removeEntry();
    }

    override public void Commit(IDictionary savedState)
    {
        base.Commit(savedState);
    }

    override public void Rollback(IDictionary savedState)
    {
        base.Rollback(savedState);
    }
}
}
