import { Link } from "react-router-dom";

const STEPS = [
  { title: "1. Yarışmanızı seçin", text: "Giriş yaptıktan sonra soracağınız yarışmayı sol menüden seçin." },
  {
    title: "2. Sorunuzu yazın",
    text: "Şartname, kılavuz veya kurallarla ilgili sorunuzu doğal dille yazın.",
  },
  {
    title: "3. Kaynaklı yanıtı görün",
    text: "Yanıt hangi belgeye dayandığını ve güven seviyesini gösterir.",
  },
  {
    title: "4. Gerekirse insana yönlenir",
    text: "Yeterli kanıt yoksa sorunuz otomatik olarak Destek Ekibi'ne iletilir; çözüldüğünde bildirim alırsınız.",
  },
];

export function SupportInfoPage() {
  return (
    <section className="mx-auto max-w-2xl px-6 py-16">
      <h1 className="mb-4 text-2xl font-semibold" style={{ color: "var(--color-text)" }}>
        Destek Nasıl Çalışır?
      </h1>
      <div className="space-y-4">
        {STEPS.map((s) => (
          <div key={s.title} className="rounded-xl border p-4" style={{ borderColor: "var(--color-border)" }}>
            <h3 className="mb-1 text-sm font-medium" style={{ color: "var(--color-text)" }}>
              {s.title}
            </h3>
            <p className="text-sm" style={{ color: "var(--color-text-muted)" }}>
              {s.text}
            </p>
          </div>
        ))}
      </div>
      <Link
        to="/login"
        className="mt-6 inline-block rounded-lg px-5 py-2.5 text-sm font-medium text-white"
        style={{ background: "var(--color-accent)" }}
      >
        Giriş Yap ve Soru Sor
      </Link>
    </section>
  );
}
