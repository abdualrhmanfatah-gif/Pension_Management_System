import { Filter,RefreshCw } from 'lucide-react';
import React from 'react';

import type { ReportFilter } from '@/api/reports';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui/Card';
import { DateInput,Input } from '@/components/ui/Input';
import { NativeSelect } from '@/components/ui/NativeSelect';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/Select';

type FilterValue = string | number | boolean | null | undefined;

interface ReportFiltersProps {
    filters: string[];
    values: ReportFilter & Record<string, any>;
    onChange: (key: string, value: FilterValue) => void;
    onApply?: () => void;
    showApplyButton?: boolean;
    compact?: boolean;
}

const ACCOUNT_LEVEL_OPTIONS = [
    { value: 'all', label: 'جميع المستويات' },
    { value: '1', label: 'المستوى 1 — الأبواب' },
    { value: '2', label: 'المستوى 2 — الحسابات العامة' },
    { value: '3', label: 'المستوى 3 — الحسابات المساعدة' },
    { value: '4', label: 'المستوى 4 — الحسابات الفرعية' },
    { value: '5', label: 'المستوى 5 — التفاصيل' },
];

const QUARTER_OPTIONS = [
    { value: '1', label: 'الفصل الأول' },
    { value: '2', label: 'الفصل الثاني' },
    { value: '3', label: 'الفصل الثالث' },
    { value: '4', label: 'الفصل الرابع' },
];

const ASSET_TYPE_OPTIONS = [
    { value: '', label: 'الكل' },
    { value: 'Lands', label: 'الأراضي' },
    { value: 'Buildings', label: 'المباني' },
    { value: 'Furniture', label: 'الأثاث والمعدات' },
    { value: 'Vehicles', label: 'المركبات' },
    { value: 'Leasehold', label: 'حقوق الإيجار' },
];

export const ReportFilters: React.FC<ReportFiltersProps> = ({ filters, values, onChange, onApply, showApplyButton = true, compact = false }) => {
    const renderFilter = (key: string) => {
        switch (key) {
            case 'fromDate':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">من تاريخ</label>
                        <DateInput
                            className="h-10"
                            value={values.fromDate || ''}
                            onChange={(val) => onChange('fromDate', val)}
                        />
                    </div>
                );
            case 'toDate':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">إلى تاريخ</label>
                        <DateInput
                            className="h-10"
                            value={values.toDate || ''}
                            onChange={(val) => onChange('toDate', val)}
                        />
                    </div>
                );
            case 'statusId':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">الحالة</label>
                        <NativeSelect
                            options={[
                                { value: '', label: 'الكل' },
                                { value: '1', label: 'نشط / معتمد' },
                                { value: '2', label: 'مغلق / ملغي' }
                            ]}
                            value={values.statusId || ''}
                            onChange={(val) => onChange('statusId', val)}
                        />
                    </div>
                );
            case 'fiscalYear':
                return (
                    <div key={key} className="w-full sm:w-32">
                        <label className="label-filter">السنة المالية</label>
                        <Input
                            type="number"
                            placeholder="2024"
                            className="h-10"
                            value={values.fiscalYear || ''}
                            onChange={(e) => onChange('fiscalYear', e.target.value)}
                        />
                    </div>
                );
            case 'portfolioId':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">رقم المحفظة</label>
                        <Input
                            type="number"
                            className="h-10"
                            value={values.portfolioId || ''}
                            onChange={(e) => onChange('portfolioId', e.target.value)}
                        />
                    </div>
                );
            case 'projectId':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">رقم المشروع</label>
                        <Input
                            type="number"
                            className="h-10"
                            value={values.projectId || ''}
                            onChange={(e) => onChange('projectId', e.target.value)}
                        />
                    </div>
                );
            case 'propertyId':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">رقم العقار</label>
                        <Input
                            type="number"
                            className="h-10"
                            value={values.propertyId || ''}
                            onChange={(e) => onChange('propertyId', e.target.value)}
                        />
                    </div>
                );
            case 'accountLevel':
                return (
                    <div key={key} className="w-full sm:w-52">
                        <label className="label-filter">المستوى</label>
                        <Select
                            value={values.accountLevel || 'all'}
                            onValueChange={(val) => onChange('accountLevel', val)}
                        >
                            <SelectTrigger className="h-10">
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                {ACCOUNT_LEVEL_OPTIONS.map(opt => (
                                    <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                );
            case 'quarter':
                return (
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter">الفصل المالي</label>
                        <Select
                            value={values.quarter || ''}
                            onValueChange={(val) => onChange('quarter', val)}
                        >
                            <SelectTrigger className="h-10">
                                <SelectValue placeholder="الكل" />
                            </SelectTrigger>
                            <SelectContent>
                                {QUARTER_OPTIONS.map(opt => (
                                    <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>
                );
            case 'assetType':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">نوع الأصل</label>
                        <NativeSelect
                            options={ASSET_TYPE_OPTIONS}
                            value={values.assetType || ''}
                            onChange={(val) => onChange('assetType', val)}
                        />
                    </div>
                );
            case 'comparisonYear':
                return (
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter">السنة السابقة</label>
                        <Input
                            type="number"
                            placeholder="2023"
                            className="h-10"
                            value={values.comparisonYear || ''}
                            onChange={(e) => onChange('comparisonYear', e.target.value)}
                        />
                    </div>
                );
            case 'accountCode':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">رقم الحساب</label>
                        <Input
                            type="text"
                            placeholder="مثال: 1101"
                            className="h-10"
                            value={values.accountCode || ''}
                            onChange={(e) => onChange('accountCode', e.target.value)}
                        />
                    </div>
                );
            case 'entityId':
                return (
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter">الكيان</label>
                        <Input
                            type="number"
                            className="h-10"
                            value={values.entityId || ''}
                            onChange={(e) => onChange('entityId', e.target.value)}
                        />
                    </div>
                );
            case 'search':
                return (
                    <div key={key} className="w-full sm:w-56">
                        <label className="label-filter">بحث</label>
                        <Input
                            type="text"
                            placeholder="بحث بالاسم، الرقم،..."
                            className="h-10"
                            value={values.search || ''}
                            onChange={(e) => onChange('search', e.target.value)}
                        />
                    </div>
                );
            default:
                return null;
        }
    };

    return (
        <Card className={`flex flex-wrap gap-4 items-end mb-6 text-start ${compact ? 'p-3' : 'p-5'}`}>
            <div className="flex items-center gap-2 mb-auto self-start pt-2 me-2">
                <div className="p-2 rounded-lg bg-primary/10 text-primary">
                    <Filter className="w-4 h-4" />
                </div>
                <span className="font-bold text-sm">تصفية التقرير</span>
            </div>

            {filters.map(renderFilter)}

            {showApplyButton && onApply && (
                <Button
                    onClick={onApply}
                    className="h-10 px-6"
                >
                    <RefreshCw className="w-4 h-4 ms-2" />
                    تحديث التقرير
                </Button>
            )}
        </Card>
    );
};
