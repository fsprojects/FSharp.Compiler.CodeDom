namespace Internal.Utilities
open System
open System.Runtime.InteropServices

module internal FSharpEnvironment =

    // The F# team version number. This version number is used for
    //     - the F# version number reported by the fsc.exe and fsi.exe banners in the CTP release
    //     - the F# version number printed in the HTML documentation generator
    //     - the .NET DLL version number for all VS2008 DLLs
    //     - the VS2008 registry key, written by the VS2008 installer
    //         HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\AssemblyFolders\Microsoft.FSharp-" + FSharpTeamVersionNumber
    // Also
    //     - for Beta2, the language revision number indicated on the F# language spec
    //
    // It is NOT the version number listed on FSharp.Core.dll
    let FSharpTeamVersionNumber = "1.9.9.9"

    module Option =
        /// Convert string into Option string where null and String.Empty result in None
        let ofString s =
            if String.IsNullOrEmpty s then None
            else Some s

    let tryRegKey subKey =
        Option.ofString
            (try
                downcast Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\"+subKey,null,null)
             with e->
                System.Diagnostics.Debug.Assert(false, sprintf "Failed in tryRegKey: %s" (e.ToString()))
                null)

    let BinFolderOfDefaultFSharpCompiler =
        try
            let key40wow = @"Software\Wow6432Node\Microsoft\FSharp\4.0\Runtime\v4.0"
            let key31wow = @"Software\Wow6432Node\Microsoft\FSharp\3.1\Runtime\v4.0"
            let key30wow = @"Software\Wow6432Node\Microsoft\FSharp\3.0\Runtime\v4.0"
            let key20wow = @"Software\Wow6432Node\Microsoft\FSharp\2.0\Runtime\v4.0"
            let key40 = @"Software\Microsoft\FSharp\4.0\Runtime\v4.0"
            let key31 = @"Software\Microsoft\FSharp\3.1\Runtime\v4.0"
            let key30 = @"Software\Microsoft\FSharp\3.0\Runtime\v4.0"
            let key20 = @"Software\Microsoft\FSharp\2.0\Runtime\v4.0"

            match tryRegKey key40 with
            | Some r ->  Some r
            | None ->
            match tryRegKey key40wow with
            | Some r ->  Some r
            | None ->
            match tryRegKey key31 with
            | Some r ->  Some r
            | None ->
            match tryRegKey key31wow with
            | Some r ->  Some r
            | None ->
            match tryRegKey key30 with
            | Some r ->  Some r
            | None ->
            match tryRegKey key30wow with
            | Some r ->  Some r
            | None ->
            match tryRegKey key20 with
            | Some r ->  Some r
            | None ->
            match tryRegKey key20wow with
            | Some r ->  Some r
            | None ->
            Option.ofString(Environment.GetEnvironmentVariable("FSHARPINSTALLDIR"))
        with _ ->
            System.Diagnostics.Debug.Assert(false, "Error while determining default location of F# compiler")
            None

