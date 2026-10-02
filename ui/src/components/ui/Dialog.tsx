import { X } from 'lucide-react';
import * as React from 'react';
import { createPortal } from 'react-dom';

import { claimDialogEvent, type DialogLayer,isTopmostDialog, registerDialogLayer } from '@/lib/dialog-stack';
import { cn } from '@/lib/utils';

// ─── Context ──────────────────────────────────────────

interface DialogContextValue {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  layer: DialogLayer | null;
}

const DialogContext = React.createContext<DialogContextValue>({
  open: false,
  onOpenChange: () => {},
  layer: null,
});

// ─── Root ─────────────────────────────────────────────

const Dialog = ({
  open = false,
  onOpenChange = () => {},
  children,
}: {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
  children?: React.ReactNode;
}) => {
  const { layer: parent } = React.useContext(DialogContext);
  const layer = React.useMemo<DialogLayer>(() => ({ parent, element: null }), [parent]);
  return <DialogContext.Provider value={{ open, onOpenChange, layer }}>
    {children}
  </DialogContext.Provider>;
};

// ─── Trigger & Close ──────────────────────────────────

const DialogTrigger = React.forwardRef<
  HTMLButtonElement,
  React.ButtonHTMLAttributes<HTMLButtonElement>
>(({ className, onClick, children, ...props }, ref) => {
  const { onOpenChange } = React.useContext(DialogContext);
  return (
    <button
      ref={ref}
      className={cn('font-sans', className)}
      onClick={(e) => {
        onClick?.(e);
        onOpenChange(true);
      }}
      {...props}
    >
      {children}
    </button>
  );
});
DialogTrigger.displayName = 'DialogTrigger';

const DialogClose = React.forwardRef<
  HTMLButtonElement,
  React.ButtonHTMLAttributes<HTMLButtonElement>
>(({ className, onClick, children, ...props }, ref) => {
  const { onOpenChange } = React.useContext(DialogContext);
  return (
    <button
      ref={ref}
      className={cn('font-sans', className)}
      onClick={(e) => {
        onClick?.(e);
        onOpenChange(false);
      }}
      {...props}
    >
      {children}
    </button>
  );
});
DialogClose.displayName = 'DialogClose';

// ─── Size Width Map ────────────────────────────────────

const SIZE_WIDTH_MAP: Record<string, string> = {
  sm: 'min(500px, 95vw)',
  md: 'min(672px, 95vw)',
  lg: 'min(800px, 95vw)',
  xl: 'min(900px, 95vw)',
  '2xl': 'min(1024px, 95vw)',
  '3xl': 'min(1100px, 95vw)',
  '4xl': 'min(1200px, 95vw)',
  '5xl': 'min(1280px, 95vw)',
  '6xl': '92vw',
  full: '85vw',
};

// ─── Content ──────────────────────────────────────────

export type ButtonVariant = "primary" | "secondary" | "destructive" | "outline" | "ghost" | "link" | "success" | "warning" | "info";

interface DialogContentProps {
  showClose?: boolean;
  size?: keyof typeof SIZE_WIDTH_MAP;
  className?: string;
  children?: React.ReactNode;
  dir?: string;
  removePadding?: boolean;
  clipContent?: boolean;
  'aria-describedby'?: string;
  'aria-labelledby'?: string;
  'aria-label'?: string;
  role?: string;
  id?: string;
  style?: React.CSSProperties;
  allowDragging?: boolean;
  portal?: boolean;
  header?: string | React.ReactNode;
  onOpenAutoFocus?: (e: { target: HTMLElement; preventDefault: () => void }) => void;
  onEscapeKeyDown?: (e: { preventDefault: () => void }) => void;
  onInteractOutside?: (e: { preventDefault: () => void }) => void;
  onKeyDown?: (e: React.KeyboardEvent) => void;
}

// ─── Focus Trap Hook ──────────────────────────────────

const FOCUSABLE_SELECTOR =
  'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

function useFocusTrap(containerRef: React.RefObject<HTMLElement | null>, enabled: boolean) {
  React.useEffect(() => {
    if (!enabled || !containerRef.current) return;

    const container = containerRef.current;

    const getFocusable = () =>
      Array.from(container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR)).filter(
        (el) =>
          !el.hasAttribute('disabled') &&
          el.getAttribute('aria-hidden') !== 'true' &&
          el.offsetParent !== null
      );

    const handleTabKey = (e: KeyboardEvent) => {
      if (e.key !== 'Tab' || !isTopmostDialog(container)) return;

      // Recompute on every Tab press so dynamic content (steps, dropdowns,
      // disabled buttons) is always accounted for.
      const focusableElements = getFocusable();
      if (focusableElements.length === 0) return;

      const firstElement = focusableElements[0];
      const lastElement = focusableElements[focusableElements.length - 1];
      const active = document.activeElement as HTMLElement | null;

      // If focus escaped the container, pull it back in.
      if (!active || !container.contains(active)) {
        e.preventDefault();
        (e.shiftKey ? lastElement : firstElement).focus();
        return;
      }

      if (e.shiftKey && active === firstElement) {
        e.preventDefault();
        lastElement.focus();
      } else if (!e.shiftKey && active === lastElement) {
        e.preventDefault();
        firstElement.focus();
      }
    };

    container.addEventListener('keydown', handleTabKey);
    return () => container.removeEventListener('keydown', handleTabKey);
  }, [enabled, containerRef]);
}

