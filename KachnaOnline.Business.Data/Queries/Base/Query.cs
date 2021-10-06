﻿// Query.cs
// Author: Ondřej Ondryáš

using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace KachnaOnline.Business.Data.Queries.Base
{
    public abstract class Query<TResult, TDbContext> where TDbContext : DbContext
    {
        protected readonly TDbContext Context;

        protected Query(TDbContext context)
        {
            Context = context;
        }

        public abstract IQueryable<TResult> Get();
    }
}
