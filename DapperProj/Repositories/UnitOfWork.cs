using System.Data;
using DapperProj.Repositories.Interfaces;

namespace DapperProj.Repositories;

public class UnitOfWork: IUnitOfWork, IDisposable
{
    public IUserRepository _userRepository { get; }
    public ICommentRepository _commentRepository { get; }

    readonly IDbTransaction _dbTransaction;

    public UnitOfWork(IUserRepository userRepository, ICommentRepository commentRepository, IDbTransaction dbTransaction)
    {
        _userRepository = userRepository;
        _commentRepository = commentRepository;
        _dbTransaction = dbTransaction;
    }

    public void Commit()
    {
        try
        {
            _dbTransaction.Commit();
        }
        catch (Exception ex)
        {
            _dbTransaction.Rollback();
        }
    }

    public void Dispose()
    {
        _dbTransaction.Connection?.Close();
        _dbTransaction.Connection?.Dispose();
        _dbTransaction.Dispose();
    }
}