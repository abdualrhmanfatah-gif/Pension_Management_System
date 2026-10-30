<<<<<<< HEAD
import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { BookOpen,Building2, Calendar, CircleDollarSign, LandPlot, Laptop, Sofa, Truck, Wrench } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { Controller,type Resolver, useForm } from "react-hook-form";
=======
import { useQuery } from "@tanstack/react-query";
import { Building2, Calendar, LandPlot, Laptop, Sofa, Truck, Wrench, CircleDollarSign, BookOpen } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { type Resolver, useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { z } from "zod";

import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
import {
    BaseFormDialog,
    Checkbox,
<<<<<<< HEAD
    DateInput,
    FormField,
    FormSection,
    Input,
    NativeSelect,
} from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks";
=======
    FormField,
    FormSection,
    Input,
    DateInput,
    NativeSelect,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ═══════════════════════════════════════
// Schema
// ═══════════════════════════════════════

const schema = z.object({
    assetName: z.string().min(2, "الاسم مطلوب"),
    description: z.string().optional().nullable().or(z.literal("")),
    categoryID: z.coerce.number().min(1, "التصنيف مطلوب"),
    assetTypeID: z.coerce.number().min(1, "النوع مطلوب"),
    serialNumber: z.string().optional().nullable().or(z.literal("")),
    purchaseDate: z.string().min(1, "تاريخ الشراء مطلوب"),
    inServiceDate: z.string().min(1, "تاريخ التشغيل مطلوب"),
    unitPrice: z.coerce.number().min(0.01, "القيمة مطلوبة"),
    salvageValue: z.coerce.number().min(0).default(0),
    currencyCode: z.string().default("YER"),
    quantity: z.coerce.number().min(1, "الكمية مطلوبة").default(1),
    depreciationRate: z.coerce.number().min(0.01, "نسبة الإهلاك مطلوبة"),
    valuationMethod: z.coerce.number().default(0),
    valuationNotes: z.string().optional().nullable().or(z.literal("")),
    initialAccumulatedDepreciation: z.coerce.number().min(0).default(0),
    location: z.string().optional().nullable().or(z.literal("")),
    assetAccountID: z.coerce.number().default(0),
    accumDeprAccountID: z.coerce.number().default(0),
    deprExpenseAccountID: z.coerce.number().default(0),
    // IT Details
    itModel: z.string().optional().nullable().or(z.literal("")),
    itManufacturer: z.string().optional().nullable().or(z.literal("")),
    itProcessorType: z.string().optional().nullable().or(z.literal("")),
    itProcessorSpeed: z.string().optional().nullable().or(z.literal("")),
    itRamSize: z.string().optional().nullable().or(z.literal("")),
    itHardDiskType: z.string().optional().nullable().or(z.literal("")),
    itHardDiskSize: z.string().optional().nullable().or(z.literal("")),
    itGpuModel: z.string().optional().nullable().or(z.literal("")),
    itScreenSize: z.string().optional().nullable().or(z.literal("")),
    itScreenResolution: z.string().optional().nullable().or(z.literal("")),
    itOperatingSystem: z.string().optional().nullable().or(z.literal("")),
    itHasWebcam: z.boolean().default(false),
    itHasWifi: z.boolean().default(true),
    itHasBluetooth: z.boolean().default(true),
    itIpAddress: z.string().optional().nullable().or(z.literal("")),
    itMacAddress: z.string().optional().nullable().or(z.literal("")),
    itWarrantyExpiry: z.string().optional().nullable().or(z.literal("")),
    itNotes: z.string().optional().nullable().or(z.literal("")),
    // Vehicle Details
    vPlateNumber: z.string().optional().nullable().or(z.literal("")),
    vPlateLetter1: z.string().optional().nullable().or(z.literal("")),
    vPlateLetter2: z.string().optional().nullable().or(z.literal("")),
    vPlateLetter3: z.string().optional().nullable().or(z.literal("")),
    vEngineNumber: z.string().optional().nullable().or(z.literal("")),
    vChassisNumber: z.string().optional().nullable().or(z.literal("")),
    vTransmissionType: z.string().optional().nullable().or(z.literal("")),
    vFuelType: z.string().optional().nullable().or(z.literal("")),
    vCylinderCount: z.coerce.number().optional().nullable(),
    vEngineCapacity: z.string().optional().nullable().or(z.literal("")),
    vHorsePower: z.coerce.number().optional().nullable(),
    vWheelCount: z.coerce.number().default(4),
    vTireSize: z.string().optional().nullable().or(z.literal("")),
    vColor: z.string().optional().nullable().or(z.literal("")),
    vModelYear: z.coerce.number().optional().nullable(),
    vRegistrationExpiry: z.string().optional().nullable().or(z.literal("")),
    vInsuranceExpiry: z.string().optional().nullable().or(z.literal("")),
    vMileage: z.coerce.number().default(0),
    vHasGps: z.boolean().default(false),
    vNotes: z.string().optional().nullable().or(z.literal("")),
    // Building Details
    bAreaSqm: z.coerce.number().optional().nullable(),
    bBuiltAreaSqm: z.coerce.number().optional().nullable(),
    bFloorCount: z.coerce.number().default(1),
    bOwnershipDocNumber: z.string().optional().nullable().or(z.literal("")),
    bLicenseNumber: z.string().optional().nullable().or(z.literal("")),
    bNotes: z.string().optional().nullable().or(z.literal("")),
    // Land Details
    lPlotNumber: z.string().optional().nullable().or(z.literal("")),
    lLandAreaSqm: z.coerce.number().optional().nullable(),
    lOwnershipDocNumber: z.string().optional().nullable().or(z.literal("")),
    lOwnershipDocDate: z.string().optional().nullable().or(z.literal("")),
    lNotes: z.string().optional().nullable().or(z.literal("")),
    // Furniture Details
    fMaterial: z.string().optional().nullable().or(z.literal("")),
    fColor: z.string().optional().nullable().or(z.literal("")),
    fNotes: z.string().optional().nullable().or(z.literal("")),
});

type FormData = z.infer<typeof schema>;

// ═══════════════════════════════════════
// Props
// ═══════════════════════════════════════

interface FixedAssetFormDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialData?: Partial<FixedAssetDto>;
    mode: "create" | "edit";
    /** تخطي القيد المحاسبي — يُستخدم عند الإضافة من صفحة الأرصدة الافتتاحية */
    skipJournalEntry?: boolean;
    onSuccess?: (asset?: { assetID?: number; assetAccountID: number; unitPrice: number; quantity: number; assetName: string; acquiredCost: number }) => void;
}

