using AuthAPI.Core.Models;
using Homework.Ticketing.System.Shared.Models;

namespace AuthAPI.DAL.Storage.GetUsers
{
    public interface IGetUsersStorage
    {
        Task<ResultModel<List<UserModel>>> GetAsync(
            int page,
            int pageSize,
            string? searchName,
            CancellationToken ct);
    }
}
