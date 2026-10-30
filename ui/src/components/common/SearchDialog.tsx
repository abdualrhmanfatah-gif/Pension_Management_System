<<<<<<< HEAD
import { useQuery } from "@tanstack/react-query";
import { Search, User } from "lucide-react";
import { useCallback, useEffect, useMemo,useRef, useState } from "react";

import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/Dialog";
import { Input } from "@/components/ui/Input";
import { matchesShortcut, SHORTCUT_GLOBAL_SEARCH } from "@/constants/shortcuts";
=======
import { Search, User } from "lucide-react";
import { useEffect, useState, useRef, useCallback, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";

import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/Dialog";
import { Input } from "@/components/ui/Input";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { cn } from "@/lib/utils";

interface SearchDialogProps<T> {
    value: string;
    onSelect: (name: string) => void;
    queryKey: string[];
    fetchFn: (searchTerm: string) => Promise<{ items: T[] }>;
    getName: (item: T) => string;
    getSubtitle?: (item: T) => string;
    title?: string;
    placeholder?: string;
    minLength?: number;
    maxResults?: number;
    hideTrigger?: boolean;
}

export function SearchDialog<T>({
    value: _value,
    onSelect,
    queryKey,
    fetchFn,
    getName,
    getSubtitle,
    title = "بحث بالاسم",
    placeholder = "بحث بالاسم أو الرقم...",
    minLength = 2,
    maxResults = 20,
    hideTrigger = false,
}: SearchDialogProps<T>) {
    const [open, setOpen] = useState(false);
    const [searchText, setSearchText] = useState("");
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [activeIndex, setActiveIndex] = useState(-1);
    const listRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (open && inputRef.current) {
            const timer = setTimeout(() => inputRef.current?.focus(), 60);
            return () => clearTimeout(timer);
        }
    }, [open]);

    useEffect(() => {
        const handleShortcut = (e: KeyboardEvent) => {
<<<<<<< HEAD
            if (matchesShortcut(e, SHORTCUT_GLOBAL_SEARCH)) {
=======
            if ((e.ctrlKey || e.metaKey) && e.shiftKey && (e.code === "KeyF" || e.key === "f" || e.key === "\u0642")) {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                e.preventDefault();
                setOpen((prev) => !prev);
            }
        };
        window.addEventListener("keydown", handleShortcut);
        return () => window.removeEventListener("keydown", handleShortcut);
    }, []);

    useEffect(() => {
        const timer = setTimeout(() => setDebouncedSearch(searchText), 300);
        return () => clearTimeout(timer);
    }, [searchText]);

    const { data: results, isLoading } = useQuery({
        queryKey: [...queryKey, debouncedSearch],
        queryFn: () => fetchFn(debouncedSearch),
        enabled: open && debouncedSearch.length >= minLength,
    });

    const items = results?.items ?? [];

    const uniqueNames = useMemo(() => {
        const seen = new Map<string, { name: string; subtitle: string }>();
        for (const item of items) {
            const name = getName(item);
            if (!name) continue;
            if (!seen.has(name)) {
                seen.set(name, { name, subtitle: getSubtitle?.(item) ?? "" });
            }
        }
        return Array.from(seen.values()).slice(0, maxResults);
    }, [items, getName, getSubtitle, maxResults]);

    const selectName = useCallback((name: string) => {
        onSelect(name);
        setOpen(false);
        setSearchText("");
        setActiveIndex(-1);
    }, [onSelect]);

    const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
        if (!uniqueNames.length) return;
        if (e.key === "ArrowDown") {
            e.preventDefault();
            setActiveIndex((prev) => (prev < uniqueNames.length - 1 ? prev + 1 : 0));
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            setActiveIndex((prev) => (prev > 0 ? prev - 1 : uniqueNames.length - 1));
        } else if (e.key === "Enter" && activeIndex >= 0) {
            e.preventDefault();
            selectName(uniqueNames[activeIndex].name);
        } else if (e.key === "Escape") {
            setOpen(false);
        }
    }, [uniqueNames, activeIndex, selectName]);

    useEffect(() => {
        if (activeIndex < 0 || !listRef.current) return;
        const el = listRef.current.children[activeIndex] as HTMLElement;
        el?.scrollIntoView({ block: "nearest" });
    }, [activeIndex]);

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            {!hideTrigger && (
                <DialogTrigger className="inline-flex items-center justify-center w-10 h-10 rounded-md border border-input bg-background transition-colors hover:bg-muted shrink-0" title={title}>
                    <Search className="w-4 h-4" />
                </DialogTrigger>
            )}
<<<<<<< HEAD
            <DialogContent className="sm:max-w-2xl p-0 gap-0 overflow-hidden border-border">
=======
            <DialogContent className="sm:max-w-2xl p-0 gap-0 overflow-hidden border-border/60">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <DialogHeader className="px-5 pt-5 pb-0 border-b-0">
                    <DialogTitle className="text-lg font-bold">{title}</DialogTitle>
                </DialogHeader>
                <div className="px-5 pt-3 pb-5">
                    <div className="relative">
                        <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-muted-foreground" />
                        <Input
                            ref={inputRef}
                            placeholder={placeholder}
                            value={searchText}
                            onChange={(e) => { setSearchText(e.target.value); setActiveIndex(-1); }}
                            onKeyDown={handleKeyDown}
<<<<<<< HEAD
                            className="pr-10 h-12 text-base border-2 border-primary focus-visible:border-primary focus-visible:ring-1 focus-visible:ring-primary/20 bg-muted/30"
=======
                            className="pr-10 h-12 text-base border-2 border-primary/20 focus-visible:border-primary/40 focus-visible:ring-1 focus-visible:ring-primary/20 bg-muted/30"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            autoFocus
                        />
                    </div>

                    {debouncedSearch.length >= minLength && (
<<<<<<< HEAD
                        <div ref={listRef} className="mt-3 max-h-[28rem] overflow-y-auto space-y-1 divide-y divide-border" role="listbox">
=======
                        <div ref={listRef} className="mt-3 max-h-[28rem] overflow-y-auto space-y-1 divide-y divide-border/40" role="listbox">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            {isLoading ? (
                                <p className="text-sm text-muted-foreground text-center py-8">جاري البحث...</p>
                            ) : uniqueNames.length > 0 ? (
                                uniqueNames.map((item, index) => (
                                    <button
                                        key={item.name}
                                        role="option"
                                        aria-selected={index === activeIndex}
                                        className={cn(
                                            "w-full text-right p-3.5 transition-colors flex items-center justify-between",
                                            index === activeIndex
                                                ? "bg-primary/5"
                                                : "hover:bg-muted/50"
                                        )}
                                        onClick={() => selectName(item.name)}
                                        onMouseEnter={() => setActiveIndex(index)}
                                    >
                                        <div className="flex items-center gap-3 min-w-0">
                                            <div className="flex items-center justify-center w-9 h-9 rounded-full bg-primary/10 shrink-0">
                                                <User className="w-4 h-4 text-primary" />
                                            </div>
                                            <span className="text-sm font-medium truncate">{item.name}</span>
                                        </div>
                                        {item.subtitle && (
                                            <span className="text-xs text-muted-foreground truncate mr-3 bg-muted/50 px-2 py-1 rounded">{item.subtitle}</span>
                                        )}
                                    </button>
                                ))
                            ) : (
                                <p className="text-sm text-muted-foreground text-center py-8">لا توجد نتائج</p>
                            )}
                        </div>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    );
}
