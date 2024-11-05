using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using Dapper;
using DapperProj.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace DapperProj.Repositories;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected MySqlConnection _sqlConnection; // Замінено SqlConnection на MySqlConnection

    protected IDbTransaction _dbTransaction;

    private readonly string _tableName;

    protected GenericRepository(MySqlConnection sqlConnection, IDbTransaction dbTransaction, string tableName)
    {
        _sqlConnection = sqlConnection;
        _dbTransaction = dbTransaction;
        _tableName = tableName;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _sqlConnection.QueryAsync<T>($"SELECT * FROM {_tableName}", transaction: _dbTransaction);
    }

    public async Task<T> GetAsync(int id)
    {
        var result = await _sqlConnection.QuerySingleOrDefaultAsync<T>($"SELECT * FROM {_tableName} WHERE Id=@Id",
            new { Id = id }, transaction: _dbTransaction);
        if (result == null)
            throw new KeyNotFoundException($"{_tableName} with id [{id}] could not be found.");
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        await _sqlConnection.ExecuteAsync($"DELETE FROM {_tableName} WHERE Id=@Id", new { Id = id },
            transaction: _dbTransaction);
    }

    public async Task<int> AddAsync(T t)
    {
        var insertQuery = GenerateInsertQuery();
        var newId = await _sqlConnection.ExecuteScalarAsync<int>(insertQuery, t, transaction: _dbTransaction);
        return newId;
    }

    public async Task<int> AddRangeAsync(IEnumerable<T> list)
    {
        var query = GenerateInsertQuery();
        return await _sqlConnection.ExecuteAsync(query, list);
    }

    public async Task ReplaceAsync(T t)
    {
        var updateQuery = GenerateUpdateQuery();
        await _sqlConnection.ExecuteAsync(updateQuery, t, transaction: _dbTransaction);
    }

    // Генерація запитів для роботи з MySQL
    private IEnumerable<PropertyInfo> GetProperties => typeof(T).GetProperties();

    private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties)
    {
        return (from prop in listOfProperties
            let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
            where attributes.Length <= 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore"
            select prop.Name).ToList();
    }

    private string GenerateUpdateQuery()
    {
        var updateQuery = new StringBuilder($"UPDATE {_tableName} SET ");
        var properties = GenerateListOfProperties(GetProperties);
        properties.ForEach(property =>
        {
            if (!property.Equals("Id"))
            {
                updateQuery.Append($"{property}=@{property},");
            }
        });
        updateQuery.Remove(updateQuery.Length - 1, 1); // Убираємо останню кому
        updateQuery.Append(" WHERE Id=@Id");
        return updateQuery.ToString();
    }

    private string GenerateInsertQuery()
    {
        var insertQuery = new StringBuilder($"INSERT INTO {_tableName} (");
        var properties = GenerateListOfProperties(GetProperties);
        properties.Remove("Id"); // Припускаємо, що Id - автоінкремент
        properties.ForEach(prop => { insertQuery.Append($"{prop},"); });
        insertQuery.Remove(insertQuery.Length - 1, 1).Append(") VALUES (");
        properties.ForEach(prop => { insertQuery.Append($"@{prop},"); });
        insertQuery.Remove(insertQuery.Length - 1, 1)
            .Append("); SELECT LAST_INSERT_ID();"); // Використовуємо LAST_INSERT_ID() для отримання нового ID
        return insertQuery.ToString();
    }
}