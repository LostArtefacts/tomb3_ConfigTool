using Newtonsoft.Json.Linq;
using System.IO;

namespace tomb3_ConfigTool.Models;

public static class Tomb3Config
{
    private static readonly string _configFile = "tomb3_ConfigTool.json";

    public static readonly string ConfigFilterExtension = "|*.json";
    public static readonly string GitHubURL = "https://github.com/lahm86/tomb3_ConfigTool";
    public static readonly string TextureGitHubURL = "https://github.com/lahm86/TRTextureReplace";

    public static string ExecutableName { get; private set; }
    public static string SetupArgs { get; private set; }
    public static string GoldArgs { get; private set; }
    public static string TextureExecutableName { get; private set; }

    static Tomb3Config()
    {
        ExecutableName = "tomb3.exe";
        SetupArgs = "-setup";
        GoldArgs = "-gold";
        TextureExecutableName = "TRTextureReplace.exe";

        if (File.Exists(_configFile))
        {
            JObject externalConfig = JObject.Parse(File.ReadAllText(_configFile));
            if (externalConfig.ContainsKey(nameof(ExecutableName)))
            {
                ExecutableName = externalConfig[nameof(ExecutableName)].ToString();
            }
            if (externalConfig.ContainsKey(nameof(SetupArgs)))
            {
                SetupArgs = externalConfig[nameof(SetupArgs)].ToString();
            }
            if (externalConfig.ContainsKey(nameof(GoldArgs)))
            {
                GoldArgs = externalConfig[nameof(GoldArgs)].ToString();
            }
            if (externalConfig.ContainsKey(nameof(TextureExecutableName)))
            {
                TextureExecutableName = externalConfig[nameof(TextureExecutableName)].ToString();
            }
        }
    }
}
