using DapperProj.Entities;

namespace DapperProj.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<int> AddCommentAsync(string commentText, int userId);
    Task<List<Comments>> GetAllCommentsAsync();
    Task<Comments> GetCommentByIdAsync(int id);
    Task UpdateCommentAsync(int id, string commentText);
    Task DeleteCommentAsync(int id);
}