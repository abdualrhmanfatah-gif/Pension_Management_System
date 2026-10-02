import { ChevronDown, KeyRound, LogOut, Menu, Moon, Sun, User as UserIcon } from "lucide-react";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import {
  Button,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";
import { useSidebarStore } from "@/stores/sidebarStore";
import { useThemeStore } from "@/stores/themeStore";

import { NavDropdown } from "./NavDropdown";
import { filterByPermissions, groups, type NavGroup,navItems } from "./navigationData";
import { NotificationBell } from "./NotificationBell";
import { SecondaryNav } from "./SecondaryNav";

// ─── Theme Toggle ────────────────────────────────────────

const ThemeToggle = memo(function ThemeToggle() {
  const isDark = useThemeStore((s) => s.isDark);
  const toggleTheme = useThemeStore((s) => s.toggle);

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={toggleTheme}
      className="h-9 w-9 sm:h-10 sm:w-10 text-muted-foreground hover:text-foreground"
      aria-label={isDark ? "الوضع النهاري" : "الوضع الليلي"}
      title={isDark ? "الوضع النهاري" : "الوضع الليلي"}
    >
      {isDark ? <Sun className="h-4 w-4 sm:h-5 sm:w-5" /> : <Moon className="h-4 w-4 sm:h-5 sm:w-5" />}
    </Button>
  );
});

// ─── User Menu ───────────────────────────────────────────

const UserMenu = memo(function UserMenu() {
  const [open, setOpen] = useState(false);
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    setOpen(false);
    logout(); // auth:logout event handled by AuthEventListener in App.tsx
  };

  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          className="flex items-center gap-2 px-1 sm:ps-3 sm:pe-2 py-2 h-auto hover:bg-muted/50 rounded-lg"
        >
          <div className="text-end hidden sm:block">
            <p className="text-sm font-medium text-foreground leading-tight">
              {user?.fullName ?? "مستخدم"}
            </p>
            <p className="text-xs text-muted-foreground">{user?.role ?? "---"}</p>
          </div>
          <div className="flex items-center justify-center w-8 h-8 sm:w-9 sm:h-9 rounded-full bg-primary/10 text-primary text-sm font-bold">
            {user?.fullName?.charAt(0) || "م"}
          </div>
          <ChevronDown
            className={cn(
              "h-4 w-4 text-muted-foreground hidden sm:block transition-transform duration-200",
              open && "rotate-180"
            )}
          />
        </Button>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" sideOffset={8} className="w-56 sm:w-64 p-1.5">
        {/* User info header */}
        <div className="px-3 py-2.5 border-b border-border/50 mb-1">
          <p className="text-sm font-semibold">{user?.fullName ?? "مستخدم"}</p>
          <p className="text-xs text-muted-foreground mt-0.5">{user?.username ?? "---"}</p>
        </div>

        {/* Menu items */}
        <div className="py-1">
          <DropdownMenuItem
            onClick={() => {
              setOpen(false);
              navigate("/account");
            }}
            className="gap-2"
          >
            <UserIcon className="h-4 w-4 text-muted-foreground shrink-0" />
            <span>حسابي</span>
          </DropdownMenuItem>
          <DropdownMenuItem
            onClick={() => {
              setOpen(false);
              navigate("/change-password");
            }}
            className="gap-2"
          >
            <KeyRound className="h-4 w-4 text-muted-foreground shrink-0" />
            <span>تغيير كلمة المرور</span>
          </DropdownMenuItem>
        </div>

        {/* Logout */}
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={handleLogout}
          className="gap-2 text-destructive focus:text-destructive focus:bg-destructive/10"
        >
          <LogOut className="h-4 w-4 shrink-0" />
          <span>تسجيل الخروج</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
});

// ─── TopNav Component ────────────────────────────────────

export const TopNav = memo(function TopNav() {
  const permissions = useAuthStore((s) => s.permissions);
  const toggleSidebar = useSidebarStore((s) => s.toggle);
  const menuBtnRef = useRef<HTMLButtonElement>(null);

  // Focus return after sidebar close
  useEffect(() => {
    const handler = () => menuBtnRef.current?.focus();
    window.addEventListener("sidebar:closed", handler);
    return () => window.removeEventListener("sidebar:closed", handler);
  }, []);

  const visibleItems = useMemo(
    () => filterByPermissions(navItems, permissions),
    [permissions]
  );

  // Group items by their group key
  const groupedItems = useMemo(() => {
    const map = new Map<NavGroup, typeof navItems>();
    for (const group of groups) {
      const groupItems = visibleItems.filter((i) => i.group === group);
      if (groupItems.length > 0) {
        map.set(group, groupItems);
      }
    }
    return map;
  }, [visibleItems]);

  return (
    <div className="sticky top-0 z-50">
      {/* ── Mobile Header ──────────────────────────── */}
      <header dir="rtl" className="flex lg:hidden items-center h-14 border-0 bg-background px-2 sm:px-4 print:hidden overflow-hidden">
        <Button
          ref={menuBtnRef}
          variant="ghost"
          size="icon"
          onClick={toggleSidebar}
          className="h-9 w-9 sm:h-11 sm:w-11 shrink-0 text-muted-foreground hover:text-foreground me-1"
          aria-label="فتح القائمة"
        >
          <Menu className="h-4 w-4 sm:h-5 sm:w-5" />
        </Button>

        <Link
          to="/dashboard"
          className="flex items-center gap-1.5 sm:gap-2 min-w-0 hover:opacity-80 transition-opacity"
        >
          <picture>
            <source srcSet="/pension-logo.webp" type="image/webp" />
            <img
              src="/pension-logo.png"
              alt="الشعار"
              className="w-7 h-7 sm:w-8 sm:h-8 object-contain shrink-0"
            />
          </picture>
          <h1 className="text-xs sm:text-sm font-black text-foreground leading-tight truncate">
            صندوق التقاعد الأمني
          </h1>
        </Link>

        <div className="flex items-center gap-0 ms-auto shrink-0">
          <ThemeToggle />
          <NotificationBell />
          <UserMenu />
        </div>
      </header>

{/* ── Desktop Header ─────────────────────────── */}
      <header dir="rtl" className="hidden lg:flex items-center h-14 border-0 border-b border-border/50 bg-background px-4 print:hidden">
        {/* Logo + Name */}
        <Link
          to="/dashboard"
          className="flex items-center gap-2.5 shrink-0 me-4 hover:opacity-80 transition-opacity"
        >
          <picture>
            <source srcSet="/pension-logo.webp" type="image/webp" />
            <img
              src="/pension-logo.png"
              alt="الشعار"
              className="w-8 h-8 object-contain shrink-0"
            />
          </picture>
          <div className="min-w-0 hidden xl:block">
            <h1 className="text-sm font-black text-foreground leading-tight truncate">
              صندوق التقاعد الأمني
            </h1>
            <p className="text-[10px] text-foreground">
              الإدارة العامة
            </p>
          </div>
        </Link>

        {/* Separator */}
        <div className="w-px h-6 bg-border/50 shrink-0" />

        {/* Navigation items */}
        <nav className="flex items-center gap-0.5 me-auto px-2">
          {Array.from(groupedItems.entries()).map(([group, items]) => (
            <NavDropdown key={group} group={group} items={items} />
          ))}
        </nav>

        {/* User controls */}
        <div className="flex items-center gap-1">
          <ThemeToggle />
          <NotificationBell />
          <UserMenu />
        </div>
      </header>

      {/* Secondary nav — sub-group tabs */}
      <SecondaryNav />
    </div>
  );
});