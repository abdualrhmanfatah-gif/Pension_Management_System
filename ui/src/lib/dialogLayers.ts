export function isTopmostDialog(element: HTMLElement | null): boolean {
  if (!element) return false;
  const dialogs = document.querySelectorAll('[data-dialog-open="true"]');
  return dialogs.item(dialogs.length - 1) === element;
}

export function hasOpenDialogPopup(): boolean {
  return !!document.querySelector(
    '[data-radix-popper-content-wrapper] [data-state="open"], [role="listbox"][data-state="open"]',
  );
}