// ═══════════════════════════════════════
// Component
// ═══════════════════════════════════════

export function FixedAssetFormDialog({
    open,
    onOpenChange,
    initialData,
    mode,
    skipJournalEntry,
    onSuccess
}: FixedAssetFormDialogProps) {
    // ─── Fresh data in edit mode ────────────────────────
    const editId = mode === "edit" ? initialData?.assetID : undefined;
    const { data: freshData } = useQuery({
        queryKey: queryKeys.fixedAssets.detail(editId!),
        queryFn: () => fixedAssetApi.getById(editId!),
        enabled: !!editId && open,
    });

    const effectiveData = useMemo(() => {
        if (mode === "edit" && freshData) return freshData as FixedAssetDto;
        return initialData;
    }, [mode, freshData, initialData]);

    // ─── Lookups ────────────────────────────────────────
    const { data: types = [] } = useQuery({
        queryKey: queryKeys.fixedAssets.types,
        queryFn: () => fixedAssetApi.getTypes(),
        staleTime: 5 * 60 * 1000,
        enabled: open,
    });

    const [selectedTypeId, setSelectedTypeId] = useState<number>(effectiveData?.assetTypeID || 0);

    const { data: categories = [] } = useQuery({
        queryKey: queryKeys.fixedAssets.categories.byType(selectedTypeId),
        queryFn: () => fixedAssetApi.getCategories(selectedTypeId || undefined),
        enabled: open
    });

    // ─── Form ───────────────────────────────────────────
    const form = useForm<FormData>({
        resolver: zodResolver(schema) as Resolver<FormData>,
        defaultValues: useMemo(() => ({
            assetName: "",
            description: "",
            categoryID: 0,
            assetTypeID: 0,
            serialNumber: "",
            purchaseDate: "",
            inServiceDate: "",
            unitPrice: 0,
            salvageValue: 0,
            currencyCode: "YER",
            quantity: 1,
            depreciationRate: 20,
            valuationMethod: 0,
            valuationNotes: "",
            location: "",
            assetAccountID: 0,
            accumDeprAccountID: 0,
            deprExpenseAccountID: 0,
            itHasWebcam: false,
            itHasWifi: true,
            itHasBluetooth: true,
            vWheelCount: 4,
            vMileage: 0,
            vHasGps: false,
            bFloorCount: 1,
        }), []),
    });

    const { register, reset, watch, setValue, control, formState: { errors, isDirty } } = form;

    // ─── Initial values for BaseFormDialog reset ────────
    const createDefaults = useMemo(() => ({
        assetName: "",
        description: "",
        categoryID: 0,
        assetTypeID: 0,
        serialNumber: "",
        purchaseDate: "",
        inServiceDate: "",
        unitPrice: 0,
        salvageValue: 0,
        currencyCode: "YER",
        quantity: 1,
        depreciationRate: 20,
        valuationMethod: 0,
        valuationNotes: "",
        location: "",
        assetAccountID: 0,
        accumDeprAccountID: 0,
        deprExpenseAccountID: 0,
        itHasWebcam: false,
        itHasWifi: true,
        itHasBluetooth: true,
        vWheelCount: 4,
        vMileage: 0,
        vHasGps: false,
        bFloorCount: 1,
    }), []);

    const initialValues = useMemo(() => {
        const asset = effectiveData;
        if (!asset) {
            if (skipJournalEntry) return { ...createDefaults, valuationMethod: 2 };
            return createDefaults;
        }
        return {
            assetName: asset.assetName || "",
            description: asset.description || "",
            categoryID: asset.categoryID || 0,
            assetTypeID: asset.assetTypeID || 0,
            serialNumber: asset.serialNumber || "",
            purchaseDate: asset.purchaseDate?.split("T")[0] || "",
            inServiceDate: asset.inServiceDate?.split("T")[0] || "",
            unitPrice: asset.unitPrice || 0,
            salvageValue: asset.salvageValue || 0,
            currencyCode: asset.currencyCode || "YER",
            quantity: asset.quantity || 1,
            depreciationRate: asset.depreciationRate || 20,
            valuationMethod: asset.valuationMethod || 0,
            valuationNotes: asset.valuationNotes || "",
            location: asset.location || "",
            assetAccountID: asset.assetAccountID || 0,
            accumDeprAccountID: asset.accumDeprAccountID || 0,
            deprExpenseAccountID: asset.deprExpenseAccountID || 0,
            itModel: asset.itDetail?.model || "",
            itManufacturer: asset.itDetail?.manufacturer || "",
            itProcessorType: asset.itDetail?.processorType || "",
            itProcessorSpeed: asset.itDetail?.processorSpeed || "",
            itRamSize: asset.itDetail?.ramSize || "",
            itHardDiskType: asset.itDetail?.hardDiskType || "",
            itHardDiskSize: asset.itDetail?.hardDiskSize || "",
            itGpuModel: asset.itDetail?.gpuModel || "",
            itScreenSize: asset.itDetail?.screenSize || "",
            itScreenResolution: asset.itDetail?.screenResolution || "",
            itOperatingSystem: asset.itDetail?.operatingSystem || "",
            itHasWebcam: asset.itDetail?.hasWebcam || false,
            itHasWifi: asset.itDetail?.hasWifi || true,
            itHasBluetooth: asset.itDetail?.hasBluetooth || true,
            itIpAddress: asset.itDetail?.ipAddress || "",
            itMacAddress: asset.itDetail?.macAddress || "",
            itWarrantyExpiry: asset.itDetail?.warrantyExpiry?.split("T")[0] || "",
            itNotes: asset.itDetail?.notes || "",
            vPlateNumber: asset.vehicleDetail?.plateNumber || "",
            vPlateLetter1: asset.vehicleDetail?.plateLetter1 || "",
            vPlateLetter2: asset.vehicleDetail?.plateLetter2 || "",
            vPlateLetter3: asset.vehicleDetail?.plateLetter3 || "",
            vEngineNumber: asset.vehicleDetail?.engineNumber || "",
            vChassisNumber: asset.vehicleDetail?.chassisNumber || "",
            vTransmissionType: asset.vehicleDetail?.transmissionType || "",
            vFuelType: asset.vehicleDetail?.fuelType || "",
            vCylinderCount: asset.vehicleDetail?.cylinderCount || 0,
            vEngineCapacity: asset.vehicleDetail?.engineCapacity || "",
            vHorsePower: asset.vehicleDetail?.horsePower || 0,
            vWheelCount: asset.vehicleDetail?.wheelCount || 4,
            vTireSize: asset.vehicleDetail?.tireSize || "",
            vColor: asset.vehicleDetail?.color || "",
            vModelYear: asset.vehicleDetail?.modelYear || 0,
            vRegistrationExpiry: asset.vehicleDetail?.registrationExpiry?.split("T")[0] || "",
            vInsuranceExpiry: asset.vehicleDetail?.insuranceExpiry?.split("T")[0] || "",
            vMileage: asset.vehicleDetail?.mileage || 0,
            vHasGps: asset.vehicleDetail?.hasGps || false,
            vNotes: asset.vehicleDetail?.notes || "",
            bAreaSqm: asset.buildingDetail?.areaSqm || 0,
            bBuiltAreaSqm: asset.buildingDetail?.builtAreaSqm || 0,
            bFloorCount: asset.buildingDetail?.floorCount || 1,
            bOwnershipDocNumber: asset.buildingDetail?.ownershipDocNumber || "",
            bLicenseNumber: asset.buildingDetail?.licenseNumber || "",
            bNotes: asset.buildingDetail?.notes || "",
            lPlotNumber: asset.landDetail?.plotNumber || "",
            lLandAreaSqm: asset.landDetail?.landAreaSqm || 0,
            lOwnershipDocNumber: asset.landDetail?.ownershipDocNumber || "",
            lOwnershipDocDate: asset.landDetail?.ownershipDocDate?.split("T")[0] || "",
            lNotes: asset.landDetail?.notes || "",
            fMaterial: asset.furnitureDetail?.material || "",
            fColor: asset.furnitureDetail?.color || "",
            fNotes: asset.furnitureDetail?.notes || "",
        };
    }, [effectiveData, createDefaults, skipJournalEntry]);

    // ─── Mutation ───────────────────────────────────────
    const mutation = useApiMutation(
        (values: FormData) => {
<<<<<<< HEAD
            const payload = buildPayload(values, selectedTypeId);
=======
            let payload = buildPayload(values, selectedTypeId);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            const cat = categories.find(c => c.categoryID === values.categoryID);
            if (cat) {
                if (!payload.assetAccountID || payload.assetAccountID === 0) payload.assetAccountID = cat.assetAccountID || 0;
                if (!payload.accumDeprAccountID || payload.accumDeprAccountID === 0) payload.accumDeprAccountID = cat.accumDeprAccountID || 0;
                if (!payload.deprExpenseAccountID || payload.deprExpenseAccountID === 0) payload.deprExpenseAccountID = cat.deprExpenseAccountID || 0;
            }
            if (mode === "create") {
                return fixedAssetApi.create({ ...payload, skipJournalEntry: skipJournalEntry ?? false });
            }
            return fixedAssetApi.update(effectiveData!.assetID!, payload);
        },
        {
            successMessage: mode === "create" ? "تم إنشاء الأصل بنجاح" : "تم تعديل الأصل بنجاح",
            invalidateKeys: [
                ["fixed-assets"],
                ["fixed-asset", editId],
            ],
        }
    );

    const handleFormSubmit = useCallback((values: FormData) => {
        // Ensure assetAccountID is set from category if not already set
        let accountID = values.assetAccountID;
        if (!accountID || accountID === 0) {
            const cat = categories.find(c => c.categoryID === values.categoryID);
            if (cat?.assetAccountID) {
                accountID = cat.assetAccountID;
            }
        }

        // Store values for onSuccess callback
        const assetData = {
            assetID: effectiveData?.assetID,
            assetAccountID: accountID,
            unitPrice: values.unitPrice,
            quantity: values.quantity,
            assetName: values.assetName,
            acquiredCost: values.unitPrice * values.quantity,
        };

        // Update the values with the correct accountID
        const updatedValues = { ...values, assetAccountID: accountID };

        mutation.mutate(updatedValues, {
            onSuccess: () => {
                onOpenChange(false);
                onSuccess?.(assetData);
            },
        });
    }, [mutation, onOpenChange, onSuccess, categories]);

    const watchUnitPrice = watch("unitPrice") ?? 0;
    const watchQty = watch("quantity") ?? 1;
    const watchSalvage = watch("salvageValue") ?? 0;
    const watchRate = watch("depreciationRate") ?? 0;
    const watchTotalCost = watchUnitPrice * watchQty;
    const annualDepr = watchRate > 0 ? ((watchTotalCost - watchSalvage) * watchRate / 100) : 0;
    const monthlyDepr = annualDepr / 12;

    const typeOptions = types.filter(t => t.isActive).map(t => ({
        value: String(t.assetTypeID),
        label: t.typeName,
    }));

    const catOptions = categories.filter(c => c.isActive).map(c => ({
        value: String(c.categoryID),
        label: `${c.categoryName} — ${c.depreciationRate}%`,
    }));

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title={mode === "create" ? "إضافة أصل جديد" : `تعديل الأصل: ${effectiveData?.assetName}`}
            description="بيانات الأصل الثابت وتفاصيله"
            size="lg"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={initialValues}
            showGlobalError
            formErrors={errors}
            submitLabel={mode === "create" ? "إضافة الأصل" : "حفظ التعديلات"}
        >
            <div className="space-y-4">
                {/* Basic Info */}
                <FormSection title="البيانات الأساسية" icon={Wrench}>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                        <FormField label="نوع الأصل" required error={errors.assetTypeID?.message}>
                            <NativeSelect
                                options={typeOptions}
                                placeholder="اختر النوع..."
                                value={watch("assetTypeID")}
                                onChange={(val) => {
                                    const v = Number(val);
                                    setValue("assetTypeID", v, { shouldValidate: true });
                                    setSelectedTypeId(v);
                                    setValue("categoryID", 0, { shouldValidate: true });
                                }}
                            />
                        </FormField>

                        <FormField label="التصنيف" required error={errors.categoryID?.message}>
                            <NativeSelect
                                options={catOptions}
                                placeholder="اختر التصنيف..."
                                value={watch("categoryID")}
                                onChange={(val) => {
                                    const catId = Number(val);
                                    setValue("categoryID", catId, { shouldValidate: true });
                                    const selectedCat = categories.find(c => c.categoryID === catId);
                                    if (selectedCat) {
                                        if (selectedCat.assetAccountID) setValue("assetAccountID", selectedCat.assetAccountID);
                                        if (selectedCat.accumDeprAccountID) setValue("accumDeprAccountID", selectedCat.accumDeprAccountID);
                                        if (selectedCat.deprExpenseAccountID) setValue("deprExpenseAccountID", selectedCat.deprExpenseAccountID);
                                        if (selectedCat.depreciationRate) setValue("depreciationRate", selectedCat.depreciationRate, { shouldValidate: true });
                                        const unitPrice = watch("unitPrice");
                                        const qty = watch("quantity");
                                        const totalCost = unitPrice * qty;
                                        if (totalCost > 0 && selectedCat.salvagePercent > 0) {
                                            setValue("salvageValue", Math.round(totalCost * selectedCat.salvagePercent / 100), { shouldValidate: true });
                                        }
                                    }
                                }}
                            />
                        </FormField>

                        <FormField label="اسم الأصل" required error={errors.assetName?.message}>
                            <Input {...register("assetName")} placeholder="اسم الأصل" />
                        </FormField>

                        <FormField label="الرقم التسلسلي" error={errors.serialNumber?.message}>
                            <Input {...register("serialNumber")} placeholder="اختياري" />
                        </FormField>

                        <FormField label="تاريخ الشراء" required error={errors.purchaseDate?.message}>
                            <Controller
                                control={control}
                                name="purchaseDate"
                                render={({ field }) => (
                                    <DateInput
                                        value={field.value ?? undefined}
                                        onChange={(v) => field.onChange(v || "")}
                                        onBlur={field.onBlur}
                                    />
                                )}
                            />
                            <input type="hidden" {...register("purchaseDate")} />
                        </FormField>
                        <FormField label="تاريخ التشغيل" required icon={Calendar} error={errors.inServiceDate?.message}>
                            <Controller
                                control={control}
                                name="inServiceDate"
                                render={({ field }) => (
                                    <DateInput
                                        value={field.value ?? undefined}
                                        onChange={(v) => field.onChange(v || "")}
                                        onBlur={field.onBlur}
                                    />
                                )}
                            />
                        </FormField>

                        <FormField label={skipJournalEntry ? "القيمة المقدرة (ر.ي)" : "سعر الوحدة (ر.ي)"} required error={errors.unitPrice?.message}>
                            <Input type="number" step="0.01" {...register("unitPrice")} placeholder={skipJournalEntry ? "القيمة التقديرية للأصل" : "سعر شراء الوحدة"} />
                        </FormField>

                        <FormField label="الكمية" required error={errors.quantity?.message}>
                            <Input type="number" {...register("quantity")} />
                        </FormField>

                        <FormField label="قيمة التخلي (ر.ي)" error={errors.salvageValue?.message}>
                            <Input type="number" step="0.01" {...register("salvageValue")} />
                        </FormField>

                        <FormField label="نسبة الإهلاك السنوية (%)" required error={errors.depreciationRate?.message}>
                            <Input type="number" step="0.01" {...register("depreciationRate")} placeholder="مثال: 20" />
                        </FormField>

                        {skipJournalEntry && (
                            <>
                                <FormField label="الإهلاك المتراكم (ر.ي)" error={errors.initialAccumulatedDepreciation?.message}>
                                    <Input type="number" step="0.01" {...register("initialAccumulatedDepreciation")} />
                                </FormField>
                            </>
                        )}

                        <FormField label="الموقع" error={errors.location?.message}>
                            <Input {...register("location")} placeholder="اختياري" />
                        </FormField>

                        <FormField label="ملاحظات التقييم" error={errors.valuationNotes?.message} className="md:col-span-2 lg:col-span-3">
                            <Input {...register("valuationNotes")} placeholder="سبب استخدام القيمة المقدرة، مرجع التقييم، إلخ" />
                        </FormField>
                    </div>
                </FormSection>

                {/* Total Cost Display */}
                <div className="bg-primary/5 rounded-lg p-4">
                    <div className="text-sm text-muted-foreground mb-2 flex items-center gap-2">
                        <CircleDollarSign className="w-4 h-4" /> {skipJournalEntry ? "القيمة الإجمالية المقدرة" : "التكلفة الإجمالية"}
                    </div>
                    <div className="text-center">
                        <div className="text-xl md:text-3xl font-bold text-primary">{Math.round(watchTotalCost).toLocaleString("ar-YE")}</div>
                        <div className="text-xs text-muted-foreground mt-1">
                            {watchUnitPrice.toLocaleString("ar-YE")} ر.ي{skipJournalEntry ? " (قيمة مقدرة)" : ""} × {watchQty} = {watchTotalCost.toLocaleString("ar-YE")} ر.ي
                        </div>
                    </div>
                </div>

                {/* Depreciation Preview */}
                <div className="bg-muted/50 rounded-lg p-4">
                    <div className="text-sm text-muted-foreground mb-2 flex items-center gap-2">
                        <CircleDollarSign className="w-4 h-4" /> تقدير الإهلاك الشهري
                    </div>
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 text-center">
                        <div>
                            <div className="text-2xl font-bold text-primary">{Math.round(monthlyDepr).toLocaleString("ar-YE")}</div>
                            <div className="text-xs text-muted-foreground">ر.ي / شهر</div>
                        </div>
                        <div>
                            <div className="text-2xl font-bold">{Math.round(annualDepr).toLocaleString("ar-YE")}</div>
                            <div className="text-xs text-muted-foreground">ر.ي / سنة</div>
                        </div>
                        <div>
                            <div className="text-2xl font-bold">{watchRate}%</div>
                            <div className="text-xs text-muted-foreground">نسبة الإهلاك السنوية</div>
                        </div>
                    </div>
                </div>

                {/* Accounting Info (auto-filled from category) */}
                {watch("categoryID") > 0 && (() => {
                    const selectedCat = categories.find(c => c.categoryID === watch("categoryID"));
                    if (!selectedCat) return null;
                    return (
                        <div className="bg-blue-50 dark:bg-blue-950/30 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
                            <div className="text-sm font-semibold mb-2 flex items-center gap-2 text-blue-700 dark:text-blue-300">
                                <BookOpen className="w-4 h-4" /> الحسابات المحاسبية (من التصنيف)
                            </div>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-3 text-sm">
                                <div>
                                    <span className="text-muted-foreground">حساب الأصل:</span>
                                    <div className="font-semibold">{selectedCat.assetAccountCode} — {selectedCat.assetAccountName}</div>
                                </div>
                                <div>
                                    <span className="text-muted-foreground">حساب الإهلاك المتراكم:</span>
                                    <div className="font-semibold">{selectedCat.accumDeprAccountName || "—"}</div>
                                </div>
                                <div>
                                    <span className="text-muted-foreground">حساب مصروف الإهلاك:</span>
                                    <div className="font-semibold">{selectedCat.deprExpenseAccountName || "—"}</div>
                                </div>
                            </div>
                        </div>
                    );
                })()}

                {/* Dynamic Details per Type */}
                {selectedTypeId === 0 && <ITDetailsSection register={register} watch={watch} setValue={setValue} control={control} />}
                {selectedTypeId === 1 && <VehicleDetailsSection register={register} watch={watch} setValue={setValue} errors={errors} control={control} />}
                {selectedTypeId === 2 && <BuildingDetailsSection register={register} errors={errors} />}
                {selectedTypeId === 3 && <LandDetailsSection register={register} errors={errors} control={control} />}
                {selectedTypeId === 4 && <FurnitureDetailsSection register={register} errors={errors} />}
            </div>
        </BaseFormDialog>
    );
}

