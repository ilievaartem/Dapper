namespace DapperProj.Repositories.Interfaces;

public interface IUnitOfWork: IDisposable
{
    IUserRepository _userRepository { get; }
    ICommentRepository _commentRepository { get; }
    void Commit();
    void Dispose();
}