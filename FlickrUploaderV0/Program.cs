using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlickrNet;
using System.IO;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace FlickrUploaderV0
{
    class Program
    {
        /// <summary>
        /// This Main uses Tasks to create multiple uploaders in 
        /// parallel using Tasks
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // STEP 1 - Get Files
            Console.WriteLine("Enter directory of photographs that need to be uploaded");
            string directory = Console.ReadLine();
            Contract.Assert(Directory.Exists(directory));

            List<string> filesToUpload = GetPictures(directory);

            Trace.WriteLine(string.Format(" Complete list of files being uploaded =========================="));

            foreach (string file in filesToUpload)
            {
                Console.WriteLine("File Name = {0}", file);
                Trace.WriteLine(string.Format("File Name = {0}", file));
            }

            Trace.WriteLine(string.Format(" End of List of files being uploaded =========================="));

            // STEP 2 - Authenticate with twitter
            Authenticate();

            // STEP3 - Upload
            //filesToUpload.Batch<string>(5, UploadPicturesInBatchesInParallel);
            filesToUpload.Batch<string>(5, UploadPicturesInBatchesSequentially);
        }

        /// <summary>
        /// Uploads batches of files sequentially
        /// </summary>
        /// <param name="files"></param>
        public static void UploadPicturesInBatchesSequentially(IEnumerable<string> files)
        {
            string title = "";
            string description = "";
            bool isPublic = false;
            bool isFamily = true;
            bool isFriend = false;
            string tags = "uploaded by my own app V2";

            Stopwatch batchProcessingWatch = Stopwatch.StartNew();

            foreach (string file in files)
            {
                UploadPicture(file, title, description, tags, isPublic, isFamily, isFriend);
            }

            batchProcessingWatch.Stop();

            string message = String.Format("Batch Processing for batch was completed in {0} ms", batchProcessingWatch.ElapsedMilliseconds);
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Uploads batches of files in parallel
        /// </summary>
        /// <param name="files"></param>
        public static void UploadPicturesInBatchesInParallel(IEnumerable<string> files)
        {
            string title = "";
            string description = "";
            bool isPublic = false;
            bool isFamily = true;
            bool isFriend = false;
            string tags = "uploaded by my own app V2";

            List<Task> tasks = new List<Task>();
            TaskFactory taskFactory = new TaskFactory();
            Stopwatch batchProcessingWatch = Stopwatch.StartNew();

            try
            {
                foreach (string file in files)
                {
                    tasks.Add(taskFactory.StartNew(() => UploadPicture(file, title, description, tags, isPublic, isFamily, isFriend)));
                }

                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Exception while uploading pictures with message {0}", ex.Message);
            }

            batchProcessingWatch.Stop();

            string message = String.Format("Batch Processing for batch was completed in {0} ms", batchProcessingWatch.ElapsedMilliseconds);
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }


        /// <summary>
        /// Authenticate with twitter to get a token
        /// </summary>
        public static void Authenticate()
        {
            // STEP 1 of authentication
            Flickr f = FlickrManager.GetInstance();
            OAuthRequestToken requestToken = f.OAuthGetRequestToken("oob");
            string url = f.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);
            System.Diagnostics.Process.Start(url);

            // STEP 2 of authentication
            Console.WriteLine("Enter the verifier code");
            string verifierCode = Console.ReadLine();

            Flickr f1 = FlickrManager.GetInstance();

            try
            {
                var accessToken = f1.OAuthGetAccessToken(requestToken, verifierCode);
                FlickrManager.OAuthToken = accessToken;
            }
            catch (FlickrApiException ex)
            {
                throw;
            }

            Trace.WriteLine("Authentication with twitter is complete");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static List<string> GetPictures(string directoryPath)
        {
            Contract.Assert(Directory.Exists(directoryPath));

            DirectoryInfo picturesDirectory = new DirectoryInfo(directoryPath);

            return picturesDirectory.GetFiles("*", SearchOption.AllDirectories).OrderBy(x => x.CreationTimeUtc).Select(x => x.FullName).ToList();
        }

        /// <summary>
        /// Upload picture
        /// </summary>
        public static string UploadPicture(string fileToUpload, string title, string description, string tags, bool isPublic, bool isFamily, bool isFriend)
        {
            Contract.Assert(File.Exists(fileToUpload));

            Flickr f = FlickrManager.GetAuthInstance();

            return f.UploadPicture(fileToUpload, title, description, tags, isPublic, isFamily, isFriend);
        }

    }

    
}
