#nullable enable
using System.Net;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    public static class HttpClientDownloadExtensions
    {
        public static async Task<string> DownloadWithCache(this HttpClient client, string url, string cacheFolder, CancellationToken cancellationToken = default)
        {
            _ = url ?? throw new ArgumentNullException(nameof(url));
            _ = cacheFolder ?? throw new ArgumentNullException(nameof(cacheFolder));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Invalid URL.", nameof(url));
            }

            var domainFolder = Path.Combine(cacheFolder, uri.Host);
            if (!Directory.Exists(domainFolder))
            {
                Directory.CreateDirectory(domainFolder);
            }

            var cacheFile = Path.Combine(domainFolder, BuildCacheFileName(uri));
            var etagFile = cacheFile + ".etag";

            if (File.Exists(cacheFile))
            {
                await RefreshCacheIfModified(client, url, cacheFile, etagFile, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await DownloadToCache(client, url, cacheFile, etagFile, cancellationToken).ConfigureAwait(false);
            }

            return cacheFile;
        }

        public static async Task<Stream> DownloadStreamWithCache(this HttpClient client, string url, string cacheFolder, CancellationToken cancellationToken = default)
        {
            var file = await client.DownloadWithCache(url, cacheFolder, cancellationToken).ConfigureAwait(false);
            return File.OpenRead(file);
        }

        private static async Task RefreshCacheIfModified(HttpClient client, string url, string cacheFile, string etagFile, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var lastModified = File.GetLastWriteTimeUtc(cacheFile);
            request.Headers.IfModifiedSince = lastModified;

            var etag = ReadEtag(etagFile);
            if (etag != null)
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", etag);
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                await WriteResponseToFile(response, cacheFile, cancellationToken).ConfigureAwait(false);
                SaveEtag(etagFile, response.Headers.ETag);
            }
        }

        private static async Task DownloadToCache(HttpClient client, string url, string cacheFile, string etagFile, CancellationToken cancellationToken)
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                await WriteResponseToFile(response, cacheFile, cancellationToken).ConfigureAwait(false);
                SaveEtag(etagFile, response.Headers.ETag);
            }
        }

        private static async Task WriteResponseToFile(HttpResponseMessage response, string filePath, CancellationToken cancellationToken)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }

        private static string? ReadEtag(string etagFile)
        {
            return File.Exists(etagFile) ? File.ReadAllText(etagFile).Trim() : null;
        }

        private static void SaveEtag(string etagFile, EntityTagHeaderValue? etag)
        {
            if (etag != null)
            {
                File.WriteAllText(etagFile, etag.ToString());
            }
        }

        private static string BuildCacheFileName(Uri uri)
        {
            var raw = uri.AbsolutePath.TrimStart('/') + uri.Query;
            if (string.IsNullOrEmpty(raw))
            {
                raw = "index";
            }
            var targetSpan = new char[raw.Length];
            raw.AsSpan().CopyTo(targetSpan);
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < targetSpan.Length; i++)
            {
                if (invalidFileNameChars.Contains(targetSpan[i]) || targetSpan[i] == '/')
                {
                    targetSpan[i] = '_';
                }
            }
            return new string(targetSpan.ToArray());
        }
    }
}
