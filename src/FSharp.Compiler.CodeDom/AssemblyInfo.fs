namespace System

open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Compiler.CodeDom.dll")>]
[<assembly: AssemblyProductAttribute("FSharp.Compiler.CodeDom.dll")>]
[<assembly: AssemblyCompanyAttribute("F# Software Foundation, Microsoft")>]
#if DEBUG
[<assembly: AssemblyConfigurationAttribute("Debug")>]
#else
[<assembly: AssemblyConfigurationAttribute("Release")>]
#endif
[<assembly: AssemblyCopyrightAttribute("Copyright 2015")>]
[<assembly: AssemblyDescriptionAttribute("A limited CodeDom implementation for F#")>]
[<assembly: AssemblyVersionAttribute("0.9.4")>]
[<assembly: AssemblyFileVersionAttribute("0.9.4")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.9.4"