// ═══════════════════════════════════════
// Detail Sections
// ═══════════════════════════════════════

function ITDetailsSection({ register, watch, setValue, control }: {
    register: ReturnType<typeof useForm<FormData>>["register"];
    watch: ReturnType<typeof useForm<FormData>>["watch"];
    setValue: ReturnType<typeof useForm<FormData>>["setValue"];
    control: ReturnType<typeof useForm<FormData>>["control"];
}) {
    return (
        <FormSection title="معلومات تقنية" icon={Laptop}>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <FormField label="الموديل"><Input {...register("itModel")} /></FormField>
                <FormField label="الشركة المصنعة"><Input {...register("itManufacturer")} /></FormField>
                <FormField label="المعالج"><Input {...register("itProcessorType")} /></FormField>
                <FormField label="سرعة المعالج"><Input {...register("itProcessorSpeed")} /></FormField>
                <FormField label="الرام"><Input {...register("itRamSize")} /></FormField>
                <FormField label="نوع القرص"><Input {...register("itHardDiskType")} /></FormField>
                <FormField label="حجم القرص"><Input {...register("itHardDiskSize")} /></FormField>
                <FormField label="كرت الشاشة"><Input {...register("itGpuModel")} /></FormField>
                <FormField label="حجم الشاشة"><Input {...register("itScreenSize")} /></FormField>
                <FormField label="دقة الشاشة"><Input {...register("itScreenResolution")} /></FormField>
                <FormField label="نظام التشغيل"><Input {...register("itOperatingSystem")} /></FormField>
                <FormField label="عنوان IP"><Input {...register("itIpAddress")} /></FormField>
                <FormField label="عنوان MAC"><Input {...register("itMacAddress")} /></FormField>
                <FormField label="انتهاء الضمان">
                    <Controller
                        control={control}
                        name="itWarrantyExpiry"
                        render={({ field }) => (
                            <DateInput
                                value={field.value ?? undefined}
                                onChange={(v) => field.onChange(v || "")}
                                onBlur={field.onBlur}
                            />
                        )}
                    />
                </FormField>
                <FormField label="ملاحظات"><Input {...register("itNotes")} /></FormField>
            </div>
            <div className="flex gap-6 mt-4">
                <label className="flex items-center gap-2 text-sm">
                    <Checkbox checked={watch("itHasWebcam")} onCheckedChange={(checked) => setValue("itHasWebcam", !!checked)} /> كاميرا
                </label>
                <label className="flex items-center gap-2 text-sm">
                    <Checkbox checked={watch("itHasWifi")} onCheckedChange={(checked) => setValue("itHasWifi", !!checked)} /> WiFi
                </label>
                <label className="flex items-center gap-2 text-sm">
                    <Checkbox checked={watch("itHasBluetooth")} onCheckedChange={(checked) => setValue("itHasBluetooth", !!checked)} /> بلوتوث
                </label>
            </div>
        </FormSection>
    );
}

