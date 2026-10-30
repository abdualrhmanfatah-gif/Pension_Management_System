<<<<<<< HEAD
import { useQuery,useQueryClient } from "@tanstack/react-query";
import { Link as LinkIcon, Loader2,Search } from "lucide-react";
import { useEffect,useState } from "react";

import { retireeApi } from "@/api/retirees";
import type { RetireeListItem } from "@/api/retirees";
=======
import { useQuery } from "@tanstack/react-query";
import { Link as LinkIcon, Loader2,Search } from "lucide-react";
import { useEffect,useState } from "react";

import { retirementApi } from "@/api/retirement";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type { SettlementItemDto } from "@/api/settlements";
import { settlementApi } from "@/api/settlements";
import { Badge,BaseFormDialog, Button, Input, Text } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { cn } from "@/lib/utils";
<<<<<<< HEAD
=======
import type { RetirementFileListItem } from "@/types/retirement";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface LinkRetireeDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    item: SettlementItemDto | null;
    onLinked: () => void;
}

export function LinkRetireeDialog({ open, onOpenChange, item, onLinked }: LinkRetireeDialogProps) {
<<<<<<< HEAD
    const queryClient = useQueryClient();
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    const [searchTerm, setSearchTerm] = useState("");
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [selectedRetireeId, setSelectedRetireeId] = useState<number | null>(null);

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(searchTerm);
        }, 500);
        return () => clearTimeout(timer);
    }, [searchTerm]);

    // Reset state when opening a new item
    useEffect(() => {
        if (open) {
            const timer = setTimeout(() => {
                setSearchTerm("");
                setDebouncedSearch("");
<<<<<<< HEAD
                setSelectedRetireeId(item?.retireeID ?? null);
=======
                setSelectedRetireeId(null);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            }, 0);
            return () => clearTimeout(timer);
        }
    }, [open, item]);

    const { data: searchResults, isLoading: isSearching } = useQuery({
        queryKey: ["retirees-search", debouncedSearch],
<<<<<<< HEAD
        queryFn: () => retireeApi.getAll({ page: 1, pageSize: 15, search: debouncedSearch, isProcessed: true }),
=======
        queryFn: () => retirementApi.getFiles({ page: 1, pageSize: 15, search: debouncedSearch }),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        enabled: open && debouncedSearch.length > 1,
    });

    const linkMutation = useApiMutation(
        async () => {
            if (!item || !selectedRetireeId) return;
            
<<<<<<< HEAD
            await settlementApi.linkItem({
                settlementItemID: item.settlementItemID,
                retireeID: selectedRetireeId
=======
            // selectedRetireeId is actually the fileID from the search results
            // We need to fetch the file details to get the actual retireeID
            const fileRes = await retirementApi.getFile(selectedRetireeId);
            const actualRetireeId = fileRes.retireeID;
            
            if (!actualRetireeId) throw new Error("لا يوجد متقاعد مرتبط بهذا الملف");

            await settlementApi.linkItem({
                settlementItemID: item.settlementItemID,
                retireeID: actualRetireeId
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            });
        },
        {
            successMessage: "تم ربط المتقاعد بنجاح",
            onSuccessCallback: () => {
<<<<<<< HEAD
                if (selectedRetireeId) {
                    queryClient.invalidateQueries({ queryKey: ["financial-entitlements", selectedRetireeId] });
                    queryClient.invalidateQueries({ queryKey: ["active-entitlement", selectedRetireeId] });
                    queryClient.invalidateQueries({ queryKey: ["retiree-statement", String(selectedRetireeId)] });
                    queryClient.invalidateQueries({ queryKey: ["settlements", selectedRetireeId] });
                    queryClient.invalidateQueries({ queryKey: ["retiree", selectedRetireeId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file"] });
                    queryClient.invalidateQueries({ queryKey: ["financial-entitlements-search"] });
                }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                onLinked();
                onOpenChange(false);
            }
        }
    );

    const handleLink = () => {
        if (!selectedRetireeId) return;
        linkMutation.mutate({}); // Passing empty object to satisfy type requirement
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="ربط قيد بملف متقاعد"
            size="md"
            footer={
                <div className="flex justify-end gap-2 w-full">
                    <Button variant="outline" onClick={() => onOpenChange(false)}>إلغاء</Button>
                    <Button
                        className="gap-2"
                        onClick={handleLink}
                        loading={linkMutation.isPending}
                        disabled={!selectedRetireeId}
                    >
                        <LinkIcon className="w-4 h-4" />
                        اعتماد الربط
                    </Button>
                </div>
            }
        >
            <div className="space-y-6" dir="rtl">
                {/* Item Info */}
                {item && (
<<<<<<< HEAD
                    <div className="bg-muted/30 p-4 rounded-xl border border-primary">
=======
                    <div className="bg-muted/30 p-4 rounded-xl border border-primary/10">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <h4 className="text-sm font-bold text-muted-foreground mb-3 border-b pb-2">بيانات القيد في القرار</h4>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm">
                            <div>
                                <Text variant="caption" className="mb-1">الاسم كما ورد في القرار</Text>
                                <p className="font-bold">{item.retireeFullName}</p>
                            </div>
                            <div>
                                <Text variant="caption" className="mb-1">الرقم العسكري</Text>
                                <p className="font-mono">{item.militaryNumber || "غير متوفر"}</p>
                            </div>
                        </div>
<<<<<<< HEAD
                        {item.retireeID && <p className="mt-3 text-xs text-muted-foreground">المتقاعد المرتبط حاليًا: {item.retireeID}. الاستحقاق المالي مُنشأ بالفعل؛ إعادة الربط بمتقاعد آخر غير مسموح.</p>}
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </div>
                )}

                {/* Search */}
                <div className="space-y-3">
                    <h4 className="text-sm font-bold">البحث عن المتقاعد في النظام</h4>
                    <div className="relative">
                        <Search className="absolute end-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                        <Input
                            placeholder="ابحث بالاسم أو الرقم العسكري أو الوطني..."
                            className="pe-9"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            autoComplete="off"
                        />
                    </div>
                </div>

                {/* Results Area */}
                <div className="max-h-64 h-auto overflow-y-auto border rounded-xl divide-y bg-card flex flex-col items-center">
                    {!debouncedSearch || debouncedSearch.length < 2 ? (
                        <div className="text-sm text-muted-foreground m-auto p-4 text-center">
                            الرجاء إدخال حرفين على الأقل للبحث عن المتقاعدين.
                        </div>
                    ) : isSearching ? (
                        <div className="flex items-center gap-2 m-auto p-4 text-muted-foreground">
                            <Loader2 className="w-4 h-4 animate-spin" /> جاري البحث...
                        </div>
                    ) : searchResults?.items?.length === 0 ? (
                        <div className="text-sm text-muted-foreground m-auto p-4 text-center">
                            لا يوجد نتائج متطابقة للبحث.
                        </div>
                    ) : (
                        <div className="w-full flex flex-col">
<<<<<<< HEAD
                            {searchResults?.items.map((retiree: RetireeListItem) => (
                                <div
                                    key={retiree.retireeID}
                                    onClick={() => setSelectedRetireeId(retiree.retireeID)}
                                    onKeyDown={(e: React.KeyboardEvent) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setSelectedRetireeId(retiree.retireeID); } }}
=======
                            {searchResults?.items.map((retiree: RetirementFileListItem) => (
                                <div
                                    key={retiree.fileID}
                                    onClick={() => setSelectedRetireeId(retiree.fileID)}
                                    onKeyDown={(e: React.KeyboardEvent) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setSelectedRetireeId(retiree.fileID); } }}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    role="button"
                                    tabIndex={0}
                                    className={cn(
                                        "p-3 flex items-start gap-3 cursor-pointer transition-colors focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset hover:bg-muted/40",
<<<<<<< HEAD
                                        selectedRetireeId === retiree.retireeID ? "bg-primary/5 border-e-[3px] border-e-primary" : ""
                                    )}
                                >
                                    <div className="w-10 h-10 rounded-full bg-secondary text-secondary-foreground flex items-center justify-center font-bold shrink-0">
                                        {retiree.fullName.charAt(0)}
                                    </div>
                                    <div className="flex-1 space-y-1 text-sm text-start">
                                        <p className="font-bold">{retiree.fullName}</p>
                                        <div className="flex flex-wrap gap-2">
                                            {retiree.militaryNumber && (
                                                <span className="text-muted-foreground text-xs">الرقم العسكري: <span className="font-mono" dir="ltr">{retiree.militaryNumber}</span></span>
=======
                                        selectedRetireeId === retiree.fileID ? "bg-primary/5 border-e-[3px] border-e-primary" : ""
                                    )}
                                >
                                    <div className="w-10 h-10 rounded-full bg-secondary text-secondary-foreground flex items-center justify-center font-bold shrink-0">
                                        {retiree.retireeFullName.charAt(0)}
                                    </div>
                                    <div className="flex-1 space-y-1 text-sm text-start">
                                        <p className="font-bold">{retiree.retireeFullName}</p>
                                        <div className="flex flex-wrap gap-2">
                                            {retiree.fileNumber && (
                                                <span className="text-muted-foreground text-xs">رقم الملف: <span className="font-mono" dir="ltr">{retiree.fileNumber}</span></span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            )}
                                        </div>
                                    </div>
                                    <div className="shrink-0 text-start">
                                        <Badge variant="outline">{retiree.rankName || "بدون رتبة"}</Badge>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </BaseFormDialog>
    );
}
