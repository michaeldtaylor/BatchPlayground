using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SimpleTaskRunner
{
    static class Program
    {
        static void Main(string[] args)
        {
            var blobName = args[0];
            var blobUri = new Uri(blobName);
            var numTopN = int.Parse(args[1]);

            var blob = new CloudBlockBlob(blobUri);
            var content = blob.DownloadText();
            var words = content.Split(' ');
            var topNWords = words.Where(word => word.Length > 0)
                .GroupBy(word => word, (key, group) => new KeyValuePair<string, long>(key, group.LongCount()))
                .OrderByDescending(x => x.Value)
                .Take(numTopN)
                .ToList();

            foreach (var pair in topNWords)
            {
                Console.WriteLine("{0} {1}", pair.Key, pair.Value);
            }
        }
    }
}
