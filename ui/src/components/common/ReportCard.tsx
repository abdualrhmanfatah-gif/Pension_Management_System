<<<<<<< HEAD
import { Ban, Banknote, BarChart3, Building2, Calculator, ClipboardList, Clock, CreditCard, FileMinus, FileText, FolderOpen, Library, type LucideIcon, PieChart, Presentation, Search, ShieldAlert, UserCircle, Users, Wrench } from 'lucide-react';

import { Heading } from "@/components/ui";
import { Card } from "@/components/ui/Card";
=======
import { Card } from "@/components/ui/Card";
import { Heading } from "@/components/ui";
import { Ban, Banknote, BarChart3, Building2, Calculator, ClipboardList, Clock, CreditCard, FileMinus, FileText, FolderOpen, Library, type LucideIcon, PieChart, Presentation, Search, ShieldAlert, UserCircle, Users, Wrench } from 'lucide-react';
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface ReportCardProps {
    title: string;
    description: string;
    icon: string;
    onClick: () => void;
}

// Map the catalog icon names to Lucide icons
const IconMap: Record<string, LucideIcon> = {
    ChartBarIcon: BarChart3,
    BarChart3Icon: BarChart3,
    BuildingOfficeIcon: Building2,
    BanknotesIcon: Banknote,
    DocumentTextIcon: FileText,
    ShieldExclamationIcon: ShieldAlert,
    WrenchScrewdriverIcon: Wrench,
    ChartPieIcon: PieChart,
    FolderOpenIcon: FolderOpen,
    UserGroupIcon: Users,
    NoSymbolIcon: Ban,
    CreditCardIcon: CreditCard,
    CalculatorIcon: Calculator,
    PresentationChartBarIcon: Presentation,
    DocumentMinusIcon: FileMinus,
    DocumentMagnifyingGlassIcon: Search,
    UsersIcon: Users,
    BuildingLibraryIcon: Library,
    ClockIcon: Clock,
    ClipboardDocumentListIcon: ClipboardList,
    UserCircleIcon: UserCircle
};

export function ReportCard({ title, description, icon, onClick }: ReportCardProps) {
    const Icon = IconMap[icon] || FileText;

    return (
        <Card 
            onClick={onClick}
            onKeyDown={(e: React.KeyboardEvent) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onClick(); } }}
            role="button"
            tabIndex={0}
<<<<<<< HEAD
            className="p-6 hover:border-primary transition-all cursor-pointer group focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
=======
            className="p-6 hover:border-primary/50 transition-all cursor-pointer group focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        >
            <div className="flex items-start gap-4">
                <div className="p-3 bg-primary/10 text-primary rounded-xl group-hover:bg-primary group-hover:text-white transition-colors">
                    <Icon className="w-6 h-6" />
                </div>
                <div className="flex-1 text-start">
                    <Heading level="h3" className="text-lg font-bold text-foreground mb-1">
                        {title}
                    </Heading>
                    <p className="text-sm text-muted-foreground leading-relaxed">
                        {description}
                    </p>
                </div>
            </div>
        </Card>
    );
}
