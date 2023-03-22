using System.IO;
using System.IO.Compression;

namespace AccountHelperWpf.Helper;

static class AssociationHelper
{
    private const string AssociationsFileName = "associations";

    public static object? TryLoad(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            using (ZipArchive archive = new (stream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry? entry = archive.GetEntry(AssociationsFileName);
                if (entry == null)
                    return null;


                Stream qwe = entry.Open();
            }
        }

        return null;
    }
}