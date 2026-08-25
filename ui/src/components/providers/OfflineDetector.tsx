import { WifiOff } from "lucide-react";
import { useEffect,useState } from "react";

export function OfflineDetector() {
    const [isOffline, setIsOffline] = useState(!navigator.onLine);

    useEffect(() => {
        const handleOffline = () => setIsOffline(true);
        const handleOnline = () => setIsOffline(false);

        window.addEventListener("offline", handleOffline);
        window.addEventListener("online", handleOnline);

        return () => {
            window.removeEventListener("offline", handleOffline);
            window.removeEventListener("online", handleOnline);
        };
    }, []);

    if (!isOffline) return null;

    return (
        <div className="flex items-center justify-center gap-2 bg-destructive px-4 py-2 text-sm text-destructive-foreground print:hidden">
            <WifiOff className="h-4 w-4 shrink-0" />
            <span>لا يوجد اتصال بالإنترنت — بعض الميزات قد لا تعمل</span>
        </div>
    );
}
