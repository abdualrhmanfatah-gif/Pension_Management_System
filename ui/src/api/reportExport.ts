import { getDownloadFilename } from "@/lib/downloadFilename";

import api from "./axios";

/** Download a server-rendered report using its Content-Disposition filename. */
export async function exportReportFile(url: string, params: object, fallbackFilename: string, timeout?: number): Promise<void> {
    const response = await api.get<Blob>(url, { params, responseType: "blob", timeout });
    const disposition = response.headers["content-disposition"];
    const filename = getDownloadFilename(typeof disposition === "string" ? disposition : undefined, fallbackFilename);
    const objectUrl = URL.createObjectURL(response.data);
    const link = document.createElement("a");
    link.href = objectUrl;
    link.download = filename;
    document.body.appendChild(link);
    try {
        link.click();
    } finally {
        link.remove();
        setTimeout(() => URL.revokeObjectURL(objectUrl), 0);
    }
}
