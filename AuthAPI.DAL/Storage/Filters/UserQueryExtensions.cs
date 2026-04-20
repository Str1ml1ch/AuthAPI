using AuthAPI.DAL.Entities;

namespace AuthAPI.DAL.Storage.Filters
{
    public static class UserQueryExtensions
    {
        public static IQueryable<User> FilterByName(this IQueryable<User> query, string? searchName)
        {
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(u => u.FirstName.Contains(searchName) || u.LastName.Contains(searchName));
            return query;
        }
    }
}
