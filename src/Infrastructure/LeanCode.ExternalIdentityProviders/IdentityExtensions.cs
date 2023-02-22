using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LeanCode.ExternalIdentityProviders;

internal static class IdentityExtensions
{
    internal static async Task EnsureIdentitySuccess(this Task<IdentityResult> resultTask)
    {
        var result = await resultTask;

        if (result.Succeeded)
        {
            return;
        }

        var errors = (result.Errors as List<IdentityError>) ?? result.Errors.ToList();

        if (errors.Count == 1)
        {
            throw new InvalidOperationException(errors[0].Description);
        }
        else
        {
            throw new AggregateException(errors.Select(s => new InvalidOperationException(s.Description)));
        }
    }
}
