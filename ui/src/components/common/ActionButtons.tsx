import { Eye, MoreHorizontal,Pencil, Trash2 } from "lucide-react";

import { Button } from "@/components/ui/Button";
import { cn } from "@/lib/utils";

interface ActionButton {
  icon: React.ReactNode;
  label: string;
  onClick: () => void;
  variant?: "ghost" | "outline" | "destructive" | "default";
  className?: string;
  disabled?: boolean;
}

interface ActionButtonsProps {
  actions: ActionButton[];
  className?: string;
  size?: "sm" | "md" | "lg" | "icon";
}

export function ActionButtons({ actions, className, size = "icon" }: ActionButtonsProps) {
  return (
    <div className={cn("flex items-center justify-center gap-1", className)}>
      {actions.map((action, index) => (
        <Button
          key={index}
          variant={action.variant || "ghost"}
          size={size}
          onClick={action.onClick}
          disabled={action.disabled}
          title={action.label}
          className={cn(
            "min-h-[44px] min-w-[44px]",
            action.className
          )}
        >
          {action.icon}
        </Button>
      ))}
    </div>
  );
}

// Preset action builders for common patterns
export function ViewAction({ onClick, className }: { onClick: () => void; className?: string }) {
  return {
    icon: <Eye className="w-4 h-4" />,
    label: "عرض",
    onClick,
    variant: "ghost" as const,
    className,
  };
}

export function EditAction({ onClick, className }: { onClick: () => void; className?: string }) {
  return {
    icon: <Pencil className="w-4 h-4" />,
    label: "تعديل",
    onClick,
    variant: "ghost" as const,
    className,
  };
}

export function DeleteAction({ onClick, className }: { onClick: () => void; className?: string }) {
  return {
    icon: <Trash2 className="w-4 h-4 text-destructive" />,
    label: "حذف",
    onClick,
    variant: "ghost" as const,
    className,
  };
}

export function MoreActions({ onClick, className }: { onClick: () => void; className?: string }) {
  return {
    icon: <MoreHorizontal className="w-4 h-4" />,
    label: "المزيد",
    onClick,
    variant: "ghost" as const,
    className,
  };
}
