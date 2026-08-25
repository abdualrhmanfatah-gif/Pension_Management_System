import { useEffect, type ReactNode } from "react";

import { useThemeStore } from "@/stores/themeStore";

export function ThemeProvider({ children }: { children: ReactNode }) {
    const isDark = useThemeStore((s) => s.isDark);

    useEffect(() => {
        const root = document.documentElement;
        if (isDark) {
            root.classList.add("dark");
        } else {
            root.classList.remove("dark");
        }
    }, [isDark]);

    return <>{children}</>;
}
