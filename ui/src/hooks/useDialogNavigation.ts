import type { RefObject } from "react";
import { useEffect, useLayoutEffect, useRef } from "react";

import { isTopmostDialog } from "@/lib/dialogLayers";

const historyKey = "__formDialogGuard";
const dialogs = new Map<symbol, () => boolean>();
let guard: { id: string; url: string } | undefined;
let removingGuard = false;
let listening = false;
let sequence = 0;

function syncHistory() {
  if (removingGuard) return;
  if (dialogs.size > 0) {
    if (!listening) {
      window.addEventListener("popstate", handlePopState);
      listening = true;
    }
    if (!guard) {
      guard = { id: String(++sequence), url: window.location.href };
      // Preserve React Router's key, idx and usr state.
      window.history.pushState({ ...window.history.state, [historyKey]: guard.id }, "");
    }
  } else {
    if (guard && window.history.state?.[historyKey] === guard.id && window.location.href === guard.url) {
      removingGuard = true;
      window.history.back();
      return;
    }
    guard = undefined;
    window.removeEventListener("popstate", handlePopState);
    listening = false;
  }
}

function handlePopState() {
  if (removingGuard) {
    // A cleanup traversal is not a user's request to close another dialog.
    removingGuard = false;
    guard = undefined;
    syncHistory();
    return;
  }
  if (!guard || window.history.state?.[historyKey] === guard.id) return;
  window.history.pushState({ ...window.history.state, [historyKey]: guard.id }, "", guard.url);
  for (const requestClose of [...dialogs.values()].reverse()) {
    if (requestClose()) break;
  }
}

export function useDialogNavigation(
  open: boolean,
  isDirty: boolean,
  dialogRef: RefObject<HTMLElement | null>,
  onClose: () => void,
) {
  const closeRef = useRef(onClose);
  useLayoutEffect(() => { closeRef.current = onClose; }, [onClose]);

  useEffect(() => {
    if (!open) return;
    const id = Symbol();
    dialogs.set(id, () => {
      if (!isTopmostDialog(dialogRef.current)) return false;
      closeRef.current();
      return true;
    });
    // Coalesce StrictMode replay and dialogs opening/closing in the same commit.
    queueMicrotask(syncHistory);
    return () => {
      dialogs.delete(id);
      queueMicrotask(syncHistory);
    };
  }, [open, dialogRef]);

  useEffect(() => {
    if (!open || !isDirty) return;
    const beforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };
    window.addEventListener("beforeunload", beforeUnload);
    return () => window.removeEventListener("beforeunload", beforeUnload);
  }, [open, isDirty]);
}
