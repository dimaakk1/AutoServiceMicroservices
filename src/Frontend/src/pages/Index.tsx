import { Link } from "react-router-dom";
import {
  ArrowRight,
  Bot,
  CheckCircle2,
  Clock,
  Gauge,
  MapPin,
  ScanLine,
  Shield,
  Wrench,
} from "lucide-react";
import { Button } from "../components/ui/button";
import heroImage from "../assets/hero-workshop.jpg";

const facts = [
  { value: "12+", label: "років досвіду" },
  { value: "500+", label: "клієнтів" },
  { value: "4.9", label: "середній рейтинг" },
  { value: "24/7", label: "онлайн-запис" },
];

const features = [
  { icon: Bot, title: "AI-діагност", desc: "Попередньо визначає можливу несправність і рекомендує послугу." },
  { icon: Wrench, title: "Досвідчені майстри", desc: "Ремонт і планове обслуговування автомобілів різних марок." },
  { icon: Shield, title: "Гарантія якості", desc: "Прозорі роботи, узгоджена ціна та контроль кожного етапу." },
  { icon: Clock, title: "Точно за часом", desc: "Запис на конкретний вільний слот без очікування в черзі." },
];

const serviceModules = [
  { code: "01", title: "Комп'ютерна діагностика", description: "Пошук помилок і комплексна перевірка систем автомобіля." },
  { code: "02", title: "Технічне обслуговування", description: "Регламентні роботи, заміна мастил, фільтрів і витратних матеріалів." },
  { code: "03", title: "Ремонт ходової", description: "Діагностика підвіски, рульового керування та гальмівної системи." },
];

