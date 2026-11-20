namespace YS.Knife.AuditLogs
{
    public static class AuditLogExtensions
    {
        public static void PushData(this IAuditLog log, string key, object value)
        {
            log.Datas.Add(key, value);
        }
        public static void SetData(this IAuditLog log, string key, object value)
        {
            log.Datas[key] = value;
        }
        public static bool HasData(this IAuditLog log, string key)
        {
            return log.Datas.ContainsKey(key);
        }
        public static bool TryPushData(this IAuditLog log, string key, object value)
        {
            if (log.Datas.ContainsKey(key))
            {
                return false;
            }
            else
            {
                log.Datas.Add(key, value);
                return true;
            }
        }
        public static T GetOrDefault<T>(this IAuditLog log, string key, T defaultValue = default)
        {
            if (log.Datas.TryGetValue(key, out var obj))
            {
                return (T)obj;
            }
            return defaultValue;
        }
        public static T PopOrDefault<T>(this IAuditLog log, string key, T defaultValue = default)
        {
            if (log.Datas.TryGetValue(key, out var obj))
            {
                log.Datas.Remove(key);
                return (T)obj;
            }
            return defaultValue;
        }

    }
}
