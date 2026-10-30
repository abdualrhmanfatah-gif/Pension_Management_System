<<<<<<< HEAD
import { BarChart3, Table } from 'lucide-react';
import React from 'react';

=======
import React from 'react';
import { BarChart3, Table } from 'lucide-react';
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { Button } from '@/components/ui/Button';

interface ChartToggleProps {
  showChart: boolean;
  onToggle: () => void;
}

export const ChartToggle: React.FC<ChartToggleProps> = ({ showChart, onToggle }) => {
  return (
    <div className="flex items-center gap-1 bg-muted/50 rounded-lg p-1">
      <Button
        variant={showChart ? 'primary' : 'ghost'}
        size="sm"
        onClick={onToggle}
        className="h-8 px-3 gap-1.5"
      >
        <BarChart3 className="w-4 h-4" />
        رسوم بيانية
      </Button>
      <Button
        variant={!showChart ? 'primary' : 'ghost'}
        size="sm"
        onClick={onToggle}
        className="h-8 px-3 gap-1.5"
      >
        <Table className="w-4 h-4" />
        جدول
      </Button>
    </div>
  );
};
