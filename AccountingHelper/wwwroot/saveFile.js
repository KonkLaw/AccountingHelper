//async function FileSaveAs(fileName, contentStreamReference)
//{
//    const arrayBuffer = await contentStreamReference.arrayBuffer();
//    //const blob = new Blob([arrayBuffer]);
//    //const url = URL.createObjectURL(blob);
//    //const anchorElement = document.createElement('a');
//    //anchorElement.href = url;
//    //anchorElement.download = fileName ?? '';
//    //anchorElement.click();
//    //anchorElement.remove();
//    //URL.revokeObjectURL(url);
//}

async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    triggerFileDownload(fileName, url);
    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;

    if (fileName) {
        anchorElement.download = fileName;
    }

    anchorElement.click();
    anchorElement.remove();
}

//function FileSaveAs(filename, fileContent) {
//    var link = document.createElement('a');
//    link.download = filename;
//    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(fileContent)
//    document.body.appendChild(link);
//    link.click();
//    document.body.removeChild(link);
//}