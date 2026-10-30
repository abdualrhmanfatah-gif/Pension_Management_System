export interface StopEntitledPersonRequest {
    stopCode: string;
    notes?: string;
    document?: File | null;
}

export interface ReactivateEntitledPersonRequest {
    reactivationCode: string;
    document?: File | null;
    civilServiceProof?: File | null;
}

export interface RecordEntitledPersonDeathRequest {
    deathDate: string;
    deathCertificate: File;
}

// The shared Axios client defaults to JSON; override it for every multipart request.
export const entitledPersonMultipart = { headers: { "Content-Type": "multipart/form-data" } };

export function entitledPersonFormData(values: object): FormData {
    const form = new FormData();
    for (const [key, value] of Object.entries(values) as [string, unknown][]) {
        if (value instanceof Blob) form.append(key, value);
        else if (typeof value === "string" || typeof value === "number" || typeof value === "boolean")
            form.append(key, String(value));
    }
    return form;
}

export function stopEntitledPersonForm(data: StopEntitledPersonRequest): FormData {
    return entitledPersonFormData({ stopCode: data.stopCode, notes: data.notes, document: data.document });
}

export function reactivateEntitledPersonForm(data: ReactivateEntitledPersonRequest): FormData {
    return entitledPersonFormData({ reactivationCode: data.reactivationCode, document: data.document,
        civilServiceProof: data.civilServiceProof });
}

export function recordEntitledPersonDeathForm(data: RecordEntitledPersonDeathRequest): FormData {
    return entitledPersonFormData({ deathDate: data.deathDate, deathCertificate: data.deathCertificate });
}
