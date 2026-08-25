import React, { useState } from 'react';
import { Download, Printer } from 'lucide-react';

import { Card } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { Heading } from '@/components/ui';
import { ReportKPICards } from './ReportKPICards';
import { ChartToggle } from './charts';
import type { ReportDataRow } from '@/api/reports';

interface AccountingReportLayoutProps {
  title: string;
  description?: string;
  icon?: string;
  isLoading?: boolean;
  children: React.ReactNode;
  chartContent?: React.ReactNode;
  kpiData?: ReportDataRow[];
  kpiColumns?: { key: string; label: string; type?: 'currency' | 'number' | 'date' | 'percent' | 'boolean' }[];
  customKPIs?: { label: string; value: string | number; color?: string }[];
  filterBar?: React.ReactNode;
  onExport?: () => void;
  onPrint?: () => void;
  footer?: React.ReactNode;
}

export const AccountingReportLayout: React.FC<AccountingReportLayoutProps> = ({
  title,
  description,
  isLoading = false,
  children,
  chartContent,
  kpiData,
  kpiColumns,
  customKPIs,
  filterBar,
  onExport,
  onPrint,
  footer,
}) => {
  const [showChart, setShowChart] = useState(false);

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <Heading level="h2" className="text-lg font-bold text-foreground">{title}</Heading>
          {description && (
            <p className="text-sm text-muted-foreground mt-1">{description}</p>
          )}
        </div>

        <div className="flex items-center gap-2">
          {chartContent && (
            <ChartToggle showChart={showChart} onToggle={() => setShowChart(!showChart)} />
          )}
          {onExport && (
            <Button variant="outline" size="sm" onClick={onExport} className="gap-2">
              <Download className="w-4 h-4" />
              تصدير Excel
            </Button>
          )}
          {onPrint && (
            <Button variant="outline" size="sm" onClick={onPrint} className="gap-2">
              <Printer className="w-4 h-4" />
              طباعة
            </Button>
          )}
        </div>
      </div>

      {/* Filter Bar */}
      {filterBar && <>{filterBar}</>}

      {/* Custom KPIs (non-data-driven) */}
      {customKPIs && customKPIs.length > 0 && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {customKPIs.map((kpi, idx) => (
            <Card key={idx} className="p-5">
              <h4 className="text-[10px] sm:text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">
                {kpi.label}
              </h4>
              <div className={`text-2xl font-black font-mono ${kpi.color || 'text-primary'}`}>
                {typeof kpi.value === 'number'
                  ? new Intl.NumberFormat('ar-YE').format(kpi.value)
                  : kpi.value}
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Data-driven KPIs */}
      {kpiData && kpiColumns && (
        <ReportKPICards data={kpiData} columns={kpiColumns} />
      )}

      {/* Loading State */}
      {isLoading && (
        <div className="space-y-4">
          <Skeleton className="h-[350px] w-full rounded-xl" />
          <Skeleton className="h-64 w-full rounded-xl" />
        </div>
      )}

      {/* Content */}
      {!isLoading && (
        <>
          {showChart && chartContent ? (
            <div className="space-y-6">
              {chartContent}
              {children}
            </div>
          ) : (
            children
          )}
        </>
      )}

      {/* Footer */}
      {footer && <>{footer}</>}
    </div>
  );
};
