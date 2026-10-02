import { useEffect, useRef } from "react";
import type { DefaultValues,FieldValues, UseFormReset } from "react-hook-form";

/**
 * Standardized hook to reset form state when a dialog opens or closes.
 * 
 * * Core Responsibilities:
 * - Resets form to defaults when dialog closes (clears stale data).
 * - Resets form to initialValues when dialog opens (fresh state).
 * - Uses a ref for defaults to prevent unnecessary re-effects if data updates while the dialog is open.
 */
export function useDialogReset<T extends FieldValues>(
  isOpen: boolean,
  reset: UseFormReset<T>,
  defaultValues?: DefaultValues<T> | T,
  resetOnOpen = true
) {
  const defaultsRef = useRef(defaultValues);
  const prevOpenRef = useRef(isOpen);
  
  useEffect(() => {
    defaultsRef.current = defaultValues;
  }, [defaultValues]);

  useEffect(() => {
    const prevOpen = prevOpenRef.current;
    prevOpenRef.current = isOpen;

    if (!isOpen && prevOpen) {
      reset(defaultsRef.current);
    } else if (isOpen && !prevOpen && resetOnOpen) {
      reset(defaultsRef.current);
    }
  }, [isOpen, reset, resetOnOpen]);
}
