namespace FSharp.Compiler.CodeDom

// run InstallUtil.exe on this dll to register the provider in the GAC
//
// note: the provider build process does not sign the dll; you will need to
// make a key with "sn -k" and add --keyfile:path_to_key to the OtherFlags
// property in FSharp.Compiler.CodeDom.fsproj before building

open System.ComponentModel
open System.Configuration
open System.IO
open System.Runtime.InteropServices
open System.Text
open System.Xml

[<RunInstaller(true)>]
type SimpleInstaller() =
    inherit Install.Installer()

    static let machine_config_path =
        let framework_path = RuntimeEnvironment.GetRuntimeDirectory()
        framework_path + @"CONFIG\machine.config"
    static let machine_config = new XmlDocument()
    static do
        machine_config.Load(machine_config_path)
    static let codedom_node =
        let cd =
          machine_config.DocumentElement.SelectSingleNode("system.codedom")
        if cd <> null then cd else
        machine_config.CreateElement("system.codedom") :> XmlNode
    static let compilers_node =
        let compilers = codedom_node.SelectSingleNode("compilers")
        if compilers <> null then compilers else
        let elt = machine_config.CreateElement("compilers")
        ignore <| codedom_node.AppendChild(elt)
        elt :> XmlNode

    static let file_extension = ".fs"
    static let language = "f#;fs;fsharp"

    static let save_config () =
        let xws = new XmlWriterSettings(Encoding=UTF8Encoding(false),
                                        Indent=true)
        use xw = XmlWriter.Create(new StreamWriter(machine_config_path), xws)
        machine_config.Save(xw)

    static let remove_entry () =
        let wanted = sprintf "compiler[@language=\"%s\"]" language
        let node = compilers_node.SelectSingleNode(wanted)
        if node = null then () else
        ignore <| compilers_node.RemoveChild(node)
        if not compilers_node.HasChildNodes then
          ignore <| codedom_node.RemoveChild(compilers_node)
        if not codedom_node.HasChildNodes then
          ignore <| machine_config.DocumentElement.RemoveChild(codedom_node)
        save_config ()

    static let add_entry () =
        remove_entry ()
        let node = machine_config.CreateElement("compiler")
        let attr = machine_config.CreateAttribute("language")
        attr.Value <- language
        let attr = node.Attributes.Append(attr)
        let attr = machine_config.CreateAttribute("extension")
        attr.Value <- file_extension
        let attr = node.Attributes.Append(attr)
        let attr = machine_config.CreateAttribute("type")
        attr.Value <- typeof<FSharpCodeProvider>.AssemblyQualifiedName
        let attr = node.Attributes.Append(attr)
        ignore <| compilers_node.AppendChild(node)
        ignore <| machine_config.DocumentElement.AppendChild(codedom_node)
        save_config ()

    override this.Install(savedState) =
        base.Install(savedState)
        add_entry ()

    override this.Uninstall(savedState) =
        base.Uninstall(savedState)
        remove_entry ()

    override this.Commit(savedState) =
        base.Commit(savedState)

    override this.Rollback(savedState) =
        base.Rollback(savedState)
