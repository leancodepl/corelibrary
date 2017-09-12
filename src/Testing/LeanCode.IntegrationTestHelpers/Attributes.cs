using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeanCode.IntegrationTestHelpers
{
    public abstract class IntegrationFactAttribute : FactAttribute
    {
        public int? CustomOrder { get; set; }

        public IntegrationFactAttribute()
        { }

        public IntegrationFactAttribute(int customOrder)
        {
            CustomOrder = customOrder;
        }

        public static int? GetCustomOrder(IMethodInfo method)
        {
            var ass = typeof(IntegrationFactAttribute).AssemblyQualifiedName;
            var attribs = method.GetCustomAttributes(ass);
            var xunitAttrib = (ReflectionAttributeInfo)attribs.SingleOrDefault();
            var attrib = (IntegrationFactAttribute)xunitAttrib?.Attribute;
            return attrib?.CustomOrder;
        }
    }

    public sealed class PreparationStepAttribute : IntegrationFactAttribute
    {
        public PreparationStepAttribute()
            : base()
        { }

        public PreparationStepAttribute(int customOrder)
            : base(customOrder)
        { }
    }

    public sealed class TestStepAttribute : IntegrationFactAttribute
    {
        public TestStepAttribute()
            : base()
        { }

        public TestStepAttribute(int customOrder)
            : base(customOrder)
        { }
    }
}
