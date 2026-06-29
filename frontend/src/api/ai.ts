import api from "./api";

export interface ChatMessage {
  role: "user" | "assistant";
  content: string;
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