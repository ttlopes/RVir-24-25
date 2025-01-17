using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class RVirEditorTools
{
    /// <summary>
    /// Menu item that opens the persistent data path in the file explorer.
    /// </summary>
    [MenuItem("Tools/Open Persistent Data Path")]
    public static void OpenPersistentDataPath()
    {
        string path = Application.persistentDataPath;

#if UNITY_EDITOR_WIN
        Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
        Process.Start("open", path);
#elif UNITY_EDITOR_LINUX
        Process.Start("xdg-open", path);
#endif
    }

    /// <summary>
    /// Menu item that runs an ADB command to enable Meta Quest MTP.
    /// </summary>
    [MenuItem("Tools/Enable Meta Quest MTP")]
    public static void RunADBCommand()
    {
        // Get the Android SDK path
        string sdkPath = GetAndroidSDKPath();

        if (string.IsNullOrEmpty(sdkPath) || !Directory.Exists(sdkPath))
        {
            UnityEngine.Debug.LogError("Android SDK is not configured in Unity. Please set the Android SDK path in the Unity Editor preferences.");
            return;
        }

        // Construct the path to the adb executable
        string adbExecutable = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";
        string adbPath = Path.Combine(sdkPath, "platform-tools", adbExecutable);

        if (!File.Exists(adbPath))
        {
            UnityEngine.Debug.LogError($"ADB executable not found at path: {adbPath}");
            return;
        }

        // Set up the process to run the ADB command
        Process process = new Process();
        process.StartInfo.FileName = adbPath;
        process.StartInfo.Arguments = "shell svc usb setFunctions mtp"; // Replace with your desired ADB command arguments
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        try
        {
            process.Start();

            // Read the output and error streams
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log("ADB Output:\n" + output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError("ADB Error:\n" + error);
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("An error occurred while running the ADB command: " + ex.Message);
        }
    }

    /// <summary>
    /// Gets the Android SDK path from Unity Editor preferences or the embedded SDK path.
    /// </summary>
    /// <returns>The Android SDK path if found; otherwise, null.</returns>
    private static string GetAndroidSDKPath()
    {
        // Try to get the SDK path from EditorPrefs
        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");

        if (!string.IsNullOrEmpty(sdkPath) && Directory.Exists(sdkPath))
        {
            return sdkPath;
        }

        // Try to get the embedded SDK path
        string unityEditorPath = EditorApplication.applicationContentsPath;
        sdkPath = Path.Combine(unityEditorPath, "PlaybackEngines", "AndroidPlayer", "SDK");

        if (Directory.Exists(sdkPath))
        {
            return sdkPath;
        }

        // The SDK path does not exist
        return null;
    }
}