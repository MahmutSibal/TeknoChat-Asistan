export function AboutPage() {
  return (
    <section className="mx-auto max-w-2xl px-6 py-16">
      <h1 className="mb-4 text-2xl font-semibold" style={{ color: "var(--color-text)" }}>
        Hakkında
      </h1>
      <div className="space-y-4 text-sm leading-relaxed" style={{ color: "var(--color-text-muted)" }}>
        <p>
          TeknoChat, TEKNOFEST yarışmacılarının şartname, kılavuz ve sık sorulan sorular gibi kaynaklarda
          kaybolmadan hızlıca doğru bilgiye ulaşmasını sağlamak için geliştirilmiş bir yapay zeka destekli
          asistandır.
        </p>
        <p>
          Sistem, sorularınızı yalnızca doğrulanmış ve güncel kaynaklara dayanarak yanıtlar; yeterli kanıt
          bulunamayan durumlarda yanıt uydurmak yerine sorunuzu doğrudan Destek Ekibi'ne yönlendirir.
        </p>
        <p>
          Platform dört farklı role hizmet verir: sorularını soran Yarışmacılar, kaynak havuzunu güncel
          tutan İçerik Yöneticileri, insana yönlenen soruları çözen Destek Ekibi ve sistemi izleyen Sistem
          Yöneticileri.
        </p>
      </div>
    </section>
  );
}
