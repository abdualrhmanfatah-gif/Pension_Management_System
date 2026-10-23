/**
 * مطابقة رد 409 مع طلب الرفع (R3) — دوال خالصة قابلة للاختبار.
 * القاعدة: لا قبول 409 كنجاح إلا بمطابقة FileID + DocumentTypeID + IDs المالك.
 */

export interface DuplicateOwnerIds {
    agentId: number | null;
    guardianId: number | null;
    retireeId: number | null;
    entitledId: number | null;
}

export interface DuplicateUploadRequest {
    fileId: number;
    documentTypeId: number;
    owner: DuplicateOwnerIds;
}

export interface DuplicateDocumentPayload {
    documentId?: number;
    fileId?: number | null;
    documentTypeID?: number | null;
    agentId?: number | null;
    guardianId?: number | null;
    retireeId?: number | null;
    entitledId?: number | null;
    entityName?: string | null;
    entityType?: string | null;
}

/** تحويل قيمة ownerEntity في القائمة ("retiree" | "agent_5" | ...) إلى IDs المالك */
export function parseOwnerEntity(ownerEntity: string): DuplicateOwnerIds {
    const empty: DuplicateOwnerIds = { agentId: null, guardianId: null, retireeId: null, entitledId: null };
    if (!ownerEntity || ownerEntity === "retiree") return empty;
    const sep = ownerEntity.indexOf("_");
    if (sep < 0) return empty;
    const kind = ownerEntity.slice(0, sep);
    const id = Number(ownerEntity.slice(sep + 1));
    if (!Number.isInteger(id) || id <= 0) return empty;
    if (kind === "agent") return { ...empty, agentId: id };
    if (kind === "guardian") return { ...empty, guardianId: id };
    if (kind === "entitled") return { ...empty, entitledId: id };
    return empty;
}

function readNumber(source: Record<string, unknown>, names: string[]): number | null {
    for (const name of names) {
        const value = source[name];
        if (value === undefined) continue;
        if (value === null || value === "") return null;
        const num = typeof value === "number" ? value : Number(String(value));
        if (Number.isFinite(num)) return num;
        return null;
    }
    return null;
}

function readString(source: Record<string, unknown>, names: string[]): string | null {
    for (const name of names) {
        const value = source[name];
        if (typeof value === "string" && value.trim()) return value;
    }
    return null;
}

/** قراءة آمنة لجسم 409 (يدعم camelCase وPascalCase) — ترجع null عند غياب documentID */
export function readDuplicatePayload(data: unknown): DuplicateDocumentPayload | null {
    if (typeof data !== "object" || data === null) return null;
    const source = data as Record<string, unknown>;
    const documentId = readNumber(source, ["documentID", "DocumentID", "documentId"]);
    if (documentId === null) return null;
    return {
        documentId,
        fileId: readNumber(source, ["fileID", "FileID", "fileId"]),
        documentTypeID: readNumber(source, ["documentTypeID", "DocumentTypeID", "documentTypeId"]),
        agentId: readNumber(source, ["agentID", "AgentID", "agentId"]),
        guardianId: readNumber(source, ["guardianID", "GuardianID", "guardianId"]),
        retireeId: readNumber(source, ["retireeID", "RetireeID", "retireeId"]),
        entitledId: readNumber(source, ["entitledID", "EntitledID", "entitledId", "EntitledPersonID", "entitledPersonID"]),
        entityName: readString(source, ["entityName", "EntityName"]),
        entityType: readString(source, ["entityType", "EntityType"]),
    };
}

/** هل الوثيقة المطابقة في رد 409 تطابق نطاق الطلب فعلًا؟ */
export function isDuplicateMatch(request: DuplicateUploadRequest, dup: DuplicateDocumentPayload): boolean {
    return (
        dup.fileId === request.fileId &&
        dup.documentTypeID === request.documentTypeId &&
        dup.agentId === request.owner.agentId &&
        dup.guardianId === request.owner.guardianId &&
        dup.retireeId === request.owner.retireeId &&
        dup.entitledId === request.owner.entitledId
    );
}

/** رسالة خطأ عند تعارض لا يطابق الطلب — تذكر المالك الحقيقي بدل النجاح الصامت */
export function duplicateMismatchMessage(dup: DuplicateDocumentPayload): string {
    if (dup.entityName) {
        return `الوثيقة مرفوعة مسبقًا باسم ${dup.entityName} — تحقق من صاحب الوثيقة قبل إعادة الرفع`;
    }
    return "الوثيقة مكررة ولا تخص هذا المالك";
}
