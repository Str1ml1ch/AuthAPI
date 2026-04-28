using System.Linq.Expressions;

namespace AuthAPI.DAL.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }
}
