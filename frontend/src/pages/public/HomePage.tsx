import { Link } from "react-router-dom";

const FEATURES = [
  {
    title: "Kaynaklı Yanıtlar",
    text: "Her cevap, hangi şartname veya kılavuza dayandığını gösterir. Emin olunmayan durumda yanıt uydurulmaz.",
  },
  {
    title: "Anlık Destek Yönlendirmesi",
    text: "Yapay zeka yeterli kanıt bulamazsa sorunuz otomatik olarak Destek Ekibi'ne yönlendirilir.",
  },
  {
    title: "Güncel Bilgi Havuzu",
    text: "İçerik Yöneticileri şartname ve kılavuzları güncel tutar, eski belgeler pasife alınır.",
  },
];

export function HomePage() {
  return (
    <div>
      <section className="mx-auto max-w-3xl px-6 py-24 text-center">
        <h1 className="text-4xl font-semibold sm:text-5xl" style={{ color: "var(--color-text)" }}>
          TEKNOFEST Yarışmacı Asistanı
        </h1>
        <p className="mt-4 text-lg" style={{ color: "var(--color-text-muted)" }}>
          Sorularınızı doğal dille sorun, doğrulanmış kaynaklara dayanan kısa ve güvenilir yanıtlar alın.
        </p>
        <div className="mt-8 flex justify-center gap-3">
          <Link
            to="/register"
            className="rounded-xl px-6 py-3 text-sm font-medium text-white transition-transform hover:-translate-y-0.5"
            style={{ background: "var(--color-accent)" }}
          >
            Hemen Başla
          </Link>
          <Link
            to="/login"
            className="rounded-xl border px-6 py-3 text-sm transition-transform hover:-translate-y-0.5"
            style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
          >
            Giriş Yap
          </Link>
        </div>
      </section>

      <section className="mx-auto max-w-4xl px-6 pb-24">
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
          {FEATURES.map((f) => (
            <div
              key={f.title}
              className="rounded-2xl border p-6 transition-all duration-300 hover:-translate-y-1"
              style={{ borderColor: "var(--color-border)", background: "var(--color-bg-subtle)" }}
            >
              <h3 className="mb-2 text-sm font-semibold" style={{ color: "var(--color-text)" }}>
                {f.title}
              </h3>
              <p className="text-sm" style={{ color: "var(--color-text-muted)" }}>
                {f.text}
              </p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
