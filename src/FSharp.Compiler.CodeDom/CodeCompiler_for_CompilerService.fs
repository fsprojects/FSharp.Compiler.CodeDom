namespace FSharp.Compiler.CodeDom.Internal

// This file is not built as part of this library, but is here to provide a
// tested working example of how CodeCompiler.fs can be modified to use
// FSharp.Compiler.Service instead.  To do so requires installing this library
// and Compiler.Service into the GAC with all the usual signing shenanigans.
//
// Since CodeDom gives a sub-optimal F# experience, the amount of work needed
// to actually do such an installation is really not worth the effort,
// especially because this implementation actually takes longer to compile a
// single file than the version that calls out to fsc.exe (though this one is
// probably faster for compiling many files).

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Diagnostics
open System.Security
open System.Security.Permissions
open System.CodeDom.Compiler

open Microsoft.FSharp.Compiler

module internal AssemblyAttributes =
    //[<assembly: System.Security.SecurityTransparent>]
    do()

//-------------------------------------------------------------------------------------------------
module internal Global =
    let debugCmdLineArgs = true

//-------------------------------------------------------------------------------------------------
module internal Compiler =
    // Generate command-line arguments for FSC
    let cmdArgsFromParameters (o:CompilerParameters) (filenames:string[]) =
      let arr = new ResizeArray<string>()
      arr.Add("fsc.exe")
      if not o.GenerateExecutable then arr.Add("-a")
      for x in o.ReferencedAssemblies do arr.Add("-r:" + x)
      arr.AddRange([|"--noframework"; "--nologo"|])
      arr.Add("-o:" + o.OutputAssembly)
      if o.IncludeDebugInformation then arr.Add("--debug+")
      if o.Win32Resource <> null then arr.Add("--win32res:" + o.Win32Resource)
      if o.CompilerOptions <> null then arr.AddRange(o.CompilerOptions.Split(' '))
      // Never treat warnings as errors - this overrides "#nowarn", but the generated code
      // will contain some warnings in almost any case...
      // if (o.TreatWarningsAsErrors) then arr.Add("--warnaserror")

      for fn in filenames do
        arr.Add(fn)

      arr.ToArray()

    let processOutput (msgs:FSharpErrorInfo[]) (res:CompilerResults)=
      for m in msgs do
        let errNo = m.Subcategory
        let ce = CompilerError(m.FileName, m.StartLineAlternate, m.StartColumn,
                               errNo, m.Message)
        ce.IsWarning <- m.Severity = FSharpErrorSeverity.Warning
        res.Errors.Add(ce) |> ignore

    // Invoke FSC compiler and parse output
    let compileFiles args (res:CompilerResults) =
        let scs = SimpleSourceCodeServices.SimpleSourceCodeServices()
        let errors, exitcode = scs.Compile(args)

        // useful when debugging
        if (Global.debugCmdLineArgs) then
          let s = res.TempFiles.AddExtension("cmdargs")
          use sw = new StreamWriter(s)
          sw.WriteLine(args)

        processOutput errors res
        res.NativeCompilerReturnValue <- exitcode

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
