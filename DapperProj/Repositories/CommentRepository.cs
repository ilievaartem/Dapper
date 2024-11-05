using DapperProj.Entities;
using DapperProj.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace DapperProj.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly MySqlConnection _sqlConnection;

    public CommentRepository(MySqlConnection sqlConnection) // Замінено SqlConnection на MySqlConnection
    {
        _sqlConnection = sqlConnection;
    }

    public async Task<List<Comments>> GetAllCommentsAsync()
    {
        var comments = new List<Comments>();

        string query = "SELECT * FROM Comments";
        using (var command = new MySqlCommand(query, _sqlConnection)) // MySqlCommand замість SqlCommand
        {
            await _sqlConnection.OpenAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    comments.Add(new Comments
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CommentText = reader.GetString(reader.GetOrdinal("CommentText")),
                        PhotoId = reader.IsDBNull(reader.GetOrdinal("PhotoId"))
                            ? (int?)null
                            : reader.GetInt32(reader.GetOrdinal("PhotoId"))
                    });
                }
            }
        }

        return comments;
    }

    public async Task<Comments> GetCommentByIdAsync(int id)
    {
        Comments comment = null;
        string query = "SELECT * FROM Comments WHERE Id = @Id";
        using (var command = new MySqlCommand(query, _sqlConnection))
        {
            command.Parameters.AddWithValue("@Id", id);
            await _sqlConnection.OpenAsync();
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    comment = new Comments
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CommentText = reader.GetString(reader.GetOrdinal("CommentText")),
                        PhotoId = reader.IsDBNull(reader.GetOrdinal("PhotoId"))
                            ? (int?)null
                            : reader.GetInt32(reader.GetOrdinal("PhotoId"))
                    };
                }
            }
        }

        return comment;
    }

    public async Task<int> AddCommentAsync(string commentText, int userId)
    {
        string query =
            "INSERT INTO Comments (CommentText, UserId) VALUES (@CommentText, @UserId); SELECT LAST_INSERT_ID();"; // Використання LAST_INSERT_ID()
        using (var command = new MySqlCommand(query, _sqlConnection))
        {
            command.Parameters.AddWithValue("@CommentText", commentText);
            command.Parameters.AddWithValue("@UserId", userId);
            try
            {
                await _sqlConnection.OpenAsync();
                var insertedId = (long)await command.ExecuteScalarAsync(); 
                return (int)insertedId; 
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }
        }
    }

    public async Task UpdateCommentAsync(int id, string commentText)
    {
        string query = "UPDATE Comments SET CommentText = @CommentText WHERE Id = @Id";
        using (var command = new MySqlCommand(query, _sqlConnection))
        {
            command.Parameters.AddWithValue("@CommentText", commentText);
            command.Parameters.AddWithValue("@Id", id);
            try
            {
                await _sqlConnection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync();
                if (affectedRows == 0)
                {
                    throw new Exception("No rows were updated. The comment may not exist.");
                }
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }
        }
    }

    public async Task DeleteCommentAsync(int id)
    {
        string query = "DELETE FROM Comments WHERE Id = @Id";
        using (var command = new MySqlCommand(query, _sqlConnection))
        {
            command.Parameters.AddWithValue("@Id", id);
            try
            {
                await _sqlConnection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync();
                if (affectedRows == 0)
                {
                    throw new Exception("No rows were deleted. The comment may not exist.");
                }
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }
        }
    }
}