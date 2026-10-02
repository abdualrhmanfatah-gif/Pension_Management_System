import { useCallback, useEffect, useRef,useState } from "react";

/**
 * Hook للتنقل بلوحة المفاتيح في القوائم
 * يدعم الأسهم + Enter + Escape
 */
export function useKeyboardNavigation(
  itemCount: number,
  onSelect: (index: number) => void
) {
  const [focusedIndex, setFocusedIndex] = useState(-1);
  const focusedIndexRef = useRef(-1);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      switch (e.key) {
        case "ArrowDown":
          e.preventDefault();
          setFocusedIndex((i) => Math.min(i + 1, itemCount - 1));
          break;
        case "ArrowUp":
          e.preventDefault();
          setFocusedIndex((i) => Math.max(i - 1, 0));
          break;
        case "Enter":
          if (focusedIndexRef.current >= 0 && focusedIndexRef.current < itemCount) {
            onSelect(focusedIndexRef.current);
          }
          break;
        case "Escape":
          setFocusedIndex(-1);
          break;
      }
    },
    [itemCount, onSelect]
  );

  useEffect(() => {
    if (focusedIndex >= 0) {
      const row = document.querySelector(`[data-row-index="${focusedIndex}"]`);
      if (row) {
        row.scrollIntoView({ behavior: "smooth", block: "nearest" });
      }
    }
    focusedIndexRef.current = focusedIndex;
  }, [focusedIndex]);

  const reset = useCallback(() => {
    setFocusedIndex(-1);
    focusedIndexRef.current = -1;
  }, []);

  return {
    focusedIndex,
    setFocusedIndex,
    handleKeyDown,
    reset,
  };
}

export default useKeyboardNavigation;
