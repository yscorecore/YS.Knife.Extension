namespace YS.Knife.Document.Converter.Xlsx2Pdf
{
    /// <summary>
    /// Worker-pool tuning for <see cref="LibreOfficeEngine"/>. Values are read from environment
    /// variables so they can be adjusted per deployment without recompiling.
    /// </summary>
    internal sealed class LibreOfficeEngineSettings
    {
        /// <summary>
        /// Environment variable pointing at the LibreOffice program directory (the folder holding soffice).
        /// </summary>
        internal const string InstallPathVariable = "YS_KNIFE_LIBREOFFICE_PATH";

        /// <summary>
        /// Environment variable overriding the maximum number of worker processes.
        /// </summary>
        internal const string MaxInstancesVariable = "YS_KNIFE_LIBREOFFICE_MAX_INSTANCES";

        /// <summary>
        /// Environment variable overriding how many workers start pre-warmed.
        /// </summary>
        internal const string MinHotStandbyVariable = "YS_KNIFE_LIBREOFFICE_MIN_HOT_STANDBY";

        /// <summary>
        /// Environment variable overriding the worker idle timeout, in minutes.
        /// </summary>
        internal const string IdleTimeoutMinutesVariable = "YS_KNIFE_LIBREOFFICE_IDLE_TIMEOUT_MINUTES";

        internal const int DefaultMaxInstances = 4;
        internal const int DefaultMinHotStandby = 1;
        internal const int DefaultIdleTimeoutMinutes = 5;

        /// <summary>
        /// Maximum number of concurrent worker processes.
        /// </summary>
        internal int MaxInstances { get; init; } = DefaultMaxInstances;

        /// <summary>
        /// Number of workers kept warm so the first conversions do not pay process-startup cost.
        /// </summary>
        internal int MinHotStandby { get; init; } = DefaultMinHotStandby;

        /// <summary>
        /// How long a surplus worker survives without work before shutting down.
        /// </summary>
        internal TimeSpan IdleTimeout { get; init; } = TimeSpan.FromMinutes(DefaultIdleTimeoutMinutes);

        /// <summary>
        /// Builds settings from environment variables, falling back to defaults for missing or
        /// unparsable values.
        /// </summary>
        internal static LibreOfficeEngineSettings FromEnvironment()
        {
            return new LibreOfficeEngineSettings
            {
                MaxInstances = ReadPositiveInt(MaxInstancesVariable, DefaultMaxInstances),
                MinHotStandby = ReadPositiveInt(MinHotStandbyVariable, DefaultMinHotStandby),
                IdleTimeout = TimeSpan.FromMinutes(ReadPositiveInt(IdleTimeoutMinutesVariable, DefaultIdleTimeoutMinutes)),
            };
        }

        private static int ReadPositiveInt(string variable, int fallback)
        {
            var raw = Environment.GetEnvironmentVariable(variable);
            return int.TryParse(raw, out var parsed) && parsed > 0 ? parsed : fallback;
        }
    }
}
