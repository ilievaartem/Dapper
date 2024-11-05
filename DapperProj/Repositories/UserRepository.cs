using System.Data;
using Dapper;
using DapperProj.Entities;
using DapperProj.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace DapperProj.Repositories;

public class UserRepository : GenericRepository<Users>, IUserRepository
{
    public UserRepository(MySqlConnection sqlConnection, IDbTransaction dbTransaction) 
        : base(sqlConnection, dbTransaction, "Users")
    {
    }

    public async Task<IEnumerable<Object>> GetUsersWithComments()
    {
        string sql = @"SELECT 
                    u.Id AS UserId,
                    u.Name AS UserName,
                    c.CommentText AS UserComment
                FROM 
                    Users u
                JOIN 
                    Comments c
                ON 
                    u.Id = c.UserId;";

        var results = await _sqlConnection.QueryAsync(sql, transaction: _dbTransaction);

        var usersWithComments = results.Select(row => new
        {
            UserId = row.UserId,
            UserName = row.UserName,
            UserComment = row.UserComment
        }).ToList();

        return usersWithComments;
    }
}