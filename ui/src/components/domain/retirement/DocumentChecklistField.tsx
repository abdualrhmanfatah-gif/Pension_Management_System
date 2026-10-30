import { CheckCircle2, FileText } from "lucide-react";

import { FormField, Text } from "@/components/ui";
import { cn } from "@/lib/utils";
import type { MissingDocOption } from "@/utils/missing-documents";

interface DocumentChecklistFieldProps {
    /** Currently selected document IDs */
    selectedIds: (string | number)[];
    /** Available documents to pick from */
    documents: MissingDocOption[];
    /** Called when the selection changes */
    onChange: (ids: (string | number)[]) => void;
    /** Validation error message */
    error?: string;
    /** Label text */
    label?: string;
    /** Empty state message */
    emptyMessage?: string;
}

/**
 * Shared checklist for selecting missing documents.
 * Used by PledgeDialog and ExemptionDialog.
 */
export function DocumentChecklistField({
    selectedIds,
    documents,
    onChange,
    error,
    label = "اختر الوثائق الناقصة",
    emptyMessage = "لا توجد وثائق ناقصة متاحة لهذا الملف",
}: DocumentChecklistFieldProps) {
    const toggle = (id: string | number) => {
        if (selectedIds.includes(id)) {
            onChange(selectedIds.filter((sid) => sid !== id));
        } else {
            onChange([...selectedIds, id]);
        }
    };

    return (
        <FormField label={label} required error={error}>
<<<<<<< HEAD
            <div className="grid grid-cols-1 md:grid-cols-2 gap-2 border border-border rounded-2xl p-3 max-h-48 overflow-y-auto bg-background/30  custom-scrollbar">
=======
            <div className="grid grid-cols-1 md:grid-cols-2 gap-2 border border-border/40 rounded-2xl p-3 max-h-48 overflow-y-auto bg-background/30  custom-scrollbar">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                {documents.length === 0 ? (
                    <Text variant="caption" className="col-span-full text-center py-6 italic font-medium">
                        {emptyMessage}
                    </Text>
                ) : (
                    documents.map((doc) => {
                        const isSelected = selectedIds.includes(doc.id);
                        return (
                            <div
                                key={String(doc.id)}
                                onClick={() => toggle(doc.id)}
                                className={cn(
                                    "flex items-center gap-2.5 p-2.5 rounded-xl border transition-all cursor-pointer",
                                    isSelected
<<<<<<< HEAD
                                        ? "bg-primary/5 border-primary text-primary"
=======
                                        ? "bg-primary/5 border-primary/30 text-primary"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        : "bg-background hover:bg-muted border-transparent text-muted-foreground"
                                )}
                            >
                                <div
                                    className={cn(
                                        "w-4 h-4 rounded border flex items-center justify-center transition-all",
                                        isSelected ? "bg-primary border-primary" : "border-border bg-white"
                                    )}
                                >
                                    {isSelected && <CheckCircle2 className="w-3 h-3 text-white animate-in zoom-in-50" />}
                                </div>
                                <div className="flex flex-col gap-0.5">
                                    <span className={cn("text-xs font-bold", isSelected && "text-primary")}>
                                        {doc.name}
                                    </span>
                                    {doc.entityName && (
<<<<<<< HEAD
                                        <span className="text-[9px] text-muted-foreground font-medium opacity-100 flex items-center gap-1">
=======
                                        <span className="text-[9px] text-muted-foreground font-medium opacity-80 flex items-center gap-1">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            <FileText className="w-2.5 h-2.5" />
                                            {doc.entityName}
                                        </span>
                                    )}
                                </div>
                            </div>
                        );
                    })
                )}
            </div>
        </FormField>
    );
}
