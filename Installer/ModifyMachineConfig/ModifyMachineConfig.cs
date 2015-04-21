using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Microsoft.Deployment.WindowsInstaller;

namespace ModifyMachineConfig {
public class ModifyMachineConfig {
    private const string cdelt = "system.codedom";
    private static readonly string machine_config_path =
        RuntimeEnvironment.GetRuntimeDirectory() + @"CONFIG\machine.config";
    private static readonly XmlDocument machine_config = new XmlDocument();
    private static readonly XmlNode codedom_node;
    private static readonly XmlNode compilers_node;
    private const string file_extension = ".fs;.fsx;.fsscript";
    private const string language = "f#;fs;fsharp";

    static ModifyMachineConfig() {
        machine_config.Load(machine_config_path);

        var cd = machine_config.DocumentElement.SelectSingleNode(cdelt);
        codedom_node = cd ?? machine_config.CreateElement(cdelt);

        var compilers = codedom_node.SelectSingleNode("compilers");
        if (null != compilers)
            compilers_node = compilers;
        else {
            var elt = machine_config.CreateElement("compilers");
            codedom_node.AppendChild(elt);
            compilers_node = elt;
        }
    }

    private static void save_config() {
        var xws = new XmlWriterSettings() {
            Encoding = new UTF8Encoding(false),
            Indent = true
        };
        var sw = new StreamWriter(machine_config_path);
        using (var xw = XmlWriter.Create(sw, xws))
            machine_config.Save(xw);
    }

    private static void remove_entry() {
        var wanted = string.Format("compiler[@language=\"{0}\"]", language);
        var node = compilers_node.SelectSingleNode(wanted);
        if (null == node)
            return;
        compilers_node.RemoveChild(node);
        if (!compilers_node.HasChildNodes)
            codedom_node.RemoveChild(compilers_node);
        if (!compilers_node.HasChildNodes)
            machine_config.DocumentElement.RemoveChild(codedom_node);
        save_config();
    }

    private static void add_entry(string provider_name) {
        remove_entry();
        var node = machine_config.CreateElement("compiler");
        var attr = machine_config.CreateAttribute("language");
        attr.Value = language;
        attr = node.Attributes.Append(attr);
        attr = machine_config.CreateAttribute("extension");
        attr.Value = file_extension;
        attr = node.Attributes.Append(attr);
        attr = machine_config.CreateAttribute("type");
        attr.Value = provider_name;
        attr = node.Attributes.Append(attr);
        compilers_node.AppendChild(node);
        machine_config.DocumentElement.AppendChild(codedom_node);
        save_config();
    }

    [CustomAction]
    public static ActionResult AddToMachineConfig(Session session) {
        session.Log("Begin AddToMachineConfig");
        try {
            add_entry(session["ASSEMBLYNAME"]);
        } catch (Exception e) {
            session.Log("ERROR in AddToMachineConfig {0}", e.ToString());
            return ActionResult.Failure;
        }
        return ActionResult.Success;
    }

    [CustomAction]
    public static ActionResult RemoveFromMachineConfig(Session session) {
        session.Log("Begin RemoveFromMachineConfig");
        try {
            remove_entry();
        } catch (Exception e) {
            session.Log("ERROR in RemoveFromMachineConfig {0}", e.ToString());
            return ActionResult.Failure;
        }
        return ActionResult.Success;
    }
}
}
