import { AlertTriangle, FileWarning, Pencil, User } from "lucide-react";
import { useState } from "react";

import api from "@/api/axios";
import { retireeApi } from "@/api/retirees";
import { retirementApi } from "@/api/retirement";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui";
import { Button, EditableImage,Heading } from "@/components/ui";
import { AlertBanner } from "@/components/ui";
import { BaseFormDialog, FormField, Input, NativeSelect,Textarea } from "@/components/ui";
import { InfoPair } from "@/components/ui/InfoPair";
import { useApiMutation } from "@/hooks/useApiMutation";
import { formatDate, getDefaultDeceasedAvatar, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { RetireeDetail,RetirementFileDetail } from "@/types/retirement";

interface BasicInfoSectionProps {
    file: RetirementFileDetail;
    retiree: RetireeDetail;
    fileId: number;
    missingDocsCount: number;
    missingDataCount: number;
    pendingReviewCount: number;
    onFileUpdated?: () => void;
}

const SOCIAL_STATUS_OPTIONS = [
    { value: "1", label: "أعزب" },
    { value: "2", label: "متزوج" },
    { value: "3", label: "أرمل" },
    { value: "4", label: "مطلق" },
];

export function BasicInfoSection({
    file,
    retiree,
    fileId,
    missingDocsCount,
    missingDataCount: _missingDataCount,
    pendingReviewCount,
    onFileUpdated,
}: BasicInfoSectionProps) {
    const canEditFile = useHasPermission("File.Edit");
    const canEditRetiree = useHasPermission("Retiree.Edit");

    const [editFileOpen, setEditFileOpen] = useState(false);
    const [editRetireeOpen, setEditRetireeOpen] = useState(false);

    const [fileFormData, setFileFormData] = useState({
        retirementDate: file.retirementDate || "",
        serviceStartDate: file.serviceStartDate || "",
        serviceEndDate: file.serviceEndDate || "",
        notes: file.notes || "",
    });

    const [retireeFormData, setRetireeFormData] = useState({
        fullName: retiree.fullName || "",
        phone: retiree.phone || "",
        address: retiree.address || "",
        city: retiree.city || "",
        governorate: retiree.governorate || "",
        socialStatusID: String(retiree.socialStatusID || 1),
    });

    const updateFileMutation = useApiMutation(
        async () => {
            await retirementApi.updateFile(fileId, {
                retirementDate: fileFormData.retirementDate || null,
                serviceStartDate: fileFormData.serviceStartDate || null,
                serviceEndDate: fileFormData.serviceEndDate || null,
                notes: fileFormData.notes || null,
            });
        },
        {
            successMessage: "تم تحديث بيانات الملف بنجاح",
            invalidateKeys: [["retirement-file", fileId]],
            onSuccess: () => {
                setEditFileOpen(false);
                onFileUpdated?.();
            },
        }
    );

    const updateRetireeMutation = useApiMutation(
        async () => {
            await retireeApi.update(retiree.retireeID, {
                fullName: retireeFormData.fullName || undefined,
                phone: retireeFormData.phone || undefined,
                address: retireeFormData.address || undefined,
                city: retireeFormData.city || undefined,
                governorate: retireeFormData.governorate || undefined,
                socialStatusID: parseInt(retireeFormData.socialStatusID) || undefined,
            });
        },
        {
            successMessage: "تم تحديث بيانات المتقدم بنجاح",
            invalidateKeys: [["retirement-file", fileId]],
            onSuccess: () => {
                setEditRetireeOpen(false);
                onFileUpdated?.();
            },
        }
    );

    const photoMutation = useApiMutation(
        async (uploadFile: File) => {
            const formData = new FormData();
            formData.append("photo", uploadFile);
            return api.post(`/retirees/${retiree.retireeID}/photo`, formData, {
                headers: { "Content-Type": "multipart/form-data" }
            });
        },
        {
            successMessage: "تم تحديث الصورة الشخصية بنجاح",
            invalidateKeys: [["retirement-file", fileId]],
        }
    );

    const handleOpenEditFile = () => {
        setFileFormData({
            retirementDate: file.retirementDate || "",
            serviceStartDate: file.serviceStartDate || "",
            serviceEndDate: file.serviceEndDate || "",
            notes: file.notes || "",
        });
        setEditFileOpen(true);
    };

    const handleOpenEditRetiree = () => {
        setRetireeFormData({
            fullName: retiree.fullName || "",
            phone: retiree.phone || "",
            address: retiree.address || "",
            city: retiree.city || "",
            governorate: retiree.governorate || "",
            socialStatusID: String(retiree.socialStatusID || 1),
        });
        setEditRetireeOpen(true);
    };

    return (
        <div className="space-y-4 fade-in">
            <Heading level="h2">البيانات الأساسية</Heading>

            <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                    <CardTitle>تفاصيل المتقاعد</CardTitle>
                    {canEditRetiree && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="gap-1.5 text-primary hover:bg-primary/10"
                            onClick={handleOpenEditRetiree}
                        >
                            <Pencil className="w-4 h-4" />
                            تعديل
                        </Button>
                    )}
                </CardHeader>
                <CardContent>
                    <div className="flex flex-col sm:flex-row gap-6">
                        {/* الصورة الشخصية */}
                        <div className="flex-shrink-0">
                            <div className="relative group">
                                <div className="absolute -inset-0.5 bg-gradient-to-br from-primary/30 to-primary/10 rounded-2xl blur opacity-50 group-hover:opacity-75 transition duration-500" />
                                <EditableImage
                                    src={getPhotoUrl(resolvePersonalPhotoPath(retiree)) || getPhotoUrl(resolvePersonalPhotoPath(file)) || (!file.isAlive ? getDefaultDeceasedAvatar(file.gender) : null)}
                                    alt={retiree.fullName}
                                    className="relative w-24 h-32 rounded-2xl border-[4px] border-card object-cover bg-muted transition-all duration-300 group-hover:scale-[1.02]"
                                    fallbackIcon={<User className="w-10 h-10 text-muted-foreground/60" />}
                                    canEdit={canEditRetiree}
                                    imgWidth={96}
                                    imgHeight={128}
                                    onUpload={async (newFile) => {
                                        const res = await photoMutation.mutateAsync(newFile);
                                        if (res?.data?.data) return getPhotoUrl(res.data.data) || undefined;
                                        return undefined;
                                    }}
                                />
                            </div>
                        </div>

                        {/* البيانات */}
                        <div className="flex-1 grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-0">
                            <InfoPair label="الاسم الكامل" value={retiree.fullName} layout="row" />
                        <InfoPair label="الرقم الوطني" value={retiree.nationalID} layout="row" mono />
                        <InfoPair label="الرقم العسكري" value={retiree.militaryNumber} layout="row" mono />
                        <InfoPair label="الوحدة" value={retiree.unitName} layout="row" />
                        <InfoPair label="الفئة" value={retiree.categoryName} layout="row" />
                        <InfoPair label="الرتبة" value={retiree.rankName} layout="row" />
                        <InfoPair label="رقم الربط" value={retiree.linkNumber} layout="row" mono />
                        <InfoPair label="تاريخ الميلاد" value={formatDate(file.birthDate)} layout="row" />
                        <InfoPair label="الحالة الاجتماعية" value={retiree.socialStatusName} layout="row" />
                        <InfoPair label="الحالة الصحية" value={retiree.healthStatusName} layout="row" />
                        <InfoPair label="الجنسية" value="يمني" layout="row" />
                        <InfoPair label="رقم الهاتف" value={retiree.phone} layout="row" mono dir="ltr" />
                        <InfoPair label="المدينة" value={retiree.city} layout="row" />
                        <InfoPair label="المحافظة" value={retiree.governorate} layout="row" />
                        <InfoPair label="العنوان" value={retiree.address} layout="row" />
                        <InfoPair label="الفرع" value={file.branchName} layout="row" />
                        <InfoPair label="تاريخ التسجيل" value={formatDate(file.createdAt, true)} layout="row" />
                        </div>
                    </div>
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                    <CardTitle>بيانات الملف</CardTitle>
                    {canEditFile && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="gap-1.5 text-primary hover:bg-primary/10"
                            onClick={handleOpenEditFile}
                        >
                            <Pencil className="w-4 h-4" />
                            تعديل
                        </Button>
                    )}
                </CardHeader>
                <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-0">
                        <InfoPair label="رقم الملف" value={file.fileNumber} layout="row" mono />
                        <InfoPair label="تاريخ التقاعد" value={formatDate(file.retirementDate)} layout="row" />
                        <InfoPair label="تاريخ بدء الخدمة" value={formatDate(file.serviceStartDate)} layout="row" />
                        <InfoPair label="تاريخ انتهاء الخدمة" value={formatDate(file.serviceEndDate)} layout="row" />
                        <InfoPair label="ملاحظات" value={file.notes} layout="row" />
                    </div>
                </CardContent>
            </Card>

            <div className="space-y-2">
                {pendingReviewCount > 0 && (
                    <AlertBanner
                        variant="warning"
                        icon={<AlertTriangle />}
                        message={
                            <div>
                                <p className="font-bold text-sm">الملف قيد المراجعة</p>
                                <p className="text-xs">يوجد {pendingReviewCount} بنود بانتظار مراجعة التعديلات الأخيرة</p>
                            </div>
                        }
                    />
                )}
                {missingDocsCount > 0 && (
                    <AlertBanner
                        variant="danger"
                        icon={<FileWarning />}
                        message={
                            <div>
                                <p className="font-bold text-sm">تنبيه: وثائق وبيانات ناقصة</p>
                                <p className="text-xs">يوجد {missingDocsCount} وثائق ناقصة مطلوب رفعها أو تغطيتها بتعهد</p>
                            </div>
                        }
                    />
                )}
            </div>

            {/* ─── حوار تعديل الملف ─── */}
            <BaseFormDialog
                open={editFileOpen}
                onOpenChange={setEditFileOpen}
                title="تعديل بيانات الملف"
                onSubmit={() => updateFileMutation.mutate(undefined)}
                isLoading={updateFileMutation.isPending}
                size="lg"
            >
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField label="تاريخ التقاعد">
                        <Input
                            type="date"
                            value={fileFormData.retirementDate}
                            onChange={(e) =>
                                setFileFormData((prev) => ({
                                    ...prev,
                                    retirementDate: e.target.value,
                                }))
                            }
                        />
                    </FormField>
                    <FormField label="تاريخ بدء الخدمة">
                        <Input
                            type="date"
                            value={fileFormData.serviceStartDate}
                            onChange={(e) =>
                                setFileFormData((prev) => ({
                                    ...prev,
                                    serviceStartDate: e.target.value,
                                }))
                            }
                        />
                    </FormField>
                    <FormField label="تاريخ انتهاء الخدمة">
                        <Input
                            type="date"
                            value={fileFormData.serviceEndDate}
                            onChange={(e) =>
                                setFileFormData((prev) => ({
                                    ...prev,
                                    serviceEndDate: e.target.value,
                                }))
                            }
                        />
                    </FormField>
                    <div className="sm:col-span-2">
                        <FormField label="ملاحظات">
                            <Textarea
                                value={fileFormData.notes}
                                onChange={(e) =>
                                    setFileFormData((prev) => ({
                                        ...prev,
                                        notes: e.target.value,
                                    }))
                                }
                                placeholder="أضف ملاحظات على الملف..."
                                className="min-h-[80px]"
                            />
                        </FormField>
                    </div>
                </div>
            </BaseFormDialog>

            {/* ─── حوار تعديل بيانات المتقدم ─── */}
            <BaseFormDialog
                open={editRetireeOpen}
                onOpenChange={setEditRetireeOpen}
                title="تعديل بيانات المتقدم"
                onSubmit={() => updateRetireeMutation.mutate(undefined)}
                isLoading={updateRetireeMutation.isPending}
                size="lg"
            >
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="sm:col-span-2">
                        <FormField label="الاسم الكامل">
                            <Input
                                value={retireeFormData.fullName}
                                onChange={(e) =>
                                    setRetireeFormData((prev) => ({
                                        ...prev,
                                        fullName: e.target.value,
                                    }))
                                }
                                placeholder="الاسم الكامل"
                            />
                        </FormField>
                    </div>
                    <FormField label="رقم الهاتف">
                        <Input
                            value={retireeFormData.phone}
                            onChange={(e) =>
                                setRetireeFormData((prev) => ({
                                    ...prev,
                                    phone: e.target.value,
                                }))
                            }
                            placeholder="7xxxxxxx"
                        />
                    </FormField>
                    <FormField label="الحالة الاجتماعية">
                        <NativeSelect
                            value={retireeFormData.socialStatusID}
                            onChange={(val) =>
                                setRetireeFormData((prev) => ({
                                    ...prev,
                                    socialStatusID: val,
                                }))
                            }
                            options={SOCIAL_STATUS_OPTIONS}
                        />
                    </FormField>
                    <FormField label="المدينة">
                        <Input
                            value={retireeFormData.city}
                            onChange={(e) =>
                                setRetireeFormData((prev) => ({
                                    ...prev,
                                    city: e.target.value,
                                }))
                            }
                            placeholder="المدينة"
                        />
                    </FormField>
                    <FormField label="المحافظة">
                        <Input
                            value={retireeFormData.governorate}
                            onChange={(e) =>
                                setRetireeFormData((prev) => ({
                                    ...prev,
                                    governorate: e.target.value,
                                }))
                            }
                            placeholder="المحافظة"
                        />
                    </FormField>
                    <div className="sm:col-span-2">
                        <FormField label="العنوان">
                            <Textarea
                                value={retireeFormData.address}
                                onChange={(e) =>
                                    setRetireeFormData((prev) => ({
                                        ...prev,
                                        address: e.target.value,
                                    }))
                                }
                                placeholder="العنوان التفصيلي"
                                className="min-h-[60px]"
                            />
                        </FormField>
                    </div>
                </div>
            </BaseFormDialog>
        </div>
    );
}
