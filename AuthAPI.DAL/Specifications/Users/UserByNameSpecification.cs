using System.Linq.Expressions;
using AuthAPI.DAL.Entities;

namespace AuthAPI.DAL.Specifications.Users
{
    public sealed class UserByNameSpecification : ISpecification<User>
    {
        private readonly string _name;
        public UserByNameSpecification(string name) => _name = name;
        public Expression<Func<User, bool>> ToExpression()
            => u => u.FirstName.Contains(_name) || u.LastName.Contains(_name);
    }
}
