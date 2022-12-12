using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace tomb3_ConfigTool.Utils;

public class UpdateChecker
{
    private static UpdateChecker _instance;

    public static UpdateChecker Instance
    {
        get => _instance ??= new UpdateChecker();
    }

    private const string _updateUrl = "https://api.github.com/repos/lahm86/tomb3_ConfigTool/releases/latest";
    private const string _userAgent = "tomb3_ConfigTool"; //https://docs.github.com/en/rest/overview/resources-in-the-rest-api#user-agent-required

    private readonly TimeSpan _initialDelay, _periodicDelay;
    private readonly CancellationTokenSource _cancelSource;
    private readonly CancellationToken _cancelToken;
    private readonly string _currentVersion;

    public event EventHandler UpdateAvailable;

    public string UpdateURL { get; private set; }

    private UpdateChecker()
    {
        //required for Win7 - https://stackoverflow.com/questions/2859790/the-request-was-aborted-could-not-create-ssl-tls-secure-channel
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        _initialDelay = new TimeSpan(0, 0, 5);
        _periodicDelay = new TimeSpan(0, 30, 0);

        _cancelSource = new CancellationTokenSource();
        _cancelToken = _cancelSource.Token;

        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        _currentVersion = string.Format("V{0}.{1}.{2}", version.Major, version.Minor, version.Build);

        Application.Current.Exit += Application_Exit;

        Run();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _cancelSource.Cancel();
    }

    private async void Run()
    {
        await Task.Delay(_initialDelay, _cancelToken);
        do
        {
            if (!_cancelToken.IsCancellationRequested)
            {
                try
                {
                    CheckForUpdates();
                }
                catch { }

                await Task.Delay(_periodicDelay, _cancelToken);
            }
        }
        while (!_cancelToken.IsCancellationRequested);
    }

    public bool CheckForUpdates()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_userAgent, "1.0"));
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        HttpResponseMessage response = client.Send(new(HttpMethod.Get, _updateUrl));
        response.EnsureSuccessStatusCode();

        using Stream receiveStream = response.Content.ReadAsStream();
        using StreamReader reader = new(receiveStream);

        JObject releaseInfo = JObject.Parse(reader.ReadToEnd());
        if (!releaseInfo.ContainsKey("tag_name"))
        {
            throw new IOException("Invalid response from GitHub - missing tag_name field.");
        }

        string latestVersion = "V1.2.2";// releaseInfo["tag_name"].ToString();
        if (string.Compare(latestVersion, _currentVersion, true) == 0)
        {
            return false;
        }

        UpdateURL = releaseInfo["html_url"].ToString();
        UpdateAvailable?.Invoke(this, EventArgs.Empty);

        return true;
    }
}
