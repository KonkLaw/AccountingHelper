using Microsoft.JSInterop;
using System.IO;
using System.Runtime.ExceptionServices;

namespace AccountingHelper.Logic;

static class SaveFileHelperClass
{
    private const string SaveFileName = "FileSaveAs";

    public async static void Save(IJSRuntime jSRuntime, string defaultFileName, string content)
    {
        try
        {
            var randomBinaryData = new byte[50 * 1024];
            var fileStream = new MemoryStream(randomBinaryData);
            using var streamRef = new DotNetStreamReference(fileStream);
            await jSRuntime.InvokeVoidAsync("downloadFileFromStream", "testStr", streamRef);
            //await jSRuntime.InvokeVoidAsync<object>("downloadFileFromStream", "testStr", streamRef);

        }
        catch (Exception ex)
        {
            throw ex;
        }


        await jSRuntime.InvokeAsync<object>(SaveFileName, defaultFileName, content);
    }
}
