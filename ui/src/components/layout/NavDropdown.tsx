import { ChevronDown } from "lucide-react";
import { memo, useCallback, useMemo, useState } from "react";
import { NavLink, useLocation } from "react-router-dom";

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";

import {
  filterByPermissions,
  groupIcons,
  groupLabels,
  type NavGroup,
  type NavItem,
  subGroupLabels,
} from "./navigationData";

// ─── Helpers ──────────────────────────────────────────────

function isActiveGroup(items: NavItem[], pathname: string): boolean {
  return items.some(
    (item) => pathname === item.path || pathname.startsWith(item.path + "/")
  );
}

// ─── DropdownNavLink (styled item inside dropdown) ──────

const DropdownNavLink = memo(function DropdownNavLink({
  item,
  onSelect,
}: {
  item: NavItem;
  onSelect: () => void;
}) {
  return (
    <DropdownMenuItem asChild>
      <NavLink
        to={item.path}
        onClick={onSelect}
        className={({ isActive }) =>
          cn(
            "flex items-center gap-2.5 px-3 py-2.5 text-base rounded-lg cursor-pointer",
            "transition-colors duration-150",
            isActive
              ? "bg-primary/10 text-primary font-semibold"
              : "text-foreground hover:bg-muted hover:text-foreground"
          )
        }
      >
        <span className="shrink-0 text-muted-foreground">{item.icon}</span>
        <span className="truncate">{item.label}</span>
      </NavLink>
    </DropdownMenuItem>
  );
});

// ─── NavDropdown Component ──────────────────────────────

interface NavDropdownProps {
  group: NavGroup;
  items: NavItem[];
}

export const NavDropdown = memo(function NavDropdown({
  group,
  items,
}: NavDropdownProps) {
  const [open, setOpen] = useState(false);
  const location = useLocation();
  const permissions = useAuthStore((s) => s.permissions);

  const visibleItems = useMemo(
    () => filterByPermissions(items, permissions),
    [items, permissions]
  );

  if (visibleItems.length === 0) return null;

  const mainItems = visibleItems.filter((i) => !i.subGroup);
  const subGroups = Array.from(
    new Set(visibleItems.filter((i) => i.subGroup && !i.nestedIn).map((i) => i.subGroup!))
  );
  const isGroupActive = isActiveGroup(visibleItems, location.pathname);

  const handleSelect = useCallback(() => setOpen(false), []);

  return (
    <DropdownMenu open={open} onOpenChange={setOpen} dir="rtl">
      <DropdownMenuTrigger
        className={cn(
          "flex items-center gap-1.5 px-3 py-2 rounded-lg text-base font-semibold transition-colors duration-150 outline-none",
          "hover:bg-muted hover:text-foreground",
          "data-[state=open]:bg-muted data-[state=open]:text-foreground",
          "data-[disabled]:pointer-events-none data-[disabled]:opacity-50",
          isGroupActive
            ? "text-primary bg-primary/5"
            : "text-muted-foreground"
        )}
      >
        <span className={cn("shrink-0", isGroupActive ? "text-primary" : "text-muted-foreground")}>
          {groupIcons[group]}
        </span>
        <span className="hidden lg:inline">{groupLabels[group]}</span>
        <ChevronDown
          className={cn(
            "h-3.5 w-3.5 shrink-0 transition-transform duration-200",
            open && "rotate-180"
          )}
        />
      </DropdownMenuTrigger>

      <DropdownMenuContent
        align="start"
        sideOffset={8}
        className={cn(
          "max-h-[70vh] overflow-y-auto",
          subGroups.length > 0 ? "w-[min(400px,90vw)]" : "w-[280px]"
        )}
      >
        {/* Main items (no subGroup) */}
        {mainItems.length > 0 && (
          <div className="p-2 pb-1">
            {mainItems.map((item) => (
              <DropdownNavLink key={item.path} item={item} onSelect={handleSelect} />
            ))}
          </div>
        )}

        {mainItems.length > 0 && subGroups.length > 0 && (
          <div className="mx-2 border-t border-border" />
        )}

        {/* All sub-groups — flat list with labels */}
        {subGroups.length > 0 && (
          <div className="p-2">
            {subGroups.map((sg) => {
              const sgDirectItems = visibleItems.filter(
                (i) => i.subGroup === sg && !i.nestedIn
              );
              const sgNestedGroups = Array.from(
                new Set(
                  visibleItems
                    .filter((i) => i.nestedIn === sg)
                    .map((i) => i.subGroup!)
                )
              );

              return (
                <div key={sg} className="mb-1">
                  {sgDirectItems.length > 0 && (
                    <DropdownMenuLabel className="px-2 py-1.5 text-[11px] font-bold text-muted-foreground uppercase tracking-wider">
                      {subGroupLabels[sg] || sg}
                    </DropdownMenuLabel>
                  )}
                  <div className="space-y-0.5">
                    {sgDirectItems.map((item) => (
                      <DropdownNavLink
                        key={item.path}
                        item={item}
                        onSelect={handleSelect}
                      />
                    ))}
                  </div>

                  {sgNestedGroups.map((nestedGroup) => (
                    <div key={nestedGroup} className="mt-2">
                      <div className="flex items-center gap-1.5 px-3 py-1 text-[10px] font-bold text-primary tracking-wider">
                        <div className="h-px w-2 bg-primary/30" />
                        {subGroupLabels[nestedGroup] || nestedGroup}
                      </div>
                      <div className="space-y-0.5 border-e-2 border-primary ms-1">
                        {visibleItems
                          .filter(
                            (i) => i.nestedIn === sg && i.subGroup === nestedGroup
                          )
                          .map((item) => (
                            <DropdownNavLink
                              key={item.path}
                              item={item}
                              onSelect={handleSelect}
                            />
                          ))}
                      </div>
                    </div>
                  ))}
                </div>
              );
            })}
          </div>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
});