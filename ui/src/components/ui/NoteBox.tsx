import { AlertTriangle, HelpCircle,Info, Lightbulb } from "lucide-react";
import React from "react";

import { cn } from "@/lib/utils";

interface NoteBoxProps {
  /** Visual variant: blue (info), yellow (warning), emerald (success), or slate (help). */
  type?: "info" | "warning" | "success" | "help";
  /** Optional bold title for the note. */
  title?: string;
  /** Main descriptive content. */
  children: React.ReactNode;
  /** Custom classes for the outer container. */
  className?: string;
  /** Optional custom icon override. @default semantic-icons */
  icon?: React.ElementType;
}

/**
 * Premium NoteBox / Contextual Callout.
 * * Variants:
 * - Info (Blue): General institutional guidance.
 * - Warning (Yellow/Amber): Important caveats or required actions.
 * - Success (Emerald): Confirmation of positive states.
 */
export function NoteBox({
  type = "info",
  title,
  children,
  className,
  icon: CustomIcon
}: NoteBoxProps) {

  const config = {
    info: {
      bg: "bg-blue-50/40",
      border: "border-blue-100/60",
      text: "text-blue-900",
      accent: "bg-blue-100/80 text-blue-600",
      icon: Info
    },
    warning: {
      bg: "bg-amber-50/50",
      border: "border-amber-100/60",
      text: "text-amber-950",
      accent: "bg-amber-100/80 text-amber-600",
      icon: AlertTriangle
    },
    success: {
      bg: "bg-emerald-50/40",
      border: "border-emerald-100/60",
      text: "text-emerald-900",
      accent: "bg-emerald-100/80 text-emerald-600",
      icon: Lightbulb
    },
    help: {
      bg: "bg-slate-50/60",
      border: "border-slate-200/60",
      text: "text-slate-800",
      accent: "bg-slate-100/80 text-slate-500",
      icon: HelpCircle
    }
  }[type];

  const Icon = CustomIcon || config.icon;

  return (
    <div
      className={cn(
        "p-4 px-5 rounded-2xl border flex gap-4 transition-all hover:bg-opacity-80 backdrop-blur-sm",
        config.bg,
        config.border,
        className
      )}
    >
      <div className={cn(
        "shrink-0 p-2.5 rounded-xl h-fit border border-white/40",
        config.accent
      )}>
        <Icon className="w-5 h-5 stroke-[2.5px]" />
      </div>

      <div className="space-y-1 py-1">
        {title && (
          <h4 className={cn("font-bold text-sm tracking-tight", config.text)}>
            {title}
          </h4>
        )}
        <div className={cn(
          "text-body leading-relaxed opacity-90 break-words",
          config.text
        )}>
          {children}
        </div>
      </div>
    </div>
  );
}
