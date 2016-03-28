using System;
using Microsoft.Deployment.WindowsInstaller;

namespace FSharp.Compiler.CodeDom {
public class ModifyMachineConfig {
    [CustomAction]
    public static ActionResult AddToMachineConfig(Session session) {
        session.Log("Begin AddToMachineConfig");
        try {
            InstallLib.addEntry(session.CustomActionData["ASSEMBLYNAME"]);
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
            InstallLib.removeEntry();
        } catch (Exception e) {
            session.Log("ERROR in RemoveFromMachineConfig {0}", e.ToString());
            return ActionResult.Failure;
        }
        return ActionResult.Success;
    }
}
}
