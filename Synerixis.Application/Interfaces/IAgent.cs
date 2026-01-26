using Synerixis.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IAgent
    {
        string Name { get; }
        string Description { get; }

        Task<AgentResponse> ExecuteAsync(AgentContext context, CancellationToken ct = default);
    }
}
