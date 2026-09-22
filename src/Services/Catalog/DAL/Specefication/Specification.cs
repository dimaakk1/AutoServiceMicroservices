using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceCatalog.DAL.Specefication
{
    public abstract class Specification<T>
    {
        public Expression<Func<T, bool>> Criteria { get; protected set; } = x => true;

        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; protected set; }
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderByDescending { get; protected set; }

        public int? Skip { get; protected set; }
        public int? Take { get; protected set; }
    }
}
