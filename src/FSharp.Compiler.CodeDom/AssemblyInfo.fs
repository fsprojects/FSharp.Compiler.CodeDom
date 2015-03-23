namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Compiler.CodeDom")>]
[<assembly: AssemblyProductAttribute("FSharp.Compiler.CodeDom")>]
[<assembly: AssemblyDescriptionAttribute("A limited CodeDom implementation for F#")>]
[<assembly: AssemblyVersionAttribute("0.9.2")>]
[<assembly: AssemblyFileVersionAttribute("0.9.2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.9.2"
