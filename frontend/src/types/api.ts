// Backend enum'larıyla birebir eşleşir (TeknofestAsistan.Domain.Enums)

export enum UserRole {
  Yarismaci = 0,
  IcerikYoneticisi = 1,
  DestekEkibi = 2,
  SistemYoneticisi = 3,
}

export enum SourceDocumentType {
  Sartname = 0,
  Kilavuz = 1,
  Sss = 2,
  Diger = 3,
}

export enum ConfidenceLevel {
  Yetersiz = 0,
  Dusuk = 1,
  Orta = 2,
  Yuksek = 3,
}

export enum SupportTicketStatus {
  Acik = 0,
  Islemde = 1,
  Cozuldu = 2,
  Kapatildi = 3,
}

export enum AnswerMode {
  YapayZeka = 0,
  TemelArama = 1,
  ClaudeBulut = 2,
}

export interface SystemStatus {
  ollama: boolean;
  claudeBulut: boolean;
  temelArama: boolean;
}

export interface AuthResponse {
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
  token: string;
  expiresAt: string;
}

export interface RegistrationPending {
  email: string;
  message: string;
}

export interface ForgotPasswordResponse {
  message: string;
}

export interface Competition {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface Category {
  id: number;
  competitionId: number;
  name: string;
  description: string | null;
}

export interface SourceDocument {
  id: number;
  title: string;
  documentType: SourceDocumentType;
  competitionId: number;
  categoryId: number | null;
  fileName: string | null;
  uploadedByUserId: number;
  validFrom: string;
  validUntil: string | null;
  isActive: boolean;
  version: number;
  createdAt: string;
}

export interface Citation {
  sourceDocumentId: number;
  sourceTitle: string;
  relevanceScore: number;
}

export interface ChatQueryResponse {
  id: number;
  questionText: string;
  answerText: string | null;
  confidenceLevel: ConfidenceLevel;
  isEscalated: boolean;
  citations: Citation[];
  supportTicketStatus: SupportTicketStatus | null;
  supportResolution: string | null;
  escalationReason: string | null;
  answerMode: AnswerMode | null;
}

export interface SupportTicket {
  id: number;
  chatQueryId: number;
  questionText: string;
  competitionId: number;
  assignedToUserId: number | null;
  status: SupportTicketStatus;
  resolution: string | null;
  createdAt: string;
  resolvedAt: string | null;
}

export interface FaqEntry {
  id: number;
  question: string;
  answer: string;
  competitionId: number;
  categoryId: number | null;
  isActive: boolean;
  createdAt: string;
}

export interface AppUser {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface ConfidenceBucket {
  level: ConfidenceLevel;
  count: number;
}

export interface TopQuestion {
  questionText: string;
  count: number;
}

export interface CompetitionAnalytics {
  competitionId: number;
  totalQuestions: number;
  escalatedCount: number;
  escalationRatePercent: number;
  confidenceDistribution: ConfidenceBucket[];
  topQuestions: TopQuestion[];
  openSupportTickets: number;
  resolvedSupportTickets: number;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ReembedResult {
  chunksFixed: number;
}

export interface ApiErrorBody {
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
