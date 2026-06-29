import { useEffect, useRef, useState } from "react";
import {
  Bot,
  MessageCircle,
  Send,
  Sparkles,
  Wrench,
} from "lucide-react";

import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "./ui/sheet";

import { Button } from "./ui/button";
import { Textarea } from "./ui/textarea";
import { Link } from "react-router-dom";
import { toast } from "sonner";

import {
  type ChatMessage,
  sendDiagnosticMessage
} from "../api/ai";

const SUGGESTIONS = [
  "Стукіт у підвісці на нерівностях",
  "Двигун важко заводиться вранці",
  "З'явився скрегіт під час гальмування",
  "Машина почала тягнути в бік",
];

export default function AiChatting() {
  const [open, setOpen] = useState(false);

  const [messages, setMessages] = useState<ChatMessage[]>([
    {
      role: "assistant",
      content:
        "Вітаю! Я AI-діагност вашого автосервісу 🔧\n\nОпишіть проблему з автомобілем, і я допоможу визначити можливі причини несправності.",
    },
  ]);

  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);

  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    scrollRef.current?.scrollTo({
      top: scrollRef.current.scrollHeight,
      behavior: "smooth",
    });
  }, [messages, loading]);

  const send = async (text: string) => {
    const content = text.trim();

    if (!content || loading) return;

    const updatedMessages: ChatMessage[] = [
      ...messages,
      {
        role: "user",
        content,
      },
    ];

    setMessages(updatedMessages);
    setInput("");
    setLoading(true);

    try {
      const response =
        await sendDiagnosticMessage(updatedMessages);

      const reply =
        response.data.reply ||
        "Не вдалося отримати відповідь.";

      setMessages([
        ...updatedMessages,
        {
          role: "assistant",
          content: reply,
        },
      ]);
    } catch (error: any) {
      console.error(error);

      toast.error(
        error?.response?.data?.message ||
          "Помилка підключення до AI"
      );

      setMessages(updatedMessages);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <button
          className="
            fixed
            bottom-6
            right-6
            z-50
            flex
            items-center
            gap-2
            rounded-full
            bg-orange-500
            text-white
            px-5
            py-3
            shadow-xl
            hover:scale-105
            transition
          "
        >
          <Bot className="h-5 w-5" />

          <span className="hidden sm:block font-medium">
            AI Діагност
          </span>
        </button>
      </SheetTrigger>

      <SheetContent className="p-0 flex flex-col sm:max-w-md">
        <SheetHeader className="border-b p-4 bg-orange-500 text-white">
          <SheetTitle className="flex items-center gap-3 text-white">
            <div className="bg-white/20 p-2 rounded-lg">
              <Wrench className="h-5 w-5" />
            </div>

            <div>
              <div>AI Діагност</div>

              <div className="text-xs font-normal opacity-80 flex items-center gap-1">
                <Sparkles className="h-3 w-3" />
                Допомога з поломками
              </div>
            </div>
          </SheetTitle>
        </SheetHeader>

        <div
          ref={scrollRef}
          className="flex-1 overflow-y-auto p-4 space-y-4"
        >
          {messages.map((message, index) => (
            <div
              key={index}
              className={`flex ${
                message.role === "user"
                  ? "justify-end"
                  : "justify-start"
              }`}
            >
              <div
                className={`
                  max-w-[85%]
                  rounded-2xl
                  px-4
                  py-3
                  text-sm
                  whitespace-pre-wrap
                  ${
                    message.role === "user"
                      ? "bg-orange-500 text-white rounded-br-sm"
                      : "bg-muted rounded-bl-sm"
                  }
                `}
              >
                {message.content}
              </div>
            </div>
          ))}

          {loading && (
            <div className="flex justify-start">
              <div className="bg-muted px-4 py-3 rounded-2xl">
                AI аналізує проблему...
              </div>
            </div>
          )}

          {messages.length === 1 && (
            <div className="space-y-2">
              <p className="text-xs text-muted-foreground">
                Популярні запити:
              </p>

              {SUGGESTIONS.map((item) => (
                <button
                  key={item}
                  onClick={() => send(item)}
                  className="
                    w-full
                    text-left
                    border
                    rounded-lg
                    p-3
                    text-sm
                    hover:bg-muted
                    transition
                  "
                >
                  <MessageCircle className="inline h-4 w-4 mr-2" />
                  {item}
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="border-t p-3">
          <div className="flex gap-2">
            <Textarea
              value={input}
              disabled={loading}
              rows={2}
              className="resize-none"
              placeholder="Опишіть симптом або проблему..."
              onChange={(e) =>
                setInput(e.target.value)
              }
              onKeyDown={(e) => {
                if (
                  e.key === "Enter" &&
                  !e.shiftKey
                ) {
                  e.preventDefault();
                  send(input);
                }
              }}
            />

            <Button
              size="icon"
              onClick={() => send(input)}
              disabled={
                loading || !input.trim()
              }
            >
              <Send className="h-4 w-4" />
            </Button>
          </div>

          <div className="flex justify-between items-center mt-2">
            <span className="text-[10px] text-muted-foreground">
              AI може помилятися. Для точної
              діагностики зверніться до сервісу.
            </span>

            <Link
              to="/booking"
              onClick={() => setOpen(false)}
              className="text-xs text-orange-500 font-medium"
            >
              Записатись →
            </Link>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}
