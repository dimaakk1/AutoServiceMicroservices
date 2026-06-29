using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceAI.BLL.Dto
{
    public class ChatRequestDto
    {
        public List<ChatMessageDto> Messages { get; set; } = [];
    }
}
