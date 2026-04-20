namespace AuthAPI.DAL.Storage.CreateUser
{
    public interface ICreateUserStorage
    {
        Task<Guid> CreateAsync(string firstName, string lastName, CancellationToken ct);
    }
}