// ─── Scroll Lock Hook ─────────────────────────────────

let scrollLockCount = 0;
let restoreScrollLock: (() => void) | null = null;

function useScrollLock(enabled: boolean) {
  React.useEffect(() => {
    if (!enabled) return;

    if (scrollLockCount++ === 0) {
      const htmlDir = document.documentElement.getAttribute('dir') || 'rtl';
      const cssProp = htmlDir === 'rtl' ? 'padding-left' : 'padding-right';
      const originalPadding = document.body.style.getPropertyValue(cssProp);
      const originalOverflow = document.body.style.overflow;
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;

      document.body.style.overflow = 'hidden';
      if (scrollbarWidth > 0) {
        const padding = parseFloat(getComputedStyle(document.body).getPropertyValue(cssProp)) || 0;
        document.body.style.setProperty(cssProp, `${padding + scrollbarWidth}px`);
      }
      restoreScrollLock = () => {
        document.body.style.overflow = originalOverflow;
        document.body.style.setProperty(cssProp, originalPadding);
      };
    }
    return () => {
      if (--scrollLockCount === 0) {
        restoreScrollLock?.();
        restoreScrollLock = null;
      }
    };
  }, [enabled]);
}

// ─── Dialog Content ───────────────────────────────────

const DialogContent = React.forwardRef<HTMLDivElement, DialogContentProps>(
  (
    {
      className,
      children,
      showClose = true,
      size = 'lg',
      removePadding = false,
      clipContent = true,
      portal = true,
      allowDragging = false,
      header: syncHeader,
      onOpenAutoFocus,
      onEscapeKeyDown,
      onInteractOutside,
      onKeyDown,
      'aria-describedby': ariaDescribedBy,
      'aria-labelledby': ariaLabelledBy,
      'aria-label': ariaLabel,
      role = 'dialog',
      id,
      style,
      dir = 'rtl',
    },
    ref
  ) => {
    const { open, onOpenChange, layer } = React.useContext(DialogContext);
    const dialogRef = React.useRef<HTMLDivElement>(null);
    const [isVisible, setIsVisible] = React.useState(false);
    const [isAnimating, setIsAnimating] = React.useState(false);

    React.useLayoutEffect(() => {
      if (open && layer && dialogRef.current) return registerDialogLayer(layer, dialogRef.current);
    }, [open, layer]);

    // Merge refs — العنصر الداخلي لا يُركّب إلا بعد تحوّل open إلى true، فمرّر
    // ref دالةً تزامن القيمة لحظة التركيب (useImperativeHandle بـ [] يلتقط null
    // عند التركيب الأول ويتجمّد، فتفشل اختصارات الحفظ وفحص أعلى نافذة).
    const setDialogRef = React.useCallback(
      (node: HTMLDivElement | null) => {
        dialogRef.current = node;
        if (typeof ref === 'function') {
          ref(node);
        } else if (ref) {
          (ref as React.MutableRefObject<HTMLDivElement | null>).current = node;
        }
      },
      [ref]
    );

    // Scroll lock
    useScrollLock(open);

    // Focus trap
    useFocusTrap(dialogRef, open);

    // Animation & focus management
    React.useEffect(() => {
      if (open) {
        setIsVisible(true);
        // Trigger animation on next frame
        requestAnimationFrame(() => {
          requestAnimationFrame(() => setIsAnimating(true));
        });

        // Auto focus after animation starts
        const timer = setTimeout(() => {
          const dialogEl = dialogRef.current;
          if (!dialogEl || !isTopmostDialog(dialogEl)) return;

          if (onOpenAutoFocus) {
            onOpenAutoFocus({
              target: dialogEl,
              preventDefault: () => {},
            });
          } else {
            const autoFocusEl = dialogEl.querySelector<HTMLElement>(
              '[autofocus], [data-autofocus]'
            );
            if (autoFocusEl) {
              autoFocusEl.focus();
            } else {
              // Focus the dialog itself or first focusable element
              const focusable = dialogEl.querySelector<HTMLElement>(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
              );
              focusable?.focus();
            }
          }
        }, 50);

        return () => clearTimeout(timer);
      } else {
        if (dialogRef.current && document.activeElement && dialogRef.current.contains(document.activeElement)) {
          (document.activeElement as HTMLElement).blur();
        }
        setIsAnimating(false);
        const timer = setTimeout(() => setIsVisible(false), 200); // Match CSS duration
        return () => clearTimeout(timer);
      }
    }, [open, onOpenAutoFocus]);

    // Escape key handler
    React.useEffect(() => {
      if (!open) return;

      const handleEscape = (e: KeyboardEvent) => {
        if (e.key !== 'Escape' || e.defaultPrevented || !isTopmostDialog(dialogRef.current)) return;
        if (document.querySelector('[data-radix-popper-content-wrapper]')) return;
        if (!claimDialogEvent(e)) return;
        let prevented = false;
        onEscapeKeyDown?.({ preventDefault: () => { prevented = true; } });
        e.preventDefault();
        if (!prevented) onOpenChange(false);
      };

      document.addEventListener('keydown', handleEscape);
      return () => document.removeEventListener('keydown', handleEscape);
    }, [open, onEscapeKeyDown, onOpenChange]);

    if (!isVisible && !open) return null;

    const handleOverlayClick = (e: React.MouseEvent) => {
      if (e.target === e.currentTarget && isTopmostDialog(dialogRef.current)) {
        let prevented = false;
        if (onInteractOutside) {
          onInteractOutside({ preventDefault: () => { prevented = true; } });
        }
        if (!prevented) {
          onOpenChange(false);
        }
      }
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
      onKeyDown?.(e);
    };

    const content = (
      <div
        className={cn(
           'fixed inset-0 z-dialog flex items-center justify-center',
          'transition-all duration-200 ease-out',
          isAnimating ? 'bg-black/50' : 'bg-black/0'
        )}
        onClick={handleOverlayClick}
        aria-hidden={!open && !isAnimating}
      >
        <div
          ref={setDialogRef}
          id={id}
          role={role}
          aria-modal="true"
          aria-label={ariaLabel}
          aria-labelledby={ariaLabelledBy}
          aria-describedby={ariaDescribedBy}
          dir={dir}
          style={{
            width: SIZE_WIDTH_MAP[size] || SIZE_WIDTH_MAP.lg,
            overscrollBehavior: 'contain',
            ...style,
          }}
          className={cn(
            'relative max-h-[90dvh] rounded-md sm:rounded-lg border bg-background',
            clipContent && 'overflow-hidden',
            'transition-all duration-200 ease-out',
            'font-sans flex flex-col',
            isAnimating
              ? 'opacity-100 translate-y-0 scale-100'
              : 'opacity-0 translate-y-4 scale-95',
            allowDragging && 'cursor-move',
            className
          )}
          onKeyDown={handleKeyDown}
          tabIndex={-1}
        >
          {/* Drag Header (if enabled) */}
          {allowDragging && syncHeader && (
            <div className="dialog-drag-handle flex items-center justify-between border-b px-[var(--dlg-section-p)] py-3 cursor-move select-none">
              <span className="text-sm font-medium text-muted-foreground">
                {typeof syncHeader === 'string' ? syncHeader : '\u00A0'}
              </span>
            </div>
          )}

          {/* Custom Close Button */}
          {showClose && (
            <button
              type="button"
              onClick={() => onOpenChange(false)}
              className={cn(
                'absolute end-2 top-2',
                'rounded-full opacity-60 ring-offset-background transition-all duration-200',
                'hover:opacity-100 hover:bg-accent hover:text-accent-foreground',
                'focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
                'disabled:pointer-events-none z-50',
                'min-h-[36px] min-w-[36px] h-9 w-9 flex items-center justify-center',
                'font-sans'
              )}
              aria-label="إغلاق النافذة"
            >
              <X className="h-5 w-5" />
              <span className="sr-only">إغلاق النافذة</span>
            </button>
          )}

          {/* Content */}
          <div
            className={cn(
              'font-sans flex flex-col flex-1 min-h-0',
              !removePadding && 'p-[var(--dlg-section-p)]'
            )}
          >
            {children}
          </div>
        </div>
      </div>
    );

    return portal ? createPortal(content, document.body) : content;
  }
);
DialogContent.displayName = 'DialogContent';

