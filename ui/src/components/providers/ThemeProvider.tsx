<<<<<<< HEAD
import { type ReactNode,useEffect } from "react";
=======
import { useEffect, type ReactNode } from "react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

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
