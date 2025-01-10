using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Xml;

namespace AccountHelperWpf.HistoryFile;

static class HistoryStorageHelper
{
    public const string DefaultExtension = "zip";

    private const string HistoryFileName = "history.json";

    private static readonly Version CurrentVersion = new Version(1, 0);

    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    
    public static bool TryLoadFromFile(string path, out HistoryData? data, out string error)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            using (ZipArchive archive = new(stream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry? entry = archive.GetEntry(HistoryFileName);
                if (entry == null)
                {
                    data = null;
                    error = "Archive with data is not correct.";
                    return true;
                }

                using (Stream entryStream = entry.Open())
                {
                    using (JsonDocument document = JsonDocument.Parse(entryStream))
                    {
                        data = default;
                        if (!document.RootElement.TryGetProperty(nameof(HistoryFile.Version), out JsonElement versionElement))
                        {
                            error = "Too old version of history file";
                            return false;
                        }
                        
                        string? versionString = versionElement.GetString();
                        if (versionString == null || !Version.TryParse(versionString, out Version? version))
                        {
                            error = "Version is not detected";
                            return false;
                        }

                        if (version != CurrentVersion)
                        {
                            error = $"Version {version} is not supported";
                            return false;
                        }
                    }
                }

                using (Stream entryStream = entry.Open())
                {
                    var fileData = JsonSerializer.Deserialize<HistoryFile>(entryStream, Options);
                    data = fileData!.History;
                    error = string.Empty;
                    return true;
                }
            }
        }
    }

    public static void Save(HistoryData associationData, string path)
    {
        var fileData = new HistoryFile
        {
            Version = CurrentVersion,
            History = associationData
        };

        using (var archiveStream = new FileStream(path, FileMode.Create))
        {
            using (ZipArchive archive = new(archiveStream, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry(HistoryFileName);
                using (Stream entryStream = entry.Open())
                {
                    JsonSerializer.Serialize(entryStream, fileData, Options);
                }
            }
        }
    }


    class HistoryFile
    {
        public Version? Version { get; set; }
        public HistoryData? History { get; set; }
    }
}