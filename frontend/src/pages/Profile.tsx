import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getProfile, updateProfile, changePassword } from "../api/user";

import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";

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
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");

  // 🔹 load profile
  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const res = await getProfile();

        setEmail(res.data.email || "");
        setUsername(res.data.userName || "");
        setFullName(res.data.fullName || "");
      } catch (err: any) {
        console.error(err);
        toast.error("Не вдалося отримати профіль");
        navigate("/auth");
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, [navigate]);

  // 🔹 save profile (username + fullname)
  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);

    try {
      await updateProfile({
        userName: username,
        fullName: fullName,
      });

      toast.success("Профіль оновлено!");
    } catch (err: any) {
      console.error(err);

      const message =
        err.response?.data?.message || "Помилка оновлення";

      toast.error(message);
    } finally {
      setSaving(false);
    }
  };

  // 🔹 change password
  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setSavingPass(true);

    try {
      await changePassword({
        currentPassword,
        newPassword,
      });

      toast.success("Пароль змінено!");

      setCurrentPassword("");
      setNewPassword("");
    } catch (err: any) {
      console.error(err);

      const message =
        err.response?.data?.message || "Помилка зміни пароля";

      toast.error(message);
    } finally {
      setSavingPass(false);
    }
  };

  if (loading) {
    return (
      <div className="container py-20 text-center">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-12 max-w-lg space-y-8">

      {/* PROFILE */}
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">
            Профіль користувача
          </CardTitle>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSave} className="space-y-4">

            <div className="space-y-2">
              <Label>Email</Label>
              <Input value={email} disabled className="bg-muted" />
            </div>

            <div className="space-y-2">
              <Label>Username</Label>
              <Input
                value={username}
                onChange={(e) => setUsername(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>Повне ім'я</Label>
              <Input
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
              />
            </div>

            <Button type="submit" variant="accent" className="w-full" disabled={saving}>
              {saving ? "Збереження..." : "Зберегти профіль"}
            </Button>

          </form>
        </CardContent>
      </Card>

      {/* PASSWORD */}
      <Card>
        <CardHeader>
          <CardTitle>Зміна пароля</CardTitle>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleChangePassword} className="space-y-4">

            <div className="space-y-2">
              <Label>Старий пароль</Label>
              <Input
                type="password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>Новий пароль</Label>
              <Input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
              />
            </div>

            <Button type="submit" variant="accent" className="w-full" disabled={savingPass}>
              {savingPass ? "Збереження..." : "Змінити пароль"}
            </Button>

          </form>
        </CardContent>
      </Card>
    </div>
    
  );
}