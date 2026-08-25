// ─── MainLayout.tsx ──────────────────────────────────────

import { useCallback, useEffect, useLayoutEffect, useRef } from "react";
import { Outlet, useLocation } from "react-router-dom";

import { OfflineDetector } from "@/components/providers/OfflineDetector";
import { useAuthStore } from "@/stores/authStore";
import { useSidebarStore } from "@/stores/sidebarStore";
import { cn } from "@/lib/utils";

import { Sidebar } from "./Sidebar";
import { TopNav } from "./TopNav";

const THROTTLE_MS = 30_000;
const FULLSCREEN_PATTERN = /^\/files\/\d+\/review/;

export function MainLayout() {
  const sidebarOpen = useSidebarStore((s) => s.isOpen);
  const closeSidebar = useSidebarStore((s) => s.close);
  const refreshPermissions = useAuthStore((s) => s.refreshPermissions);
  const location = useLocation();
  const lastRefreshRef = useRef(0);

  const isFullscreenPage = FULLSCREEN_PATTERN.test(location.pathname);

  // ═══════════════════════════════════════════════════════
  // 1. Throttled refresh — محسّن: دالة مستقرة لا تتغير مع إعادة التصيير
  // ═══════════════════════════════════════════════════════
  const throttledRefresh = useCallback(() => {
    const now = Date.now();
    if (now - lastRefreshRef.current > THROTTLE_MS) {
      lastRefreshRef.current = now;
      refreshPermissions(true);
    }
  }, [refreshPermissions]);

  // ── Refresh permissions on route change ──
  useEffect(() => {
    throttledRefresh();
  }, [location.pathname, throttledRefresh]);

  // ── Refresh permissions when user returns to the tab ──
  useEffect(() => {
    const handleVisibility = () => {
      if (document.visibilityState === "visible") throttledRefresh();
    };
    document.addEventListener("visibilitychange", handleVisibility);
    return () => document.removeEventListener("visibilitychange", handleVisibility);
  }, [throttledRefresh]);

  // ═══════════════════════════════════════════════════════
  // 2. Scroll Lock — useLayoutEffect بدلاً من useEffect
  //    + حفظ القيمة الأصلية واستعادتها بدلاً من مسحها فقط
  // ═══════════════════════════════════════════════════════
  useLayoutEffect(() => {
    if (!sidebarOpen) return;

    const scrollY = window.scrollY;
    const originalOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    return () => {
      document.body.style.overflow = originalOverflow;
      window.scrollTo(0, scrollY);
    };
  }, [sidebarOpen]);

  // ═══════════════════════════════════════════════════════
  // 3. Fullscreen Page — إرجاع مبكر لتبسيط الشجرة وإزالة الشروط المتكررة
  // ═══════════════════════════════════════════════════════
  if (isFullscreenPage) {
    return (
      <div className="min-h-screen bg-background print:bg-white relative overflow-x-clip watermark-bg">
        <Outlet />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background print:bg-white relative overflow-x-clip watermark-bg">
      {/* ═══════════════════════════════════════════════════
          5. Backdrop — إزالة will-change-opacity (يسبب بطئاً على الجوال)
             + إضافة transition-colors للوضوح البصري
          ═══════════════════════════════════════════════════ */}
      <div
        aria-hidden="true"
        className={cn(
          "fixed inset-0 z-40 bg-black/40 lg:hidden",
          "transition-all duration-300",
          sidebarOpen ? "opacity-100" : "opacity-0 pointer-events-none"
        )}
        onClick={closeSidebar}
      />

      <div className="relative flex min-h-screen">
        {/* ═══════════════════════════════════════════════════
            6. Sidebar — mobile overlay wrapper (div, not aside)
               + inert when closed to prevent hidden focus
            ═══════════════════════════════════════════════════ */}
        <div
          className="lg:hidden print:hidden shrink-0"
          role="dialog"
          aria-modal="true"
          aria-hidden={!sidebarOpen}
          inert={sidebarOpen ? undefined : true}
        >
          <Sidebar />
        </div>

        {/* Main content */}
        <div className="flex-1 min-w-0">
          <TopNav />
          <OfflineDetector />
          <main className="p-4 sm:p-6 print:p-0 overflow-x-clip">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}