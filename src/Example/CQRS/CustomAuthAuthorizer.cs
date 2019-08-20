using System.Threading.Tasks;
using LeanCode.CQRS.Security;
using LeanCode.Example;

namespace Example.CQRS
{
    public class CustomAuthAuthorizer : CustomAuthorizer<AppContext, IFooRelated>, CustomAuth
    {
        protected override Task<bool> CheckIfAuthorizedAsync(AppContext appContext, IFooRelated obj)
        {
            return Task.FromResult(obj.FooId == appContext.UserId);
        }
    }
}
