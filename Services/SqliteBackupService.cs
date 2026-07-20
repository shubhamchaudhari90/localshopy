using Microsoft.Data.Sqlite;
using System.Text;

namespace localshopyNew.Services
{
    public class SqliteBackupService
    {
        private readonly string _connectionString;

        public SqliteBackupService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public string GenerateBackupScript()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("-- LocalShopy SQLite Backup");
            sql.AppendLine($"-- Generated : {DateTime.Now}");
            sql.AppendLine();

            sql.AppendLine("PRAGMA foreign_keys=OFF;");
            sql.AppendLine("BEGIN TRANSACTION;");
            sql.AppendLine();

            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            var tables = GetTables(connection);
            foreach (var table in tables)
            {
                sql.AppendLine();
                sql.AppendLine($"-- Backup Table : {table}");
                sql.AppendLine();
                var columns = GetColumns(connection, table);
                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{table}]";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    List<string> values = new();
                    foreach (var column in columns)
                    {
                        values.Add(ConvertToSql(reader[column]));
                    }
                    sql.AppendLine($"INSERT INTO [{table}] " + $"([{string.Join("],[", columns)}]) " + $"VALUES ({string.Join(",", values)});");
                }
                sql.AppendLine();
            }
            sql.AppendLine();
            sql.AppendLine("COMMIT;");
            sql.AppendLine("PRAGMA foreign_keys=ON;");
            return sql.ToString();
        }

        private List<string> GetTables(SqliteConnection connection)
        {
            List<string> tables = new();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name
                FROM sqlite_master
                WHERE type='table'
                AND name NOT LIKE 'sqlite_%'
                ORDER BY name;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            return tables;
        }

        private List<string> GetColumns(SqliteConnection connection, string table)
        {
            List<string> columns = new();
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info([{table}]);";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(reader["name"].ToString());
            }
            return columns;
        }

        private string ConvertToSql(object value)
        {
            if (value == DBNull.Value) return "NULL";

            if (value is byte[] bytes)
            {
                return $"X'{Convert.ToHexString(bytes)}'";
            }

            if (value is string || value is DateTime)
            {
                return "'" + value.ToString().Replace("'", "''") + "'";
            }
            if (value is bool)
            {
                return (bool)value ? "1" : "0";
            }
            return value.ToString();
        }
    }
}
