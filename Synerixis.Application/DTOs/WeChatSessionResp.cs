using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class WeChatSessionResp
    {
        public string OpenId { get; set; }
        public string SessionKey { get; set; }
        public string UnionId { get; set; }  // 可选，部分场景有
        public int ErrCode { get; set; }
        public string Errmsg { get; set; }
    }
}
