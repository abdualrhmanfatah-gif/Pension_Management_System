/**
 * Stepper — A horizontal multi-step progress indicator.
 */
import { Check } from "lucide-react";
import React from "react";

import { cn } from "@/lib/utils";

export interface StepItem {
  label: string;
  description?: string;
  icon?: React.ReactNode;
  /** اختصار كيبورد للوصول — يُعرض كـ title و aria-keyshortcuts فقط بدون أي تغيير مرئي */
  shortcut?: string;
}

interface StepperProps {
  steps: StepItem[];
  activeStep: number;
  onStepClick?: (stepIndex: number) => void;
  allowAllStepsClickable?: boolean;
  className?: string;
}

export function Stepper({
  steps,
  activeStep,
  onStepClick,
  allowAllStepsClickable,
  className,
}: StepperProps) {
  return (
    <div className={cn("w-full", className)}>
      {/* ─── Desktop Layout ─── */}
      <div
        className="hidden sm:flex items-center w-full"
        role="navigation"
        aria-label="خطوات التقدم"
      >
        {steps.map((step, index) => {
          const isCompleted = index < activeStep;
          const isActive = index === activeStep;
          const isClickable = Boolean(onStepClick) && (allowAllStepsClickable || index <= activeStep);

          return (
            <React.Fragment key={index}>
              {/* Step Item */}
              <div className="relative z-10 flex items-center shrink-0 group">
                <button
                  type="button"
                  onClick={() => isClickable && onStepClick?.(index)}
                  disabled={!isClickable}
                  aria-current={isActive ? "step" : undefined}
                  aria-label={`${step.label}${step.description ? `: ${step.description}` : ""}`}
                  title={step.shortcut ? `${step.label} (${step.shortcut})` : undefined}
                  aria-keyshortcuts={step.shortcut}
                  className={cn(
                    "flex items-center gap-3 bg-transparent transition-all focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 rounded-lg p-1",
                    isClickable ? "cursor-pointer" : "cursor-default",
                    isClickable && !isActive && "hover:scale-105"
                  )}
                >
                  {/* Circle */}
                  <div
                    className={cn(
                      "relative w-8 h-8 sm:w-9 sm:h-9 rounded-full flex items-center justify-center text-xs sm:text-sm font-bold shrink-0 transition-all duration-300 shadow-sm",
                      isCompleted && "bg-emerald-500 text-white shadow-emerald-500/20",
                      isActive && "bg-primary text-primary-foreground shadow-primary/25 ring-4 ring-primary/10",
                      !isActive && !isCompleted && "bg-muted text-muted-foreground"
                    )}
                  >
                    {isCompleted ? (
                      <Check className="w-4 h-4 sm:w-4.5 sm:h-4.5" />
                    ) : step.icon ? (
                      step.icon
                    ) : (
                      <span>{index + 1}</span>
                    )}

                    {isActive && (
                      <span className="absolute inset-0 rounded-full animate-ping bg-primary/20" />
                    )}
                  </div>

                  {/* Label & Description */}
                  <div className="text-start min-w-0 max-w-[150px]">
                    <p
                      className={cn(
                        "text-xs sm:text-sm leading-snug transition-colors line-clamp-1 font-bold",
                        isActive && "text-primary",
                        isCompleted && "text-emerald-600 dark:text-emerald-400",
                        !isActive && !isCompleted && "text-muted-foreground"
                      )}
                      style={{
                        fontFamily: 'var(--font-sans)',
                      }}
                    >
                      {step.label}
                    </p>
                    {step.description && (
                      <p
                        className="text-[11px] sm:text-xs leading-tight line-clamp-1"
                        style={{
                          fontFamily: 'var(--font-sans)',
                          color: 'hsl(var(--muted-foreground))',
                        }}
                      >
                        {step.description}
                      </p>
                    )}
                  </div>
                </button>
              </div>

              {/* Connector Line */}
              {index < steps.length - 1 && (
                <div className="flex-1 h-1 z-0 relative overflow-hidden mx-3 lg:mx-4 bg-muted rounded-full">
                  <div
                    className={cn(
                      "absolute inset-y-0 start-0 rounded-full bg-emerald-500 transition-all duration-500 ease-out",
                      isCompleted ? "w-full" : "w-0"
                    )}
                  />
                </div>
              )}
            </React.Fragment>
          );
        })}
      </div>

      {/* ─── Mobile Layout ─── */}
      <div
        className="flex sm:hidden items-center justify-center gap-2 px-2 py-1 pb-6"
        role="navigation"
        aria-label="خطوات التقدم"
      >
        {steps.map((step, index) => {
          const isCompleted = index < activeStep;
          const isActive = index === activeStep;
          const isClickable = Boolean(onStepClick) && (allowAllStepsClickable || index <= activeStep);

          return (
            <React.Fragment key={index}>
              <button
                type="button"
                onClick={() => isClickable && onStepClick?.(index)}
                disabled={!isClickable}
                aria-current={isActive ? "step" : undefined}
                title={step.shortcut ? `${step.label} (${step.shortcut})` : step.label}
                aria-label={step.shortcut ? `${step.label}، اختصار ${step.shortcut}` : step.label}
                aria-keyshortcuts={step.shortcut}
                className={cn(
                  "relative flex flex-col items-center justify-center transition-all focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 rounded-full",
                  isClickable ? "cursor-pointer active:scale-95" : "cursor-default"
                )}
              >
                {/* Circle */}
                <div
                  className={cn(
                    "relative w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold transition-all duration-300 shadow-sm",
                    isCompleted && "bg-emerald-500 text-white shadow-emerald-500/20",
                    isActive && "bg-primary text-primary-foreground shadow-primary/20 ring-4 ring-primary/10",
                    !isActive && !isCompleted && "bg-muted text-muted-foreground"
                  )}
                >
                  {isCompleted ? (
                    <Check className="w-3.5 h-3.5" />
                  ) : (
                    <span>{index + 1}</span>
                  )}
                </div>

                {/* Absolute Label - Prevents breaking flex alignment */}
                {isActive && (
                  <span
                    className="absolute top-10 text-xs w-max max-w-[80px] text-primary text-center leading-tight truncate"
                    style={{
                      fontFamily: 'var(--font-sans)',
                      fontWeight: 'var(--font-weight-semibold)',
                    }}
                  >
                    {step.label}
                  </span>
                )}
              </button>

              {/* Connector Line */}
              {index < steps.length - 1 && (
                <div className="flex-1 h-0.5 rounded-full min-w-[20px] max-w-[40px] relative overflow-hidden bg-muted">
                  <div
                    className={cn(
                      "absolute inset-y-0 start-0 rounded-full bg-emerald-500 transition-all duration-500 ease-out",
                      isCompleted ? "w-full" : "w-0"
                    )}
                  />
                </div>
              )}
            </React.Fragment>
          );
        })}
      </div>
    </div>
  );
}