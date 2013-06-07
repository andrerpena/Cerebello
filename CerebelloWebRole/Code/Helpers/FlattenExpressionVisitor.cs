using System.Linq;
using System.Linq.Expressions;

namespace CerebelloWebRole.Code
{
    public class FlattenExpressionVisitor<T> : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType == typeof(T))
            {
                return Expression.MakeMemberAccess(
                    node.Expression,
                    node.Expression.Type.GetMember(node.Member.Name).Single());
            }
            return base.VisitMember(node);
        }
    }
}