function VehicleDetailsSection({ register, watch, setValue, control }: {
    register: ReturnType<typeof useForm<FormData>>["register"];
    watch: ReturnType<typeof useForm<FormData>>["watch"];
    setValue: ReturnType<typeof useForm<FormData>>["setValue"];
    control: ReturnType<typeof useForm<FormData>>["control"];
    errors: ReturnType<typeof useForm<FormData>>["formState"]["errors"];
}) {
    return (
        <FormSection title="معلومات المركبة" icon={Truck}>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <FormField label="رقم اللوحة"><Input {...register("vPlateNumber")} /></FormField>
                <FormField label="حرف 1"><Input {...register("vPlateLetter1")} /></FormField>
                <FormField label="حرف 2"><Input {...register("vPlateLetter2")} /></FormField>
                <FormField label="حرف 3"><Input {...register("vPlateLetter3")} /></FormField>
                <FormField label="رقم المحرّك"><Input {...register("vEngineNumber")} /></FormField>
                <FormField label="رقم الهيكل"><Input {...register("vChassisNumber")} /></FormField>
                <FormField label="نوع الناقل">
                    <NativeSelect
                        options={[{ value: "أوتوماتيك", label: "أوتوماتيك" }, { value: "يدوي", label: "يدوي" }]}
                        placeholder="اختر..."
                        value={watch("vTransmissionType") || ""}
                        onChange={(val) => setValue("vTransmissionType", val)}
                    />
                </FormField>
                <FormField label="نوع الوقود">
                    <NativeSelect
                        options={[{ value: "بنزين", label: "بنزين" }, { value: "ديزل", label: "ديزل" }, { value: "كهرباء", label: "كهرباء" }, { value: "هايبرد", label: "هايبرد" }]}
                        placeholder="اختر..."
                        value={watch("vFuelType") || ""}
                        onChange={(val) => setValue("vFuelType", val)}
                    />
                </FormField>
                <FormField label="عدد الأسطوانات"><Input type="number" {...register("vCylinderCount")} /></FormField>
                <FormField label="سعة المحرّك"><Input {...register("vEngineCapacity")} /></FormField>
                <FormField label="الحصان"><Input type="number" {...register("vHorsePower")} /></FormField>
                <FormField label="اللون"><Input {...register("vColor")} /></FormField>
                <FormField label="سنة الصنع"><Input type="number" {...register("vModelYear")} /></FormField>
                <FormField label="انتهاء التسجيل">
                    <Controller
                        control={control}
                        name="vRegistrationExpiry"
                        render={({ field }) => (
                            <DateInput
                                value={field.value ?? undefined}
                                onChange={(v) => field.onChange(v || "")}
                                onBlur={field.onBlur}
                            />
                        )}
                    />
                </FormField>
                <FormField label="انتهاء التأمين">
                    <Controller
                        control={control}
                        name="vInsuranceExpiry"
                        render={({ field }) => (
                            <DateInput
                                value={field.value ?? undefined}
                                onChange={(v) => field.onChange(v || "")}
                                onBlur={field.onBlur}
                            />
                        )}
                    />
                </FormField>
                <FormField label="المسافة (كم)"><Input type="number" {...register("vMileage")} /></FormField>
                <FormField label="مقاس الإطارات"><Input {...register("vTireSize")} /></FormField>
            </div>
            <div className="mt-4">
                <label className="flex items-center gap-2 text-sm">
                    <Checkbox checked={watch("vHasGps")} onCheckedChange={(checked) => setValue("vHasGps", !!checked)} /> GPS
                </label>
            </div>
        </FormSection>
    );
}

