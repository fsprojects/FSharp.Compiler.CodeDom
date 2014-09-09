namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Compiler.CodeDom")>]
[<assembly: AssemblyProductAttribute("FSharp.Compiler.CodeDom")>]
[<assembly: AssemblyDescriptionAttribute("A limited CodeDom implemenation for F#")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
