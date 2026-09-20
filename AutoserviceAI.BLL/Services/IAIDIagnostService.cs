using AutoserviceAI.BLL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceAI.BLL.Services
{
    public interface IAiDiagnosticService
    {
        Task<AiDiagnosticResponseDto> ChatAsync(
            List<ChatMessageDto> messages);
    }
}