// ─── Header, Footer, Title, Description ───────────────

const DialogHeader = ({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) => (
  <div
    className={cn(
      'flex flex-col gap-1 text-center sm:text-[start]',
      'px-[var(--dlg-section-p)] py-3',
      'border-b font-sans',
      className
    )}
    {...props}
  />
);
DialogHeader.displayName = 'DialogHeader';

const DialogFooter = ({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) => (
  <div
    className={cn(
      'flex flex-col-reverse gap-1.5 sm:flex-row sm:justify-end sm:gap-2',
      'px-[var(--dlg-section-p)] py-[var(--dlg-footer-py)]',
      'border-t bg-muted/5 font-sans',
      className
    )}
    {...props}
  />
);
DialogFooter.displayName = 'DialogFooter';

const DialogTitle = React.forwardRef<HTMLHeadingElement, React.HTMLAttributes<HTMLHeadingElement>>(
  ({ className, ...props }, ref) => (
    <h2
      ref={ref}
      className={cn(
        'text-lg sm:text-xl font-semibold leading-tight tracking-tight text-foreground font-sans',
        className
      )}
      {...props}
    />
  )
);
DialogTitle.displayName = 'DialogTitle';

const DialogDescription = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLParagraphElement>>(
  ({ className, ...props }, ref) => (
    <p
      ref={ref}
      className={cn('text-sm text-muted-foreground leading-relaxed font-sans', className)}
      {...props}
    />
  )
);
DialogDescription.displayName = 'DialogDescription';

export {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
};
export type { DialogContentProps };
