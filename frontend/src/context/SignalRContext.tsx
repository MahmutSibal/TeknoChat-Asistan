import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from "react";
import * as signalR from "@microsoft/signalr";
import { useAuth } from "./AuthContext";
import { getAuthToken } from "../api/client";

interface AnswerChunkEvent {
  correlationId: string;
  chunk: string;
  isFinal: boolean;
}

interface TicketResolvedEvent {
  chatQueryId: number;
  resolution: string;
}

interface NewTicketEvent {
  ticketId: number;
  questionText: string;
  competitionId: number;
}

interface SignalRContextValue {
  onAnswerChunk: (handler: (e: AnswerChunkEvent) => void) => () => void;
  onTicketResolved: (handler: (e: TicketResolvedEvent) => void) => () => void;
  onNewTicket: (handler: (e: NewTicketEvent) => void) => () => void;
}

const SignalRContext = createContext<SignalRContextValue | undefined>(undefined);

const HUB_URL = `${import.meta.env.VITE_API_BASE_URL}/hubs/notifications`;

type Listener<T> = (event: T) => void;

export function SignalRProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const chunkListeners = useRef(new Set<Listener<AnswerChunkEvent>>());
  const resolvedListeners = useRef(new Set<Listener<TicketResolvedEvent>>());
  const newTicketListeners = useRef(new Set<Listener<NewTicketEvent>>());

  useEffect(() => {
    if (!isAuthenticated) {
      connectionRef.current?.stop();
      connectionRef.current = null;
      return;
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => getAuthToken() ?? "" })
      .withAutomaticReconnect()
      .build();

    connection.on("AnswerChunk", (e: AnswerChunkEvent) => chunkListeners.current.forEach((fn) => fn(e)));
    connection.on("TicketResolved", (e: TicketResolvedEvent) => resolvedListeners.current.forEach((fn) => fn(e)));
    connection.on("NewTicketCreated", (e: NewTicketEvent) => newTicketListeners.current.forEach((fn) => fn(e)));

    connection.start().catch((err) => console.error("SignalR bağlantı hatası:", err));
    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [isAuthenticated]);

  const value: SignalRContextValue = {
    onAnswerChunk: (handler) => {
      chunkListeners.current.add(handler);
      return () => chunkListeners.current.delete(handler);
    },
    onTicketResolved: (handler) => {
      resolvedListeners.current.add(handler);
      return () => resolvedListeners.current.delete(handler);
    },
    onNewTicket: (handler) => {
      newTicketListeners.current.add(handler);
      return () => newTicketListeners.current.delete(handler);
    },
  };

  return <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>;
}

export function useSignalR(): SignalRContextValue {
  const ctx = useContext(SignalRContext);
  if (!ctx) throw new Error("useSignalR, SignalRProvider içinde kullanılmalı");
  return ctx;
}

export function useToasts() {
  const [toasts, setToasts] = useState<{ id: number; text: string }[]>([]);
  const nextId = useRef(0);

  const push = (text: string) => {
    const id = nextId.current++;
    setToasts((prev) => [...prev, { id, text }]);
    setTimeout(() => setToasts((prev) => prev.filter((t) => t.id !== id)), 6000);
  };

  return { toasts, push };
}
