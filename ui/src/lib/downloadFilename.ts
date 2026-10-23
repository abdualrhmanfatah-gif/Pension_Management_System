/** Read one filename from Content-Disposition, preferring its UTF-8 form. */
export function getDownloadFilename(contentDisposition: string | null | undefined, fallback: string): string {
    const extended = contentDisposition?.match(/(?:^|;)\s*filename\*\s*=\s*(?:"([^"]*)"|([^;]*))/i);
    const encoded = (extended?.[1] ?? extended?.[2] ?? "").trim().match(/^UTF-8'[^']*'(.*)$/i);
    if (encoded) {
        try {
            const filename = decodeURIComponent(encoded[1]).trim();
            if (filename) return filename;
        } catch {
            // Fall back to the plain filename if percent encoding is malformed.
        }
    }

    const plain = contentDisposition?.match(/(?:^|;)\s*filename\s*=\s*(?:"((?:\\.|[^"\\])*)"|([^;]*))/i);
    const filename = (plain?.[1] ?? plain?.[2] ?? "").replace(/\\(["\\])/g, "$1").trim();
    return filename || fallback;
}
