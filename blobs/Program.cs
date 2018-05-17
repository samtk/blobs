using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;


namespace blobs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Azure storage excercise");
            Console.WriteLine();
            ProcessAsync().GetAwaiter().GetResult();
            Console.WriteLine("Press key to exit");
            Console.ReadLine();
        }

        private static async Task ProcessAsync()
        {
            CloudStorageAccount storageAccount = null;
            String storageconnectionstring = Environment.GetEnvironmentVariable("storageconnectionstring");
            if (CloudStorageAccount.TryParse(storageconnectionstring, out storageAccount))
            {
                Console.WriteLine("Successfullt passed storage connection string");
                String sourceFile = "";
                String destinationFile = "";
                CloudBlobContainer cloudblobcontainer = null;
                try
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    cloudblobcontainer = cloudBlobClient.GetContainerReference("testcontainer" + Guid.NewGuid().ToString());
                    await cloudblobcontainer.CreateAsync();
                    Console.WriteLine("created container '{0}'", cloudblobcontainer.Name);
                    Console.WriteLine();
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudblobcontainer.SetPermissionsAsync(permissions);

                    String localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    String localFileName = "BlobFile_" + Guid.NewGuid().ToString() + ".txt";
                    sourceFile = Path.Combine(localPath, localFileName);
                    File.WriteAllText(sourceFile, "Hello, World!");

                    Console.WriteLine("Temp file = '{0}'",sourceFile);
                    Console.WriteLine("Uploading to blob storage as blob '{0}'", localFileName);
                    Console.WriteLine();

                    CloudBlockBlob cloudBlockBlob = cloudblobcontainer.GetBlockBlobReference(localFileName);
                    await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                    Console.WriteLine("listing blobs in the container");
                    BlobContinuationToken blobContinuationToken = null;
                    do
                    {
                        var results = await cloudblobcontainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                        blobContinuationToken = results.ContinuationToken;
                        foreach (IListBlobItem item in results.Results)
                        {
                            Console.WriteLine(item.Uri);
                        }
                    } while (blobContinuationToken != null);
                    Console.WriteLine();
                    destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                    Console.WriteLine("Downloading blob to {0}", destinationFile);
                    Console.WriteLine();
                    await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);

                }
                catch (StorageException e)
                {
                    Console.WriteLine("Error: '{0}'", e.Message);
                }
                finally
                {
                    Console.WriteLine("Press key to delete sample file");
                    Console.ReadLine();
                    Console.WriteLine("deleting container blob contains");
                    if (cloudblobcontainer != null) {
                        await cloudblobcontainer.DeleteIfExistsAsync();
                    }
                    Console.WriteLine("deleting all created local files");
                    Console.WriteLine();
                    File.Delete(sourceFile);
                    File.Delete(destinationFile);
                }
            }
            else {
                Console.WriteLine("Failed to pass storage connection string");
            }
        }
    }
}