export default function Index() {
  const openAiChat = () => {
    document.querySelector<HTMLButtonElement>('[aria-label="AI діагностика"]')?.click();
  };

  return (
    <div>
      <section className="border-b border-border bg-card">
        <div className="container grid border-x border-border p-0 lg:grid-cols-[minmax(0,1.4fr)_minmax(320px,.6fr)]">
          <div className="relative flex min-h-[590px] flex-col justify-end overflow-hidden border-b border-border lg:border-b-0 lg:border-r">
            <img src={heroImage} alt="Майстер працює з автомобілем" className="absolute inset-0 h-full w-full object-cover opacity-55" />
            <div className="absolute inset-0 bg-gradient-to-t from-background via-background/65 to-background/10" />
            <div className="relative p-7 md:p-12 lg:p-16">
              <div className="technical-label mb-6 flex items-center gap-3"><span className="h-px w-8 bg-accent" />Професійний автосервіс</div>
              <h1 className="max-w-4xl text-6xl leading-[.82] sm:text-7xl md:text-8xl lg:text-9xl">
                АвтоПро<br /><span className="text-accent">Ремонт без здогадок.</span>
              </h1>
              <p className="mt-7 max-w-xl text-base leading-relaxed text-foreground/75 md:text-lg">
                Спочатку знаходимо причину, пояснюємо рішення та погоджуємо ціну — тільки потім ремонтуємо.
              </p>
              <div className="mt-8 flex flex-wrap gap-3">
                <Button asChild size="lg" className="shadow-glow"><Link to="/booking">Записатися <ArrowRight /></Link></Button>
                <Button asChild size="lg" variant="outline"><Link to="/services">Перелік робіт</Link></Button>
              </div>
            </div>
          </div>

          <aside className="flex flex-col">
            <div className="flex items-center justify-between border-b border-border p-6">
              <div>
                <div className="technical-label text-muted-foreground">Статус станції</div>
                <div className="mt-2 flex items-center gap-2 text-xs font-semibold uppercase"><span className="h-2 w-2 bg-success" /> Приймаємо авто</div>
              </div>
              <span className="font-display text-4xl text-accent">01</span>
            </div>
            {serviceModules.map((service) => (
              <Link to="/services" key={service.code} className="group flex-1 border-b border-border p-7 transition-colors hover:bg-muted/50">
                <div className="flex justify-between"><span className="technical-label">Модуль {service.code}</span><span className="h-8 w-1 bg-border group-hover:bg-accent" /></div>
                <h2 className="mt-5 text-3xl leading-none">{service.title}</h2>
                <p className="mt-3 text-sm text-muted-foreground">{service.description}</p>
                <div className="mt-5 flex items-center justify-between border-t pt-4 text-xs font-semibold"><span>Детальніше</span><ArrowRight className="h-4 w-4 text-accent" /></div>
              </Link>
            ))}
            <button onClick={openAiChat} className="flex items-center justify-between bg-accent p-6 text-left text-xs font-semibold uppercase text-accent-foreground">
              <span className="flex items-center gap-2"><Bot className="h-5 w-5" /> Перевірити симптоми</span><ArrowRight className="h-4 w-4" />
            </button>
          </aside>
        </div>
      </section>

      <section className="border-b border-border">
        <div className="container grid grid-cols-2 border-x border-border p-0 md:grid-cols-4">
          {facts.map((fact, index) => (
            <div key={fact.label} className={`border-border p-6 md:p-8 ${index < facts.length - 1 ? "md:border-r" : ""} ${index % 2 === 0 ? "border-r md:border-r" : ""}`}>
              <div className="font-display text-4xl md:text-5xl">{fact.value}</div>
              <div className="technical-label mt-1 text-muted-foreground">{fact.label}</div>
            </div>
          ))}
        </div>
      </section>

      <section className="py-16 md:py-24">
        <div className="container grid gap-10 lg:grid-cols-[.7fr_1.3fr] lg:gap-16">
          <div>
            <div className="technical-label mb-4">Система роботи</div>
            <h2 className="text-5xl leading-[.9] md:text-7xl">Сервіс без сюрпризів у фінальному чеку.</h2>
          </div>
          <div className="grid border-l border-t border-border sm:grid-cols-2">
            {features.map((feature, index) => (
              <div key={feature.title} className="border-b border-r border-border p-6 transition-colors hover:bg-card">
                <div className="flex items-center justify-between"><feature.icon className="h-5 w-5 text-accent" /><span className="technical-label text-muted-foreground">0{index + 1}</span></div>
                <h3 className="mt-8 text-2xl">{feature.title}</h3>
                <p className="mt-2 text-sm leading-relaxed text-muted-foreground">{feature.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="border-y border-border bg-card py-16 md:py-24">
        <div className="container">
          <div className="mb-10 flex items-end justify-between gap-6">
            <div><div className="technical-label mb-3">Цифрові інструменти</div><h2 className="text-5xl md:text-7xl">Сервіс у вашому кабінеті</h2></div>
          </div>
          <div className="grid border-l border-t border-border md:grid-cols-3">
            {[
              { icon: ScanLine, title: "VIN-декодер", text: "Перевірте заводські характеристики автомобіля за VIN-кодом.", to: "/vin-decoder" },
              { icon: Gauge, title: "Мій гараж", text: "Зберігайте автомобілі та обирайте потрібний під час запису.", to: "/my-vehicles" },
              { icon: Bot, title: "AI-діагностика", text: "Опишіть симптоми й отримайте попередню рекомендацію послуги.", to: "/services" },
            ].map((item, index) => (
              <Link to={item.to} key={item.title} className="group border-b border-r border-border p-7 hover:bg-muted/50">
                <div className="flex items-center justify-between"><item.icon className="h-6 w-6 text-accent" /><span className="technical-label text-muted-foreground">SYS-0{index + 1}</span></div>
                <h3 className="mt-10 text-3xl group-hover:text-accent">{item.title}</h3>
                <p className="mt-3 text-sm leading-relaxed text-muted-foreground">{item.text}</p>
                <div className="mt-6 flex items-center gap-2 text-xs font-semibold uppercase">Відкрити <ArrowRight className="h-4 w-4 text-accent" /></div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      <section className="py-16 md:py-24">
        <div className="container grid border border-border bg-primary shadow-elevated md:grid-cols-2">
          <div className="border-b border-border p-8 md:border-b-0 md:border-r md:p-12">
            <div className="technical-label mb-4">Онлайн-запис</div>
            <h2 className="text-5xl leading-none md:text-6xl">Оберіть послугу та зручний час</h2>
            <p className="mt-4 max-w-md text-muted-foreground">Система покаже доступні слоти, а автомобіль можна обрати з вашого гаража або записатися без нього.</p>
            <Button asChild size="lg" className="mt-7"><Link to="/booking">Обрати час <ArrowRight /></Link></Button>
          </div>
          <div className="divide-y divide-border">
            {[
              { icon: CheckCircle2, text: "Прозорі ціни" },
              { icon: Gauge, text: "Сучасна діагностика" },
              { icon: Shield, text: "Контроль замовлення" },
              { icon: MapPin, text: "Зручний онлайн-запис" },
            ].map((item) => <div key={item.text} className="flex items-center gap-4 p-5"><item.icon className="h-5 w-5 text-accent" /><span className="text-sm font-semibold uppercase">{item.text}</span></div>)}
          </div>
        </div>
      </section>
    </div>
  );
}
