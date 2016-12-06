using System;
using System.Collections.Generic;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.DataAccess
{
    public interface IRepository<TEntity> : IRepository<TEntity, Guid>
        where TEntity : class, IAggregateRoot<Guid>
    { }

    public interface IRepository<TEntity, in TIdentity>
        where TEntity : class, IAggregateRoot<TIdentity>
    {
        TEntity Find(TIdentity id);
        void Add(TEntity entity);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
    }
}
