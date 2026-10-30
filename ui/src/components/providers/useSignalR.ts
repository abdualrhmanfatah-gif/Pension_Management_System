import type { HubConnection } from "@microsoft/signalr";
import { createContext, useContext } from "react";

export const NotificationContext = createContext<HubConnection | null>(null);

export const useSignalR = () => useContext(NotificationContext);
