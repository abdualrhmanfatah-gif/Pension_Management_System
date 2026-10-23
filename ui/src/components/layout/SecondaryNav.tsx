import { ChevronDown } from "lucide-react";
import { memo, useEffect, useMemo, useState } from "react";
import { NavLink, useLocation } from "react-router-dom";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
import { useNavStore } from "@/stores/navStore";

import {
  filterByPermissions,
  groups,
  type NavGroup,
  type NavItem,
  navItems,
  subGroupLabels,
} from "./navigationData";

// ─── Helpers ──────────────────────────────────────────────

function isPathActive(pathname: string, path: string): boolean {
  return pathname === path || pathname.startsWith(`${path}/`);
}

function getActiveGroup(visibleItems: NavItem[], pathname: string): NavGroup | null {
  for (const group of groups) {
    const groupItems = visibleItems.filter((item) => item.group === group);
    if (groupItems.some((item) => isPathActive(pathname, item.path))) {
      return group;
    }
  }
  return null;
}

// ─── SubGroup Dropdown ───────────────────────────────────

const SubGroupDropdown = memo(function SubGroupDropdown({
  label,
  items,
}: {
  label: string;
  items: NavItem[];
}) {
  const [open, setOpen] = useState(false);
  const location = useLocation();

  const isActive = items.some((item) => isPathActive(location.pathname, item.path));

  return (
    <DropdownMenu open={open} onOpenChange={setOpen} dir="rtl">
      <DropdownMenuTrigger
        className={cn(
          "flex items-center gap-1 px-3 py-2 rounded-md text-base font-semibold transition-colors duration-150 outline-none whitespace-nowrap",
          isActive
            ? "bg-primary/10 text-primary"
            : "text-muted-foreground hover:bg-muted hover:text-foreground"
        )}
      >
        <span>{label}</span>
        <ChevronDown
          className={cn("h-3 w-3 shrink-0 transition-transform duration-200", open && "rotate-180")}
        />
      </DropdownMenuTrigger>

      <DropdownMenuContent align="start" sideOffset={4} className="w-56 p-1">
        {items.map((item) => (
          <DropdownMenuItem key={item.path} asChild>
            <NavLink
              to={item.path}
              onClick={() => setOpen(false)}
              className={({ isActive: linkActive }) =>
                cn(
                  "no-underline flex items-center gap-2 px-3 py-2 text-base rounded-lg cursor-pointer transition-colors duration-150",
                  linkActive ? "bg-primary/10 text-primary font-semibold" : "text-foreground hover:bg-muted"
                )
              }
            >
              <span className="shrink-0 text-muted-foreground">{item.icon}</span>
              <span className="truncate">{item.label}</span>
            </NavLink>
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
});

// ─── SecondaryNav Component ──────────────────────────────

export const SecondaryNav = memo(function SecondaryNav() {
  const permissions = useAuthStore((state) => state.permissions);
  const location = useLocation();
  const setHasSecondaryNav = useNavStore((s) => s.setHasSecondaryNav);

  const visibleItems = useMemo(
    () => filterByPermissions(navItems, permissions ?? []),
    [permissions]
  );

  const activeGroup = useMemo(() => {
    const detected = getActiveGroup(visibleItems, location.pathname);
    return detected;
  }, [visibleItems, location.pathname]);

  const groupItems = visibleItems.filter((item) => item.group === activeGroup);

  const subGroups = useMemo(() => {
    if (groupItems.length === 0) return [];
    return Array.from(
      new Set(groupItems.filter((item) => item.subGroup).map((item) => item.subGroup!))
    );
  }, [groupItems]);

  const flatItems = useMemo(() => {
    return groupItems.filter((item) => !item.subGroup);
  }, [groupItems]);

  const isVisible = subGroups.length > 0 || flatItems.length > 0;

  useEffect(() => {
    setHasSecondaryNav(isVisible);
    return () => { setHasSecondaryNav(false); };
  }, [isVisible, setHasSecondaryNav]);

  if (!isVisible) return null;

  return (
    <div
      dir="rtl"
      className={cn(
        "sticky top-14 z-40 hidden lg:flex items-center h-10 border-b border-border bg-background px-4 print:hidden"
      )}
    >
      <nav className="flex items-center gap-1 overflow-x-auto [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
        {flatItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              cn(
                "no-underline flex items-center gap-1.5 px-3 py-2 rounded-md text-base font-semibold transition-colors duration-150 outline-none whitespace-nowrap",
                isActive
                  ? "bg-primary/10 text-primary"
                  : "text-muted-foreground hover:bg-muted hover:text-foreground"
              )
            }
          >
            <span className="shrink-0 text-muted-foreground">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
        {subGroups.map((subGroup) => {
          const items = groupItems.filter((item) => item.subGroup === subGroup);
          if (items.length === 0) return null;

          return (
            <SubGroupDropdown
              key={subGroup}
              label={subGroupLabels[subGroup] || subGroup}
              items={items}
            />
          );
        })}
      </nav>
    </div>
  );
});
