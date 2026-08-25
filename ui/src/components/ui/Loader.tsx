"use client";

import { Loader2,ShieldCheck } from "lucide-react";
import { forwardRef, memo } from "react";

import { cn } from "@/lib/utils";

export interface LoaderProps {
  className?: string;
  size?: number;
  text?: string;
  variant?: "inline" | "page" | "fullscreen";
  isSecurity?: boolean;
  /** التحكم في الظهور/الإخفاء مع أنيميشن سلس */
  visible?: boolean;
}

/* ========================================================================
   مكون Spinner منفصل مع Memoization لمنع إعادة الرندر غير الضرورية
   ======================================================================== */
const Spinner = memo(function Spinner({
  size,
  isSecurity,
}: {
  size: number;
  isSecurity: boolean;
}) {
  const rings = [
    {
      size: size + 48,
      className: "border-[3px] border-dashed border-primary/20 animate-[spin_5s_linear_infinite]",
    },
    {
      size: size + 28,
      className: "border-[3px] border-transparent border-t-primary/60 border-r-primary/60 animate-[spin_3s_linear_infinite_reverse]",
    },
    {
      size: size + 12,
      className: "border-[2px] border-transparent border-b-primary/40 animate-[spin_2s_linear_infinite]",
    },
  ];

  return (
    <div className="relative flex items-center justify-center">
      {/* تأثير الإضاءة المحيطة (Ambient Glow) */}
      <div
        className="absolute rounded-full bg-primary/[0.07] animate-pulse blur-3xl"
        style={{ width: size + 80, height: size + 80 }}
      />

      {/* الحلقات الدوارة */}
      {rings.map((ring, index) => (
        <div
          key={index}
          className={cn("absolute rounded-full", ring.className)}
          style={{ width: ring.size, height: ring.size }}
        />
      ))}

      {/* توهج داخلي */}
      <div
        className="absolute rounded-full bg-primary/10 animate-pulse blur-lg"
        style={{ width: size, height: size }}
      />

      {/* المركز */}
      <div
        className="relative z-10 flex items-center justify-center bg-background rounded-full border border-border/50"
        style={{ width: size, height: size }}
      >
        {isSecurity ? (
          <ShieldCheck
            className="text-primary animate-[pulse_2s_ease-in-out_infinite]"
            size={size * 0.5}
            strokeWidth={2}
          />
        ) : (
          <Loader2
            className="animate-spin text-primary"
            size={size * 0.55}
            strokeWidth={2.5}
          />
        )}
      </div>
    </div>
  );
});

/* ========================================================================
   المكون الرئيسي
   ======================================================================== */
export const Loader = forwardRef<HTMLDivElement, LoaderProps>(function Loader(
  {
    className,
    size = 56,
    text = "جاري التحميل...",
    variant = "inline",
    isSecurity = false,
    visible = true,
  },
  ref
) {
  const content = (
    <div
      ref={ref}
      role="status"
      aria-live="polite"
      aria-busy={visible}
      aria-label={text}
      className={cn(
        "flex flex-col items-center justify-center gap-5 transition-all duration-500 ease-out",
        visible
          ? "opacity-100 scale-100 translate-y-0"
          : "opacity-0 scale-95 translate-y-2 pointer-events-none",
        className
      )}
    >
      <Spinner size={size} isSecurity={isSecurity} />

      {text && (
        <div className="flex flex-col items-center gap-1.5 text-center">
          <p className="text-sm font-semibold text-foreground/90 tracking-wide animate-pulse">
            {text}
          </p>
          {isSecurity && (
            <span className="text-xs text-muted-foreground/80 font-medium">
              نظام إدارة صندوق التقاعد الأمني
            </span>
          )}
        </div>
      )}

      {/* نص مخفي لقارئات الشاشة */}
      <span className="sr-only">{text}</span>
    </div>
  );

  if (variant === "fullscreen") {
    return (
      <div
        className={cn(
          "fixed inset-0 flex items-center justify-center bg-background/90 z-50 transition-opacity duration-500",
          visible ? "opacity-100" : "opacity-0 pointer-events-none"
        )}
      >
        {content}
      </div>
    );
  }

  if (variant === "page") {
    return (
      <div className="flex items-center justify-center min-h-[60dvh] w-full animate-in fade-in zoom-in-95 duration-500">
        {content}
      </div>
    );
  }

  return content;
});

Loader.displayName = "Loader";