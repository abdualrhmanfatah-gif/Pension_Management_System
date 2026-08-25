import { Building2, Calendar, DollarSign, User, Wrench } from "lucide-react";

import type { FixedAssetDto } from "@/api/fixedAssets";
import { VALUATION_METHODS } from "@/api/fixedAssets";
import { Card, CardContent, CardHeader, CardTitle, InfoPair } from "@/components/ui";
import { formatCurrency, formatDate } from "@/lib/utils";
import { ASSET_STATUS_LABELS } from "@/constants/assets";

interface AssetInfoTabProps {
    asset: FixedAssetDto;
}

export function AssetInfoTab({ asset }: AssetInfoTabProps) {
    const status = ASSET_STATUS_LABELS[asset.status] || "—";

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="flex items-center gap-2 text-sm">
                        <Building2 className="w-4 h-4" />
                        بيانات الأصل
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <InfoPair label="رقم الأصل" value={asset.assetNumber} />
                    <InfoPair label="اسم الأصل" value={asset.assetName} />
                    <InfoPair label="الوصف" value={asset.description || "—"} />
                    <InfoPair label="الرقم التسلسلي" value={asset.serialNumber || "—"} />
                    <InfoPair label="الموقع" value={asset.location || "—"} />
                    <InfoPair label="الحالة" value={status} />
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="flex items-center gap-2 text-sm">
                        <Calendar className="w-4 h-4" />
                        التواريخ
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <InfoPair label="تاريخ الشراء" value={formatDate(asset.purchaseDate)} />
                    <InfoPair label="تاريخ التشغيل" value={formatDate(asset.inServiceDate)} />
                    <InfoPair label="آخر إهلاك" value={asset.lastDepreciationDate ? formatDate(asset.lastDepreciationDate) : "لم يتم بعد"} />
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="flex items-center gap-2 text-sm">
                        <DollarSign className="w-4 h-4" />
                        القيم المالية
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <InfoPair label="طريقة التقييم" value={VALUATION_METHODS[asset.valuationMethod] || "—"} />
                    <InfoPair label="سعر الوحدة" value={formatCurrency(asset.unitPrice)} />
                    <InfoPair label="الكمية" value={asset.quantity} />
                    <InfoPair label="التكلفة الإجمالية" value={formatCurrency(asset.acquiredCost)} />
                    <InfoPair label="قيمة التخلي" value={formatCurrency(asset.salvageValue)} />
                    <InfoPair label="الإهلاك المتراكم" value={formatCurrency(asset.accumulatedDepreciation)} />
                    <InfoPair label="القيمة الدفترية" value={formatCurrency(asset.netBookValue)} />
                    {asset.valuationNotes && <InfoPair label="ملاحظات التقييم" value={asset.valuationNotes} />}
                </CardContent>
            </Card>

            <Card>
                <CardHeader className="pb-3">
                    <CardTitle className="flex items-center gap-2 text-sm">
                        <Wrench className="w-4 h-4" />
                        الإهلاك
                    </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                    <InfoPair label="نسبة الإهلاك السنوية" value={`${asset.depreciationRate}%`} />
                    <InfoPair label="الكمية" value={asset.quantity} />
                </CardContent>
            </Card>

            {asset.custodianEmployeeName && (
                <Card className="md:col-span-2">
                    <CardHeader className="pb-3">
                        <CardTitle className="flex items-center gap-2 text-sm">
                            <User className="w-4 h-4" />
                            العهدة
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <InfoPair label="المحمل بالعهدة" value={asset.custodianEmployeeName} />
                    </CardContent>
                </Card>
            )}
        </div>
    );
}
