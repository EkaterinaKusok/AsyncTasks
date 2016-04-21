using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task
{
    public static class Tasks
    {
        /// <summary>
        /// Returns the content of required uri's.
        /// Method has to use the synchronous way and can be used to compare the
        ///  performace of sync/async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris)
        {
            if (uris == null)
                throw new NullReferenceException();
            List<string> result = new List<string>();
            foreach (var resource in uris)
            {
                var webRequest = WebRequest.Create(resource);
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    result.Add(reader.ReadToEnd());
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace 
        /// of sync \ async approaches. 
        /// maxConcurrentStreams parameter should control the maximum of concurrent streams 
        /// that are running at the same time (throttling). 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        /// 
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            if (uris == null)
                throw new NullReferenceException();
            IEnumerable<string> result = uris
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(maxConcurrentStreams)
                .Select(uri => (HttpWebRequest) WebRequest.Create(uri))
                .Select(webRequest =>
                {
                    HttpWebResponse response;
                    try
                    {
                        response = (HttpWebResponse) webRequest.GetResponse();
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException("webResponseExeption", e);
                    }
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new ApplicationException("responseStream not available");
                    }
                    using (var reader = new StreamReader(responseStream))
                    {
                        return reader.ReadToEnd();
                    }
                });
            return result;
        }


        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static async Task<string> GetMD5Async(this Uri resource)
        {
            if (resource == null)
                throw new NullReferenceException();
            var t = await Task<string>.Run(() =>
            {
                var webRequest = WebRequest.Create(resource);
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    MD5 md5Hasher = MD5.Create();
                    byte[] data = md5Hasher.ComputeHash(content);
                    StringBuilder sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                        sBuilder.Append(data[i].ToString("x2"));
                    return sBuilder.ToString();
                }
            });
            return t;
        }
    }
}
