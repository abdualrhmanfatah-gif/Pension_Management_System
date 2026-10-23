import React, { useCallback,useEffect, useState } from "react";

import { Tabs, TabsContent,TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { matchesShortcut, SHORTCUT_NEXT_TAB, SHORTCUT_PREV_TAB } from "@/constants/shortcuts";
import { cn } from "@/lib/utils";

interface TabItem {
  id: string;
  label: string;
  icon?: React.ReactNode;
  content: React.ReactNode;
}

interface TabbedFormSectionProps {
  tabs: TabItem[];
  defaultTab?: string;
  className?: string;
  tabListClassName?: string;
  /** Pass the active dialog state to enable precise keyboard hook behavior */
  isDialogOpen?: boolean; 
  /** Error count per tab id — shows a badge on the tab label */
  tabErrors?: Record<string, number>;
  /** Called when the user switches tabs — allows per-tab validation */
  onTabChange?: (tabId: string) => void;
}

/**
 * Advanced Tabbed Form Section.
 * Implements smooth fade-in transitions and Ctrl+Tab / Ctrl+Shift+Tab navigation
 * matching the robustness of the useDialogKeyboard hook.
 */
export function TabbedFormSection({
  tabs,
  defaultTab,
  className,
  tabListClassName,
  isDialogOpen = true,
  tabErrors,
  onTabChange,
}: TabbedFormSectionProps) {
  const [activeTab, setActiveTab] = useState(defaultTab || tabs[0]?.id);

  const handleTabChange = useCallback((tabId: string) => {
    onTabChange?.(tabId);
    setActiveTab(tabId);
  }, [onTabChange]);

  const handleNextTab = useCallback(() => {
    setActiveTab(current => {
      const idx = tabs.findIndex(t => t.id === current);
      const nextIdx = (idx + 1) % tabs.length;
      return tabs[nextIdx].id;
    });
  }, [tabs]);

  const handlePrevTab = useCallback(() => {
    setActiveTab(current => {
      const idx = tabs.findIndex(t => t.id === current);
      const prevIdx = (idx - 1 + tabs.length) % tabs.length;
      return tabs[prevIdx].id;
    });
  }, [tabs]);

  // Keyboard navigation using central shortcuts registry
  useEffect(() => {
    if (!isDialogOpen) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if (matchesShortcut(event, SHORTCUT_PREV_TAB)) {
        event.preventDefault();
        handlePrevTab();
      } else if (matchesShortcut(event, SHORTCUT_NEXT_TAB)) {
        event.preventDefault();
        handleNextTab();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [handleNextTab, handlePrevTab, isDialogOpen]);

  return (
    <Tabs value={activeTab} onValueChange={handleTabChange} className={cn("w-full", className)} dir="rtl">
      <TabsList className={cn(
        "glass border border-border p-1.5 w-full justify-start overflow-x-auto mb-6 rounded-xl bg-muted/30",
        tabListClassName
      )}>
        {tabs.map(tab => {
          const errorCount = tabErrors?.[tab.id] ?? 0;
          return (
            <TabsTrigger 
              key={tab.id} 
              value={tab.id} 
              className="gap-2.5 min-w-[120px] rounded-lg transition-all duration-300 data-[state=active]:bg-background data-[state=active]:text-primary"
            >
              {tab.icon && <span className="opacity-100 data-[state=active]:opacity-100">{tab.icon}</span>}
              <span className="font-medium">{tab.label}</span>
              {errorCount > 0 && (
                <span className="inline-flex items-center justify-center h-5 min-w-[20px] px-1.5 rounded-full bg-destructive text-destructive-foreground text-caption-md font-bold leading-none">
                  {errorCount}
                </span>
              )}
            </TabsTrigger>
          );
        })}
      </TabsList>

      <div className="relative">
        {tabs.map(tab => (
          <TabsContent 
            key={tab.id} 
            value={tab.id} 
            // Enhanced aesthetic fade-in animation
            className="space-y-6 outline-none animate-in fade-in-0 slide-in-from-bottom-2 duration-300 ease-out fill-mode-forwards"
          >
            {tab.content}
          </TabsContent>
        ))}
      </div>
    </Tabs>
  );
}
