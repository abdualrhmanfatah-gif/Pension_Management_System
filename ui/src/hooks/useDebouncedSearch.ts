import { useEffect, useState } from "react";

/**
 * useDebouncedSearch — A hook that debounces search input values.
 * Eliminates the duplicated debounce pattern across 15+ pages.
 *
 * @param initialValue - Initial search value (default: "")
 * @param delay - Debounce delay in ms (default: 500)
 * @returns Object with searchTerm, setSearchTerm, and debouncedValue
 */
export function useDebouncedSearch(initialValue = "", delay = 500) {
    const [searchTerm, setSearchTerm] = useState(initialValue);
    const [debouncedValue, setDebouncedValue] = useState(initialValue);

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedValue(searchTerm);
        }, delay);
        return () => clearTimeout(timer);
    }, [searchTerm, delay]);

    return { searchTerm, setSearchTerm, debouncedValue };
}
