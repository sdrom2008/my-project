using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class DecryptPhoneDto
    {
        public string Code { get; set; }
        public string EncryptedData { get; set; }
        public string Iv { get; set; }
    }
}
