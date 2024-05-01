using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HomeBudget.Core
{
    public abstract class Enumeration(int id, string name) : IComparable
    {
        public string Name { get; } = name;

        public int Id { get; } = id;

        public override string ToString() => Name;

        public static IEnumerable<T> GetAll<T>()
            where T : Enumeration
        {
            return typeof(T).GetFields(BindingFlags.Public |
                                        BindingFlags.Static |
                                        BindingFlags.DeclaredOnly)
                        .Select(f => f.GetValue(null))
                        .Cast<T>();
        }

        public override bool Equals(object obj)
        {
            if (obj is not Enumeration otherValue)
            {
                return false;
            }

            var typeMatches = GetType() == obj.GetType();
            var valueMatches = Id.Equals(otherValue.Id);

            return typeMatches && valueMatches;
        }

        public int CompareTo(object other) => Id.CompareTo(((Enumeration)other)!.Id);
    }
}
