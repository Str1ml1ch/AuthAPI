namespace AuthAPI.DAL.Storage.RemoveUser
{
    public interface IRemoveUserStorage
    {
        Task RemoveByIdAsync(Guid id, CancellationToken ct);
    }
}
