using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace LeanCode.CQRS.Default
{
    class CombinedCQRSModule : Module
    {
        private readonly List<Module> modules = new List<Module>();

        public void AddModule(Module m) => modules.Add(m);

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var m in modules)
            {
                builder.RegisterModule(m);
            }
        }
    }
}
