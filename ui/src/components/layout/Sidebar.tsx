// ─── Sidebar.tsx ───────────────────────────────────────

import * as Accordion from "@radix-ui/react-accordion";
import * as Collapsible from "@radix-ui/react-collapsible";
import {
  LogOut,
  Minus,
  Plus,
  User,
  X,
} from "lucide-react";
import { memo, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";

import { Button } from "@/components/ui";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
import { useSidebarStore } from "@/stores/sidebarStore";

import {
  dashboardItem,
  filterByPermissions,
  groupIcons,
  groupLabels,
  groups,
  type NavItem,
  navItems,
  subGroupLabels,
} from "./navigationData";

// ─── Memoized Components ─────────────────────────────────

const SidebarNavLink = memo(function SidebarNavLink({ item, onClick }: { item: NavItem, onClick?: () => void }) {
  return (
    <NavLink
      to={item.path}
      onClick={onClick}
      className={({ isActive }) =>
        cn(
          "relative flex items-center gap-3 rounded-xl px-3 py-3 text-sm font-semibold transition-all duration-200 group overflow-hidden hover:no-underline outline-none min-h-[44px]",
          isActive
            ? "bg-white/[0.12] text-white"
            : "text-white hover:bg-white/[0.08]"
        )
      }
    >
      {({ isActive }) => (
        <>
          {isActive && (
            <div className="absolute end-0 top-1/2 -translate-y-1/2 w-1.5 h-1/2 bg-amber-400 rounded-s-full" />
          )}
          <span
            className={cn(
              "shrink-0 transition-transform duration-200 group-hover:scale-110",
              isActive
                ? "text-amber-400"
                : "text-white group-hover:text-amber-400"
            )}
          >
            {item.icon}
          </span>
          <span className="truncate tracking-wide flex-1 text-start">
            {item.label}
          </span>
        </>
      )}
    </NavLink>
  );
});

const SubGroupSection = memo(function SubGroupSection({
  items,
  subGroup,
  onItemClick,
}: {
  items: NavItem[];
  subGroup: string;
  onItemClick?: () => void;
}) {
  const subItems = items.filter((i) => i.subGroup === subGroup && !i.nestedIn);
  const nestedSubGroups = Array.from(
    new Set(items.filter((i) => i.nestedIn === subGroup).map((i) => i.subGroup!))
  );
  
  if (subItems.length === 0 && nestedSubGroups.length === 0) return null;

  return (
    <Accordion.Item value={subGroup} className="border-0 mt-1">
      <Accordion.Header>
        <Accordion.Trigger
          className={cn(
            "flex w-full items-center gap-2 rounded-lg px-2 py-2 min-h-[36px]",
            "text-xs font-bold text-white tracking-wide",
            "transition-all hover:bg-white/[0.04] hover:text-white",
            "hover:no-underline outline-none sub-group-trigger",
            "[&_svg]:transition-all [&_svg]:duration-200"
          )}
        >
          <div className="h-[1px] w-3 bg-white/[0.12] shrink-0" />
          <span className="flex-1 text-start">
            {subGroupLabels[subGroup] || subGroup}
          </span>
          <span className="text-white group-hover:text-white group-data-[state=open]:text-amber-400">
            <Plus className="h-4 w-4 hidden group-data-[state=closed]:block" />
            <Minus className="h-4 w-4 hidden group-data-[state=open]:block" />
          </span>
        </Accordion.Trigger>
      </Accordion.Header>
      <Accordion.Content className="overflow-hidden data-[state=closed]:animate-slide-up data-[state=open]:animate-slide-down">
        <div className="space-y-0.5 ps-2 border-s border-white ms-1 mt-0.5 pb-1">
          {subItems.map((item) => (
            <SidebarNavLink key={item.path} item={item} onClick={onItemClick} />
          ))}
          {nestedSubGroups.map((nestedGroup) => (
            <div key={nestedGroup} className="mt-2">
              <div className="text-[10px] font-bold text-amber-400 px-2 py-1 tracking-wider">
                {subGroupLabels[nestedGroup] || nestedGroup}
              </div>
              <div className="space-y-0.5 border-s border-amber-400 ms-1 mt-0.5">
                {items
                  .filter((i) => i.nestedIn === subGroup && i.subGroup === nestedGroup)
                  .map((item) => (
                    <SidebarNavLink key={item.path} item={item} onClick={onItemClick} />
                  ))}
              </div>
            </div>
          ))}
        </div>
      </Accordion.Content>
    </Accordion.Item>
  );
});

// ─── Sidebar Component ─────────────────────────────────

function useIsMobile(query = "(max-width: 1023px)") {
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    if (typeof window === "undefined") return;
    const mql = window.matchMedia(query);
    setIsMobile(mql.matches);
    const handler = (e: MediaQueryListEvent) => setIsMobile(e.matches);
    mql.addEventListener("change", handler);
    return () => mql.removeEventListener("change", handler);
  }, [query]);

  return isMobile;
}

