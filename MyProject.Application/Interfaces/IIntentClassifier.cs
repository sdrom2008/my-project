using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Application.Interfaces
{
    public interface IIntentClassifier
    {
        Task<string> ClassifyAsync(string userMessage, CancellationToken ct = default);
    }
}
