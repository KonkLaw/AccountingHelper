using System.IO;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace AccountHelperWpf.HistoryFile;

static class HistoryStorageHelper
{
    public const string DefaultExtension = "zip";

    private const string HistoryFileName = "history.json";

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
                    data = JsonSerializer.Deserialize<HistoryData>(entryStream, Options);
                    error = string.Empty;
                    return true;
                }
            }
        }
    }

    public static void Save(HistoryData associationData, string path)
    {
        using (var archiveStream = new FileStream(path, FileMode.Create))
        {
            using (ZipArchive archive = new(archiveStream, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry(HistoryFileName);
                using (Stream entryStream = entry.Open())
                {
                    JsonSerializer.Serialize(entryStream, associationData, Options);
                }
            }
        }
    }
}