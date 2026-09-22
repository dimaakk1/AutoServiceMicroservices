using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (ValueObject)obj;
            var thisValues = GetEqualityComponents().GetEnumerator();
            var otherValues = other.GetEqualityComponents().GetEnumerator();

            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (thisValues.Current is null ^ otherValues.Current is null)
                    return false;

                if (thisValues.Current != null && !thisValues.Current.Equals(otherValues.Current))
                    return false;
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var obj in GetEqualityComponents())
                hash = hash * 23 + (obj?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
