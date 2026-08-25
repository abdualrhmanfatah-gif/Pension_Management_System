import type { LucideIcon } from "lucide-react";
import React from "react";

import { cn } from "@/lib/utils";

import { Button } from "./Button";

interface FormSectionProps {
  title?: string;
  description?: string;
  icon?: LucideIcon | null;
  children: React.ReactNode;
  className?: string;
  contentClassName?: string;
  badge?: string | number;
  collapsible?: boolean;
  defaultOpen?: boolean;
  separator?: boolean;
}

export function FormSection({
  title,
  description,
  icon: Icon,
  children,
  className,
  contentClassName,
  badge,
  collapsible = false,
  defaultOpen = true,
  separator = false,
}: FormSectionProps) {
  const [isOpen, setIsOpen] = React.useState(defaultOpen);

  return (
    <section
      className={cn(
        "group/section",
        separator && "pt-4 border-t border-border/30 first:border-t-0 first:pt-0",
        className
      )}
    >
      {/* ─── Header ─── */}
      {(title || Icon) && (
        <div
          className={cn(
            "flex items-center gap-2 mb-3 pb-2 border-b border-border/30",
            collapsible && "cursor-pointer select-none"
          )}
          onClick={() => collapsible && setIsOpen((p) => !p)}
          role={collapsible ? "button" : undefined}
          aria-expanded={collapsible ? isOpen : undefined}
          tabIndex={collapsible ? 0 : undefined}
          onKeyDown={(e) => {
            if (collapsible && (e.key === "Enter" || e.key === " ")) {
              e.preventDefault();
              setIsOpen((p) => !p);
            }
          }}
        >
          {/* Icon — small inline */}
          {Icon && (
            <Icon className="w-4 h-4 text-muted-foreground shrink-0" strokeWidth={2} />
          )}

          {/* Title + Badge */}
          {title && (
            <h3 className="text-sm font-semibold text-foreground flex-1 min-w-0 truncate">
              {title}
            </h3>
          )}
          {badge !== undefined && (
            <span className="inline-flex items-center justify-center h-5 min-w-[1.25rem] px-1.5 text-[10px] font-semibold rounded-full bg-primary/10 text-primary shrink-0">
              {badge}
            </span>
          )}

          {/* Collapse Toggle */}
          {collapsible && (
            <Button
              variant="ghost"
              size="icon"
              className="p-1 min-h-[32px] min-w-[32px] text-muted-foreground/50 hover:text-foreground"
              aria-label={isOpen ? "طي القسم" : "توسيع القسم"}
              onClick={(e) => {
                e.stopPropagation();
                setIsOpen((p) => !p);
              }}
            >
              <svg
                className={cn(
                  "w-3.5 h-3.5 transition-transform duration-200",
                  !isOpen && "-rotate-90"
                )}
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                strokeWidth={2.5}
              >
                <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
              </svg>
            </Button>
          )}
        </div>
      )}

      {/* ─── Description ─── */}
      {description && (
        <p className="text-xs text-muted-foreground/70 mb-3 leading-relaxed">
          {description}
        </p>
      )}

      {/* ─── Content ─── */}
      <div
        className={cn(
          "grid transition-all duration-200 ease-out",
          isOpen ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0"
        )}
      >
        <div className={cn("overflow-hidden", contentClassName)}>
          {children}
        </div>
      </div>
    </section>
  );
}
