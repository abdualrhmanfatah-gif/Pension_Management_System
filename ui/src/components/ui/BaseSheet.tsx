import * as React from "react";

import { Button } from "@/components/ui/Button";
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetFooter,
    SheetHeader,
    SheetTitle,
} from "@/components/ui/Sheet";
import { cn } from "@/lib/utils";

import type { BaseOverlayProps } from "./BaseFormDialog";

export interface BaseSheetProps extends BaseOverlayProps {
    title: string;
    description?: string;
    side?: "left" | "right" | "top" | "bottom";
    isLoading?: boolean;
    onSubmit?: (e?: React.BaseSyntheticEvent) => Promise<void> | void;
    children: React.ReactNode;
    footer?: React.ReactNode;
    submitLabel?: string;
    cancelLabel?: string;
    className?: string;
}

/**
 * Standardized wrapper for Side Overlays (Sheets).
 * Follows the same rigid layout as BaseFormDialog.
 * Automatically supports "Enter" key for submission when onSubmit is provided.
 */
export function BaseSheet({
    open: controlledOpen,
    isOpen: legacyOpen,
    onOpenChange: controlledOnOpenChange,
    onClose: legacyClose,
    title,
    description,
    side = "right",
    isLoading = false,
    onSubmit,
    children,
    footer,
    submitLabel = "حفظ",
    cancelLabel = "إلغاء",
    className,
}: BaseSheetProps) {
    // ─── State Management ──────────────────────────────────────

    const effectiveOpen = controlledOpen !== undefined ? controlledOpen : !!legacyOpen;

    const handleRequestClose = React.useCallback(() => {
        if (controlledOnOpenChange) {
            controlledOnOpenChange(false);
        } else if (legacyClose) {
            legacyClose();
        }
    }, [controlledOnOpenChange, legacyClose]);

    const handleOpenChange = (openState: boolean) => {
        if (!openState) {
            handleRequestClose();
        } else if (controlledOnOpenChange) {
            controlledOnOpenChange(true);
        }
    };

    // ─── Event Handlers ───────────────────────────────────────

    const handleFormSubmit = (e: React.FormEvent) => {
        if (onSubmit) {
            e.preventDefault();
            onSubmit(e);
        }
    };

    const Wrapper: React.ElementType = onSubmit ? "form" : "div";
    const wrapperProps = onSubmit
        ? {
            onSubmit: handleFormSubmit,
            className: "flex flex-col h-full",
            noValidate: true as const,
        }
        : {
            className: "flex flex-col h-full",
        };

    return (
        <Sheet open={effectiveOpen} onOpenChange={handleOpenChange}>
            <SheetContent 
                side={side} 
                className={cn("flex flex-col p-0 overflow-hidden", className)}
                aria-describedby={undefined}
            >
                {/* Form Wrapper for keyboard shortcuts (Enter to submit) */}
                <Wrapper {...wrapperProps}>
                    <SheetHeader className="px-4 sm:px-6 pt-4 sm:pt-6 pb-3 sm:pb-4 mb-0 border-b bg-background z-10 text-[start]">
                        <SheetTitle className="text-lg sm:text-xl font-bold text-primary">{title}</SheetTitle>
                        {description && (
                            <SheetDescription className="mt-1.5 text-sm text-[start]">
                                {description}
                            </SheetDescription>
                        )}
                    </SheetHeader>

                    {/* Body: Standardized spacing */}
                    <div className="flex-1 p-3 sm:p-5 space-y-5 sm:space-y-6 overflow-y-auto min-h-0 bg-background/50 -webkit-overflow-scrolling-touch" style={{ overscrollBehavior: "contain" }}>
                        {children}
                    </div>

                    {/* Footer: Unified action buttons layout */}
                    <div className="px-4 sm:px-6 py-3 sm:py-4 border-t bg-muted/20 z-10 shrink-0">
                        {footer !== undefined ? (
                            footer
                        ) : (
                            <SheetFooter className="mt-0 pt-0 border-t-0 flex flex-col-reverse sm:flex-row gap-3 justify-end sm:justify-start">
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={handleRequestClose}
                                    disabled={isLoading}
                                    className="w-full sm:w-auto min-w-[100px] min-h-[44px]"
                                >
                                    {cancelLabel}
                                </Button>
                                <Button
                                    type="submit"
                                    loading={isLoading}
                                    className="w-full sm:w-auto min-w-[120px] min-h-[44px]"
                                >
                                    {submitLabel}
                                </Button>
                            </SheetFooter>
                        )}
                    </div>
                </Wrapper>
            </SheetContent>
        </Sheet>
    );
}
