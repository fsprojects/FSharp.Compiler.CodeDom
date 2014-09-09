#nowarn "62" // This construct is for ML compatibility.

//-------------------------------------------------------------------------------------------------
// Internal members

namespace FSharp.Compiler.CodeDom

open System
open System.IO
open System.Text
open System.CodeDom
open System.CodeDom.Compiler

open FSharp.Compiler.CodeDom.Internal
      
// Generic code generator that can be specialized with preprocessing function and additional 
// CodeGenerator configuration options (this is used by ASP.NET generator)
//   preprocCu: 
type internal FSharpCodeGenerator(preprocCu, addop) = 
      
    let usingStringWriter f =
        let sb = new StringBuilder()
        use sw = new StringWriter(sb)
        f (sw :> TextWriter)
        let res = sb.ToString()
        res
          
    interface ICodeGenerator with

        // Identifier related functions            
        member __.CreateEscapedIdentifier value = Generator.makeEscapedIdentifier value

        member __.CreateValidIdentifier value = Generator.makeValidIdentifier value

        member __.IsValidIdentifier value = Generator.isValidIdentifier value 

        member __.ValidateIdentifier value =
          if (not (Generator.isValidIdentifier value)) then 
            raise (ArgumentException(sprintf "'%s' is not a valid F# identifier!" value))
      
        // Implementations of code generation related functions
        member __.GenerateCodeFromCompileUnit(compileUnit, textWriter, options) =
            Generator.createContext textWriter options addop
            |> Generator.generateCompileUnit compileUnit preprocCu
            |> ignore
                
        member __.GenerateCodeFromExpression(codeExpr, textWriter, options) =
            Generator.createContext textWriter options addop
            |> Generator.generateExpression codeExpr
            |> ignore
                
        member __.GenerateCodeFromNamespace(codeNamespace, textWriter, options) =
            Generator.createContext textWriter options addop
            |> Generator.generateNamespace codeNamespace
            |> ignore
                
        member __.GenerateCodeFromStatement(codeStatement, textWriter, options) =
            Generator.createContext textWriter options addop
            |> Generator.generateStatement codeStatement
            |> ignore
                    
        member __.GenerateCodeFromType(codeTypeDecl, textWriter, options) =
            Generator.createContext textWriter options addop
            |> Generator.generateTypeDeclOnly codeTypeDecl
            |> ignore

        member __.GetTypeOutput t =
            usingStringWriter (fun sw ->
              Generator.createContext sw (CodeGeneratorOptions()) addop
              |> Generator.generateTypeRef t |> ignore)

        member __.Supports (supports) =
           supports &&&  
           (GeneratorSupport.ReturnTypeAttributes ||| 
            GeneratorSupport.ParameterAttributes ||| 
            GeneratorSupport.AssemblyAttributes ||| 
            GeneratorSupport.StaticConstructors ||| 
            GeneratorSupport.NestedTypes ||| 
            GeneratorSupport.EntryPointMethod |||
            GeneratorSupport.GotoStatements ||| 
            GeneratorSupport.MultipleInterfaceMembers |||
            GeneratorSupport.ChainedConstructorArguments) = enum 0

// Generic code compiler - the argument is a function for sorting files that can be used by
// tool-specific providers if needed (like in case of ASP.NET)
type internal FSharpCodeCompiler(sortFiles) = 
    interface ICodeCompiler with       
        member this.CompileAssemblyFromDom (options,compileUnit) =
          (this :> ICodeCompiler).CompileAssemblyFromDomBatch (options, [|compileUnit|])

        member this.CompileAssemblyFromSource (options,source) =
          (this :> ICodeCompiler).CompileAssemblyFromSourceBatch (options, [|source|])                

        member this.CompileAssemblyFromFile (options,fileName) =
          (this :> ICodeCompiler).CompileAssemblyFromFileBatch (options, [|fileName|])
                    
        member __.CompileAssemblyFromDomBatch (options,compilationUnits) =
          let res = new CompilerResults(options.TempFiles)
          let files = 
              compilationUnits 
              |> Array.map ( fun cu -> 
                  let fn = res.TempFiles.AddExtension("fs", false)
                  use wr = new StreamWriter(fn, false, Encoding.UTF8)
                  Generator.createContext wr (CodeGeneratorOptions()) Generator.AdditionalOptions.None 
                      |> (Generator.generateCompileUnit cu (fun _ -> ())) |> ignore
                  fn)        
          Compiler.compileAssemblyFromFileBatch options files res sortFiles

        member __.CompileAssemblyFromSourceBatch (options,sources) =
            let res = new CompilerResults(options.TempFiles)
            let files = 
                sources |> Array.map (fun src -> 
                    let fn = res.TempFiles.AddExtension("fs", false)
                    use wr = new StreamWriter(fn)
                    wr.Write(src) 
                    fn)

            Compiler.compileAssemblyFromFileBatch options files res sortFiles       
                  
        member __.CompileAssemblyFromFileBatch (options,fileNames) =
          Compiler.compileAssemblyFromFileBatch options fileNames (CompilerResults(options.TempFiles)) sortFiles
          

type FSharpCodeProvider() = 
    inherit CodeDomProvider()
    override this.FileExtension = "fs"
        
    override __.CreateCompiler() =
        FSharpCodeCompiler(id) :> ICodeCompiler

    override __.CreateGenerator() =
        FSharpCodeGenerator((fun _ -> ()), Generator.AdditionalOptions.None) :> _

