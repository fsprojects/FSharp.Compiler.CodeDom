namespace FSharp.Compiler.CodeDom

open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Compiler.CodeDom.dll")>]
[<assembly: AssemblyProductAttribute("FSharp.Compiler.CodeDom.dll")>]
[<assembly: AssemblyDescriptionAttribute("A limited CodeDom implementation for F#")>]
[<assembly: AssemblyVersionAttribute("0.9.4")>]
[<assembly: AssemblyFileVersionAttribute("0.9.4")>]
[<assembly: AssemblyDefaultAliasAttribute("FSharp.Compiler.CodeDom.dll")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.9.4"
