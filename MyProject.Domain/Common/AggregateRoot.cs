using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Domain.Common
{
    public abstract class AggregateRoot<TKey>
    {
        public TKey Id { get; protected set; } = default!;
    }
}
