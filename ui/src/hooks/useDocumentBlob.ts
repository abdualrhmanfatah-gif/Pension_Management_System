import { useState, useEffect } from "react";
import api, { getFileUrl } from "@/api/axios";

/**
 * جلب ملف كـ blob URL مع التنظيف التلقائي.
 * يمر عبر JWT interceptor (authenticated).
 */
export function useDocumentBlob(
    filePath: string | null | undefined,
    options?: { enabled?: boolean }
): { blobUrl: string | null; loading: boolean } {
    const [blobUrl, setBlobUrl] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (!filePath || options?.enabled === false) {
            setBlobUrl(null);
            return;
        }

        let cancelled = false;
        setLoading(true);

        const url = getFileUrl(filePath);
        if (!url) {
            setBlobUrl(null);
            setLoading(false);
            return;
        }

        api.get(url, { responseType: "blob" })
            .then((res) => {
                if (cancelled) return;
                const blob = new Blob([res.data], {
                    type: res.headers["content-type"] || "application/octet-stream",
                });
                const objectUrl = URL.createObjectURL(blob);
                setBlobUrl(objectUrl);
            })
            .catch(() => {
                if (!cancelled) setBlobUrl(null);
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
            setBlobUrl((prev) => {
                if (prev) URL.revokeObjectURL(prev);
                return null;
            });
        };
    }, [filePath, options?.enabled]);

    return { blobUrl, loading };
}
