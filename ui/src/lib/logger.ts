type LogLevel = "debug" | "info" | "warn" | "error";

interface LogContext {
    component?: string;
    action?: string;
    [key: string]: unknown;
}

const LEVEL_PRIORITY: Record<LogLevel, number> = {
    debug: 0,
    info: 1,
    warn: 2,
    error: 3,
};

function shouldLog(level: LogLevel): boolean {
    const minLevel: LogLevel =
        import.meta.env.PROD ? "warn" : "debug";
    return LEVEL_PRIORITY[level] >= LEVEL_PRIORITY[minLevel];
}

function formatMessage(level: LogLevel, message: string, context?: LogContext): string {
    const prefix = `[${level.toUpperCase()}]`;
    const ctx = context
        ? ` (${Object.entries(context)
              .filter(([, v]) => v !== undefined)
              .map(([k, v]) => `${k}:${v}`)
              .join(", ")})`
        : "";
    return `${prefix}${ctx} ${message}`;
}

export const logger = {
    debug(message: string, context?: LogContext) {
        if (!shouldLog("debug")) return;
        console.debug(formatMessage("debug", message, context));
    },

    info(message: string, context?: LogContext) {
        if (!shouldLog("info")) return;
        console.info(formatMessage("info", message, context));
    },

    warn(message: string, context?: LogContext) {
        if (!shouldLog("warn")) return;
        console.warn(formatMessage("warn", message, context));
    },

    error(message: string, error?: unknown, context?: LogContext) {
        if (!shouldLog("error")) return;
        console.error(formatMessage("error", message, context), error ?? "");

        // Future: send to remote logging service (Sentry, etc.)
        // if (import.meta.env.PROD) { captureException(error, { ...context }); }
    },
};
