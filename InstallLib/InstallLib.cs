using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;

namespace FSharp.Compiler.CodeDom
{
public static class InstallLib
{
    private const string cdelt = "system.codedom";
    private static string machine_config_path =
      ConfigurationManager.OpenMachineConfiguration().FilePath;
    private static readonly XmlDocument machine_config = new XmlDocument();
    private static readonly XmlNode codedom_node;
    private static readonly XmlNode compilers_node;
    private const string file_extension = ".fs;.fsx;.fsscript";
    private const string language = "f#;fs;fsharp";


    static InstallLib()
    {
        machine_config.Load(machine_config_path);

        codedom_node =
            machine_config.DocumentElement.SelectSingleNode(cdelt)
         ?? (XmlNode)machine_config.CreateElement(cdelt);

        var compilers = codedom_node.SelectSingleNode("compilers");
        if (null != compilers)
            compilers_node = compilers;
        else {
            var elt = machine_config.CreateElement("compilers");
            codedom_node.AppendChild(elt);
            compilers_node = elt;
        }
    }

    private static void saveConfig()
    {
        var xws = new XmlWriterSettings() {
            Encoding = new UTF8Encoding(false),
            Indent = true
        };
        var sw = new StreamWriter(machine_config_path);
        using (var xw = XmlWriter.Create(sw, xws))
            machine_config.Save(xw);
    }

    public static void removeEntry()
    {
        var wanted = string.Format("compiler[@language=\"{0}\"]", language);
        var node = compilers_node.SelectSingleNode(wanted);
        if (null == node)
            return;
        compilers_node.RemoveChild(node);
        if (!compilers_node.HasChildNodes)
            codedom_node.RemoveChild(compilers_node);
        if (!codedom_node.HasChildNodes)
            machine_config.DocumentElement.RemoveChild(codedom_node);
        saveConfig();
    }

    public static void addEntry(string assemblyName)
    {
        removeEntry();
        var node = machine_config.CreateElement("compiler");
        var attr = machine_config.CreateAttribute("language");
        attr.Value = language;
        attr = node.Attributes.Append(attr);
        attr = machine_config.CreateAttribute("extension");
        attr.Value = file_extension;
        node.Attributes.Append(attr);
        attr = machine_config.CreateAttribute("type");
        attr.Value = assemblyName;
        attr = node.Attributes.Append(attr);
        compilers_node.AppendChild(node);
        machine_config.DocumentElement.AppendChild(codedom_node);
        saveConfig();
    }
}
}
