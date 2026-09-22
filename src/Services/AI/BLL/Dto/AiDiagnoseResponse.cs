using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceAI.BLL.Dto
{
    public class AiDiagnoseResponse
    {
        public string Type { get; set; } = "suggestion"; // question | suggestion | final
        public string Message { get; set; } = "";
        public string? Diagnosis { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public double Confidence { get; set; }
    }
}
