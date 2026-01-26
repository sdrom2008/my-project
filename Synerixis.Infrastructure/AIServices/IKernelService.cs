using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Cnblogs.SemanticKernel.Connectors.DashScope;

namespace Synerixis.Infrastructure.AIServices
{
    public interface IKernelService
    {
        Kernel GetKernel();
    }
}
