#nullable enable
using System.Net;
using System.Net.Http.Headers;

namespace System.Net.Http;

public class HttpClientDownloadExtensionsTest : IDisposable
{
    private readonly string _testFolder;

    public HttpClientDownloadExtensionsTest()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), "http_cache_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }
    }

    #region DownloadWithCache - 首次下载

    [Fact]
    public async Task DownloadWithCache_FirstTime_DownloadsFile()
    {
        // Arrange
        var content = "hello world";
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache1");

        // Act
        var result = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        File.Exists(result).Should().BeTrue();
        File.ReadAllText(result).Should().Be(content);
    }

    [Fact]
    public async Task DownloadWithCache_FirstTime_SavesEtag()
    {
        // Arrange
        var handler = new MockHandler(req =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("data")
            };
            resp.Headers.ETag = new EntityTagHeaderValue("\"abc123\"");
            return resp;
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_etag1");

        // Act
        var result = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        var etagFile = result + ".etag";
        File.Exists(etagFile).Should().BeTrue();
        File.ReadAllText(etagFile).Should().Contain("abc123");
    }

    [Fact]
    public async Task DownloadWithCache_FirstTime_NoEtag_DoesNotCreateEtagFile()
    {
        // Arrange
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("data")
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_noetag");

        // Act
        var result = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        File.Exists(result + ".etag").Should().BeFalse();
    }

    #endregion

    #region DownloadWithCache - 缓存命中 (304)

    [Fact]
    public async Task DownloadWithCache_CacheExists_ServerReturns304_KeepsCachedFile()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_304");
        Directory.CreateDirectory(cacheFolder);

        var originalContent = "original content";
        var cacheFile = Path.Combine(cacheFolder, BuildCachePath("http://localhost/file.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        File.WriteAllText(cacheFile, originalContent);

        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.NotModified));
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        var result = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        File.ReadAllText(result).Should().Be(originalContent);
    }

    #endregion

    #region DownloadWithCache - 缓存过期更新

    [Fact]
    public async Task DownloadWithCache_CacheExists_ServerReturnsNewContent_UpdatesFile()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_update");
        Directory.CreateDirectory(cacheFolder);

        var cacheFile = Path.Combine(cacheFolder, BuildCachePath("http://localhost/file.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        File.WriteAllText(cacheFile, "old content");

        var newContent = "new content";
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(newContent)
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        var result = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        File.ReadAllText(result).Should().Be(newContent);
    }

    [Fact]
    public async Task DownloadWithCache_CacheExists_ServerReturnsNewContent_UpdatesEtag()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_update_etag");
        Directory.CreateDirectory(cacheFolder);

        var cacheFile = Path.Combine(cacheFolder, BuildCachePath("http://localhost/file.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        File.WriteAllText(cacheFile, "old");
        File.WriteAllText(cacheFile + ".etag", "\"old-etag\"");

        var handler = new MockHandler(req =>
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("new")
            };
            resp.Headers.ETag = new EntityTagHeaderValue("\"new-etag\"");
            return resp;
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        File.ReadAllText(cacheFile + ".etag").Should().Contain("new-etag");
    }

    #endregion

    #region DownloadWithCache - 条件请求头

    [Fact]
    public async Task DownloadWithCache_CacheExists_SendsIfModifiedSinceHeader()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_ifmod");
        Directory.CreateDirectory(cacheFolder);

        var cacheFile = Path.Combine(cacheFolder, BuildCachePath("http://localhost/file.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        File.WriteAllText(cacheFile, "cached");

        DateTimeOffset? capturedIfModifiedSince = null;
        var handler = new MockHandler(req =>
        {
            capturedIfModifiedSince = req.Headers.IfModifiedSince;
            return new HttpResponseMessage(HttpStatusCode.NotModified);
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        capturedIfModifiedSince.Should().NotBeNull();
    }

    [Fact]
    public async Task DownloadWithCache_EtagExists_SendsIfNoneMatchHeader()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_ifnonematch");
        Directory.CreateDirectory(cacheFolder);

        var cacheFile = Path.Combine(cacheFolder, BuildCachePath("http://localhost/file.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        File.WriteAllText(cacheFile, "cached");
        File.WriteAllText(cacheFile + ".etag", "\"my-etag\"");

        string? capturedIfNoneMatch = null;
        var handler = new MockHandler(req =>
        {
            capturedIfNoneMatch = req.Headers.TryGetValues("If-None-Match", out var values)
                ? string.Join(",", values) : null;
            return new HttpResponseMessage(HttpStatusCode.NotModified);
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        capturedIfNoneMatch.Should().NotBeNull();
        capturedIfNoneMatch.Should().Contain("my-etag");
    }

    [Fact]
    public async Task DownloadWithCache_FirstDownload_DoesNotSendConditionalHeaders()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_noconditional");
        string? capturedIfNoneMatch = null;
        DateTimeOffset? capturedIfModifiedSince = null;

        var handler = new MockHandler(req =>
        {
            capturedIfNoneMatch = req.Headers.TryGetValues("If-None-Match", out var values)
                ? string.Join(",", values) : null;
            capturedIfModifiedSince = req.Headers.IfModifiedSince;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("data")
            };
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        capturedIfNoneMatch.Should().BeNull();
        capturedIfModifiedSince.Should().BeNull();
    }

    #endregion

    #region DownloadWithCache - 异常处理

    [Fact]
    public async Task DownloadWithCache_NullUrl_ThrowsArgumentNullException()
    {
        using var client = new HttpClient();

        var act = () => client.DownloadWithCache(null!, _testFolder);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DownloadWithCache_NullCacheFolder_ThrowsArgumentNullException()
    {
        using var client = new HttpClient();

        var act = () => client.DownloadWithCache("http://localhost/file.txt", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DownloadWithCache_ServerError_DoesNotCreateFile()
    {
        // Arrange
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_error");

        // Act
        var result = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        File.Exists(result).Should().BeFalse();
    }

    [Fact]
    public async Task DownloadWithCache_CancellationRequested_Throws()
    {
        // Arrange
        var handler = new MockHandler(req =>
        {
            req.RequestUri!.ToString(); // prevent unused warning
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("data")
            };
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_cancel");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var act = () => client.DownloadWithCache("http://localhost/file.txt", cacheFolder, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region DownloadWithCache - 缓存目录自动创建

    [Fact]
    public async Task DownloadWithCache_CacheFolderNotExists_CreatesIt()
    {
        // Arrange
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("data")
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "auto_create_dir");

        // Act
        await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        Directory.Exists(cacheFolder).Should().BeTrue();
    }

    #endregion

    #region DownloadWithCache - 域名目录结构

    [Fact]
    public async Task DownloadWithCache_CreatesFileUnderDomainSubFolder()
    {
        // Arrange
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("data")
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_domain");

        // Act
        var result = await client.DownloadWithCache("http://localhost/abc/file.txt", cacheFolder);

        // Assert
        var domainFolder = Path.Combine(cacheFolder, "localhost");
        Directory.Exists(domainFolder).Should().BeTrue();
        result.Should().StartWith(domainFolder);
        Path.GetFileName(result).Should().Be("abc_file.txt");
    }

    [Fact]
    public async Task DownloadWithCache_UrlWithQuery_IncludesQueryInFileName()
    {
        // Arrange
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("data")
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_query");

        // Act
        var result = await client.DownloadWithCache("http://localhost/abc/img.jpg?v=2", cacheFolder);

        // Assert
        var fileName = Path.GetFileName(result);
        fileName.Should().Contain("img");
        fileName.Should().Contain("v=2");
    }

    #endregion

    #region DownloadWithCache - 文件名生成

    [Fact]
    public async Task DownloadWithCache_UrlWithSpecialChars_ReplacesInvalidFileNameChars()
    {
        // Arrange
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("data")
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_special");

        // Act
        var result = await client.DownloadWithCache("http://localhost/path/to/file?q=1", cacheFolder);

        // Assert
        var fileName = Path.GetFileName(result);
        Path.GetInvalidFileNameChars().Any(c => fileName.Contains(c)).Should().BeFalse();
    }

    #endregion

    #region DownloadStreamWithCache

    [Fact]
    public async Task DownloadStreamWithCache_ReturnsReadableStreamWithContent()
    {
        // Arrange
        var content = "stream content";
        var handler = new MockHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var cacheFolder = Path.Combine(_testFolder, "cache_stream");

        // Act
        using var stream = await client.DownloadStreamWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        text.Should().Be(content);
    }

    [Fact]
    public async Task DownloadStreamWithCache_UsesCachedFile()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_stream_cached");
        Directory.CreateDirectory(cacheFolder);

        var cachedContent = "cached stream data";
        var cacheFile = Path.Combine(cacheFolder, BuildCachePath("http://localhost/file.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        File.WriteAllText(cacheFile, cachedContent);

        int requestCount = 0;
        var handler = new MockHandler(req =>
        {
            requestCount++;
            return new HttpResponseMessage(HttpStatusCode.NotModified);
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        using var stream = await client.DownloadStreamWithCache("http://localhost/file.txt", cacheFolder);

        // Assert
        requestCount.Should().Be(1);
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        text.Should().Be(cachedContent);
    }

    #endregion

    #region 多次下载完整流程

    [Fact]
    public async Task DownloadWithCache_FullFlow_DownloadThen304ThenUpdate()
    {
        // Arrange
        var cacheFolder = Path.Combine(_testFolder, "cache_fullflow");
        var callIndex = 0;

        var handler = new MockHandler(req =>
        {
            callIndex++;
            return callIndex switch
            {
                1 => new HttpResponseMessage(HttpStatusCode.OK) // 首次下载
                {
                    Content = new StringContent("v1"),
                    Headers = { ETag = new EntityTagHeaderValue("\"v1\"") }
                },
                2 => new HttpResponseMessage(HttpStatusCode.NotModified), // 304 缓存命中
                3 => new HttpResponseMessage(HttpStatusCode.OK) // 内容更新
                {
                    Content = new StringContent("v2"),
                    Headers = { ETag = new EntityTagHeaderValue("\"v2\"") }
                },
                _ => new HttpResponseMessage(HttpStatusCode.NotModified)
            };
        });
        using var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        // Act 1: 首次下载
        var file1 = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);
        var content1 = File.ReadAllText(file1);
        var etag1 = File.ReadAllText(file1 + ".etag");

        // Act 2: 缓存命中 (304)
        var file2 = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);
        var content2 = File.ReadAllText(file2);

        // Act 3: 服务端更新
        var file3 = await client.DownloadWithCache("http://localhost/file.txt", cacheFolder);
        var content3 = File.ReadAllText(file3);
        var etag3 = File.ReadAllText(file3 + ".etag");

        // Assert
        content1.Should().Be("v1");
        etag1.Should().Contain("v1");

        content2.Should().Be("v1"); // 304 未更新
        file2.Should().Be(file1);   // 同一个缓存文件

        content3.Should().Be("v2"); // 已更新
        etag3.Should().Contain("v2");
    }

    #endregion

    #region Helpers

    private static string BuildCachePath(string url)
    {
        var uri = new Uri(url);
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
        return Path.Combine(uri.Host, new string(targetSpan.ToArray()));
    }

    private class MockHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_handler(request));
        }
    }

    #endregion
}
