(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
Tutorial
========

This is an example of generating some code using the FSharpCleanCodeProvider. First we reference the assembly:
*)
#r "FSharp.Compiler.CodeDom.dll"
open FSharp.Compiler.CodeDom

(** 

Next we create an instance of the provider:
*)
let cleanProvider = new FSharp.Compiler.CodeDom.FSharpCleanCodeProvider()

(**
Now we add some Code DOM constructs to a code unit:
*)
open System.CodeDom

let ccu = CodeCompileUnit()
let nsp = CodeNamespace("MyNamespace")
let ty = CodeTypeDeclaration("MyClass")
let meth = CodeEntryPointMethod(Name="Main")
ty.Members.Add meth
nsp.Types.Add(ty) 
ccu.Namespaces.Add(nsp)

(**
Now we generate the code:
*)

let options = Compiler.CodeGeneratorOptions()

let sw = new System.IO.StringWriter()
cleanProvider.GenerateCodeFromCompileUnit(ccu,sw,options)
let code = sw.ToString()

(**

The generated code is shown as a string below:

*)
"""
namespace MyNamespace

type MyClass() = 
        static member Main  () =
            ()

module __EntryPoint =

    [<EntryPoint>]
    let Main (args:string[]) =
        MyClass.Main()
        0"
"""
(** 
Some extra code has been generated to match the F# model for entry points.

*)
