import { useQuery } from "@tanstack/react-query";
import { AlertCircle, AlertOctagon,ArrowRight, Clock, DollarSign, FileText, History, Info, MapPin, Wrench } from "lucide-react";
import { useState } from "react";
import { Link,useParams } from "react-router-dom";

import { investmentApi } from "@/api/investment";
<<<<<<< HEAD
import { AddCostDialog } from "@/components/domain/investment/AddCostDialog";
import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { MaintenanceCostsTab } from "@/components/domain/investment/MaintenanceCostsTab";
import { WorkOrderDialog } from "@/components/domain/investment/WorkOrderDialog";
import { Badge, Breadcrumb, Button, Card, Heading, LoadingIndicator, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";
import { useHasPermission } from "@/stores/authStore";
=======
import { Badge, Breadcrumb, Button, Card, Heading, LoadingIndicator, StatCard, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";
import { AddCostDialog } from "@/components/domain/investment/AddCostDialog";
import { WorkOrderDialog } from "@/components/domain/investment/WorkOrderDialog";
import { useHasPermission } from "@/stores/authStore";

import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";
import { MaintenanceCostsTab } from "@/components/domain/investment/MaintenanceCostsTab";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

/**
 * صفحة تفاصيل بلاغ صيانة
 */
export function MaintenanceDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const ticketId = Number(id);
    const canViewCosts = useHasPermission('Investment.Finance');

    const [costModalOpen, setCostModalOpen] = useState(false);
    const [workOrderOpen, setWorkOrderOpen] = useState(false);

    // جلب تفاصيل البلاغ
    const { data: request, isLoading, isError } = useQuery({
        queryKey: ["maintenance-request", ticketId],
        queryFn: () => investmentApi.getRequest(ticketId),
        enabled: !!ticketId
    });

    if (isLoading) return <LoadingIndicator message="جاري تحميل بيانات البلاغ..." />;

    if (isError) {
        return (
            <div className="flex flex-col items-center justify-center p-6 sm:p-20 text-center space-y-4 animate-in fade-in duration-500" dir="rtl">
<<<<<<< HEAD
                <div className="p-4 bg-danger/5 rounded-full text-danger ">
=======
                <div className="p-4 bg-red-500/5 rounded-full text-red-600 ">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <AlertCircle className="w-12 h-12" />
                </div>
                <Heading level="h2" className="text-base md:text-lg lg:text-xl font-bold">تعذر تحميل بيانات البلاغ</Heading>
                <Link to="/investment/maintenance" className="text-primary hover:underline font-medium">العودة لقائمة الصيانة</Link>
            </div>
        );
    }

    if (!request) {
        return (
            <div className="flex flex-col items-center justify-center p-6 sm:p-20 text-center space-y-4 animate-in fade-in duration-500" dir="rtl">
<<<<<<< HEAD
                <div className="p-4 bg-danger/5 rounded-full text-danger ">
=======
                <div className="p-4 bg-red-500/5 rounded-full text-red-600 ">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <AlertCircle className="w-12 h-12" />
                </div>
                <Heading level="h2" className="text-base md:text-lg lg:text-xl font-bold">عذراً، البلاغ غير موجود</Heading>
                <Link to="/investment/maintenance" className="text-primary hover:underline font-medium">العودة لقائمة الصيانة</Link>
            </div>
        );
    }

    const getStatusInfo = (status: string) => {
        switch (status) {
            case 'Completed': return { text: "منجزة", variant: 'success' as const };
            case 'InProgress': return { text: "قيد التنفيذ", variant: 'info' as const };
            case 'Pending': return { text: "قيد المراجعة", variant: 'warning' as const };
            case 'New': return { text: "جديدة", variant: 'outline' as const };
            default: return { text: status, variant: 'outline' as const };
        }
    };

    const statusInfo = getStatusInfo(request.status);

    const getPriorityLabel = (priority?: string) => {
        switch (priority) {
            case "High": return "عالي";
            case "Medium": return "متوسط";
            case "Low": return "منخفض";
            case "Urgent": return "عاجل جداً";
            default: return priority || "غير محدد";
        }
    };

    return (
        <div className="space-y-6 text-start animate-in fade-in duration-500" dir="rtl">
            <Breadcrumb items={[
                { label: "الاستثمارات", href: "/investment" },
                { label: "الصيانة", href: "/investment/maintenance" },
                { label: `بلاغ #${request.requestID}` }
            ]} />

            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to="/investment/maintenance" className="p-2 hover:bg-muted rounded-full transition-colors border " title="عودة">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <div className="flex items-center gap-3">
                            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-black tracking-tight">بلاغ صيانة #{request.requestID}</Heading>
                            <Badge variant={statusInfo.variant} className="font-bold  px-3">
                                {statusInfo.text}
                            </Badge>
                        </div>
                        <Text variant="description" className="flex items-center gap-1.5 mt-1 font-medium">
                            <MapPin className="w-4 h-4 text-muted-primary" /> {request.propertyName}
<<<<<<< HEAD
                            {request.unitNumber && <span className="mx-1 opacity-100">|</span>}
=======
                            {request.unitNumber && <span className="mx-1 opacity-30">|</span>}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            {request.unitNumber && <span>وحدة: {request.unitNumber}</span>}
                        </Text>
                    </div>
                </div>

                <div className="flex gap-2">
                    <Button onClick={() => setWorkOrderOpen(true)} className="gap-2  font-bold">
                        <Wrench className="w-4 h-4" /> أوامر العمل
                    </Button>
                </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard
                    title="تاريخ البلاغ"
                    value={new Date(request.reportedDate).toLocaleDateString("ar-YE")}
                    icon={<Clock className="w-4 h-4 text-primary" />}
                />
                <StatCard
                    title="الأولوية"
                    value={getPriorityLabel(request.priority)}
<<<<<<< HEAD
                    icon={<AlertOctagon className="w-4 h-4 text-warning" />}
=======
                    icon={<AlertOctagon className="w-4 h-4 text-amber-500" />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    variant={request.priority === 'Urgent' ? 'rose' : 'blue'}
                />
                <StatCard
                    title="التكلفة التقديرية"
                    value="—"
<<<<<<< HEAD
                    icon={<DollarSign className="w-4 h-4 text-success" />}
=======
                    icon={<DollarSign className="w-4 h-4 text-emerald-500" />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    variant="emerald"
                />
                <StatCard
                    title="المسؤول"
                    value="إدارة الأصول"
<<<<<<< HEAD
                    icon={<Info className="w-4 h-4 text-info" />}
=======
                    icon={<Info className="w-4 h-4 text-blue-500" />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                />
            </div>

            <Tabs defaultValue="details" className="w-full">
                <div className="flex items-center w-full overflow-x-auto pb-1 mb-4 scrollbar-hide">
                    <TabsList className="bg-muted/50 p-1 h-auto flex gap-1">
                        <TabsTrigger value="details" className="gap-2 px-6 py-2.5">
                            <Info className="w-4 h-4" /> التفاصيل
                        </TabsTrigger>
                        <TabsTrigger value="status" className="gap-2 px-6 py-2.5">
                            <History className="w-4 h-4" /> السجل
                        </TabsTrigger>
                        <TabsTrigger value="documents" className="gap-2 px-6 py-2.5">
                            <FileText className="w-4 h-4" /> الوثائق
                        </TabsTrigger>
                        {canViewCosts && (
                            <TabsTrigger value="costs" className="gap-2 px-6 py-2.5">
                                <DollarSign className="w-4 h-4" /> التكاليف
                            </TabsTrigger>
                        )}
                    </TabsList>
                </div>

                <TabsContent value="details" className="mt-0 outline-none">
<<<<<<< HEAD
                    <Card className="p-6  rounded-2xl border-border">
                        <Heading level="h3" className="text-sm font-black text-muted-foreground uppercase tracking-widest mb-4 flex items-center gap-2">
                            <FileText className="w-4 h-4 text-primary" /> وصف المشكلة / الطلب
                        </Heading>
                        <Text variant="body" className="leading-relaxed text-foreground whitespace-pre-wrap">{request.description}</Text>
=======
                    <Card className="p-6  rounded-2xl border-border/60">
                        <Heading level="h3" className="text-sm font-black text-muted-foreground uppercase tracking-widest mb-4 flex items-center gap-2">
                            <FileText className="w-4 h-4 text-primary" /> وصف المشكلة / الطلب
                        </Heading>
                        <Text variant="body" className="leading-relaxed text-foreground/90 whitespace-pre-wrap">{request.description}</Text>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Card>
                </TabsContent>

                <TabsContent value="status" className="mt-0 outline-none">
                    <Card className="p-6 sm:p-16 text-center text-muted-foreground  rounded-2xl border-dashed">
<<<<<<< HEAD
                        <History className="w-12 h-12 mx-auto mb-4 opacity-100" />
                        <Text variant="body" className="text-lg font-bold">سجل الحالات والمتابعة</Text>
                        <Text variant="label" className="opacity-100">سيتم عرض كافة التغييرات التي طرأت على هذا البلاغ قريباً.</Text>
=======
                        <History className="w-12 h-12 mx-auto mb-4 opacity-20" />
                        <Text variant="body" className="text-lg font-bold">سجل الحالات والمتابعة</Text>
                        <Text variant="label" className="opacity-70">سيتم عرض كافة التغييرات التي طرأت على هذا البلاغ قريباً.</Text>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Card>
                </TabsContent>

                <TabsContent value="documents" className="mt-0 outline-none">
                    <InvestmentDocumentsManager
                        entityType="maintenance-ticket"
                        entityId={request.requestID}
                        entityName={`بلاغ صيانة رقم ${request.requestID}`}
                    />
                </TabsContent>

                {canViewCosts && (
                    <TabsContent value="costs" className="mt-0 outline-none">
                        <MaintenanceCostsTab onAddCost={() => setCostModalOpen(true)} />
                    </TabsContent>
                )}
            </Tabs>

            <AddCostDialog
                open={costModalOpen}
                onClose={() => setCostModalOpen(false)}
                ticketId={ticketId}
            />

            <WorkOrderDialog
                open={workOrderOpen}
                onClose={() => setWorkOrderOpen(false)}
                request={request}
            />
        </div>
    );
}
