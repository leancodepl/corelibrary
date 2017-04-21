﻿using System;
using System.Linq;
using System.Reflection;

namespace LeanCode.CQRS.Security
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AuthorizeWithPermissionAttribute : Attribute
    {
        public string Permission { get; }

        public AuthorizeWithPermissionAttribute(string Permission)
        {
            this.Permission = Permission;
        }

        public static string[] GetPermissions(Type type)
        {
            return type.GetTypeInfo()
                .GetCustomAttributes<AuthorizeWithPermissionAttribute>()
                .Select(attr => attr.Permission)
                .ToArray();
        }

        public static string[] GetPermissions(object obj) => GetPermissions(obj.GetType());
    }
}
