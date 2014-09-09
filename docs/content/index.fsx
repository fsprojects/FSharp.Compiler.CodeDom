(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
F# Compiler CodeDom Implementations
===================

Documentation

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The F# Compiler CodeDom tool can be <a href="https://nuget.org/packages/FSharp.ProjectTemplate">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.Compiler.CodeDom</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Overview
-------

The FSharp.Compiler.CodeDom component contains three CodeDom code generators related to F#.

* _FSharpCodeProvider_. Generates compilable, correct but messy F# code for as many CodeDom constructs as possible. 
  Suitable for generating code that is never read nor edited by humans.

* _FSharpCleanCodeProvider_.  Generates clean F# code for a smaller subset of CodeDom constructs. 
  Suitable for generating templates of code that may later be edited by humans.

* _FSharpAspNetCodeProvider_. A legacy provider for quirks associated with earlier versions of ASP.NET code.

Example
-------

Please see the [tutorial](tutorial.html).

Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include a tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [Tutorial](tutorial.html) contains a further explanation of this sample library.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/FSharp.Compiler.CodeDom/tree/master/docs/content
  [gh]: https://github.com/fsprojects/FSharp.Compiler.CodeDom
  [issues]: https://github.com/fsprojects/FSharp.Compiler.CodeDom/issues
  [readme]: https://github.com/fsprojects/FSharp.Compiler.CodeDom/blob/master/README.md
  [license]: https://github.com/fsprojects/FSharp.Compiler.CodeDom/blob/master/LICENSE.txt
*)
