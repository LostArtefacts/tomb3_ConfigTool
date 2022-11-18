using System.Diagnostics;

namespace tomb3_ConfigTool.Utils;

public static class ProcessUtils
{
    public static void Start(string fileName, string arguments = null)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true
        });
    }
}
