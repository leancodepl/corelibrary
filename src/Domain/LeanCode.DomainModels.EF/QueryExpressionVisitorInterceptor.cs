using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LeanCode.DomainModels.EF;

public sealed class QueryExpressionVisitorInterceptor(ExpressionVisitor visitor) : IQueryExpressionInterceptor
{
    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData) =>
        visitor.Visit(queryExpression);
}
