module FSharp.Compiler.CodeDom.Tests

open FSharp.Compiler.CodeDom
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  Assert.AreEqual(42,42)
