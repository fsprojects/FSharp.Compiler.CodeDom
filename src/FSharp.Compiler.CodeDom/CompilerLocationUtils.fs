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

    [<DllImport("Advapi32.dll", CharSet = CharSet.Unicode, BestFitMapping = false)>]
    extern uint32 RegOpenKeyExW(UIntPtr _hKey, string _lpSubKey, uint32 _ulOptions, int _samDesired, UIntPtr & _phkResult);

    [<DllImport("Advapi32.dll", CharSet = CharSet.Unicode, BestFitMapping = false)>]
    extern uint32 RegQueryValueExW(UIntPtr _hKey, string _lpValueName, uint32 _lpReserved, uint32 & _lpType, IntPtr _lpData, int & _lpchData);

    [<DllImport("Advapi32.dll")>]
    extern uint32 RegCloseKey(UIntPtr _hKey)

    module Option =
        /// Convert string into Option string where null and String.Empty result in None
        let ofString s =
            if String.IsNullOrEmpty s then None
            else Some s

    // MaxPath accounts for the null-terminating character, for example, the maximum path on the D drive is "D:\<256 chars>\0".
    // See: ndp\clr\src\BCL\System\IO\Path.cs
    let maxPath = 260;
    let maxDataLength = (new System.Text.UTF32Encoding()).GetMaxByteCount(maxPath)
    let KEY_WOW64_DEFAULT = 0x0000
    let KEY_WOW64_32KEY = 0x0200
    let HKEY_LOCAL_MACHINE = UIntPtr(0x80000002u)
    let KEY_QUERY_VALUE = 0x1
    let REG_SZ = 1u

    let GetDefaultRegistryStringValueViaDotNet(subKey: string)  =
        Option.ofString
            (try
                downcast Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\"+subKey,null,null)
             with e->
                System.Diagnostics.Debug.Assert(false, sprintf "Failed in GetDefaultRegistryStringValueViaDotNet: %s" (e.ToString()))
                null)

    let Get32BitRegistryStringValueViaPInvoke(subKey:string) =
        Option.ofString
            (try
                // 64 bit flag is not available <= Win2k
                let options =
                    match Environment.OSVersion.Version.Major with
                    | major when major >= 5 -> KEY_WOW64_32KEY
                    | _ -> KEY_WOW64_DEFAULT


                let mutable hkey = UIntPtr.Zero;
                let pathResult = Marshal.AllocCoTaskMem(maxDataLength);

                try
                    let res = RegOpenKeyExW(HKEY_LOCAL_MACHINE,subKey, 0u, KEY_QUERY_VALUE ||| options, & hkey)
                    if res = 0u then
                        let mutable uType = REG_SZ;
                        let mutable cbData = maxDataLength;

                        let res = RegQueryValueExW(hkey, null, 0u, &uType, pathResult, &cbData);

                        if (res = 0u && cbData > 0 && cbData <= maxDataLength) then
                            Marshal.PtrToStringUni(pathResult, (cbData - 2)/2);
                        else
                            null
                    else
                        null
                finally
                    if hkey <> UIntPtr.Zero then
                        RegCloseKey(hkey) |> ignore

                    if pathResult <> IntPtr.Zero then
                        Marshal.FreeCoTaskMem(pathResult)
             with e->
                System.Diagnostics.Debug.Assert(false, sprintf "Failed in Get32BitRegistryStringValueViaPInvoke: %s" (e.ToString()))
                null)

    let is32Bit = IntPtr.Size = 4

    let tryRegKey(subKey:string) =
        if is32Bit then
            GetDefaultRegistryStringValueViaDotNet(subKey)
        else
            Get32BitRegistryStringValueViaPInvoke(subKey)

    let BinFolderOfDefaultFSharpCompiler =
        try
            let key31 = @"Software\Microsoft\FSharp\3.1\Runtime\v4.0"
            let key30 = @"Software\Microsoft\FSharp\3.0\Runtime\v4.0"
            let key20 = @"Software\Microsoft\FSharp\2.0\Runtime\v4.0"

            match tryRegKey key31 with
            | Some r ->  Some r
            | None ->
            match tryRegKey key30 with
            | Some r ->  Some r
            | None ->
            match tryRegKey key20 with
            | Some r ->  Some r
            | None ->
            None
        with _ ->
            System.Diagnostics.Debug.Assert(false, "Error while determining default location of F# compiler")
            None

