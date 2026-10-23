import { useQuery } from "@tanstack/react-query";
import { ArrowLeftRight, Clock,History, User } from "lucide-react";

import { retirementApi } from "@/api/retirement";
import { Heading } from "@/components/ui";
import { EmptyState } from "@/components/ui";
import { cn, formatDate } from "@/lib/utils";

interface HistorySectionProps {
    fileId: number;
}

export function HistorySection({ fileId }: HistorySectionProps) {
    const { data: changeLog = [], isLoading } = useQuery({
        queryKey: ["file-change-log", fileId],
        queryFn: () => retirementApi.getChangeLog(fileId),
        enabled: !!fileId,
        staleTime: 30_000,
    });

    if (isLoading) {
        return (
            <div className="space-y-4 animate-in fade-in duration-500" dir="rtl">
                <div className="flex items-center gap-2.5 pb-2 border-b border-border">
                    <div className="p-2 rounded-xl bg-primary/10 text-primary">
                        <History className="w-5 h-5" />
                    </div>
                    <Heading level="h2" className="text-lg font-black">سجل التغييرات والتحولات</Heading>
                </div>
                <div className="relative space-y-0">
                    <div className="absolute end-[5px] top-2 bottom-2 w-0.5 bg-muted rounded-full" />
                    {[1, 2, 3].map((i) => (
                        <div key={i} className="relative pe-8 pb-6">
                            <div className="absolute end-[2px] top-1.5 w-2.5 h-2.5 rounded-full bg-muted animate-pulse" />
                            <div className="bg-card border border-border rounded-xl p-3.5 space-y-2">
                                <div className="h-4 bg-muted animate-pulse rounded w-3/4" />
                                <div className="h-3 bg-muted animate-pulse rounded w-1/2" />
                                <div className="h-3 bg-muted animate-pulse rounded w-1/3" />
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-4 animate-in fade-in duration-500" dir="rtl">
            <div className="flex items-center gap-2.5 pb-2 border-b border-border">
                <div className="p-2 rounded-xl bg-primary/10 text-primary">
                    <History className="w-5 h-5" />
                </div>
                <Heading level="h2" className="text-lg font-black">سجل التغييرات والتحولات</Heading>
            </div>

            {changeLog.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16">
                    <EmptyState 
                        icon={History} 
                        title="لا توجد تغييرات مسجلة" 
                        description="لم يتم تسجيل أي عمليات تعديل على هذا الملف بعد"
                    />
                </div>
            ) : (
                <div className="relative space-y-0">
                    {/* الخط الرأسي */}
                    <div className="absolute end-[5px] top-2 bottom-2 w-0.5 bg-border rounded-full" />
                    
                    {changeLog.map((item, idx) => {
                        const isLast = idx === changeLog.length - 1;
                        
                        return (
                            <div
                                key={`${item.id}-${idx}`}
                                className={cn(
                                    "relative pe-8 pb-6 group min-w-0 overflow-hidden",
                                    isLast && "pb-0"
                                )}
                            >
                                {/* النقطة */}
                                <div className={cn(
                                    "absolute end-[2px] top-2 w-2.5 h-2.5 rounded-full border-2 border-background transition-all duration-300",
                                    "bg-primary group-hover:scale-125 group-hover:ring-2 group-hover:ring-primary/20"
                                )} />
                                
                                {/* البطاقة */}
                                <div className="bg-card rounded-xl border border-border p-3.5 md:p-4 shadow-sm hover:shadow-md hover:border-primary transition-all duration-300 overflow-hidden">
                                    {/* العنوان + التاريخ */}
                                    <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-2 mb-2 min-w-0 overflow-hidden">
                                        <p className="font-bold text-sm text-foreground leading-relaxed break-words flex-1 min-w-0">
                                            {item.description}
                                        </p>
                                        <div className="flex items-center gap-1.5 shrink-0 text-[10px] text-muted-foreground bg-muted/50 px-2 py-1 rounded-md whitespace-nowrap">
                                            <Clock className="w-3 h-3" />
                                            <span>{formatDate(item.changeDate)}</span>
                                        </div>
                                    </div>
                                    
                                    {/* تفاصيل التغيير */}
                                    {item.fieldChanges && item.fieldChanges.length > 0 && (
                                        <div className="mt-2.5 space-y-1.5 bg-muted/30 rounded-lg p-2.5 border border-border overflow-hidden">
                                            {item.fieldChanges.map((fc, fi) => (
                                                <div 
                                                    key={fi} 
                                                    className="flex items-center justify-between gap-3 text-xs min-w-0 overflow-hidden"
                                                >
                                                    <span className="font-bold text-muted-foreground shrink-0">
                                                        {fc.fieldName}
                                                    </span>
                                                    <div className="flex-1 min-w-0 flex items-center justify-end gap-2 overflow-hidden">
                                                        <span className="truncate text-muted-foreground line-through opacity-100 text-start" dir="ltr">
                                                            {fc.oldValue || "—"}
                                                        </span>
                                                        <ArrowLeftRight className="w-3 h-3 shrink-0 text-primary opacity-100" />
                                                        <span className="truncate font-bold text-foreground text-start" dir="ltr">
                                                            {fc.newValue || "—"}
                                                        </span>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                    
                                    {/* المستخدم */}
                                    <div className="flex items-center gap-1.5 mt-2.5 pt-2 border-t border-border min-w-0 overflow-hidden">
                                        <User className="w-3 h-3 text-muted-foreground shrink-0" />
                                        <p className="text-[10px] text-muted-foreground font-medium truncate">
                                            بواسطة: <span className="font-bold text-foreground">{item.changedByUserName || "النظام"}</span>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
}