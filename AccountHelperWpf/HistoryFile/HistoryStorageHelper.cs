using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace AccountHelperWpf.HistoryFile;

static class HistoryStorageHelper
{
    public const string DefaultExtension = "zip";

    private const string HistoryFileName = "history.json";

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
                    data = JsonSerializer.Deserialize<HistoryData>(entryStream);
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
                    JsonSerializerOptions options = new() { WriteIndented = true };
                    JsonSerializer.Serialize(entryStream, associationData, options);
                }
            }
        }
    }
}