using System.Collections.Generic;

namespace MyProject.Application.DTOs
{
    public class ChatResponse
    {
        /// <summary>
        /// 给用户的自然语言回复
        /// </summary>
        public string Reply { get; set; } = string.Empty;

        /// <summary>
        /// 回复类型：text / order / appointment / product / other
        /// </summary>
        public string Type { get; set; } = "text";

        /// <summary>
        /// 根据 Type 携带的额外数据（JSON 对象转字典）
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }
    }
}