export function Sidebar() {
  const permissions = useAuthStore((s) => s.permissions);
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();
  const isOpen = useSidebarStore((s) => s.isOpen);
  const closeSidebar = useSidebarStore((s) => s.close);
  const asideRef = useRef<HTMLElement>(null);
  const isMobile = useIsMobile();

  const handleClose = useCallback(() => {
    closeSidebar();
    if (isMobile) {
      window.dispatchEvent(new CustomEvent("sidebar:closed"));
      (document.activeElement as HTMLElement)?.blur();
    }
  }, [closeSidebar, isMobile]);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const visibleItems = useMemo(() => filterByPermissions(navItems, permissions), [permissions]);

  const defaultOpenGroups = useMemo(() => {
    return groups.filter((g) =>
      visibleItems.some((i) => i.group === g && i.path !== "/dashboard")
    );
  }, [visibleItems]);

  const [openGroups, setOpenGroups] = useState<string[]>(defaultOpenGroups);

  // تحديث المجموعات المفتوحة عند تغير الصلاحيات
  useEffect(() => {
    setOpenGroups((prev) => {
      const stillValid = prev.filter((g) =>
        visibleItems.some((i) => i.group === g)
      );
      const newGroups = defaultOpenGroups.filter(
        (g) => !stillValid.includes(g)
      );
      return [...stillValid, ...newGroups];
    });
  }, [defaultOpenGroups, visibleItems]);

  const shouldHide = isMobile && !isOpen;

  // ── Swipe to close (mobile) ──
  const touchStartX = useRef(0);
  const touchStartY = useRef(0);

  const handleTouchStart = useCallback((e: React.TouchEvent) => {
    touchStartX.current = e.touches[0].clientX;
    touchStartY.current = e.touches[0].clientY;
  }, []);

  const handleTouchEnd = useCallback((e: React.TouchEvent) => {
    if (!isMobile) return;
    const deltaX = e.changedTouches[0].clientX - touchStartX.current;
    const deltaY = Math.abs(e.changedTouches[0].clientY - touchStartY.current);
    // Swipe right > 80px to close (RTL sidebar on right)
    if (deltaX > 80 && deltaY < 50) {
      handleClose();
    }
  }, [isMobile, handleClose]);

  // ── Focus trap + Escape key (mobile a11y) ──
  useEffect(() => {
    if (!isMobile || !isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        handleClose();
        return;
      }
      if (e.key === "Tab" && asideRef.current) {
        const focusable = asideRef.current.querySelectorAll<HTMLElement>(
          'a[href], button:not([disabled]), [tabindex]:not([tabindex="-1"])'
        );
        if (!focusable.length) return;
        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        if (e.shiftKey && document.activeElement === first) {
          e.preventDefault();
          last.focus();
        } else if (!e.shiftKey && document.activeElement === last) {
          e.preventDefault();
          first.focus();
        }
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    // Focus first link on open
    requestAnimationFrame(() => {
      asideRef.current?.querySelector<HTMLElement>("a[href], button")?.focus();
    });

    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isMobile, isOpen, handleClose]);

  return (
    <aside
      ref={asideRef}
      dir="rtl"
      aria-hidden={shouldHide}
      className={cn(
        "fixed top-0 right-0 z-[60] flex h-dvh w-64 max-w-[85vw] flex-col will-change-transform",
        "bg-[hsl(var(--sidebar))] text-[hsl(var(--sidebar-foreground))]",
        "border-l border-white",
        "transition-transform duration-300 ease-out",
        isOpen ? "max-lg:translate-x-0" : "max-lg:translate-x-full",
        "lg:translate-x-0"
      )}
      style={{
        paddingTop: "env(safe-area-inset-top)",
        paddingBottom: "env(safe-area-inset-bottom)",
      }}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
    >
      {/* ── Header ─────────────────────────────── */}
      <div className="shrink-0 px-5 py-6 border-b border-white">
        <div className="flex items-center gap-3.5">
          <picture>
            <source srcSet="/pension-logo.webp" type="image/webp" />
            <img
              src="/pension-logo.png"
              alt="الشعار"
              className="w-11 h-11 object-contain shrink-0"
            />
          </picture>
          <div className="min-w-0 flex-1">
            <h1 className="text-sm font-black text-white leading-tight truncate tracking-wide">
              صندوق التقاعد الأمني
            </h1>
            <p className="text-xs text-white mt-1 truncate font-medium tracking-wider">
              الإدارة العامة
            </p>
          </div>
          <Button
            variant="ghost"
            size="icon"
            onClick={handleClose}
            className="lg:hidden h-10 w-10 text-white hover:text-white hover:bg-white/[0.08]"
            aria-label="إغلاق القائمة"
          >
            <X className="h-5 w-5" />
          </Button>
        </div>
      </div>

      {/* ── Navigation ───────────────────────── */}
      <nav
        className="flex-1 overflow-y-auto py-3 px-2.5"
        aria-label="القائمة الرئيسية"
        style={{
          scrollbarWidth: "thin",
          scrollbarColor: "rgba(255,255,255,0.08) transparent",
        }}
      >
        {/* Dashboard Link */}
        <div className="mb-2">
          <SidebarNavLink item={dashboardItem} onClick={handleClose} />
        </div>

        {groups.map((group) => {
          const items = visibleItems.filter(
            (i) => i.group === group && i.path !== "/dashboard"
          );
          if (items.length === 0) return null;

          const subGroups = Array.from(
            new Set(items.filter((i) => i.subGroup && !i.nestedIn).map((i) => i.subGroup!))
          );
          const mainItems = items.filter((i) => !i.subGroup);
          const isOpenGroup = openGroups.includes(group);

          return (
            <Collapsible.Root
              key={group}
              open={isOpenGroup}
              onOpenChange={(open) => {
                setOpenGroups((prev) =>
                  open ? [...prev, group] : prev.filter((g) => g !== group)
                );
              }}
              className="mb-0.5"
            >
              <Collapsible.Trigger
                className={cn(
                  "flex w-full items-center gap-3 rounded-xl px-3 py-3 min-h-[44px]",
                  "text-xs font-bold text-white tracking-wide",
                  "transition-all hover:bg-white/[0.04] hover:text-white",
                  "hover:no-underline outline-none group",
                  "[&_svg]:transition-all [&_svg]:duration-200"
                )}
              >
                <span className="shrink-0 text-white group-hover:text-amber-400 group-data-[state=open]:text-amber-400 group-hover:scale-110 transition-transform">
                  {groupIcons[group]}
                </span>
                <span className="flex-1 text-start">
                  {groupLabels[group]}
                </span>
                  <span className="text-white group-hover:text-white group-data-[state=open]:text-amber-400">
                  <Plus className="h-5 w-5 hidden group-data-[state=closed]:block" />
                  <Minus className="h-5 w-5 hidden group-data-[state=open]:block" />
                </span>
              </Collapsible.Trigger>
              <Collapsible.Content className="overflow-hidden data-[state=closed]:animate-slide-up data-[state=open]:animate-slide-down">
                <div className="space-y-0.5 pb-2 pt-0.5 ps-2">
                  {/* Main items */}
                  {mainItems.map((item) => (
                    <SidebarNavLink key={item.path} item={item} onClick={handleClose} />
                  ))}

                  {/* SubGroups */}
                  {subGroups.length > 0 && (
                    <Accordion.Root
                      type="multiple"
                      defaultValue={[]}
                      className="mt-2 space-y-0.5"
                    >
                      {subGroups.map((subGroup) => (
                        <SubGroupSection
                          key={subGroup}
                          items={items}
                          subGroup={subGroup}
                          onItemClick={handleClose}
                        />
                      ))}
                    </Accordion.Root>
                  )}
                </div>
              </Collapsible.Content>
            </Collapsible.Root>
          );
        })}
      </nav>

      {/* ── User Footer ────────────────────────── */}
      <div className="shrink-0 border-t border-white px-3 py-3">
        <div className="flex items-center gap-2.5">
          <div className="flex items-center justify-center w-10 h-10 rounded-md bg-white/[0.08] shrink-0">
            <User className="h-4 w-4 text-white" />
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-xs font-semibold text-white truncate leading-tight">
              {user?.fullName || "مستخدم"}
            </p>
            <p className="text-[10px] sm:text-xs text-white truncate">
              {user?.role || ""}
            </p>
          </div>
          <Button
            variant="ghost"
            size="icon"
            onClick={handleLogout}
            className="h-10 w-10 text-white hover:text-red-400 hover:bg-white/[0.08]"
            aria-label="تسجيل الخروج"
          >
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </aside>
  );
}