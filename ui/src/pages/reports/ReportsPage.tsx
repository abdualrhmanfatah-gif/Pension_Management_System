import { FileText } from "lucide-react";
import React from 'react';
import { useNavigate } from 'react-router-dom';

import { ReportCard } from '@/components/common/ReportCard';
import { Breadcrumb, Heading, PageLayout, Text } from "@/components/ui";

import { REPORT_CATALOG } from '../../api/reports';

export const ReportsPage: React.FC = () => {
    const navigate = useNavigate();

    // Group reports by category
    const groupedReports = REPORT_CATALOG.reduce((acc, report) => {
        if (!acc[report.category]) acc[report.category] = [];
        acc[report.category].push(report);
        return acc;
    }, {} as Record<string, typeof REPORT_CATALOG>);

    return (
        <PageLayout>
            {/* ── Breadcrumb & Header ────────────────────────── */}
            <div className="space-y-4">
                <Breadcrumb items={[{ label: "النظام", href: "/" }, { label: "مركز التقارير" }]} />

                <div className="flex items-center gap-4">
                    <div className="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center">
                        <FileText className="w-6 h-6 text-primary" />
                    </div>
                    <div>
                        <Heading level="h1">مركز التقارير المؤسسية</Heading>
                        <Text variant="description" className="mt-1">نظام التقارير الذكية لدعم اتخاذ القرار والامتثال</Text>
                    </div>
                </div>
            </div>

            {/* ── Reports Grid ──────────────────────────────── */}
            <div className="space-y-12">
                {Object.entries(groupedReports).map(([category, reports]) => (
                    <div key={category} className="space-y-6">
                        <div className="flex items-center gap-3">
                            <Heading level="h2">
                                {category}
                            </Heading>
                            <div className="h-px flex-1 bg-border/60" />
                            <span className="text-xs font-bold text-muted-foreground uppercase tracking-widest bg-muted px-2 py-0.5 rounded">
                                {reports.length} تقارير
                            </span>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                            {reports.map((report) => (
                                <ReportCard
                                    key={report.id}
                                    title={report.name}
                                    description={report.description}
                                    icon={report.icon}
                                    onClick={() => navigate(`/reports/${report.id}`)}
                                />
                            ))}
                        </div>
                    </div>
                ))}
            </div>
        </PageLayout>
    );
};


