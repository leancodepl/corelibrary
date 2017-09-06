using Autofac;
using AutoMapper;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace LeanCode.IdentityServer.PersistedGrantStore
{
    class PersistedGrantStoreProfile : Profile
    {
        public PersistedGrantStoreProfile()
        {
            CreateMap<PersistedGrantEntity, PersistedGrant>(MemberList.Destination);
            CreateMap<PersistedGrant, PersistedGrantEntity>(MemberList.Source);
        }
    }
}
