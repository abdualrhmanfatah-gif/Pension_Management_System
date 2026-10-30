<<<<<<< HEAD
import { useEffect, useState } from "react";

import api, { getFileRelativeUrl } from "@/api/axios";
=======
import { useState, useEffect } from "react";
import api, { getFileUrl } from "@/api/axios";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

/**
 * جلب ملف كـ blob URL مع التنظيف التلقائي.
 * يمر عبر JWT interceptor (authenticated).
<<<<<<< HEAD
 *
 * الجلب بمسار نسبي (baseURL: "") فيبقى الطلب same-origin عبر Vite proxy
 * في التطوير و URL Rewrite في الإنتاج: بلا preflight وبلا CORS.
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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

<<<<<<< HEAD
        const url = getFileRelativeUrl(filePath);
=======
        const url = getFileUrl(filePath);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (!url) {
            setBlobUrl(null);
            setLoading(false);
            return;
        }

<<<<<<< HEAD
        api.get(url, { baseURL: "", responseType: "blob" })
=======
        api.get(url, { responseType: "blob" })
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
