import { apiRequest } from "./client";
import type {
  AppUser,
  AuthResponse,
  Category,
  ChatQueryResponse,
  Competition,
  CompetitionAnalytics,
  FaqEntry,
  ForgotPasswordResponse,
  PagedResult,
  ReembedResult,
  RegistrationPending,
  SourceDocument,
  SourceDocumentType,
  SupportTicket,
  SupportTicketStatus,
  UserRole,
} from "../types/api";

// ---- Auth ----
export const authApi = {
  register: (data: { fullName: string; email: string; password: string; recaptchaToken: string }) =>
    apiRequest<RegistrationPending>("/api/auth/register", { method: "POST", body: data }),
  verifyEmail: (data: { email: string; code: string }) =>
    apiRequest<AuthResponse>("/api/auth/verify-email", { method: "POST", body: data }),
  resendVerification: (data: { email: string }) =>
    apiRequest<void>("/api/auth/resend-verification", { method: "POST", body: data }),
  login: (data: { email: string; password: string; recaptchaToken: string }) =>
    apiRequest<AuthResponse>("/api/auth/login", { method: "POST", body: data }),
  google: (idToken: string) =>
    apiRequest<AuthResponse>("/api/auth/google", { method: "POST", body: { idToken } }),
  forgotPassword: (data: { email: string }) =>
    apiRequest<ForgotPasswordResponse>("/api/auth/forgot-password", { method: "POST", body: data }),
  resetPassword: (data: { email: string; resetToken: string; newPassword: string }) =>
    apiRequest<void>("/api/auth/reset-password", { method: "POST", body: data }),
};

// ---- Competitions ----
export const competitionsApi = {
  list: (pageNumber = 1, pageSize = 50) =>
    apiRequest<PagedResult<Competition>>("/api/competitions", { query: { pageNumber, pageSize } }),
  get: (id: number) => apiRequest<Competition>(`/api/competitions/${id}`),
  create: (data: { name: string; description?: string }) =>
    apiRequest<Competition>("/api/competitions", { method: "POST", body: data }),
  update: (id: number, data: { name: string; description?: string; isActive: boolean }) =>
    apiRequest<Competition>(`/api/competitions/${id}`, { method: "PUT", body: data }),
};

// ---- Categories ----
export const categoriesApi = {
  listByCompetition: (competitionId: number) =>
    apiRequest<Category[]>("/api/categories", { query: { competitionId } }),
  create: (data: { competitionId: number; name: string; description?: string }) =>
    apiRequest<Category>("/api/categories", { method: "POST", body: data }),
};

// ---- Source Documents ----
export const documentsApi = {
  list: (competitionId: number, categoryId?: number, pageNumber = 1, pageSize = 50) =>
    apiRequest<PagedResult<SourceDocument>>("/api/sourcedocuments", {
      query: { competitionId, categoryId, pageNumber, pageSize },
    }),
  createFromText: (data: {
    title: string;
    documentType: SourceDocumentType;
    competitionId: number;
    categoryId?: number;
    fileName?: string;
    content: string;
    uploadedByUserId: number;
  }) => apiRequest<SourceDocument>("/api/sourcedocuments", { method: "POST", body: data }),
  uploadFile: (metadata: {
    Title: string;
    DocumentType: number;
    CompetitionId: number;
    CategoryId?: number;
    UploadedByUserId: number;
  }, file: File) => {
    const form = new FormData();
    Object.entries(metadata).forEach(([key, value]) => {
      if (value !== undefined) form.append(key, String(value));
    });
    form.append("file", file);
    return apiRequest<SourceDocument>("/api/sourcedocuments/upload", { method: "POST", body: form, isFormData: true });
  },
  deactivate: (id: number) => apiRequest<void>(`/api/sourcedocuments/${id}/deactivate`, { method: "POST" }),
  reembedMissing: (competitionId?: number) =>
    apiRequest<ReembedResult>("/api/sourcedocuments/reembed-missing", { method: "POST", query: { competitionId } }),
};

// ---- Chat ----
export const chatApi = {
  ask: (data: { competitionId: number; categoryId?: number; questionText: string; correlationId?: string }) =>
    apiRequest<ChatQueryResponse>("/api/chat/ask", { method: "POST", body: { ...data, userId: null } }),
  myHistory: (competitionId: number) =>
    apiRequest<ChatQueryResponse[]>("/api/chat/my-history", { query: { competitionId } }),
  history: (competitionId: number) =>
    apiRequest<ChatQueryResponse[]>("/api/chat/history", { query: { competitionId } }),
};

// ---- Support Tickets ----
export const ticketsApi = {
  listOpen: (pageNumber = 1, pageSize = 50) =>
    apiRequest<PagedResult<SupportTicket>>("/api/supporttickets/open", { query: { pageNumber, pageSize } }),
  get: (id: number) => apiRequest<SupportTicket>(`/api/supporttickets/${id}`),
  assign: (id: number, assignedToUserId: number) =>
    apiRequest<SupportTicket>(`/api/supporttickets/${id}/assign`, { method: "POST", body: { assignedToUserId } }),
  resolve: (id: number, resolution: string) =>
    apiRequest<SupportTicket>(`/api/supporttickets/${id}/resolve`, { method: "POST", body: { resolution } }),
};

// ---- FAQ ----
export const faqApi = {
  list: (competitionId: number, categoryId?: number, pageNumber = 1, pageSize = 50) =>
    apiRequest<PagedResult<FaqEntry>>("/api/faq", { query: { competitionId, categoryId, pageNumber, pageSize } }),
  create: (data: {
    question: string;
    answer: string;
    competitionId: number;
    categoryId?: number;
    createdByUserId: number;
    sourceChatQueryId?: number;
  }) => apiRequest<FaqEntry>("/api/faq", { method: "POST", body: data }),
};

// ---- Users ----
export const usersApi = {
  list: (pageNumber = 1, pageSize = 50) => apiRequest<PagedResult<AppUser>>("/api/users", { query: { pageNumber, pageSize } }),
  create: (data: { fullName: string; email: string; password: string; role: UserRole }) =>
    apiRequest<AppUser>("/api/users", { method: "POST", body: data }),
};

// ---- Analytics ----
export const analyticsApi = {
  competition: (competitionId: number) =>
    apiRequest<CompetitionAnalytics>(`/api/analytics/competitions/${competitionId}`),
};

export type { SupportTicketStatus };
