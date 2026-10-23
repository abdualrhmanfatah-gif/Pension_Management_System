import { useQuery } from "@tanstack/react-query";
import { Check,Pencil, Plus, RefreshCw, Save, Search, Settings, Trash2, X } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

import { adminApi, type SettingItem } from "@/api/admin";
import { Card, Heading, Loader, PageLayout, StickyHeader,Text, useConfirm } from "@/components/ui";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/Select";
import { useApiMutation } from "@/hooks";

export function SystemSettingsPage() {
    const { confirm, ConfirmDialogElement } = useConfirm();
    const [filterCategory, setFilterCategory] = useState("");
    const [filterSearch, setFilterSearch] = useState("");
    const [editingId, setEditingId] = useState<number | null>(null);
    const [editValue, setEditValue] = useState("");
    const [showCreate, setShowCreate] = useState(false);
    const [newSetting, setNewSetting] = useState({ key: "", value: "", description: "", category: "" });

    const { data: categories = [], refetch: refetchCategories } = useQuery({
        queryKey: ["admin", "settings", "categories"],
        queryFn: () => adminApi.getCategories()
    });

    const { data: settings = [], isLoading: loading, isError: settingsError, refetch: fetchSettings } = useQuery({
        queryKey: ["admin", "settings", filterCategory],
        queryFn: () => adminApi.getSettings(filterCategory || undefined)
    });

    const updateMutation = useApiMutation(
        (data: { id: number, value: string }) => adminApi.updateSetting(data.id, data.value),
        {
            successMessage: "تم تحديث الإعداد بنجاح",
            onSuccessCallback: () => {
                setEditingId(null);
                fetchSettings();
            }
        }
    );

    const createMutation = useApiMutation(
        (data: typeof newSetting) => adminApi.createSetting(data),
        {
            successMessage: "تم إنشاء الإعداد بنجاح",
            onSuccessCallback: () => {
                setShowCreate(false);
                setNewSetting({ key: "", value: "", description: "", category: "" });
                fetchSettings();
                refetchCategories();
            }
        }
    );

    const deleteMutation = useApiMutation(
        (id: number) => adminApi.deleteSetting(id),
        {
            successMessage: "تم حذف الإعداد",
            onSuccessCallback: () => fetchSettings()
        }
    );

    const startEdit = (s: SettingItem) => { setEditingId(s.settingID); setEditValue(s.settingValue); };
    const cancelEdit = () => { setEditingId(null); setEditValue(""); };

    const saveEdit = async (id: number) => {
        updateMutation.mutate({ id, value: editValue });
    };

    const createSetting = async () => {
        if (!newSetting.key || !newSetting.value) {
            toast.error("يرجى ملء المفتاح والقيمة");
            return;
        }
        createMutation.mutate(newSetting);
    };

    const deleteSetting = async (id: number) => {
        const verified = await confirm({
            title: "تأكيد الحذف",
            description: "هل أنت متأكد من حذف هذا الإعداد؟",
            confirmLabel: "حذف",
            cancelLabel: "إلغاء",
            variant: "danger"
        });

        if (!verified) return;
        deleteMutation.mutate(id);
    };

    const filtered = settings.filter(s =>
        !filterSearch || s.settingKey.toLowerCase().includes(filterSearch.toLowerCase()) ||
        (s.description && s.description.toLowerCase().includes(filterSearch.toLowerCase()))
    );

    // Group by category
    const grouped = filtered.reduce<Record<string, SettingItem[]>>((acc, s) => {
        const cat = s.category || "عام";
        if (!acc[cat]) acc[cat] = [];
        acc[cat].push(s);
        return acc;
    }, {});

    return (
        <PageLayout>
            {/* Header */}
            <StickyHeader>
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="p-2.5 rounded-xl bg-gradient-to-br from-emerald-500/20 to-teal-500/20">
                            <Settings className="w-6 h-6 text-emerald-600 dark:text-emerald-400" />
                        </div>
                        <div>
                            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold">إعدادات النظام</Heading>
                            <Text variant="description">إدارة المعلمات والتكوينات العامة للنظام</Text>
                        </div>
                    </div>
                    <div className="flex gap-2">
                        <Button variant="outline" size="sm" onClick={() => fetchSettings()}><RefreshCw className="w-4 h-4 ms-1" /> تحديث</Button>
                        <Button size="sm" onClick={() => setShowCreate(true)}><Plus className="w-4 h-4 ms-1" /> إعداد جديد</Button>
                    </div>
                </div>
            </StickyHeader>

            {/* Create Form */}
            {showCreate && (
                <Card className="p-4 space-y-3 border-primary">
                    <div className="flex items-center justify-between">
                        <Heading level="h3" className="font-semibold text-sm">إنشاء إعداد جديد</Heading>
                        <Button variant="ghost" size="sm" onClick={() => setShowCreate(false)} aria-label="إلغاء"><X className="w-4 h-4" /></Button>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        <Input placeholder="المفتاح (مثال: MaxLoginAttempts)" value={newSetting.key} onChange={e => setNewSetting(p => ({ ...p, key: e.target.value }))} />
                        <Input placeholder="القيمة" value={newSetting.value} onChange={e => setNewSetting(p => ({ ...p, value: e.target.value }))} />
                        <Input placeholder="الوصف" value={newSetting.description} onChange={e => setNewSetting(p => ({ ...p, description: e.target.value }))} />
                        <Input placeholder="الفئة" value={newSetting.category} onChange={e => setNewSetting(p => ({ ...p, category: e.target.value }))} />
                    </div>
                    <Button size="sm" onClick={createSetting}><Save className="w-4 h-4 ms-1" /> حفظ</Button>
                </Card>
            )}

            {/* Filters */}
            <div className="flex gap-3">
                <div className="relative flex-1 max-w-xs">
                    <Search className="absolute end-3 top-2.5 w-4 h-4 text-muted-foreground" />
                    <Input placeholder="بحث عن إعداد..." value={filterSearch} onChange={e => setFilterSearch(e.target.value)} className="pe-9" />
                </div>
                <Select value={filterCategory} onValueChange={(v) => setFilterCategory(v === "__all__" ? "" : v)}>
                    <SelectTrigger className="min-h-[44px]">
                        <SelectValue placeholder="كل الفئات" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="__all__">كل الفئات</SelectItem>
                        {categories.map(c => <SelectItem key={c} value={c}>{c}</SelectItem>)}
                    </SelectContent>
                </Select>
            </div>

            {/* Settings Groups */}
            {loading ? (
                <div className="py-16"><Loader text="جاري تحميل الإعدادات..." /></div>
            ) : settingsError ? (
                <div className="border-2 border-destructive rounded-xl py-16 text-center text-destructive">تعذر تحميل الإعدادات</div>
            ) : Object.keys(grouped).length === 0 ? (
                <div className="border-2 border-dashed rounded-xl py-16 text-center text-muted-foreground">لا توجد إعدادات</div>
            ) : (
                <div className="space-y-4">
                    {Object.entries(grouped).map(([cat, items]) => (
                        <div key={cat} className="border rounded-xl overflow-hidden">
                            <div className="bg-muted/50 px-4 py-2.5 border-b">
                                <Heading level="h3" className="font-semibold text-sm">{cat}</Heading>
                            </div>
                            <div className="divide-y">
                                {items.map(s => (
                                    <div key={s.settingID} className="px-4 py-3 flex flex-col sm:flex-row items-start sm:items-center gap-2 sm:gap-4 hover:bg-muted/20 transition-colors">
                                        <div className="flex-1 min-w-0">
                                            <Text variant="label" className="font-mono">{s.settingKey}</Text>
                                            {s.description && <Text variant="caption" className="mt-0.5">{s.description}</Text>}
                                        </div>
                                        <div className="flex items-center gap-2 shrink-0">
                                            {editingId === s.settingID ? (
                                                <>
                                                    <Input value={editValue} onChange={e => setEditValue(e.target.value)} className="w-full sm:w-48 h-8 text-sm" autoFocus />
                                                    <Button variant="ghost" size="sm" onClick={() => saveEdit(s.settingID)} className="text-emerald-600" aria-label="حفظ التعديل"><Check className="w-4 h-4" /></Button>
                                                    <Button variant="ghost" size="sm" onClick={cancelEdit} className="text-red-600" aria-label="إلغاء التعديل"><X className="w-4 h-4" /></Button>
                                                </>
                                            ) : (
                                                <>
                                                    <code className="bg-muted px-2 py-1 rounded text-sm max-w-[150px] sm:max-w-[200px] truncate block">{s.settingValue}</code>
                                                    <Button variant="ghost" size="sm" onClick={() => startEdit(s)} aria-label="تعديل"><Pencil className="w-3.5 h-3.5" /></Button>
                                                    <Button variant="ghost" size="sm" onClick={() => deleteSetting(s.settingID)} className="text-red-600" aria-label="حذف"><Trash2 className="w-3.5 h-3.5" /></Button>
                                                </>
                                            )}
                                        </div>
                                        {s.updatedByUser && (
                                            <p className="text-[10px] sm:text-xs text-muted-foreground shrink-0">آخر تعديل: {s.updatedByUser}</p>
                                        )}
                                    </div>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>
            )}
            {ConfirmDialogElement}
        </PageLayout>
    );
}

