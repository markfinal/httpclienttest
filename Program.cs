using System;

namespace httpclienttest
{
    static class Log
    {
        public static void Info(string message)
        {
            Console.WriteLine(message);
        }
        public static void DebugMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    class Source
    {
        private readonly string RemotePath = "https://sourceforge.net/projects/boost/files/boost/1.67.0/boost_1_67_0.tar.gz/download";
        private readonly string ArchivePath = "archive.something";
        private readonly string ExtractTo = "here";

        public System.Threading.Tasks.Task
        DownloadAndExtractPackageViaHTTP()
        {
            var client = new System.Net.Http.HttpClient
            {
                BaseAddress = new System.Uri(this.RemotePath),
                Timeout = System.TimeSpan.FromMilliseconds(-1) // infinite
            };
            client.DefaultRequestHeaders.Accept.Clear();

            var downloadTask = client.GetAsync(client.BaseAddress);
            Log.Info($"Source downloading: {this.RemotePath} to {this.ArchivePath}...");

            var savingTask = downloadTask.ContinueWith(t =>
            {
                if (!t.Result.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to download {this.RemotePath} because {t.Result.ReasonPhrase}");
                }

                /*
                var parentDir = System.IO.Path.GetDirectoryName(this.ArchivePath);
                if (!System.IO.Directory.Exists(parentDir))
                {
                    System.IO.Directory.CreateDirectory(parentDir);
                }
                */

                using (var stream = new System.IO.FileStream(
                            this.ArchivePath,
                            System.IO.FileMode.Create,
                            System.IO.FileAccess.Write,
                            System.IO.FileShare.None
                            ))
                {
                    t.Result.Content.CopyToAsync(stream).Wait(); // waiting since it's already in a task
                }
            });

            var extractingTask = savingTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    throw t.Exception;
                }
                Log.DebugMessage($"Extracting {this.ArchivePath} to {this.ExtractTo}...");
            });

            return extractingTask;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var source = new Source();
            source.DownloadAndExtractPackageViaHTTP().Wait();
        }
    }
}
