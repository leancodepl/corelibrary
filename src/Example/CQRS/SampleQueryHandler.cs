using System.Text;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.Execution;

namespace LeanCode.Example.CQRS
{
    public class SampleQueryHandler : IQueryHandler<AppContext, SampleQuery, SampleQuery.Result>
    {
        public Task<SampleQuery.Result> ExecuteAsync(AppContext context, SampleQuery query)
        {
            var sb = new StringBuilder();
            sb.Append("UserId (context): ");
            sb.AppendLine(context.UserId.ToString());
            sb.Append("Header (context): ");
            sb.AppendLine(context.Header);
            sb.AppendLine("LeanCode");
            return Task.FromResult(new SampleQuery.Result(sb.ToString()));
        }
    }
}
