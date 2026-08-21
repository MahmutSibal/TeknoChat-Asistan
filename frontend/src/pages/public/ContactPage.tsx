export function ContactPage() {
  return (
    <section className="mx-auto max-w-2xl px-6 py-16">
      <h1 className="mb-4 text-2xl font-semibold" style={{ color: "var(--color-text)" }}>
        İletişim
      </h1>
      <p className="mb-6 text-sm" style={{ color: "var(--color-text-muted)" }}>
        Sorularınız veya geri bildirimleriniz için bize ulaşabilirsiniz.
      </p>
      <div className="space-y-3">
        <a
          href="mailto:mahmutsibal9@gmail.com"
          className="block rounded-xl border p-4 text-sm hover:opacity-80"
          style={{ borderColor: "var(--color-border)", color: "var(--color-text)" }}
        >
          <span className="font-medium">E-posta:</span> mahmutsibal9@gmail.com
        </a>
        <div className="rounded-xl border p-4 text-sm" style={{ borderColor: "var(--color-border)", color: "var(--color-text-muted)" }}>
          Yarışma şartnamesiyle ilgili sorularınız için giriş yaptıktan sonra <strong>Sohbet</strong> ekranını
          kullanmanızı öneririz — yanıt yalnızca doğrulanmış kaynaklara dayanır ve gerekirse otomatik olarak
          Destek Ekibi'ne yönlendirilir.
        </div>
      </div>
    </section>
  );
}