function BuildingDetailsSection({ register }: {
    register: ReturnType<typeof useForm<FormData>>["register"];
    errors: ReturnType<typeof useForm<FormData>>["formState"]["errors"];
}) {
    return (
        <FormSection title="معلومات المبنى" icon={Building2}>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <FormField label="المساحة (م²)"><Input type="number" step="0.01" {...register("bAreaSqm")} /></FormField>
                <FormField label="المساحة المبنية (م²)"><Input type="number" step="0.01" {...register("bBuiltAreaSqm")} /></FormField>
                <FormField label="عدد الطوابق"><Input type="number" {...register("bFloorCount")} /></FormField>
                <FormField label="رقم السجل العقاري"><Input {...register("bOwnershipDocNumber")} /></FormField>
                <FormField label="رقم الترخيص"><Input {...register("bLicenseNumber")} /></FormField>
                <FormField label="ملاحظات"><Input {...register("bNotes")} /></FormField>
            </div>
        </FormSection>
    );
}

function LandDetailsSection({ register, control }: {
    register: ReturnType<typeof useForm<FormData>>["register"];
    control: ReturnType<typeof useForm<FormData>>["control"];
    errors: ReturnType<typeof useForm<FormData>>["formState"]["errors"];
}) {
    return (
        <FormSection title="معلومات الأرض" icon={LandPlot}>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <FormField label="رقم القطعة"><Input {...register("lPlotNumber")} /></FormField>
                <FormField label="المساحة (م²)"><Input type="number" step="0.01" {...register("lLandAreaSqm")} /></FormField>
                <FormField label="رقم السجل العقاري"><Input {...register("lOwnershipDocNumber")} /></FormField>
                <FormField label="تاريخ السجل العقاري">
                    <Controller
                        control={control}
                        name="lOwnershipDocDate"
                        render={({ field }) => (
                            <DateInput
                                value={field.value ?? undefined}
                                onChange={(v) => field.onChange(v || "")}
                                onBlur={field.onBlur}
                            />
                        )}
                    />
                </FormField>
                <FormField label="ملاحظات"><Input {...register("lNotes")} /></FormField>
            </div>
        </FormSection>
    );
}

