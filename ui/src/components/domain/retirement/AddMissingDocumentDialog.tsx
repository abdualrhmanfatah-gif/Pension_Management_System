import { CheckCircle2,Info, Search } from "lucide-react";
import { useCallback, useEffect,useMemo, useState } from "react";

import { documentsApi } from "@/api/documents";
import { 
  BaseFormDialog, 
  FormSection, 
  Input, 
  NoteBox,
  Text
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { cn } from "@/lib/utils";

interface AddMissingDocumentModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSaved?: () => void;
    fileId: number;
    docTypes: { id: number; name: string }[];
    existingMissingTypeIds: number[];
}

/**
 * Standardized AddMissingDocumentDialog.
 * Refactored for Batch 2: Strictly uses BaseFormDialog, FormSection, and standardized spacing.
 */
const DRAFT_KEY = "missing_docs_draft";

export function AddMissingDocumentDialog({ 
    open, 
    onOpenChange, 
    onSaved,
    fileId,
    docTypes,
    existingMissingTypeIds
}: AddMissingDocumentModalProps) {
    const [selectedIds, setSelectedIds] = useState<number[]>(() => {
        const saved = sessionStorage.getItem(DRAFT_KEY);
        if (saved) {
            try {
                return JSON.parse(saved);
            } catch { /* ignore */ }
        }
        return [];
    });
    const [searchTerm, setSearchTerm] = useState("");

    // Save draft on change
    useEffect(() => {
        sessionStorage.setItem(DRAFT_KEY, JSON.stringify(selectedIds));
    }, [selectedIds]);

    const mutation = useApiMutation(
        async (ids: number[]) => {
            for (const typeId of ids) {
                await documentsApi.createMissing({
                    fileID: fileId,
                    documentTypeID: typeId,
                });
            }
        },
        {
            successMessage: "تم تسجيل النواقص بنجاح",
            onSuccess: () => {
                sessionStorage.removeItem(DRAFT_KEY);
                onSaved?.();
                onOpenChange(false);
            }
        }
    );

    const filteredTypes = useMemo(() => {
        return docTypes.filter(t => {
            const alreadyMissing = existingMissingTypeIds.some(id => Number(id) === Number(t.id));
            const matchesSearch = t?.name?.toLowerCase().includes(searchTerm.toLowerCase());
            return matchesSearch && !alreadyMissing;
        });
    }, [docTypes, existingMissingTypeIds, searchTerm]);

    const toggleSelection = (id: number) => {
        setSelectedIds(prev => 
            prev.includes(id) ? prev.filter(i => i !== id) : [...prev, id]
        );
    };

    const handleFormSubmit = async () => {
        if (selectedIds.length === 0) return;
        mutation.mutate(selectedIds);
    };

    const handleReset = useCallback(() => {
        setSelectedIds([]);
        setSearchTerm("");
    }, []);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="إضافة وثائق ناقصة للملف"
            description="اختر الوثائق غير المتوفرة في الملف التقاعدي حالياً لتعريفها كمتطلبات ناقصة"
            size="full"
            isLoading={mutation.isPending}
            onSubmit={handleFormSubmit}
            resetOnOpen
            reset={handleReset}
            submitLabel={`تسجيل النواقص المختارة (${selectedIds.length})`}
        >
            <div className="space-y-4">
                <FormSection title="البحث والاختيار" icon={Search}>
                    <div className="space-y-4">
                        <div className="relative group">
                            <Search className="absolute end-3 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground group-focus-within:text-primary transition-colors" />
                            <Input 
                                placeholder="ابحث عن اسم الوثيقة (مثال: شهادة، بطاقة...)" 
                                className="pe-10 h-12 rounded-xl border-border/60 focus:border-primary"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                            />
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3 max-h-[50vh] sm:max-h-[400px] overflow-y-auto p-1 custom-scrollbar">
                            {filteredTypes.length === 0 ? (
                                <div className="col-span-full py-12 flex flex-col items-center justify-center text-center bg-muted/20 rounded-2xl border-2 border-dashed border-border/40">
                                    <div className="p-4 bg-muted rounded-full mb-3">
                                        <Info className="w-8 h-8 text-muted-foreground/40" />
                                    </div>
                                    <Text variant="description" className="font-medium max-w-[200px]">
                                        {searchTerm ? "لا توجد وثائق تطابق البحث" : "لا توجد وثائق إضافية متاحة للتسجيل كمتطلبات ناقصة"}
                                    </Text>
                                </div>
                            ) : (
                                filteredTypes.map(t => {
                                    const isSelected = selectedIds.includes(t.id);
                                    return (
                                        <div 
                                            key={t.id}
                                            className={cn(
                                                "flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-all duration-200 group relative overflow-hidden",
                                                isSelected 
                                                    ? "border-primary bg-primary/5 ring-1 ring-primary/20" 
                                                    : "border-transparent bg-muted/30 hover:bg-muted/60 hover:border-border/60"
                                            )}
                                            onClick={() => toggleSelection(t.id)}
                                        >
                                            <div className={cn(
                                                "w-5 h-5 rounded border-2 flex items-center justify-center transition-all",
                                                isSelected ? "bg-primary border-primary" : "border-muted-foreground/30 bg-background"
                                            )}>
                                                {isSelected && <CheckCircle2 className="w-3.5 h-3.5 text-white" />}
                                            </div>
                                            <span className={cn(
                                                "text-sm font-bold transition-colors",
                                                isSelected ? "text-primary" : "text-foreground/70"
                                            )}>
                                                {t.name}
                                            </span>
                                            
                                            {/* Decorative indicator for selection */}
                                            {isSelected && (
                                                <div className="absolute start-0 top-0 bottom-0 w-1 bg-primary" />
                                            )}
                                        </div>
                                    );
                                })
                            )}
                        </div>
                    </div>
                </FormSection>

                <NoteBox type="info" title="توجيه">
                    سيتم إدراج الوثائق المختارة في قائمة المتطلبات باللون الأحمر، مما ينبه الموظفين لضرورة استكمالها لاحقاً.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
