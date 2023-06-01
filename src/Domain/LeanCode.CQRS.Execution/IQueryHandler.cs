using System.Threading.Tasks;
using LeanCode.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.CQRS.Execution;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> ExecuteAsync(HttpContext context, TQuery query);
}
