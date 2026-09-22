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

import { toast } from "sonner";
import api from "../../api/api";


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
    } catch {
      toast.error("Помилка завантаження");
    } finally {
      setLoading(false);
    }
  };


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
      const payload = {
        name: form.name,
        price: Number(form.price),
        categoryName: form.categoryName,
      };

      if (editing) {
        await api.put(`/Catalog/Service/${editing.serviceId}`, payload);
        toast.success("Оновлено");
      } else {
        await api.post("/Catalog/Service", payload);
        toast.success("Створено");
      }

      setOpen(false);
      loadAll();
    } catch {
      toast.error("Помилка збереження");
    }
  };

  const deleteService = async (id: number) => {
    await api.delete(`/Catalog/Service/${id}`);
    toast.success("Видалено");
    loadAll();
  };


  const saveCategory = async () => {
    try {
      if (catEdit) {
        await api.put(`/Catalog/Category/${catEdit.categoryId}`, catForm);
        toast.success("Оновлено");
      } else {
        await api.post("/Catalog/Category", catForm);
        toast.success("Створено");
      }

      setCatOpen(false);
      loadAll();
    } catch {
      toast.error("Помилка категорії");
    }
  };

  const deleteCategory = async (id: number) => {
    await api.delete(`/Catalog/Category/${id}`);
    toast.success("Видалено");
    loadAll();
  };


  if (loading) {
    return (
      <div className="container py-20 text-center text-muted-foreground">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-8 max-w-6xl">

      <div className="flex items-center gap-4 mb-6">

        <Link
          to="/admin"
          className="text-muted-foreground hover:text-orange-500 transition"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-3xl font-bold">
          Послуги
        </h1>

        <Button
          onClick={openCreate}
          className="ml-auto bg-orange-500 hover:bg-orange-600"
        >
          <Plus className="w-4 h-4 mr-1" />
          Додати
        </Button>

        <Button
          variant="outline"
          onClick={() => {
            setCatEdit(null);
            setCatForm({ name: "" });
            setCatOpen(true);
          }}
        >
          Категорії
        </Button>

      </div>

      <div className="border rounded-lg overflow-hidden">

        <Table>
          <TableHeader className="bg-muted">
            <TableRow>
              <TableHead>Назва</TableHead>
              <TableHead>Категорія</TableHead>
              <TableHead>Ціна</TableHead>
              <TableHead className="text-right">Дії</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {services.map((s) => (
              <TableRow key={s.serviceId} className="hover:bg-muted/40">

                <TableCell className="font-medium">
                  {s.name}
                </TableCell>

                <TableCell className="text-muted-foreground">
                  {s.categoryName}
                </TableCell>

                <TableCell>
                  {s.price} ₴
                </TableCell>

                <TableCell className="flex justify-end gap-2">

                  <Button
                    size="icon"
                    variant="ghost"
                    onClick={() => openEdit(s)}
                  >
                    <Pencil className="h-4 w-4" />
                  </Button>

                  <Button
                    size="icon"
                    variant="ghost"
                    onClick={() => deleteService(s.serviceId)}
                    className="text-red-500"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>

                </TableCell>

              </TableRow>
            ))}
          </TableBody>

        </Table>
      </div>


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

            <select
              className="w-full border rounded-md p-2"
              value={form.categoryName}
              onChange={(e) =>
                setForm({ ...form, categoryName: e.target.value })
              }
            >
              <option value="">Категорія</option>
              {categories.map((c) => (
                <option key={c.categoryId} value={c.name}>
                  {c.name}
                </option>
              ))}
            </select>

            <Button
              className="w-full bg-orange-500 hover:bg-orange-600"
              onClick={saveService}
            >
              <Save className="h-4 w-4 mr-1" />
              Зберегти
            </Button>

          </div>
        </DialogContent>
      </Dialog>


      <Dialog open={catOpen} onOpenChange={setCatOpen}>
        <DialogContent>

          <DialogHeader>
            <DialogTitle>Категорії</DialogTitle>
          </DialogHeader>

          <div className="space-y-3">

            <Input
              placeholder="Назва"
              value={catForm.name}
              onChange={(e) =>
                setCatForm({ name: e.target.value })
              }
            />

            <Button
              onClick={saveCategory}
              className="bg-orange-500 hover:bg-orange-600 w-full"
            >
              Зберегти
            </Button>

          </div>

          <div className="mt-4 space-y-2">

            {categories.map((c) => (
              <div
                key={c.categoryId}
                className="flex justify-between border rounded p-2"
              >

                <span>{c.name}</span>

                <div className="flex gap-2">

                  <Button
                    size="icon"
                    variant="ghost"
                    onClick={() => {
                      setCatEdit(c);
                      setCatForm({ name: c.name });
                      setCatOpen(true);
                    }}
                  >
                    <Pencil className="h-4 w-4" />
                  </Button>

                  <Button
                    size="icon"
                    variant="ghost"
                    className="text-red-500"
                    onClick={() => deleteCategory(c.categoryId)}
                  >
                    <Trash2 className="h-4 w-4" />
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