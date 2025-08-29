using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
namespace System.Data
{
    public static class ConnectionExtensions
    {
        public static DbProviderFactory GetFactory(this DbConnection connection)
        {
            return DbProviderFactories.GetFactory(connection);
        }
        public static object ExecuteScalar(this DbConnection connection, string sql, IDictionary<string, object> args = null)
        {
            TryOpen(connection);
            using var command = CreateCommand(connection, sql, args);
            var result = command.ExecuteScalar();
            return result == DBNull.Value ? null : result;
        }

        public static T ExecuteScalar<T>(this DbConnection connection, string sql, IDictionary<string, object> args = null)
        {
            return connection.ExecuteScalar(sql, args).ToValue<T>();
        }

        public static async Task<T> ExecuteScalarAsync<T>(this DbConnection connection, string sql, IDictionary<string, object> args = null, CancellationToken cancellationToken = default)
        {
            return (await connection.ExecuteScalarAsync(sql, args, cancellationToken)).ToValue<T>();
        }

        public static async Task<T> ExecuteScalarAsync<T>(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
        {
            return (await connection.ExecuteScalarAsync(sql, default, cancellationToken)).ToValue<T>();
        }
        public static async Task<object> ExecuteScalarAsync(this DbConnection connection, string sql, IDictionary<string, object> args = null, CancellationToken cancellationToken = default)
        {
            await TryOpenAsync(connection, cancellationToken);
            await using var command = CreateCommand(connection, sql, args);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result == DBNull.Value ? null : result;
        }

        public static int ExecuteNonQuery(this DbConnection connection, string sql, IDictionary<string, object> args = null)
        {
            _ = sql ?? throw new ArgumentNullException(nameof(sql));
            TryOpen(connection);
            using var command = CreateCommand(connection, sql, args);
            return command.ExecuteNonQuery();
        }
        public static Task<int> ExecuteNonQueryAsync(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
        {
            return ExecuteNonQueryAsync(connection, sql, default, cancellationToken);
        }
        public static async Task<int> ExecuteNonQueryAsync(this DbConnection connection, string sql, IDictionary<string, object> args = null, CancellationToken cancellationToken = default)
        {
            _ = sql ?? throw new ArgumentNullException(nameof(sql));
            await TryOpenAsync(connection, cancellationToken);
            await using var command = CreateCommand(connection, sql, args);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        private static DbCommand CreateCommand(DbConnection connection, string sql, IDictionary<string, object> args)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql.EndsWith(';') ? sql : sql + ';';
            command.CommandType = CommandType.Text;
            if (args?.Count > 0)
            {
                foreach (var kv in args)
                {
                    if (kv.Value is IDbDataParameter dataParameter)
                    {
                        dataParameter.ParameterName = kv.Key;
                        command.Parameters.Add(dataParameter);
                    }
                    else
                    {
                        var parameter = command.CreateParameter();
                        parameter.Value = kv.Value;
                        parameter.ParameterName = kv.Key;
                        command.Parameters.Add(parameter);
                    }
                }
            }

            return command;
        }

        public static void ExecuteNonQuery(this DbConnection connection, params string[] sqls)
        {
            if (sqls != null)
            {
                Array.ForEach(sqls, p =>
                     ExecuteNonQuery(connection, p)
                );
            }
        }

        public static async Task ExecuteNonQueryAsync(this DbConnection connection, string[] sqls, CancellationToken cancellationToken = default)
        {
            foreach (var sql in sqls ?? Array.Empty<string>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteNonQueryAsync(connection, sql, null, cancellationToken);
            }
        }

        public static void ExecuteSqlScriptFile(this DbConnection connection, string sqlFile, string sqlSplit = "", Action<(int StartLine, int LineCount, string Sql, int Result)> callback = null)
        {
            using var reader = new StreamReader(sqlFile);
            ExecuteSqlScript(connection, reader, sqlSplit, callback);
        }
        public static Task ExecuteSqlScriptFileAsync(this DbConnection connection, string sqlFile, string sqlSplit = "", Action<(int StartLine, int LineCount, string Sql, int Result)> callback = null, CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(sqlFile);
            return ExecuteSqlScriptAsync(connection, reader, sqlSplit, callback, cancellationToken);
        }

        public static void ExecuteSqlScript(this DbConnection connection, TextReader textReader, string sqlSplit = "", Action<(int StartLine, int LineCount, string Sql, int Result)> callback = null)
        {
            var lines = new List<string>();
            var seq = 0;
            var (currentLine, segmentStartLine) = (1, 1);
            while (true)
            {
                string line = textReader.ReadLine();
                string trimedLine = line?.Trim();
                if (trimedLine == null)
                {
                    ExecuteCurrentStringBuilder();
                    break;
                }
                currentLine = seq++;
                if (trimedLine.Equals(sqlSplit ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
                {
                    ExecuteCurrentStringBuilder();
                }
                else
                {
                    lines.Add(line);
                }
            }


            void ExecuteCurrentStringBuilder()
            {
                var sql = string.Join('\n', lines);
                if (!string.IsNullOrWhiteSpace(sql))
                {
                    var result = connection.ExecuteNonQuery(sql);
                    callback?.Invoke((segmentStartLine, lines.Count, sql, result));
                }
                lines.Clear();
                segmentStartLine = currentLine + 1;

            }

        }

        public static async Task ExecuteSqlScriptAsync(this DbConnection connection, TextReader textReader, string sqlSplit = "", Action<(int StartLine, int LineCount, string Sql, int Result)> callback = null, CancellationToken cancellationToken = default)
        {
            var lines = new List<string>();
            var seq = 0;
            var (currentLine, segmentStartLine) = (1, 1);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string line = await textReader.ReadLineAsync();
                string trimedLine = line?.Trim();
                if (trimedLine == null)
                {
                    await ExecuteCurrentStringBuilderAsync();
                    break;
                }
                currentLine = seq++;
                if (trimedLine.Equals(sqlSplit ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
                {
                    await ExecuteCurrentStringBuilderAsync();
                }
                else
                {
                    lines.Add(line);
                }
            }


            async Task ExecuteCurrentStringBuilderAsync()
            {
                var sql = string.Join('\n', lines);
                if (!string.IsNullOrWhiteSpace(sql))
                {
                    var result = await connection.ExecuteNonQueryAsync(sql, null, cancellationToken);
                    callback?.Invoke((segmentStartLine, lines.Count, sql, result));
                }
                lines.Clear();
                segmentStartLine = currentLine + 1;

            }

        }

        public static void ExecuteSqlScript(this DbConnection connection, string sqlScripts, string sqlSplit = "", Action<(int StartLine, int LineCount, string Sql, int Result)> callback = null)
        {
            using var reader = new StringReader(sqlScripts ?? string.Empty);
            ExecuteSqlScript(connection, reader, sqlSplit, callback);
        }
        public static Task ExecuteSqlScriptAsync(this DbConnection connection, string sqlScripts, string sqlSplit = "", Action<(int StartLine, int LineCount, string Sql, int Result)> callback = null, CancellationToken cancellationToken = default)
        {
            using var reader = new StringReader(sqlScripts ?? string.Empty);
            return ExecuteSqlScriptAsync(connection, reader, sqlSplit, callback, cancellationToken);
        }
        public static bool ExecuteExists(this DbConnection connection, string sql, Func<DataRow, bool> condition = null)
        {
            var table = connection.ExecuteReaderAsTable(sql);
            if (condition != null)
            {
                return table.AsEnumerable().Any(condition);
            }
            return table.AsEnumerable().Any();
        }
        public static async Task<bool> ExecuteExistsAsync(this DbConnection connection, string sql, Func<DataRow, bool> condition = null, CancellationToken cancellationToken = default)
        {
            var table = await connection.ExecuteReaderAsTableAsync(sql, cancellationToken);
            if (condition != null)
            {
                return table.AsEnumerable().Any(condition);
            }
            return table.AsEnumerable().Any();
        }
        public static IDataReader ExecuteReader(this DbConnection connection, string sql)
        {
            TryOpen(connection);
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            return command.ExecuteReader();

        }
        public static async Task<IDataReader> ExecuteReaderAsync(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
        {
            await TryOpenAsync(connection, cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            var reader = await command.ExecuteReaderAsync(cancellationToken);
            return reader;

        }
        public static DataTable ExecuteReaderAsTable(this DbConnection connection, string sql)
        {
            using var reader = ExecuteReader(connection, sql);
            return reader.LoadDataTable();
        }
        public static async Task<DataTable> ExecuteReaderAsTableAsync(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
        {
            using var reader = await ExecuteReaderAsync(connection, sql, cancellationToken);
            return reader.LoadDataTable();
        }
        public static List<IDictionary<string, object>> ExecuteReaderAsList(this DbConnection connection, string sql)
        {
            using var reader = ExecuteReader(connection, sql);
            return reader.LoadData().ToList();
        }
        public static async Task<List<IDictionary<string, object>>> ExecuteReaderAsListAsync(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
        {
            using var reader = await ExecuteReaderAsync(connection, sql, cancellationToken);
            return reader.LoadData(cancellationToken).ToList();
        }
        public static List<T> ExecuteReaderAsList<T>(this DbConnection connection, string sql)
            where T : new()
        {
            using var reader = ExecuteReader(connection, sql);
            return reader.LoadObjectData<T>().ToList();
        }
        public static async Task<List<T>> ExecuteReaderAsListAsync<T>(this DbConnection connection, string sql, CancellationToken cancellationToken = default)
          where T : new()
        {
            using var reader = await ExecuteReaderAsync(connection, sql, cancellationToken);
            return reader.LoadObjectData<T>(cancellationToken).ToList();
        }
        public static List<Tuple<T1, T2>> ExecuteReaderAsList<T1, T2>(this DbConnection connection, string sql)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p => new Tuple<T1, T2>(p.FieldValue<T1>(0), p.FieldValue<T2>(1))).ToList();
        }
        public static List<Tuple<T1, T2, T3>> ExecuteReaderAsList<T1, T2, T3>(this DbConnection connection, string sql)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p => new Tuple<T1, T2, T3>(p.FieldValue<T1>(0), p.FieldValue<T2>(1), p.FieldValue<T3>(2))).ToList();
        }
        public static List<Tuple<T1, T2, T3, T4>> ExecuteReaderAsList<T1, T2, T3, T4>(this DbConnection connection, string sql)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p =>
                new Tuple<T1, T2, T3, T4>(p.FieldValue<T1>(0), p.FieldValue<T2>(1), p.FieldValue<T3>(2), p.FieldValue<T4>(3))).ToList();
        }
        public static List<Tuple<T1, T2, T3, T4, T5>> ExecuteReaderAsList<T1, T2, T3, T4, T5>(this DbConnection connection, string sql)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p =>
                new Tuple<T1, T2, T3, T4, T5>(p.FieldValue<T1>(0), p.FieldValue<T2>(1), p.FieldValue<T3>(2), p.FieldValue<T4>(3), p.FieldValue<T5>(4))).ToList();
        }
        public static List<Tuple<T1, T2, T3, T4, T5, T6>> ExecuteReaderAsList<T1, T2, T3, T4, T5, T6>(this DbConnection connection, string sql)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p =>
                new Tuple<T1, T2, T3, T4, T5, T6>(p.FieldValue<T1>(0), p.FieldValue<T2>(1), p.FieldValue<T3>(2), p.FieldValue<T4>(3), p.FieldValue<T5>(4), p.FieldValue<T6>(5))).ToList();
        }
        public static List<T> ExecuteSingleColumnReader<T>(this DbConnection connection, string sql, int columnIndex = 0)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p => p.FieldValue<T>(columnIndex)).ToList();
        }
        public static List<T> ExecuteSingleColumnReader<T>(this DbConnection connection, string sql, string columnName)
        {
            var table = ExecuteReaderAsTable(connection, sql);
            return table.AsEnumerable().Select(p => p.FieldValue<T>(columnName)).ToList();
        }

        public static void TryOpen(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public static async Task TryOpenAsync(this DbConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
        }
        private static DataTable LoadDataTable(this IDataReader reader)
        {
            //Create datatable to hold schema and data seperately
            //Get schema of our actual table
            DataTable schema = reader.GetSchemaTable();
            DataTable table = new DataTable();
            if (schema == null)
            {
                //This is our datatable filled with data
                return table;
            }
            for (int i = 0; i < schema.Rows.Count; i++)
            {
                //Create new column for each row in schema table
                //Set properties that are causing errors and add it to our datatable
                //Rows in schema table are filled with information of columns in our actual table
                DataColumn Col = new DataColumn(schema.Rows[i]["ColumnName"].ToString(), (Type)schema.Rows[i]["DataType"]);
                Col.AllowDBNull = true;
                Col.Unique = false;
                Col.AutoIncrement = false;
                table.Columns.Add(Col);
            }

            while (reader.Read())
            {
                //Read data and fill it to our datatable
                DataRow Row = table.NewRow();
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Row[i] = reader[i];
                }
                table.Rows.Add(Row);
            }
            //This is our datatable filled with data
            return table;
        }
        private static IEnumerable<IDictionary<string, object>> LoadData(this IDataReader reader, CancellationToken cancellationToken = default)
        {
            using var schema = reader.GetSchemaTable();
            if (schema != null)
            {
                var allColumns = schema.Rows.OfType<DataRow>().Select(p => p.FieldValue<string>("ColumnName")).ToList();
                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return allColumns.ToImmutableDictionary(p => p, p => reader[p].ToValue<object>());
                }
            }
            else
            {
                throw new InvalidOperationException("The data reader has no schema information.");
            }
        }
        private static IEnumerable<T> LoadObjectData<T>(this IDataReader reader, CancellationToken cancellationToken = default)
            where T : new()
        {
            using var schema = reader.GetSchemaTable();
            if (schema != null)
            {
                var allColumns = schema.Rows.OfType<DataRow>().Select(p => p.FieldValue<string>("ColumnName")).ToList();
                var properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToDictionary(p => p, p => allColumns.FirstOrDefault(t => string.Equals(p.Name, t, StringComparison.InvariantCultureIgnoreCase)));
                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var obj = new T();
                    foreach (var (k, v) in properties.Where(p => p.Value != null))
                    {
                        var val = reader[v].ToValue(k.PropertyType);
                        k.SetValue(obj, val);
                    }
                    yield return obj;
                }
            }
            else
            {
                throw new InvalidOperationException("The data reader has no schema information.");
            }
        }
        private static T ToValue<T>(this object val)
        {
            if (val == null || val == DBNull.Value)
            {
                return default(T);
            }
            else if (typeof(T) == typeof(object))
            {
                return (T)val;
            }
            else
            {
                var valType = Nullable.GetUnderlyingType(typeof(T));
                return (T)Convert.ChangeType(val, valType ?? typeof(T));
            }
        }
        private static object ToValue(this object val, Type type)
        {
            if (val == null || val == DBNull.Value)
            {
                return type.GetDefaultValue();
            }
            else if (type == typeof(object))
            {
                return val;
            }
            else
            {
                var valType = Nullable.GetUnderlyingType(type);
                return Convert.ChangeType(val, valType ?? type);
            }
        }
        private static T FieldValue<T>(this DataRow row, int index) => row[index].ToValue<T>();
        private static T FieldValue<T>(this DataRow row, string columnName) => row[columnName].ToValue<T>();
    }

}


