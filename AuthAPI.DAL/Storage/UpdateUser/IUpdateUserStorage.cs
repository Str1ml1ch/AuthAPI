namespace AuthAPI.DAL.Storage.UpdateUser
{
    public interface IUpdateUserStorage
    {
        Task UpdateAsync(Guid id, string firstName, string lastName, CancellationToken ct);
    }
}
