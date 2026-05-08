import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  ArrowLeft,
  Plus,
  Pencil,
  Trash2,
  Save,
} from "lucide-react";

import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { Textarea } from "../../components/ui/textarea";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "../../components/ui/dialog";

import { Label } from "../../components/ui/label";
import { toast } from "sonner";
import api from "../../api/api";

/* ================= TYPES ================= */

type Category = {
  categoryId: number;
  name: string;
};

type Service = {
  serviceId: number;
  name: string;
  price: number;
  categoryName: string;
};

/* ================= COMPONENT ================= */

export default function AdminServices() {
  const [services, setServices] = useState<Service[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Service | null>(null);

  const [catOpen, setCatOpen] = useState(false);
  const [catEdit, setCatEdit] = useState<Category | null>(null);

  const [form, setForm] = useState({
    name: "",
    price: "",
    categoryName: "",
  });

  const [catForm, setCatForm] = useState({
    name: "",
  });

  /* ================= LOAD ================= */

  useEffect(() => {
    loadAll();
  }, []);

  const loadAll = async () => {
    try {
      setLoading(true);

      const [servicesRes, catRes] = await Promise.all([
        api.get("/Catalog/Service"),
        api.get("/Catalog/Category"),
      ]);

      setServices(servicesRes.data);
      setCategories(catRes.data);
    } catch (e) {
      toast.error("Помилка завантаження");
    } finally {
      setLoading(false);
    }
  };

  /* ================= SERVICE CRUD ================= */

  const openCreate = () => {
    setEditing(null);
    setForm({ name: "", price: "", categoryName: "" });
    setOpen(true);
  };

  const openEdit = (s: Service) => {
    setEditing(s);
    setForm({
      name: s.name,
      price: String(s.price),
      categoryName: s.categoryName,
    });
    setOpen(true);
  };

  const saveService = async () => {
    try {
      if (!form.name || !form.price) {
        toast.error("Заповніть поля");
        return;
      }

      const payload = {
        name: form.name,
        price: Number(form.price),
        categoryName: form.categoryName,
      };

      if (editing) {
        await api.put(
          `/Catalog/Service/${editing.serviceId}`,
          payload
        );
        toast.success("Оновлено");
      } else {
        await api.post("/Catalog/Service", payload);
        toast.success("Створено");
      }

      setOpen(false);
      loadAll();
    } catch (e) {
      toast.error("Помилка збереження");
    }
  };

  const deleteService = async (id: number) => {
    await api.delete(`/Catalog/Service/${id}`);
    toast.success("Видалено");
    loadAll();
  };

  /* ================= CATEGORY CRUD ================= */

  const saveCategory = async () => {
    try {
      if (!catForm.name) return;

      if (catEdit) {
        await api.put(
          `/Catalog/Category/${catEdit.categoryId}`,
          catForm
        );
        toast.success("Категорія оновлена");
      } else {
        await api.post("/Catalog/Category", catForm);
        toast.success("Категорія створена");
      }

      setCatOpen(false);
      loadAll();
    } catch {
      toast.error("Помилка категорії");
    }
  };

  const deleteCategory = async (id: number) => {
    await api.delete(`/Catalog/Category/${id}`);
    toast.success("Категорія видалена");
    loadAll();
  };

  /* ================= UI ================= */

  if (loading) return <div className="p-10">Завантаження...</div>;

  return (
    <div className="container py-8">

      {/* HEADER */}
      <div className="flex items-center gap-3 mb-6">
        <Link to="/admin">
          <ArrowLeft />
        </Link>

        <h1 className="text-xl font-bold text-orange-600">
          Послуги
        </h1>

        <Button
          onClick={openCreate}
          className="ml-auto bg-orange-500"
        >
          <Plus className="w-4 h-4 mr-1" />
          Додати
        </Button>

        {/* CATEGORY BUTTON */}
        <Button
          onClick={() => {
            setCatEdit(null);
            setCatForm({ name: "" });
            setCatOpen(true);
          }}
          variant="outline"
        >
          Категорії
        </Button>
      </div>

      {/* TABLE */}
      <Table>
        <TableHeader>
          <TableRow className="bg-orange-50">
            <TableHead>Назва</TableHead>
            <TableHead>Категорія</TableHead>
            <TableHead>Ціна</TableHead>
            <TableHead>Дії</TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {services.map((s) => (
            <TableRow key={s.serviceId}>
              <TableCell>{s.name}</TableCell>
              <TableCell>{s.categoryName}</TableCell>
              <TableCell>{s.price} ₴</TableCell>

              <TableCell className="flex gap-2">
                <Button
                  size="icon"
                  variant="ghost"
                  onClick={() => openEdit(s)}
                >
                  <Pencil />
                </Button>

                <Button
                  size="icon"
                  variant="ghost"
                  onClick={() => deleteService(s.serviceId)}
                  className="text-red-500"
                >
                  <Trash2 />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      {/* ================= SERVICE DIALOG ================= */}

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? "Редагування" : "Створення"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-3">

            <Input
              placeholder="Назва"
              value={form.name}
              onChange={(e) =>
                setForm({ ...form, name: e.target.value })
              }
            />

            <Input
              placeholder="Ціна"
              value={form.price}
              onChange={(e) =>
                setForm({ ...form, price: e.target.value })
              }
            />

            {/* DROPDOWN CATEGORY */}
            <select
              className="w-full border p-2 rounded"
              value={form.categoryName}
              onChange={(e) =>
                setForm({
                  ...form,
                  categoryName: e.target.value,
                })
              }
            >
              <option value="">Оберіть категорію</option>
              {categories.map((c) => (
                <option key={c.categoryId} value={c.name}>
                  {c.name}
                </option>
              ))}
            </select>

            <Button
              className="w-full bg-orange-500"
              onClick={saveService}
            >
              <Save className="w-4 h-4 mr-1" />
              Зберегти
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      {/* ================= CATEGORY DIALOG ================= */}

      <Dialog open={catOpen} onOpenChange={setCatOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Категорії</DialogTitle>
          </DialogHeader>

          <Input
            placeholder="Назва категорії"
            value={catForm.name}
            onChange={(e) =>
              setCatForm({ name: e.target.value })
            }
          />

          <Button onClick={saveCategory} className="bg-orange-500">
            Зберегти
          </Button>

          {/* list */}
          <div className="mt-4 space-y-2">
            {categories.map((c) => (
              <div
                key={c.categoryId}
                className="flex justify-between border p-2 rounded"
              >
                <span>{c.name}</span>

                <div className="flex gap-2">
                  <Button
                    size="icon"
                    onClick={() => {
                      setCatEdit(c);
                      setCatForm({ name: c.name });
                      setCatOpen(true);
                    }}
                  >
                    <Pencil />
                  </Button>

                  <Button
                    size="icon"
                    className="text-red-500"
                    onClick={() =>
                      deleteCategory(c.categoryId)
                    }
                  >
                    <Trash2 />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}