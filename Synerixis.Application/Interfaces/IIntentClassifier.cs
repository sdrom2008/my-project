using Synerixis.Application.DTOs;
using Synerixis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IIntentClassifier
    {
        /// <summary>
        /// 根据用户输入和最近对话历史，分类意图
        /// </summary>
        Task<ChatIntent> ClassifyAsync(string userInput, IReadOnlyList<ChatMessageDto> recentHistory);
    }
}
