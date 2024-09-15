using System;

namespace HomeBudget.Core.CQRS
{
    public class BaseCommand
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
