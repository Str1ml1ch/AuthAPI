using AuthAPI.Core.Models;

namespace AuthAPI.DAL.Storage.GetUserById
{
    public interface IGetUserByIdStorage
    {
        Task<UserModel?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    }
}
