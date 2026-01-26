using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IIntentClassifier
    {
        Task<string> ClassifyAsync(string userMessage, CancellationToken ct = default);
    }
}
