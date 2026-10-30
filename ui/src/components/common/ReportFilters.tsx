import { Filter,RefreshCw } from 'lucide-react';
import React from 'react';

import type { ReportFilter } from '@/api/reports';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui/Card';
<<<<<<< HEAD
import { DateInput,Input } from '@/components/ui/Input';
=======
import { Input, DateInput } from '@/components/ui/Input';
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">من تاريخ</label>
                        <DateInput
                            className="h-11 sm:h-9 [&>input[type=text]]:h-full [&>input[type=text]]:py-1.5 [&>input[type=text]]:text-sm sm:[&>button]:min-h-9 sm:[&>button]:min-w-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">من تاريخ</label>
                        <DateInput
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.fromDate || ''}
                            onChange={(val) => onChange('fromDate', val)}
                        />
                    </div>
                );
            case 'toDate':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">إلى تاريخ</label>
                        <DateInput
                            className="h-11 sm:h-9 [&>input[type=text]]:h-full [&>input[type=text]]:py-1.5 [&>input[type=text]]:text-sm sm:[&>button]:min-h-9 sm:[&>button]:min-w-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">إلى تاريخ</label>
                        <DateInput
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.toDate || ''}
                            onChange={(val) => onChange('toDate', val)}
                        />
                    </div>
                );
            case 'statusId':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">الحالة</label>
                        <NativeSelect
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">الحالة</label>
                        <NativeSelect
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                        <label className="label-filter mb-1">السنة المالية</label>
                        <Input
                            type="number"
                            placeholder="2024"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">السنة المالية</label>
                        <Input
                            type="number"
                            placeholder="2024"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.fiscalYear || ''}
                            onChange={(e) => onChange('fiscalYear', e.target.value)}
                        />
                    </div>
                );
            case 'portfolioId':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">رقم المحفظة</label>
                        <Input
                            type="number"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">رقم المحفظة</label>
                        <Input
                            type="number"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.portfolioId || ''}
                            onChange={(e) => onChange('portfolioId', e.target.value)}
                        />
                    </div>
                );
            case 'projectId':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">رقم المشروع</label>
                        <Input
                            type="number"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">رقم المشروع</label>
                        <Input
                            type="number"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.projectId || ''}
                            onChange={(e) => onChange('projectId', e.target.value)}
                        />
                    </div>
                );
            case 'propertyId':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">رقم العقار</label>
                        <Input
                            type="number"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">رقم العقار</label>
                        <Input
                            type="number"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.propertyId || ''}
                            onChange={(e) => onChange('propertyId', e.target.value)}
                        />
                    </div>
                );
            case 'accountLevel':
                return (
                    <div key={key} className="w-full sm:w-52">
<<<<<<< HEAD
                        <label className="label-filter mb-1">المستوى</label>
=======
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">المستوى</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <Select
                            value={values.accountLevel || 'all'}
                            onValueChange={(val) => onChange('accountLevel', val)}
                        >
<<<<<<< HEAD
                            <SelectTrigger className="h-11 py-1.5 text-sm sm:h-9">
=======
                            <SelectTrigger className="h-10">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                        <label className="label-filter mb-1">الفصل المالي</label>
=======
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">الفصل المالي</label>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <Select
                            value={values.quarter || ''}
                            onValueChange={(val) => onChange('quarter', val)}
                        >
<<<<<<< HEAD
                            <SelectTrigger className="h-11 py-1.5 text-sm sm:h-9">
=======
                            <SelectTrigger className="h-10">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">نوع الأصل</label>
                        <NativeSelect
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">نوع الأصل</label>
                        <NativeSelect
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            options={ASSET_TYPE_OPTIONS}
                            value={values.assetType || ''}
                            onChange={(val) => onChange('assetType', val)}
                        />
                    </div>
                );
            case 'comparisonYear':
                return (
                    <div key={key} className="w-full sm:w-40">
<<<<<<< HEAD
                        <label className="label-filter mb-1">السنة السابقة</label>
                        <Input
                            type="number"
                            placeholder="2023"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">السنة السابقة</label>
                        <Input
                            type="number"
                            placeholder="2023"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.comparisonYear || ''}
                            onChange={(e) => onChange('comparisonYear', e.target.value)}
                        />
                    </div>
                );
            case 'accountCode':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">رقم الحساب</label>
                        <Input
                            type="text"
                            placeholder="مثال: 1101"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">رقم الحساب</label>
                        <Input
                            type="text"
                            placeholder="مثال: 1101"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.accountCode || ''}
                            onChange={(e) => onChange('accountCode', e.target.value)}
                        />
                    </div>
                );
            case 'entityId':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">الكيان</label>
                        <Input
                            type="number"
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-48">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">الكيان</label>
                        <Input
                            type="number"
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.entityId || ''}
                            onChange={(e) => onChange('entityId', e.target.value)}
                        />
                    </div>
                );
            case 'search':
                return (
<<<<<<< HEAD
                    <div key={key} className="w-full sm:w-48">
                        <label className="label-filter mb-1">بحث</label>
                        <Input
                            type="text"
                            placeholder="بحث بالاسم، الرقم،..."
                            className="h-11 py-1.5 text-sm sm:h-9"
=======
                    <div key={key} className="w-full sm:w-56">
                        <label className="block text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1.5">بحث</label>
                        <Input
                            type="text"
                            placeholder="بحث بالاسم، الرقم،..."
                            className="h-10"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            value={values.search || ''}
                            onChange={(e) => onChange('search', e.target.value)}
                        />
                    </div>
                );
<<<<<<< HEAD
            case 'decisionType':
                return (
                    <div key={key} className="w-full sm:w-40">
                        <label className="label-filter mb-1">نوع القرار</label>
                        <Input
                            type="text"
                            placeholder="مثال: تسوية رتب"
                            className="h-11 py-1.5 text-sm sm:h-9"
                            value={values.decisionType || ''}
                            onChange={(e) => onChange('decisionType', e.target.value)}
                        />
                    </div>
                );
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            default:
                return null;
        }
    };

    return (
<<<<<<< HEAD
        <Card className={`flex flex-wrap items-end gap-x-3 gap-y-2 mb-4 text-start ${compact ? 'p-2' : 'p-3'}`}>
            <div className="flex w-full shrink-0 items-center gap-2 sm:w-auto sm:self-center">
                <div className="p-1.5 rounded-md bg-primary/10 text-primary">
=======
        <Card className={`flex flex-wrap gap-4 items-end mb-6 text-start ${compact ? 'p-3' : 'p-5'}`}>
            <div className="flex items-center gap-2 mb-auto self-start pt-2 me-2">
                <div className="p-2 rounded-lg bg-primary/10 text-primary">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <Filter className="w-4 h-4" />
                </div>
                <span className="font-bold text-sm">تصفية التقرير</span>
            </div>

            {filters.map(renderFilter)}

            {showApplyButton && onApply && (
                <Button
                    onClick={onApply}
<<<<<<< HEAD
                    className="h-11 w-full px-4 text-sm sm:h-9 sm:min-h-9 sm:w-auto"
                >
                    <RefreshCw className="w-4 h-4" />
=======
                    className="h-10 px-6"
                >
                    <RefreshCw className="w-4 h-4 ms-2" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    تحديث التقرير
                </Button>
            )}
        </Card>
    );
};