type FSharpAspNetCodeProvider() = 

    inherit CodeDomProvider()

    /// Preprocessing of the CodeDom compile unit from ASP.NET
    let aspNetPreProcessCompileUnit (c:CodeCompileUnit) =
        
        // Remove partial calsses from ASP.NET generated code (we can leave empty namespaces)
        // F# doesn't support partial classes, so we need to remove generated 'second' part of the 
        // handwritten class that contains fields for all controls - these have to be written 
        // manually in the handwritten code...
        for ns in c.Namespaces do
            // Phase 1. Find classes to remove
            let toRemove = 
                ns.Types 
                |> Seq.cast
                |> Seq.filter (fun (t:CodeTypeDeclaration)  -> t.IsPartial ) 
                |> Seq.toList
            // Phase 2. remove (destructive). 
            toRemove |> List.iter ns.Types.Remove;

            
        // Fix one very specific bug in ASP.NET generated CodeDom tree.
        // In one case it generates "<null>.SetStringResourcePointer" instead of
        // "this.SetStringResourcePointer" (Reported here: 
        // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=164512)
        for ns in c.Namespaces do
          for t in ns.Types do 
            for m in t.Members do 
              match m with 
                | :? CodeMemberMethod as meth ->
                    if (meth.Name = "FrameworkInitialize") then 
                      for smt in meth.Statements do
                        match smt with 
                        | :? CodeExpressionStatement as stm -> 
                            match stm.Expression with 
                              | :? CodeMethodInvokeExpression as inv ->
                                if ((inv.Method.TargetObject = null) && (inv.Method.MethodName = "SetStringResourcePointer")) then 
                                  inv.Method.TargetObject <- new CodeThisReferenceExpression()
                              | _ -> ()
                        | _ -> ()
                    else ()
                | _ -> ()
              

    // Sorts ASP.NET source files
    // ASP.NET compiler sends three kinds of files
    // - CodeDOM generated file from ASPX file
    // - handwritten file (codebehind for the previous)
    // - fast factory for creating the first one
    // 
    // We need to send them to compiler in following order:
    // - handwritten, aspx, handwritten, aspx, ..., factory
    // 
    // File name is: "Something.num.fs" where num is an index, but the array isn't
    // sorted so we sort them using the index. Then we find all ASPX and swap 
    // ASPX and handwritten file. Factory should be the last file.
        

    /// Does the file contain the given text?
    let fileContains (text:string) (filename:string) = 
      try
        use fs = new IO.StreamReader(filename)
        let rec testLine () = 
            let line = fs.ReadLine()
            (line <> null) && (line.IndexOf(text) <> -1 || testLine())
        testLine()
      with _ -> false

    /// Swap two elements of an array
    let swap (arr:'a[]) i j = 
        let tmp = arr.[j] 
        arr.[j] <- arr.[i]
        arr.[i] <- tmp


    /// Compiled forms of ASPX source files always contain "(__initialized:bool)".
    /// The factory file contains "static member Create_ASP_"
    let isFactoryFile filename = 
        fileContains "static member Create_ASP_" filename &&
        fileContains "FastObjectFactory" filename 

    let isGeneratedFromAspxFile filename = 
        fileContains "static val mutable private __initialized:bool" filename &&
        fileContains "<autogenerated>" filename

    let isUserFile filename = 
        not (isGeneratedFromAspxFile filename)  &&
        not (isFactoryFile filename) 

      // REVIEW: will "Temporary ASP.NET Files" survive internationalization????
    let indexOfAspNetFile(el:string) = 
          let n = 
            try
              let bnum = el.LastIndexOf(".", el.Length - 4)
              Int32.Parse(el.Substring(bnum + 1, el.Length - bnum - 4))
            with _ -> -1
          if (el.IndexOf("Temporary ASP.NET Files") <> -1) 
          then n
          else -1
      
          
    let aspNetSortFiles (files:string[]) =

        // Verify that files are from ASP.NET & sort them.
        let aspFiles = files |> Array.map (fun el -> indexOfAspNetFile el, el)              
        let allAspNet = aspFiles |> Array.forall (fun (n, a) -> n <> -1)
            
        let sortedFiles = 
            if (allAspNet) then
                Array.sortInPlaceWith (fun (n1, _) (n2, _) -> n1 - n2) aspFiles
                let filesMod = aspFiles |> Array.map snd 
                    
                //System.Windows.Forms.MessageBox.Show(sprintf "filesMod = %A" filesMod) |> ignore
                // Rearrange files
                // NOTE: ack! surely there's a nicer way to do this!
                let mutable i = 0
                while (i < filesMod.Length) do
                    if  (isGeneratedFromAspxFile filesMod.[i]
                          && not (isFactoryFile filesMod.[i]) 
                          && isUserFile filesMod.[i+1]) 
                    then
                        swap filesMod i (i+1)
                        i <- i + 1
                    i <- i + 1
                filesMod
            else
                files
        //System.Windows.Forms.MessageBox.Show(sprintf "sortedFiles = %A" sortedFiles) |> ignore
        sortedFiles

    override this.FileExtension = "fs"
        
    override this.CreateCompiler() =
        FSharpCodeCompiler(aspNetSortFiles) :> ICodeCompiler
            
    override this.CreateGenerator() =
            
        // If you set the options to "Generator.AdditionalOptions.UnknonwFieldsAsLocals"
        // than the generator will treat all fields that are not declared in the class as locals
        // (and will generated "fld" instead of "this.fld")            
        // This will make it possible to use implicit class syntax in ASP.NET, but only when
        // the fields will have non-private visibility (because we need to access them from inherited class
            
        // AspNetArrays is workaround for ASP.NET which generates wrong type in array initializers
        // (according to the CodeDOM test suite it should be "int[]" for [| 1; 2 |], but ASP.NET gives us "int" !! 
        let opts = Generator.AdditionalOptions.AspNetArrays
        FSharpCodeGenerator(aspNetPreProcessCompileUnit, opts) :> _
        