function FurnitureDetailsSection({ register }: {
    register: ReturnType<typeof useForm<FormData>>["register"];
    errors: ReturnType<typeof useForm<FormData>>["formState"]["errors"];
}) {
    return (
        <FormSection title="معلومات الأثاث" icon={Sofa}>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <FormField label="المادة"><Input {...register("fMaterial")} /></FormField>
                <FormField label="اللون"><Input {...register("fColor")} /></FormField>
                <FormField label="ملاحظات"><Input {...register("fNotes")} /></FormField>
            </div>
        </FormSection>
    );
}

// ═══════════════════════════════════════
// Payload Builder
// ═══════════════════════════════════════

function buildPayload(data: FormData, typeId: number) {
    const base: Record<string, unknown> = {
        assetName: data.assetName,
        description: data.description || null,
        categoryID: data.categoryID,
        assetTypeID: data.assetTypeID,
        serialNumber: data.serialNumber || null,
        purchaseDate: data.purchaseDate,
        inServiceDate: data.inServiceDate,
        unitPrice: data.unitPrice,
        acquiredCost: data.unitPrice * data.quantity,
        salvageValue: data.salvageValue,
        currencyCode: data.currencyCode,
        quantity: data.quantity,
        depreciationRate: data.depreciationRate,
        valuationMethod: data.valuationMethod,
        valuationNotes: data.valuationNotes || null,
        initialAccumulatedDepreciation: data.initialAccumulatedDepreciation || 0,
        location: data.location || null,
        assetAccountID: data.assetAccountID,
        accumDeprAccountID: data.accumDeprAccountID,
        deprExpenseAccountID: data.deprExpenseAccountID,
    };

    switch (typeId) {
        case 0: // IT
            base.itDetail = {
                model: data.itModel || null,
                manufacturer: data.itManufacturer || null,
                processorType: data.itProcessorType || null,
                processorSpeed: data.itProcessorSpeed || null,
                ramSize: data.itRamSize || null,
                hardDiskType: data.itHardDiskType || null,
                hardDiskSize: data.itHardDiskSize || null,
                gpuModel: data.itGpuModel || null,
                screenSize: data.itScreenSize || null,
                screenResolution: data.itScreenResolution || null,
                operatingSystem: data.itOperatingSystem || null,
                hasWebcam: data.itHasWebcam,
                hasWifi: data.itHasWifi,
                hasBluetooth: data.itHasBluetooth,
                ipAddress: data.itIpAddress || null,
                macAddress: data.itMacAddress || null,
                warrantyExpiry: data.itWarrantyExpiry || null,
                notes: data.itNotes || null,
            };
            break;
        case 1: // Vehicle
            base.vehicleDetail = {
                plateNumber: data.vPlateNumber || null,
                plateLetter1: data.vPlateLetter1 || null,
                plateLetter2: data.vPlateLetter2 || null,
                plateLetter3: data.vPlateLetter3 || null,
                engineNumber: data.vEngineNumber || null,
                chassisNumber: data.vChassisNumber || null,
                transmissionType: data.vTransmissionType || null,
                fuelType: data.vFuelType || null,
                cylinderCount: data.vCylinderCount || null,
                engineCapacity: data.vEngineCapacity || null,
                horsePower: data.vHorsePower || null,
                wheelCount: data.vWheelCount,
                tireSize: data.vTireSize || null,
                color: data.vColor || null,
                modelYear: data.vModelYear || null,
                registrationExpiry: data.vRegistrationExpiry || null,
                insuranceExpiry: data.vInsuranceExpiry || null,
                mileage: data.vMileage,
                hasGps: data.vHasGps,
                notes: data.vNotes || null,
            };
            break;
        case 2: // Building
            base.buildingDetail = {
                areaSqm: data.bAreaSqm || null,
                builtAreaSqm: data.bBuiltAreaSqm || null,
                floorCount: data.bFloorCount,
                ownershipDocNumber: data.bOwnershipDocNumber || null,
                licenseNumber: data.bLicenseNumber || null,
                notes: data.bNotes || null,
            };
            break;
        case 3: // Land
            base.landDetail = {
                plotNumber: data.lPlotNumber || null,
                landAreaSqm: data.lLandAreaSqm || null,
                ownershipDocNumber: data.lOwnershipDocNumber || null,
                ownershipDocDate: data.lOwnershipDocDate || null,
                notes: data.lNotes || null,
            };
            break;
        case 4: // Furniture
            base.furnitureDetail = {
                material: data.fMaterial || null,
                color: data.fColor || null,
                notes: data.fNotes || null,
            };
            break;
    }

    return base;
}
