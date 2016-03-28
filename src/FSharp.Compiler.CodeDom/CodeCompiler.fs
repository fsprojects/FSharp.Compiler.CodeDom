namespace FSharp.Compiler.CodeDom.Internal

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Diagnostics
open System.Security
open System.Security.Permissions
open System.CodeDom.Compiler

module internal AssemblyAttributes =
    //[<assembly: System.Security.SecurityTransparent>]
    do()

//-------------------------------------------------------------------------------------------------
module internal Global =
    let debugCmdLineArgs = true
    let (++) x y = Path.Combine(x,y)

    // search for "fsc.exe"
    let FscExeBaseName =
      if System.Type.GetType("Mono.Runtime") <> null then "fsharpc"
      else "fsc.exe"


    let FscPath =
        let tryFscPath x =
            match x with
            | None -> None
            | Some dir ->
                 let fscName = dir  ++ FscExeBaseName
                 if File.Exists(fscName) then Some(fscName) else None

        let search0 = tryFscPath Internal.Utilities.FSharpEnvironment.BinFolderOfDefaultFSharpCompiler
        match search0 with
        | Some res -> res
        | None -> FscExeBaseName

    // regular expressions for parsing result
    let regParseFsOutput = Regex(@"(?<file>[^\(]*)\((?<line>[0-9]*),(?<col>[0-9]*)\):\s(?<type>[^:]*)\s(?<err>[^:]*):\s(?<msg>.*)", RegexOptions.Compiled)
    let regParseFsOutputNoNum = Regex(@"(?<file>[^\(]*)\((?<line>[0-9]*),(?<col>[0-9]*)\):\s(?<type>[^:]*)\s(?<msg>.*)", RegexOptions.Compiled)

//-------------------------------------------------------------------------------------------------
module internal Compiler =
    let id a = a
    let (+>) (ctx:StringBuilder) (foo:StringBuilder -> StringBuilder) = foo ctx
    let (--) (ctx:StringBuilder) (str:string) = ctx.Append(str)
    let untypedFold f col st = Seq.fold f st (Seq.cast col)

    // Generate command-line arguments for FSC
    let cmdArgsFromParameters (o:CompilerParameters) (filenames:string[]) =
      let sb = new StringBuilder(50)
      (sb
        +> if (not o.GenerateExecutable) then (fun ctx -> ctx -- "-a ") else id
        +> untypedFold (fun ctx e -> ctx -- "-r:\"" -- e -- "\" ") o.ReferencedAssemblies
        -- "--noframework --nologo -o:\"" -- o.OutputAssembly -- "\" "
        +> if (o.IncludeDebugInformation) then (fun ctx -> ctx -- "--debug+ ") else id
        +> if (o.Win32Resource <> null) then (fun ctx -> ctx -- "--win32res:\"" -- o.Win32Resource -- "\" ") else id
        +> if (o.CompilerOptions <> null) then (fun ctx -> ctx -- o.CompilerOptions -- " ") else id)

        // Never treat warnings as errors - this overrides "#nowarn", but the generated code
        // will contain some warnings in almost any case...
        //   +> if (o.TreatWarningsAsErrors) then (fun ctx -> ctx -- "--warnaserror ") else id

        |> ignore

      for fn in filenames do
        sb.AppendFormat(" \"{0}\"", fn) |> ignore

      sb.ToString()

    // Process FSC output
    let processMsg msg (res:CompilerResults) =
      let m =
        let t1 = Global.regParseFsOutput.Match(msg)
        if (t1.Success) then t1 else Global.regParseFsOutputNoNum.Match(msg)
      let ce =
        if (m.Success) then
          let errNo = (if (m.Groups.Item("err") <> null) then (m.Groups.Item("err")).Value else "")
          let ce = CompilerError(m.Groups.Item("file").Value, Int32.Parse(m.Groups.Item("line").Value),
                                 Int32.Parse(m.Groups.Item("col").Value), errNo, m.Groups.Item("msg").Value)
          ce.IsWarning <- ((m.Groups.Item("type")).Value = "warning")
          ce
        else new CompilerError("unknown-file", 0, 0, "0", msg)
      res.Errors.Add(ce) |> ignore

    // Invoke FSC compiler and parse output
    let compileFiles args (res:CompilerResults) =
        let p = new Process()
        p.StartInfo.FileName <- Global.FscPath
        p.StartInfo.UseShellExecute <- false
        p.StartInfo.Arguments <- args
        p.StartInfo.RedirectStandardError <- true
        p.StartInfo.CreateNoWindow <- true
        p.Start() |> ignore

        // useful when debugging
        if (Global.debugCmdLineArgs) then
          let s = res.TempFiles.AddExtension("cmdargs")
          use sw = new StreamWriter(s)
          sw.WriteLine(args)

        let mutable serr = ""
        let mutable smsg = ""
        while (serr <- p.StandardError.ReadLine(); serr <> null) do
          if (serr.Trim().Length = 0 && smsg <> "") then
            processMsg smsg res; smsg <- ""
          else
            smsg <- smsg + " " + (serr.Trim())
        if (smsg <> "") then processMsg smsg res
        p.WaitForExit()
        res.NativeCompilerReturnValue <- p.ExitCode

    // Compile assembly from given array of files
    let compileAssemblyFromFileBatch (options:CompilerParameters) (fileNames:string[])
                                     (results:CompilerResults) sortf : CompilerResults =

      // Call 'fix' sorting function
      let fileNames : string[] = sortf fileNames
      let createdAssembly =
        if (options.OutputAssembly = null || options.OutputAssembly.Length = 0) then begin
          let extension = if (options.GenerateExecutable) then "exe" else "dll"
          options.OutputAssembly <- results.TempFiles.AddExtension(extension, not options.GenerateInMemory)

          // Create an empty assembly, so the file can be later accessed using current credential.
          let fs = new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite)
          fs.Close()
          true
        end else false
      ignore(results.TempFiles.AddExtension("pdb"))

      // Compile..
      let args = cmdArgsFromParameters options fileNames
      compileFiles args results

      if (options.GenerateInMemory) then
        use fs = new FileStream(options.OutputAssembly, FileMode.Open, FileAccess.Read, FileShare.Read)
        let count = int32 fs.Length
        if (count > 0) then
          let buffer = (Array.zeroCreate count)
          fs.Read(buffer, 0, count) |> ignore
          (new SecurityPermission(SecurityPermissionFlag.ControlEvidence)).Assert()
          try
            results.CompiledAssembly <- System.Reflection.Assembly.Load(buffer, null)
          finally
            CodeAccessPermission.RevertAssert()
      else
        results.PathToAssembly <- options.OutputAssembly

      // Delete the assembly if we created it
      if createdAssembly then File.Delete(options.OutputAssembly)
      results
