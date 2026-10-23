import type { QualityFlag } from "./document-scanner/quality";

export const BULK_UPLOAD_RETENTION_MS = 60 * 60 * 1000;

export interface BulkUploadDraftFile {
    id: string;
    file: File;
    fileName: string;
    fileSize: number;
    documentTypeID: number;
    ownerEntity: string;
    notes: string;
    status: "pending" | "uploading" | "success" | "error";
    error?: string;
    documentId?: number;
    needsReview: boolean;
    reviewConfirmed: boolean;
    qualityFlags: QualityFlag[];
}

export interface BulkUploadDraft {
    key: string;
    expiresAt: number;
    files: BulkUploadDraftFile[];
    needsFinalization: boolean;
}

const STORE = "drafts";
let pending: Promise<unknown> = Promise.resolve();
let expiryTimer: ReturnType<typeof setTimeout> | undefined;
let scheduledExpiry = Infinity;

export function bulkUploadDraftKey(userId: number, fileId: number): string {
    return `${userId}:${fileId}`;
}

export function prepareBulkUploadDraft(
    key: string,
    files: BulkUploadDraftFile[],
    needsFinalization: boolean,
    now = Date.now(),
): BulkUploadDraft | null {
    if (!files.length || (!needsFinalization && files.every((file) => file.status === "success"))) return null;
    return {
        key,
        expiresAt: now + BULK_UPLOAD_RETENTION_MS,
        needsFinalization,
        files: files.map((item) => ({
            id: item.id, file: item.file, fileName: item.fileName, fileSize: item.fileSize,
            documentTypeID: item.documentTypeID, ownerEntity: item.ownerEntity, notes: item.notes,
            // A request interrupted by a reload has an unknown outcome; retry through server deduplication.
            status: item.status === "uploading" ? "pending" : item.status,
            error: item.error, documentId: item.documentId,
            needsReview: item.needsReview, reviewConfirmed: item.reviewConfirmed,
            qualityFlags: [...item.qualityFlags],
        })),
    };
}

function openDatabase(): Promise<IDBDatabase> {
    return new Promise((resolve, reject) => {
        if (typeof indexedDB === "undefined") {
            reject(new Error("التخزين المحلي غير متاح في هذا المتصفح"));
            return;
        }
        const request = indexedDB.open("pensionms-bulk-upload", 1);
        request.onupgradeneeded = () => request.result.createObjectStore(STORE, { keyPath: "key" });
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
        request.onblocked = () => reject(new Error("تعذر فتح التخزين المحلي"));
    });
}

// Serialize transactions, including reads, so a quick close/reopen sees the last queued save.
function transaction<T>(run: (store: IDBObjectStore, complete: (value: T) => void) => void): Promise<T> {
    const operation = pending.catch(() => undefined).then(async () => {
        const database = await openDatabase();
        return new Promise<T>((resolve, reject) => {
            const tx = database.transaction(STORE, "readwrite");
            let result: T;
            tx.oncomplete = () => { database.close(); resolve(result); };
            tx.onabort = () => { database.close(); reject(tx.error); };
            tx.onerror = () => { database.close(); reject(tx.error); };
            run(tx.objectStore(STORE), (value) => { result = value; });
        });
    });
    pending = operation;
    return operation;
}

function purgeExpired(store: IDBObjectStore, now: number): void {
    let nextExpiry = Infinity;
    const cursor = store.openCursor();
    cursor.onsuccess = () => {
        const entry = cursor.result;
        if (!entry) {
            if (Number.isFinite(nextExpiry)) scheduleExpiry(nextExpiry);
            return;
        }
        const expiresAt = (entry.value as BulkUploadDraft).expiresAt;
        if (expiresAt <= now) entry.delete();
        else nextExpiry = Math.min(nextExpiry, expiresAt);
        entry.continue();
    };
}

function scheduleExpiry(expiresAt: number): void {
    if (scheduledExpiry <= expiresAt) return;
    if (expiryTimer) clearTimeout(expiryTimer);
    scheduledExpiry = expiresAt;
    expiryTimer = setTimeout(() => {
        scheduledExpiry = Infinity;
        expiryTimer = undefined;
        void transaction<void>((store, complete) => {
            purgeExpired(store, Date.now());
            complete(undefined);
        }).catch(() => undefined);
    }, Math.max(0, expiresAt - Date.now()));
}

export function loadBulkUploadDraft(key: string): Promise<BulkUploadDraft | null> {
    return transaction<BulkUploadDraft | null>((store, complete) => {
        const now = Date.now();
        purgeExpired(store, now);
        const request = store.get(key);
        request.onsuccess = () => {
            const draft = request.result as BulkUploadDraft | undefined;
            if (!draft || draft.expiresAt <= now) { complete(null); return; }
            scheduleExpiry(draft.expiresAt);
            complete(draft);
        };
    });
}

export function saveBulkUploadDraft(draft: BulkUploadDraft | null, key: string): Promise<void> {
    return transaction<void>((store, complete) => {
        purgeExpired(store, Date.now());
        if (draft) {
            store.put(draft);
            scheduleExpiry(draft.expiresAt);
        } else store.delete(key);
        complete(undefined);
    });
}
