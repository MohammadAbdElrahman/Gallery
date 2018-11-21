using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Auth;


public class HomeController : Controller
{
    static CloudBlobClient blobClient;
    const string blobContainerName = "images";
    static CloudBlobContainer blobContainer;

 
    public async Task<ActionResult> Index()
    {
        try
        {
            StorageCredentials Credentials=new StorageCredentials("facegraphdevtest2", "g8pSeynFOPISrCujAz5yWMPTMfd3umo3u6eGgGhtrQpd3ij2v/nv7vYyscb4pdNz8sgq2ToXjt42hdTOQPS4vA==");

            CloudStorageAccount storageAccount =new  CloudStorageAccount(Credentials,true);

            // Create a blob 
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(blobContainerName);
            await blobContainer.CreateIfNotExistsAsync();

          
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Gets all Blobs 
            List<Uri> allBlobs = new List<Uri>();
            foreach (IListBlobItem blob in blobContainer.ListBlobs())
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                    allBlobs.Add(blob.Uri);
            }

            return View(allBlobs);
        }
        catch (Exception ex)
        {
            ViewData["message"] = ex.Message;
            ViewData["trace"] = ex.StackTrace;
            return View("Error");
        }
    }

  
    [HttpPost]
    public async Task<ActionResult> UploadAsync()
    {
        try
        {
            HttpFileCollectionBase files = Request.Files;
            int fileCount = files.Count;

            if (fileCount > 0)
            {
                for (int i = 0; i < fileCount; i++)
                {
                    CloudBlockBlob blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(files[i].FileName));
                    await blob.UploadFromStreamAsync(files[i].InputStream);

                }
            }
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewData["message"] = ex.Message;
            ViewData["trace"] = ex.StackTrace;
            return View("Error");
        }
    }

    //[Authorize]
    [HttpPost]
    public async Task<ActionResult> DeleteImage(string name)
    {
        try
        {
            Uri uri = new Uri(name);
            string filename = Path.GetFileName(uri.LocalPath);

            var blob = blobContainer.GetBlockBlobReference(filename);
            await blob.DeleteIfExistsAsync();

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewData["message"] = ex.Message;
            ViewData["trace"] = ex.StackTrace;
            return View("Error");
        }
    }

    //[Authorize]
    [HttpPost]
    public async Task<ActionResult> DeleteAll()
    {
        try
        {
            foreach (var blob in blobContainer.ListBlobs())
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                {
                    await ((CloudBlockBlob)blob).DeleteIfExistsAsync();
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewData["message"] = ex.Message;
            ViewData["trace"] = ex.StackTrace;
            return View("Error");
        }
    }
 
    private string GetRandomBlobName(string filename)
    {
        string ext = Path.GetExtension(filename);
        return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
    }
}