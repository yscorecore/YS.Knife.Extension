using LibreOfficeKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using LibreOfficeConverter = LibreOfficeKit.Converter;

namespace YS.Knife.Document.Converter.Xlsx2Pdf
{
    /// <summary>
    /// Owns the LibreOfficeKit <see cref="LibreOfficeConverter"/> worker pool and performs the actual conversion.
    /// <para>
    /// LibreOfficeKit is not thread-safe - only one LOK instance may exist per process - so a single
    /// pool is created lazily and reused for the lifetime of the engine. Creating the pool spawns
    /// worker processes, so it is deliberately deferred until the first real conversion.
    /// </para>
    /// </summary>
    [Service(typeof(LibreOfficeEngine), ServiceLifetime.Singleton)]
    public partial class LibreOfficeEngine : IDisposable
    {
        private readonly object _syncRoot = new();
        private readonly ILogger _logger;
        private readonly LibreOfficeEngineSettings _settings;
        private LibreOfficeConverter? _converter;
        private bool _disposed;

        public LibreOfficeEngine(ILogger<LibreOfficeEngine>? logger = null)
            : this(LibreOfficeEngineSettings.FromEnvironment(), logger)
        {
        }

        internal LibreOfficeEngine(LibreOfficeEngineSettings settings, ILogger? logger = null)
        {
            _settings = settings;
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// True when a LibreOffice installation can be located. Callers can use this to fail fast
        /// with an actionable message instead of surfacing a native loader error.
        /// </summary>
        public static bool IsLibreOfficeInstalled()
        {
            return TryResolveInstallPath() is not null;
        }

        /// <summary>
        /// Returns the configured (or auto-detected) LibreOffice program directory, or null when none was found.
        /// A directory only counts when it actually contains the LibreOfficeKit native entry point, so a
        /// half-removed installation directory is not mistaken for a working one.
        /// </summary>
        public static string? TryResolveInstallPath()
        {
            // 1. Explicit override via the project-specific variable.
            var configured = Environment.GetEnvironmentVariable(LibreOfficeEngineSettings.InstallPathVariable);
            if (IsValidInstallPath(configured))
            {
                return configured;
            }

            // 2. LibreOfficeKit itself honours LOK_PROGRAM_PATH.
            var lokPath = Environment.GetEnvironmentVariable("LOK_PROGRAM_PATH");
            if (IsValidInstallPath(lokPath))
            {
                return lokPath;
            }

            // 3. Known install locations, most common first.
            var candidates = new[]
            {
                @"C:\Program Files\LibreOffice\program",
                @"C:\Program Files (x86)\LibreOffice\program",
                @"/usr/lib/libreoffice/program",
                @"/usr/lib64/libreoffice/program",
                @"/opt/libreoffice/program",
                @"/Applications/LibreOffice.app/Contents/MacOS",
            };

            foreach (var candidate in candidates)
            {
                if (IsValidInstallPath(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool IsValidInstallPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                // The LOK entry point on Windows is "soffice.dll" (next to soffice.exe); on Unix
                // and macOS it is "liblibreofficekit.so"/".dylib". Checking for the executable
                // binary (or the well-known LOK library) avoids false positives from leftover,
                // partially uninstalled directories.
                var hasWindowsExe = File.Exists(Path.Combine(path, "soffice.exe"))
                    || File.Exists(Path.Combine(path, "soffice.bin"));
                var hasLokLibrary = File.Exists(Path.Combine(path, "liblibreofficekit.so"))
                    || File.Exists(Path.Combine(path, "liblibreofficekit.dylib"));
                return hasWindowsExe || hasLokLibrary;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a spreadsheet stream into a PDF written to <paramref name="outputStream"/>.
        /// <paramref name="inputFileName"/> is used only to pick the correct import filter - the
        /// caller's stream carries no name of its own. Streams are not disposed by this method.
        /// </summary>
        internal void Convert(Stream inputStream, Stream outputStream, string? inputFileName, Xlsx2PdfOption? options, CancellationToken token = default)
        {
            options ??= new Xlsx2PdfOption();

            var converter = GetConverter();
            var pdfOptions = LibreOfficeOptionsMapper.Map(options);
            var timeout = LibreOfficeOptionsMapper.MapTimeout(options.TimeoutSeconds);

            // LibreOfficeKit identifies the document by file extension, so the temp file must keep it.
            var sourceExtension = Path.GetExtension(inputFileName ?? string.Empty);
            if (string.IsNullOrEmpty(sourceExtension))
            {
                sourceExtension = ".xlsx";
            }

            var tempDirectory = CreateTempDirectory();
            var tempInput = Path.Combine(tempDirectory, $"input{sourceExtension}");
            var tempOutput = Path.Combine(tempDirectory, "output.pdf");

            try
            {
                token.ThrowIfCancellationRequested();

                using (var file = File.Create(tempInput))
                {
                    inputStream.CopyTo(file);
                }

                token.ThrowIfCancellationRequested();

                // The file-path overload is used (rather than the stream overload) because it is the
                // only one that accepts a CancellationToken, letting a cancelled request tear down the
                // worker process instead of running to completion.
                var conversion = converter.ConvertToPdfAsync(
                    tempInput,
                    tempOutput,
                    timeout,
                    pdfOptions,
                    null,
                    token);

                // Convert is synchronous on the handler contract, so bridge without capturing the
                // current SynchronizationContext - otherwise a UI/test context could deadlock.
                Task.Run(() => conversion, token).GetAwaiter().GetResult();

                token.ThrowIfCancellationRequested();

                if (!File.Exists(tempOutput))
                {
                    throw new InvalidOperationException(
                        $"LibreOffice did not produce a PDF for '{inputFileName ?? "input"}'.");
                }

                using (var pdf = File.OpenRead(tempOutput))
                {
                    pdf.CopyTo(outputStream);
                }
            }
            finally
            {
                TryDeleteDirectory(tempDirectory);
            }
        }

        private LibreOfficeConverter GetConverter()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LibreOfficeEngine));
            }

            // Fast path: already built.
            var existing = Volatile.Read(ref _converter);
            if (existing is not null)
            {
                return existing;
            }

            lock (_syncRoot)
            {
                if (_converter is not null)
                {
                    return _converter;
                }

                var installPath = TryResolveInstallPath();
                if (installPath is null)
                {
                    throw new InvalidOperationException(
                        "LibreOffice was not found. Install it, or point the "
                        + $"{LibreOfficeEngineSettings.InstallPathVariable} environment variable at the "
                        + "LibreOffice program directory (the folder containing soffice).");
                }

                _logger.LogInformation(
                    "Initializing LibreOffice worker pool at {installPath} (max {max}, hot standby {standby})",
                    installPath, _settings.MaxInstances, _settings.MinHotStandby);

                _converter = new LibreOfficeConverter(
                    _settings.MaxInstances,
                    _settings.MinHotStandby,
                    _settings.IdleTimeout,
                    null,
                    null,
                    installPath,
                    null);

                return _converter;
            }
        }

        private static string CreateTempDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"ys-knife-lo-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static void TryDeleteDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // Temp-file cleanup is best effort; a locked file must not fail the conversion.
            }
        }

        public void Dispose()
        {
            LibreOfficeConverter? converter;
            lock (_syncRoot)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                converter = _converter;
                _converter = null;
            }

            if (converter is null)
            {
                return;
            }

            try
            {
                converter.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to dispose the LibreOffice worker pool cleanly.");
            }
        }
    }
}
