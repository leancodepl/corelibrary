
LeanCode.CQRS.Annotations±
,LeanCode.CQRS.Annotations.PathAliasAttributeçRegister command/query/operation under additional HTTP path. This could be useful for reorganizing contracts namespaces without introducing a braking change.
    This attribute can be used multiple times.
namespace LncdApp.Contracts;
    {
        [PathAlias("LncdApp.Contacts.Commands")]
        public class MyCommand : ICommand { }
    }
MyCommand class will be registered as 'cqrs-base/command/LncdApp.Contracts.MyCommand' and 'cqrs-base/command/LncdApp.Contracts.Commands.MyCommand'R

"Ì
"Path