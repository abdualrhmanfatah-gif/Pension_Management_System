import type { RetirementFileListItem } from "@/types/retirement";

/** Absent document data must not imply a missing document. */
export function retirementFileCardClassName(file: Pick<RetirementFileListItem, "hasDocuments" | "missingDocumentsCount">) {
    if (file.hasDocuments === false) {
        return "max-sm:bg-[color:color-mix(in_srgb,hsl(var(--gov-danger))_10%,hsl(var(--card)))]";
    }
    if ((file.missingDocumentsCount ?? 0) > 0) {
        return "max-sm:bg-[color:color-mix(in_srgb,hsl(var(--gov-warning))_10%,hsl(var(--card)))]";
    }
    if (file.hasDocuments === true) {
        return "max-sm:bg-[color:color-mix(in_srgb,hsl(var(--gov-success))_10%,hsl(var(--card)))]";
    }
    return undefined;
}
