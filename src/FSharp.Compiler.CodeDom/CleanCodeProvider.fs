#nowarn "62" // This construct is for ML compatibility.

//-------------------------------------------------------------------------------------------------
// Public types

namespace FSharp.Compiler.CodeDom

open System
open System.IO
open System.Text
open System.CodeDom.Compiler
open FSharp.Compiler.CodeDom.Internal

type FSharpCleanCodeProvider() = 
    inherit CodeDomProvider()

    [<System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")>]
    override __.FileExtension = "fs"
    
    [<System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")>]
    override __.CreateCompiler() = 
        raise (NotSupportedException("Compilation not supported."))

    [<System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")>]            
    override __.CreateGenerator() =
        let usingStringWriter f =
            let sb = new StringBuilder()
            use sw = new StringWriter(sb)
            f (sw :> TextWriter)
            let res = sb.ToString()
            res
      
        { new ICodeGenerator with
            // Identifier related functions            
            member __.CreateEscapedIdentifier value = Generator.makeEscapedIdentifier value

            member __.CreateValidIdentifier value = Generator.makeValidIdentifier value

            member __.IsValidIdentifier value = Generator.isValidIdentifier value 

            member __.ValidateIdentifier value =
              if (not (Generator.isValidIdentifier value)) then 
                raise (ArgumentException(sprintf "'%s' is not a valid F# identifier!" value))
  
            // Implementations of code generation related functions
            member __.GenerateCodeFromCompileUnit(compileUnit, textWriter, options) =
                CleanGenerator.createContext textWriter options CleanGenerator.AdditionalOptions.None
                |> CleanGenerator.generateCompileUnit compileUnit 
                |> ignore
            
            member __.GenerateCodeFromExpression(codeExpr, textWriter, options) =
                CleanGenerator.createContext textWriter options CleanGenerator.AdditionalOptions.None
                |> CleanGenerator.generateExpression codeExpr
                |> ignore
            
            member __.GenerateCodeFromNamespace(codeNamespace, textWriter, options) =
                CleanGenerator.createContext textWriter options CleanGenerator.AdditionalOptions.None
                |> CleanGenerator.generateNamespace codeNamespace
                |> ignore
            
            member __.GenerateCodeFromStatement(codeStatement, textWriter, options) =
                CleanGenerator.createContext textWriter options CleanGenerator.AdditionalOptions.None
                |> CleanGenerator.generateStatement codeStatement
                |> ignore
                
            member __.GenerateCodeFromType(codeTypeDecl, textWriter, options) =
                CleanGenerator.createContext textWriter options CleanGenerator.AdditionalOptions.None 
                |> CleanGenerator.generateTypeDeclOnly codeTypeDecl
                |> ignore

            member __.GetTypeOutput t =
                usingStringWriter (fun sw ->
                  CleanGenerator.createContext sw (CodeGeneratorOptions()) CleanGenerator.AdditionalOptions.None
                  |> CleanGenerator.generateTypeRef t
                  |> ignore)

            member __.Supports supports =
              (supports &&&  (GeneratorSupport.ReturnTypeAttributes ||| 
                              GeneratorSupport.ParameterAttributes ||| 
                              GeneratorSupport.AssemblyAttributes ||| 
                              GeneratorSupport.StaticConstructors ||| 
                              GeneratorSupport.NestedTypes ||| 
                              GeneratorSupport.EntryPointMethod |||
                              GeneratorSupport.GotoStatements ||| 
                              GeneratorSupport.MultipleInterfaceMembers |||
                              GeneratorSupport.ChainedConstructorArguments
                              ) = enum 0) }

