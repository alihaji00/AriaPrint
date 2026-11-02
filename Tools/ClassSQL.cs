#nullable enable

using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// ClassSQL
/// </summary>

public class ClassSQL
{
    private string connString;

    public int timeOut_Second = 0;

    public ClassSQL(string conection_str)
    {
        connString = conection_str;
    }

    public ClassSQL()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        connString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    private SqlConnection GetSqlConnection()
    {
        return new SqlConnection(connString);
    }

    private int? GetCommandTimeout(int? commandTimeout)
    {
        if (commandTimeout.HasValue)
        {
            return commandTimeout;
        }

        if (timeOut_Second > 0)
        {
            return timeOut_Second;
        }

        return null;
    }

    private static string NormalizeParameterName(string name)
    {
        return name.Trim().TrimStart('@');
    }

    private static string QuoteIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Identifier is required.", nameof(name));
        }

        string[] parts = name.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            throw new ArgumentException("Identifier is invalid.", nameof(name));
        }

        return string.Join(
            ".",
            parts.Select(part =>
            {
                string cleanPart = part.Trim();

                if (cleanPart.StartsWith("[", StringComparison.Ordinal) &&
                    cleanPart.EndsWith("]", StringComparison.Ordinal))
                {
                    cleanPart = cleanPart[1..^1];
                }

                if (cleanPart.Length == 0 ||
                    cleanPart.Any(character =>
                        !(char.IsLetterOrDigit(character) || character == '_')))
                {
                    throw new ArgumentException("Identifier is invalid.", nameof(name));
                }

                return $"[{cleanPart}]";
            }));
    }

    private static IReadOnlyList<SqlParameterItem> ExtractParameters(object? source)
    {
        if (source is null)
        {
            return Array.Empty<SqlParameterItem>();
        }

        List<SqlParameterItem> result = new List<SqlParameterItem>();

        if (source is DynamicParameters dynamicParameters)
        {
            foreach (string parameterName in dynamicParameters.ParameterNames)
            {
                string normalizedName = NormalizeParameterName(parameterName);
                object? value = dynamicParameters.Get<object?>(parameterName);

                result.Add(new SqlParameterItem(normalizedName, value));
            }

            return result;
        }

        if (source is IDictionary dictionary)
        {
            foreach (DictionaryEntry item in dictionary)
            {
                if (item.Key is null)
                {
                    continue;
                }

                string normalizedName = NormalizeParameterName(item.Key.ToString() ?? string.Empty);

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    continue;
                }

                result.Add(new SqlParameterItem(normalizedName, item.Value));
            }

            return result;
        }

        PropertyInfo[] properties = source
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo property in properties)
        {
            if (!property.CanRead)
            {
                continue;
            }

            result.Add(new SqlParameterItem(property.Name, property.GetValue(source)));
        }

        return result;
    }

    public async Task<int> ExecuteAsync(
        string query,
        object? paramAnonymous = null,
        int? commandTimeout = null)
    {
        return await ExeQueryAsync(query, paramAnonymous, commandTimeout);
    }

    public async Task<int> ExeQueryAsync(
        string query,
        object? paramAnonymous = null,
        int? commandTimeout = null)
    {
        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        return await conn.ExecuteAsync(
            query,
            paramAnonymous,
            commandTimeout: GetCommandTimeout(commandTimeout));
    }

    public async Task<int> DeleteDynamicAsync(
        string tbl,
        object? whereParam)
    {
        IReadOnlyList<SqlParameterItem> whereItems = ExtractParameters(whereParam);

        if (whereItems.Count == 0)
        {
            throw new ArgumentException("At least one WHERE parameter is required.", nameof(whereParam));
        }

        DynamicParameters parameters = new DynamicParameters();
        List<string> conditions = new List<string>();

        foreach (SqlParameterItem item in whereItems)
        {
            string columnName = QuoteIdentifier(item.Name);

            if (item.Value is null || item.Value == DBNull.Value)
            {
                conditions.Add($"{columnName} IS NULL");
                continue;
            }

            string parameterName = $"w_{item.Name}";

            conditions.Add($"{columnName} = @{parameterName}");
            parameters.Add(parameterName, item.Value);
        }

        string query = $"DELETE FROM {QuoteIdentifier(tbl)} WHERE {string.Join(" AND ", conditions)}";

        return await ExeQueryAsync(query, parameters);
    }

    public async Task<int> InsertDynamicAsync(
        string tbl,
        object? paramAnonymous)
    {
        IReadOnlyList<SqlParameterItem> items = ExtractParameters(paramAnonymous);

        if (items.Count == 0)
        {
            throw new ArgumentException("At least one insert parameter is required.", nameof(paramAnonymous));
        }

        string fields = string.Join(", ", items.Select(item => QuoteIdentifier(item.Name)));
        string values = string.Join(", ", items.Select(item => $"@{item.Name}"));

        DynamicParameters parameters = new DynamicParameters();

        foreach (SqlParameterItem item in items)
        {
            parameters.Add(item.Name, item.Value);
        }

        string query = $"INSERT INTO {QuoteIdentifier(tbl)} ({fields}) VALUES ({values})";

        return await ExeQueryAsync(query, parameters);
    }

    public async Task<int> UpdateDynamicAsync(
        string tbl,
        object? paramAnonymous,
        object? whereParam)
    {
        IReadOnlyList<SqlParameterItem> updateItems = ExtractParameters(paramAnonymous);
        IReadOnlyList<SqlParameterItem> whereItems = ExtractParameters(whereParam);

        if (updateItems.Count == 0)
        {
            throw new ArgumentException("At least one update parameter is required.", nameof(paramAnonymous));
        }

        if (whereItems.Count == 0)
        {
            throw new ArgumentException("At least one WHERE parameter is required.", nameof(whereParam));
        }

        DynamicParameters parameters = new DynamicParameters();
        List<string> setItems = new List<string>();
        List<string> conditions = new List<string>();

        foreach (SqlParameterItem item in updateItems)
        {
            string parameterName = $"u_{item.Name}";

            setItems.Add($"{QuoteIdentifier(item.Name)} = @{parameterName}");
            parameters.Add(parameterName, item.Value);
        }

        foreach (SqlParameterItem item in whereItems)
        {
            string columnName = QuoteIdentifier(item.Name);

            if (item.Value is null || item.Value == DBNull.Value)
            {
                conditions.Add($"{columnName} IS NULL");
                continue;
            }

            string parameterName = $"w_{item.Name}";

            conditions.Add($"{columnName} = @{parameterName}");
            parameters.Add(parameterName, item.Value);
        }

        string query =
            $"UPDATE {QuoteIdentifier(tbl)} " +
            $"SET {string.Join(", ", setItems)} " +
            $"WHERE {string.Join(" AND ", conditions)}";

        return await ExeQueryAsync(query, parameters);
    }


    public async Task<T?> SelectOneAsync<T>(
        string selectQuery,
        object? parameters = null)
    {
        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        return await conn.QueryFirstOrDefaultAsync<T>(
            selectQuery,
            parameters,
            commandTimeout: GetCommandTimeout(null)
        );
    }

    public async Task<IEnumerable<IDictionary<string, object>>> SelectAsync(
        string selectQuery,
        object? parameters = null)
    {
        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        IEnumerable<dynamic> result = await conn.QueryAsync(
            selectQuery,
            parameters,
            commandTimeout: GetCommandTimeout(null));

        return result.Cast<IDictionary<string, object>>();
    }

    public async Task<IEnumerable<T>> SelectAsync<T>(
        string selectQuery,
        object? parameters = null)
    {
        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        return await conn.QueryAsync<T>(
            selectQuery,
            parameters,
            commandTimeout: GetCommandTimeout(null));
    }

    public async Task<IEnumerable<T>> SelectAsync<T>(
        string selectQuery,
        int pageSize,
        int pageNumber,
        object? parameters = null)
    {
        if (!selectQuery.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Please set the ORDER BY clause.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        int offset = (pageNumber - 1) * pageSize;
        string query = $"{selectQuery} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        return await conn.QueryAsync<T>(
            query,
            parameters,
            commandTimeout: GetCommandTimeout(null));
    }

    public async Task<int> ExeQueryBatchAsync(
        string query,
        IEnumerable<object>? parametersList = null,
        int? commandTimeout = null,
        int batchSize = 500)
    {
        if (parametersList is null)
        {
            return 0;
        }

        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        List<object> allParameters = parametersList.ToList();

        if (allParameters.Count == 0)
        {
            return 0;
        }

        int totalAffected = 0;

        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        for (int index = 0; index < allParameters.Count; index += batchSize)
        {
            List<object> batch = allParameters
                .Skip(index)
                .Take(batchSize)
                .ToList();

            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                totalAffected += await conn.ExecuteAsync(
                    query,
                    batch,
                    transaction,
                    GetCommandTimeout(commandTimeout));

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        return totalAffected;
    }

    public async Task<IDictionary<string, object>?> SelectOneAsync(
        string selectQuery,
        object? parameters = null)
    {
        using SqlConnection conn = GetSqlConnection();

        await conn.OpenAsync();

        dynamic? result = await conn.QueryFirstOrDefaultAsync(
            selectQuery,
            parameters,
            commandTimeout: GetCommandTimeout(null));

        return result as IDictionary<string, object>;
    }

    private sealed class SqlParameterItem
    {
        public SqlParameterItem(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object? Value { get; }
    }
}
