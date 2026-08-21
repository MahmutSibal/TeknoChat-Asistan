import { AnswerMode, ConfidenceLevel, SupportTicketStatus } from "../types/api";

export const confidenceLabels: Record<ConfidenceLevel, string> = {
  [ConfidenceLevel.Yetersiz]: "Yetersiz",
  [ConfidenceLevel.Dusuk]: "Düşük",
  [ConfidenceLevel.Orta]: "Orta",
  [ConfidenceLevel.Yuksek]: "Yüksek",
};

export const confidenceColors: Record<ConfidenceLevel, string> = {
  [ConfidenceLevel.Yetersiz]: "#9ca3af",
  [ConfidenceLevel.Dusuk]: "#f59e0b",
  [ConfidenceLevel.Orta]: "#3b82f6",
  [ConfidenceLevel.Yuksek]: "#22c55e",
};

export const answerModeLabels: Record<AnswerMode, string> = {
  [AnswerMode.YapayZeka]: "Yapay Zeka Yanıtı",
  [AnswerMode.TemelArama]: "Temel Arama Modu",
};

export const ticketStatusLabels: Record<SupportTicketStatus, string> = {
  [SupportTicketStatus.Acik]: "Açık",
  [SupportTicketStatus.Islemde]: "İşlemde",
  [SupportTicketStatus.Cozuldu]: "Çözüldü",
  [SupportTicketStatus.Kapatildi]: "Kapatıldı",
};

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString("tr-TR", { dateStyle: "medium", timeStyle: "short" });
}
