import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
  getProfile,
  updateProfile,
  changePassword,
} from "../api/user";

import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "../components/ui/card";

import {
  User,
  Mail,
  ShieldCheck,
  LockKeyhole,
  Save,
} from "lucide-react";

import { toast } from "sonner";

export default function Profile() {
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);

  const [saving, setSaving] = useState(false);
  const [savingPass, setSavingPass] = useState(false);

  // PROFILE
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [fullName, setFullName] = useState("");

  // PASSWORD
  const [currentPassword, setCurrentPassword] =
    useState("");

  const [newPassword, setNewPassword] =
    useState("");

  // LOAD PROFILE
  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const res = await getProfile();

        setEmail(res.data.email || "");
        setUsername(res.data.userName || "");
        setFullName(res.data.fullName || "");
      } catch (err: any) {
        console.error(err);

        toast.error(
          "Не вдалося завантажити профіль"
        );

        navigate("/auth");
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, [navigate]);

  // SAVE PROFILE
  const handleSave = async (
    e: React.FormEvent
  ) => {
    e.preventDefault();

    setSaving(true);

    try {
      await updateProfile({
        userName: username,
        fullName,
      });

      toast.success("Профіль оновлено");
    } catch (err: any) {
      console.error(err);

      const message =
        err.response?.data?.message ||
        "Помилка оновлення профілю";

      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  // CHANGE PASSWORD
  const handleChangePassword = async (
    e: React.FormEvent
  ) => {
    e.preventDefault();

    setSavingPass(true);

    try {
      await changePassword({
        currentPassword,
        newPassword,
      });

      toast.success("Пароль змінено");

      setCurrentPassword("");
      setNewPassword("");
    } catch (err: any) {
      console.error(err);

      const message =
        err.response?.data?.message ||
        "Помилка зміни пароля";

      toast.error(message);
    } finally {
      setSavingPass(false);
    }
  };

  // LOADING
  if (loading) {
    return (
      <div className="container py-24 text-center">
        <div className="text-lg font-medium">
          Завантаження профілю...
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-muted/30">
      <div className="container max-w-5xl py-12">

        {/* HEADER */}
        <div className="mb-10">
          <h1 className="text-4xl font-bold tracking-tight mb-2">
            Мій профіль
          </h1>

          <p className="text-muted-foreground text-lg">
            Керуйте особистими даними та безпекою акаунта
          </p>
        </div>

        <div className="grid gap-8 lg:grid-cols-[1fr_380px]">

          {/* LEFT SIDE */}
          <div className="space-y-8">

            {/* PROFILE CARD */}
            <Card className="rounded-2xl border shadow-sm">
              <CardHeader className="border-b bg-muted/30 rounded-t-2xl">
                <div className="flex items-center gap-3">
                  <div className="w-11 h-11 rounded-xl bg-orange-100 flex items-center justify-center">
                    <User className="h-5 w-5 text-orange-500" />
                  </div>

                  <div>
                    <CardTitle className="text-2xl">
                      Особисті дані
                    </CardTitle>

                    <p className="text-sm text-muted-foreground mt-1">
                      Інформація вашого профілю
                    </p>
                  </div>
                </div>
              </CardHeader>

              <CardContent className="p-6">
                <form
                  onSubmit={handleSave}
                  className="space-y-5"
                >

                  {/* EMAIL */}
                  <div className="space-y-2">
                    <Label>Email</Label>

                    <div className="relative">
                      <Mail className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                      <Input
                        value={email}
                        disabled
                        className="pl-10 bg-muted"
                      />
                    </div>
                  </div>

                  {/* USERNAME */}
                  <div className="space-y-2">
                    <Label>Username</Label>

                    <div className="relative">
                      <User className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                      <Input
                        value={username}
                        onChange={(e) =>
                          setUsername(
                            e.target.value
                          )
                        }
                        className="pl-10"
                        placeholder="Username"
                      />
                    </div>
                  </div>

                  {/* FULLNAME */}
                  <div className="space-y-2">
                    <Label>Повне ім’я</Label>

                    <div className="relative">
                      <ShieldCheck className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                      <Input
                        value={fullName}
                        onChange={(e) =>
                          setFullName(
                            e.target.value
                          )
                        }
                        className="pl-10"
                        placeholder="Ваше ім’я"
                      />
                    </div>
                  </div>

                  <Button
                    type="submit"
                    disabled={saving}
                    className="w-full bg-orange-500 hover:bg-orange-600 text-white rounded-xl h-11"
                  >
                    <Save className="h-4 w-4 mr-2" />

                    {saving
                      ? "Збереження..."
                      : "Зберегти зміни"}
                  </Button>
                </form>
              </CardContent>
            </Card>

            {/* PASSWORD CARD */}
            <Card className="rounded-2xl border shadow-sm">
              <CardHeader className="border-b bg-muted/30 rounded-t-2xl">
                <div className="flex items-center gap-3">
                  <div className="w-11 h-11 rounded-xl bg-orange-100 flex items-center justify-center">
                    <LockKeyhole className="h-5 w-5 text-orange-500" />
                  </div>

                  <div>
                    <CardTitle className="text-2xl">
                      Безпека
                    </CardTitle>

                    <p className="text-sm text-muted-foreground mt-1">
                      Зміна пароля акаунта
                    </p>
                  </div>
                </div>
              </CardHeader>

              <CardContent className="p-6">
                <form
                  onSubmit={handleChangePassword}
                  className="space-y-5"
                >

                  {/* CURRENT PASSWORD */}
                  <div className="space-y-2">
                    <Label>
                      Поточний пароль
                    </Label>

                    <Input
                      type="password"
                      value={currentPassword}
                      onChange={(e) =>
                        setCurrentPassword(
                          e.target.value
                        )
                      }
                      placeholder="••••••••"
                    />
                  </div>

                  {/* NEW PASSWORD */}
                  <div className="space-y-2">
                    <Label>
                      Новий пароль
                    </Label>

                    <Input
                      type="password"
                      value={newPassword}
                      onChange={(e) =>
                        setNewPassword(
                          e.target.value
                        )
                      }
                      placeholder="••••••••"
                    />
                  </div>

                  <Button
                    type="submit"
                    disabled={savingPass}
                    className="w-full bg-orange-500 hover:bg-orange-600 text-white rounded-xl h-11"
                  >
                    <LockKeyhole className="h-4 w-4 mr-2" />

                    {savingPass
                      ? "Збереження..."
                      : "Змінити пароль"}
                  </Button>
                </form>
              </CardContent>
            </Card>
          </div>

          {/* RIGHT SIDE */}
          <div className="space-y-6">

            {/* USER CARD */}
            <Card className="rounded-2xl border shadow-sm overflow-hidden">
              <div className="h-24 bg-gradient-to-r from-orange-500 to-orange-400" />

              <CardContent className="relative pt-0 pb-6">

                <div className="w-20 h-20 rounded-2xl bg-white border-4 border-white shadow-md flex items-center justify-center -mt-10 mb-4">
                  <User className="h-9 w-9 text-orange-500" />
                </div>

                <h2 className="text-2xl font-bold">
                  {username || "Користувач"}
                </h2>

                <p className="text-muted-foreground mt-1">
                  {email}
                </p>

                {fullName && (
                  <div className="mt-5 rounded-xl bg-muted/50 p-4">
                    <p className="text-sm text-muted-foreground mb-1">
                      Повне ім’я
                    </p>

                    <p className="font-medium">
                      {fullName}
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* INFO */}
            <Card className="rounded-2xl border shadow-sm">
              <CardContent className="p-6">
                <h3 className="font-semibold text-lg mb-4">
                  Інформація
                </h3>

                <div className="space-y-4 text-sm">

      

                  <div className="flex items-center justify-between">
                    <span className="text-muted-foreground">
                      Статус акаунта
                    </span>

                    <span className="px-3 py-1 rounded-full bg-green-100 text-green-700 text-xs font-medium">
                      Активний
                    </span>
                  </div>

                </div>
              </CardContent>
            </Card>

          </div>
        </div>
      </div>
    </div>
  );
}