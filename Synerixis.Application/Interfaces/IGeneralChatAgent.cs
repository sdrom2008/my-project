using Synerixis.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IGeneralChatAgent
    {
        Task<string> GenerateReplyAsync(string input, ChatContext context);
    }
}
