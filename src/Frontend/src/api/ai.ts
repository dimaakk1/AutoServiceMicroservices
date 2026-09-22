import api from "./api";

export interface ChatMessage {
  role: "user" | "assistant";
  content: string;
  recommendation?: AiServiceRecommendation;
}

export interface AiServiceRecommendation {
  serviceId: number;
  name: string;
  price: number;
  reason: string;
}

export interface AiDiagnosticResponse {
  reply: string;
  recommendation?: AiServiceRecommendation | null;
}

export interface ChatRequest {
  messages: ChatMessage[];
}

export const sendDiagnosticMessage = (
  messages: ChatMessage[]
) => {
  return api.post("/ai-diagnostic/chat", {
    messages,
  });
};
