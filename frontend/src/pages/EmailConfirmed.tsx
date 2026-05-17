import { useEffect, useState, useRef } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";

import { Card, CardContent } from "../components/ui/card";
import { Button } from "../components/ui/button";

import {
  CheckCircle2,
  XCircle,
  Loader2,
  Wrench,
} from "lucide-react";

export default function EmailConfirmed() {
  const [status, setStatus] = useState<
    "loading" | "success" | "error"
  >("loading");

  const [params] = useSearchParams();
  const navigate = useNavigate();

  const hasRun = useRef(false);

  useEffect(() => {
    if (hasRun.current) return;
    hasRun.current = true;

    const statusFromUrl = params.get("status");



    if (statusFromUrl === "success") {
      setStatus("success");
    } else {
      setStatus("error");
    }
  }, [params]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-muted/30 px-4">

      <div className="w-full max-w-md">

        <div className="flex justify-center mb-6">
          <div className="w-16 h-16 rounded-2xl bg-orange-500 flex items-center justify-center shadow-lg">
            <Wrench className="h-8 w-8 text-white" />
          </div>
        </div>

        <Card className="rounded-3xl border shadow-xl">
          <CardContent className="p-8 text-center space-y-5">

            {status === "loading" && (
              <>
                <Loader2 className="h-10 w-10 animate-spin text-orange-500 mx-auto" />

                <h2 className="text-2xl font-semibold">
                  Перевірка email...
                </h2>

                <p className="text-muted-foreground">
                  Зачекайте секунду
                </p>
              </>
            )}

            {status === "success" && (
              <>
                <CheckCircle2 className="h-12 w-12 text-green-500 mx-auto" />

                <h2 className="text-2xl font-bold text-green-600">
                  Email підтверджено
                </h2>

                <p className="text-muted-foreground">
                  Ваш акаунт успішно активовано.
                </p>

                <Button
                  onClick={() => navigate("/auth")}
                  className="w-full bg-orange-500 hover:bg-orange-600 text-white"
                >
                  Перейти до входу
                </Button>
              </>
            )}

            {status === "error" && (
              <>
                <XCircle className="h-12 w-12 text-red-500 mx-auto" />

                <h2 className="text-2xl font-bold text-red-600">
                  Email не підтверджено
                </h2>

                <p className="text-muted-foreground">
                  Посилання недійсне або вже використане.
                </p>

                <Button
                  onClick={() => navigate("/auth")}
                  className="w-full bg-orange-500 hover:bg-orange-600 text-white"
                >
                  Повернутись до авторизації
                </Button>
              </>
            )}

          </CardContent>
        </Card>

      </div>
    </div>
  );
}