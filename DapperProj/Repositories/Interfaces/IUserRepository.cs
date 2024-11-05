using DapperProj.Entities;

namespace DapperProj.Repositories.Interfaces;

public interface IUserRepository: IGenericRepository<Users>
{
    Task<IEnumerable<Object>> GetUsersWithComments();
}