using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class IOSBuildPostProcessor {

    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string path) {

        if (buildTarget == BuildTarget.iOS) {

            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            Debug.Log(">> Automation, plist ... <<");

            // example of changing a value:
            // rootDict.SetString("CFBundleVersion", "6.6.6");

            // example of adding a boolean key...
            // < key > ITSAppUsesNonExemptEncryption </ key > < false />
            rootDict.SetString("NSBluetoothPeripheralUsageDescription", "This app may use bluetooth to detect nearby beacons.");
            rootDict.SetString("NSBluetoothAlwaysUsageDescription", "This app may be awoken by nearby beacons to alert you of items of interest.");

            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
