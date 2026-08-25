import { useEffect } from "react";

/**
 * Hook to add Ctrl+Alt+N keyboard shortcut for opening create/add dialogs.
 * @param handler - Function to call when shortcut is triggered
 * @param enabled - Whether the shortcut is enabled (usually a permission check)
 */
export function useCreateShortcut(handler: () => void, enabled: boolean) {
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if ((e.ctrlKey || e.metaKey) && e.altKey && e.code === "KeyN") {
                e.preventDefault();
                e.stopPropagation();
                if (enabled) {
                    handler();
                }
            }
        };
        document.addEventListener("keydown", handleKeyDown, true);
        return () => document.removeEventListener("keydown", handleKeyDown, true);
    }, [handler, enabled]);
}
