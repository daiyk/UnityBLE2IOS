using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class UnityBLE2IOSBuildProcessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            ProcessiOSBuild(pathToBuiltProject);
        }
    }

    private static void ProcessiOSBuild(string pathToBuiltProject)
    {
        // Get plist path
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;

        // Add Bluetooth usage descriptions
        rootDict.SetString("NSBluetoothAlwaysUsageDescription", 
            "This app needs Bluetooth access to discover and connect to nearby devices.");
        rootDict.SetString("NSBluetoothPeripheralUsageDescription", 
            "This app needs Bluetooth access to communicate with peripheral devices.");

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());

        // Update Xcode project
        string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        // Get target GUID
        string targetGuid = proj.GetUnityMainTargetGuid();

        // Add CoreBluetooth framework
        proj.AddFrameworkToProject(targetGuid, "CoreBluetooth.framework", false);

        // Set deployment target to iOS 10.0 minimum (required for CoreBluetooth)
        proj.SetBuildProperty(targetGuid, "IPHONEOS_DEPLOYMENT_TARGET", "10.0");

        // Write the project
        File.WriteAllText(projPath, proj.WriteToString());

        UnityEngine.Debug.Log("UnityBLE2IOS: iOS build processed successfully!");
    }
}
