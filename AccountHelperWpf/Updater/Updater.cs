using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using AccountHelperWpf.Views;
using Newtonsoft.Json;

namespace AccountHelperWpf.Updater;

class UpdateController
{
    public static void CheckUpdates()
    {
        NewVersionInfo? newVersionOrNot = GetNewVersionUrl();
        if (newVersionOrNot == null)
			return;

        NewVersionInfo newVersion = newVersionOrNot.Value;

        MessageBoxResult messageBoxResult = MessageBox.Show(
            $"New version ({newVersion.Version}) is available. Update app?", string.Empty,
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (messageBoxResult != MessageBoxResult.Yes)
            return;

        RunUpdate(newVersion);
    }

    private static void RunUpdate(NewVersionInfo newVersionInfo)
    {
        string tempFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccountHelper");

        string archiveLocalPath = Path.Combine(tempFolder, "AccountHelper.zip");
        if (Directory.Exists(tempFolder))
            Directory.Delete(tempFolder, true);
        Directory.CreateDirectory(tempFolder);

        Task DownloadAndSave() => Download(newVersionInfo.DownloadUrl, archiveLocalPath);

        Task ExtractArchive() => Task.Run(() =>
        {
            ZipFile.ExtractToDirectory(archiveLocalPath, tempFolder);
            File.Delete(archiveLocalPath);
        });

        new UpdateInfoWindow(DownloadAndSave, ExtractArchive).ShowDialog();

        RunFilesSwapScrip(tempFolder);
    }

    private static void RunFilesSwapScrip(string tempFolder)
    {
        string appFolder = AppDomain.CurrentDomain.BaseDirectory;

        string updateCmd =
            $@"/c timeout 1 /nobreak>nul & rmdir /S /Q ""{appFolder}"" & mkdir ""{appFolder}"" & xcopy /s ""{tempFolder}"" ""{appFolder}"" & rmdir /S /Q ""{tempFolder}"" & ""{appFolder}AccountHelperWpf.exe""";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = updateCmd
        };
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        Environment.Exit(0);
    }

    private static async Task Download(string downloadUrl, string savePath)
    {
        using (HttpClient client = new HttpClient())
        {
            using (HttpResponseMessage httpResponse = await client.GetAsync(downloadUrl))
            {
                await using (Stream contentStream = await httpResponse.Content.ReadAsStreamAsync())
                {
                    await using (FileStream fileStream = File.Create(savePath))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }
            }
        }
    }

    private static NewVersionInfo? GetNewVersionUrl()
	{
		const string url = "https://api.github.com/repos/KonkLaw/AccountingHelper/contents/Build?ref=main";

		using (HttpClient client = new HttpClient())
		{
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AccountHelperApp", "1.0"));
			string response = client.GetStringAsync(url).Result;
			List<FileItem> fileItems = JsonConvert.DeserializeObject<List<FileItem>>(response)!;

			const string fileNamePrefix = "AccountHelper v";
			List<FileItem> updateArchives = fileItems.Where(item => item.name.StartsWith(fileNamePrefix)).ToList();
			if (updateArchives.Count != 1)
                return null;

            Version? currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
			if (currentVersion == null)
				return null;

            FileItem archiveItem = updateArchives[0];
            string fileName = Path.GetFileNameWithoutExtension(archiveItem.name);
            string versionString = fileName.Substring(fileNamePrefix.Length);

            if (Version.TryParse(versionString, out Version? newVersion) && currentVersion < newVersion)
            {
                return new NewVersionInfo(archiveItem.download_url, newVersion);
            }
            return null;
        }
	}

    readonly record struct NewVersionInfo(string DownloadUrl, Version Version);

	class FileItem
	{
		public string name { get; set; }
		//public string path { get; set; }
		//public string sha { get; set; }
		//public long size { get; set; }
		//public string url { get; set; }
		//public string html_url { get; set; }
		//public string git_url { get; set; }
		public string download_url { get; set; }
		//public string type { get; set; }
		//public Links _links { get; set; }
	}

	//class Links
	//{
	//	public string self { get; set; }
	//	public string git { get; set; }
	//	public string html { get; set; }
	//}
}