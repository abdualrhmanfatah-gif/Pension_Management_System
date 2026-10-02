import { forwardRef, type ReactNode, useCallback,useMemo } from 'react';

import { REPORT_HEADER_TEXT } from '@/components/domain/print/reportHeader.shared';
import { cn } from '@/lib/utils';

/* ═══════════════════════════════════════════════════════════
   الأنواع
   ═══════════════════════════════════════════════════════════ */

export type PageSize = 'A4' | 'A3' | 'A5' | 'letter';
export type PageOrientation = 'portrait' | 'landscape';

export type WatermarkPosition =
  | 'center'
  | 'top'
  | 'bottom'
  | 'left'
  | 'right'
  | 'top-left'
  | 'top-right'
  | 'bottom-left'
  | 'bottom-right';

export interface PrintLayoutConfig {
  republicTitle?: string;
  ministryTitle?: string;
  departmentTitle?: string;
  sectionTitle?: string;

  republicLogo?: string;
  ministryLogo?: string;
  basmalahImage?: string;
  centerLogo?: string;
  centerLogoFallback?: string;
  watermarkImage?: string;

  showRepublicLogo?: boolean;
  showMinistryLogo?: boolean;
  showBasmalah?: boolean;
  showCenterLogo?: boolean;
  showDate?: boolean;
  showReportNumber?: boolean;
  showAttachments?: boolean;
  showWatermark?: boolean;
  showFooter?: boolean;

  watermarkSize?: number;
  watermarkOpacity?: number;
  watermarkPosition?: WatermarkPosition;
  watermarkOffsetX?: number;
  watermarkOffsetY?: number;

  footerText?: string;
  showExportDate?: boolean;

  pageSize?: PageSize;
  orientation?: PageOrientation;
}

export interface PrintLayoutProps {
  reportName?: string;
  reportNumber?: string;
  date?: string;
  attachments?: string;
  children?: ReactNode;
  hideFooter?: boolean;
  printOnly?: boolean;
  config?: Partial<PrintLayoutConfig>;
}

/* ═══════════════════════════════════════════════════════════
   الثوابت
   ═══════════════════════════════════════════════════════════ */

const DEFAULT_CONFIG: PrintLayoutConfig = {
  republicTitle: REPORT_HEADER_TEXT.republic,
  ministryTitle: REPORT_HEADER_TEXT.ministry,
  departmentTitle: REPORT_HEADER_TEXT.department,
  sectionTitle: REPORT_HEADER_TEXT.section,

  republicLogo: '/republic_header.png',
  ministryLogo: '/ministry_header.png',
  basmalahImage: '/basmalah.png',
  centerLogo: '/pension-logo.png',
  centerLogoFallback: '/logo_republic_color.png',
  watermarkImage: '/pension-logo.png',

  showRepublicLogo: true,
  showMinistryLogo: true,
  showBasmalah: true,
  showCenterLogo: true,
  showDate: true,
  showReportNumber: true,
  showAttachments: true,
  showWatermark: true,
  showFooter: true,

  watermarkSize: 0.75,
  watermarkOpacity: 0.07,
  watermarkPosition: 'center',
  watermarkOffsetX: 0,
  watermarkOffsetY: 0,

  footerText: REPORT_HEADER_TEXT.footer,
  showExportDate: true,

  pageSize: 'A4',
  orientation: 'portrait',
};

const PAGE_SIZES: Record<PageSize, { width: string; height: string }> = {
  A4: { width: '210mm', height: '297mm' },
  A3: { width: '297mm', height: '420mm' },
  A5: { width: '148mm', height: '210mm' },
  letter: { width: '216mm', height: '279mm' },
};

const WATERMARK_POSITION_CLASSES: Record<WatermarkPosition, string> = {
  center: 'items-center justify-center',
  top: 'items-start justify-center pt-10',
  bottom: 'items-end justify-center pb-10',
  left: 'items-center justify-start pl-10',
  right: 'items-center justify-end pr-10',
  'top-left': 'items-start justify-start pt-10 pl-10',
  'top-right': 'items-start justify-end pt-10 pr-10',
  'bottom-left': 'items-end justify-start pb-10 pl-10',
  'bottom-right': 'items-end justify-end pb-10 pr-10',
};

/* ═══════════════════════════════════════════════════════════
   دوال مساعدة
   ═══════════════════════════════════════════════════════════ */

function formatArabicDate(dateStr?: string) {
  const date = dateStr ? new Date(dateStr) : new Date();
  const opts: Intl.DateTimeFormatOptions = { numberingSystem: 'arab' };
  return {
    day: date.toLocaleDateString('ar-YE', { ...opts, day: 'numeric' }),
    month: date.toLocaleDateString('ar-YE', { ...opts, month: 'numeric' }),
    year: date.toLocaleDateString('ar-YE', { ...opts, year: 'numeric' }),
    full: date.toLocaleDateString('ar-YE', {
      ...opts,
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }),
  };
}

function useImageErrorHandler(fallback?: string) {
  return useCallback(
    (e: React.SyntheticEvent<HTMLImageElement>) => {
      const target = e.currentTarget;
      if (fallback && target.src !== fallback) {
        target.src = fallback;
      } else {
        target.style.display = 'none';
        const fallbackText = target.nextElementSibling;
        if (fallbackText) fallbackText.classList.remove('hidden');
      }
    },
    [fallback]
  );
}

/* ═══════════════════════════════════════════════════════════
   مكونات فرعية
   ═══════════════════════════════════════════════════════════ */

function PrintStyles({ pageSize, orientation, watermarkSize, watermarkOpacity, showWatermark }: {
  pageSize: PageSize;
  orientation: PageOrientation;
  watermarkSize: number;
  watermarkOpacity: number;
  showWatermark: boolean;
}) {
  const page = PAGE_SIZES[pageSize];
  const wmSize = `${watermarkSize * 100}%`;
  const wmOpacity = showWatermark ? watermarkOpacity : 0;

  return (
    <style>{`
      @page {
        margin: 0;
        size: ${pageSize} ${orientation};
      }

      @media print {
        html, body {
          margin: 0 !important;
          padding: 0 !important;
          height: auto !important;
        }

        .pl-root {
          background: transparent !important;
          padding: 0 !important;
          margin: 0 !important;
          -webkit-print-color-adjust: exact !important;
          print-color-adjust: exact !important;
        }

        .pl-page {
          box-shadow: none !important;
          padding: 8mm !important;
          margin: 0 auto !important;
          width: 100% !important;
          min-height: auto !important;
        }

        .pl-wrapper {
          border: none !important;
          min-height: auto !important;
        }

        .pl-border-print {
          display: block !important;
          position: fixed;
          inset: 8mm;
          border: 4px double #000;
          z-index: 1000;
          pointer-events: none;
          box-sizing: border-box;
        }

        .pl-watermark {
          position: fixed !important;
          inset: 0 !important;
        }

        .pl-header {
          padding: 10px 15px !important;
          margin-bottom: 18px !important;
        }

        .pl-footer {
          position: fixed;
          bottom: 2mm;
          left: 10mm;
          right: 10mm;
          margin-top: 0 !important;
          z-index: 1001;
        }

        .pl-footer-row {
          border-top: none !important;
        }

        /* منع كسر العناصر المهمة */
        .pl-wrapper table tr,
        .pl-wrapper section,
        .pl-wrapper .break-inside-avoid {
          break-inside: avoid !important;
        }

        .pl-subject,
        .pl-header {
          break-after: avoid !important;
        }
      }

      /* أنماط الشاشة */
      .pl-page {
        width: ${page.width};
        min-height: ${page.height};
        margin: 0 auto;
        padding: 20px 20px 1px;
        background: #fff;
        box-shadow: 0 5px 15px rgba(0,0,0,0.2);
        box-sizing: border-box;
        position: relative;
        display: flex;
        flex-direction: column;
      }

      .pl-watermark img {
        width: ${wmSize};
        height: ${wmSize};
        object-fit: contain;
        opacity: ${wmOpacity};
        user-select: none;
        pointer-events: none;
      }

      .pl-input-line {
        display: inline-block;
        text-align: center;
        padding: 0 3px;
        font-weight: 800;
        min-width: 30px;
        line-height: 1.3;
        color: #000;
      }
    `}</style>
  );
}

function Watermark({ image, position, offsetX, offsetY }: {
  image: string;
  position: WatermarkPosition;
  offsetX: number;
  offsetY: number;
}) {
  const transformStyle = (offsetX !== 0 || offsetY !== 0)
    ? { transform: `translate(${offsetX}px, ${offsetY}px)` }
    : undefined;

  return (
    <div
      className={cn(
        'pl-watermark absolute inset-0 z-[1] flex overflow-hidden pointer-events-none',
        WATERMARK_POSITION_CLASSES[position]
      )}
      aria-hidden="true"
      style={transformStyle}
    >
      <img src={image} alt="" draggable="false" />
    </div>
  );
}

function PrintHeader({
  config,
  dateObj,
  reportNumber,
  attachments,
}: {
  config: PrintLayoutConfig;
  dateObj: ReturnType<typeof formatArabicDate>;
  reportNumber?: string;
  attachments?: string;
}) {
  const onRepublicError = useImageErrorHandler();
  const onMinistryError = useImageErrorHandler();
  const onCenterError = useImageErrorHandler(config.centerLogoFallback);
  const onBasmalahError = useCallback((e: React.SyntheticEvent<HTMLImageElement>) => {
    e.currentTarget.style.display = 'none';
  }, []);

  return (
    <header className="pl-header flex justify-between items-center border-b-4 border-double border-black px-[15px] py-3 mb-5">
      {/* اليمين */}
      <div className="w-[30%] flex flex-col items-center gap-0.5 text-center">
        {config.showRepublicLogo && (
          <>
            <img
              src={config.republicLogo}
              alt={config.republicTitle}
              className="block w-full max-w-[140px] h-auto mx-auto mb-1"
              onError={onRepublicError}
            />
            <div className="hidden text-lg font-black tracking-wide leading-tight">
              {config.republicTitle}
            </div>
          </>
        )}

        {config.showMinistryLogo && (
          <>
            <img
              src={config.ministryLogo}
              alt={config.ministryTitle}
              className="block w-full max-w-[120px] h-auto mx-auto mb-0.5"
              onError={onMinistryError}
            />
            <div className="hidden text-base font-extrabold leading-snug">
              {config.ministryTitle}
            </div>
          </>
        )}

        <p className="font-extrabold text-[12px] mt-1 leading-snug whitespace-nowrap">
          {config.departmentTitle}
        </p>
        {config.sectionTitle && (
          <p className="font-extrabold text-[11px] mt-0.5 leading-snug whitespace-nowrap">
            {config.sectionTitle}
          </p>
        )}
      </div>

      {/* الوسط */}
      <div className="w-[30%] flex flex-col items-center justify-center gap-1.5 text-center">
        {config.showBasmalah && (
          <img
            src={config.basmalahImage}
            alt="بسم الله الرحمن الرحيم"
            className="w-[130px] h-auto object-contain"
            onError={onBasmalahError}
          />
        )}
        {config.showCenterLogo && (
          <img
            src={config.centerLogo}
            alt="الشعار"
            className="w-[80px] h-auto object-contain"
            onError={onCenterError}
          />
        )}
      </div>

      {/* اليسار */}
      <div className="w-[34%] flex justify-start pt-1">
        <div className="text-right text-[12px] font-extrabold leading-[2] text-black space-y-1">
          {config.showDate && (
            <p className="flex items-baseline gap-1.5 whitespace-nowrap m-0">
              <span>التاريـــخ :</span>
              <span className="pl-input-line w-8">{dateObj.day}</span>
              <span>/</span>
              <span className="pl-input-line w-7">{dateObj.month}</span>
              <span>/</span>
              <span className="pl-input-line w-[45px]">{dateObj.year}</span>
              <span>م</span>
            </p>
          )}
          {config.showReportNumber && (
            <p className="flex items-baseline gap-1.5 whitespace-nowrap m-0">
              <span>الرقـــــــم :</span>
              <span className="pl-input-line w-[90px]">{reportNumber || ''}</span>
            </p>
          )}
          {config.showAttachments && (
            <p className="flex items-start gap-1.5 m-0">
              <span className="shrink-0">القيد :</span>
              <span
                className="pl-input-line min-w-[90px] max-w-[180px] text-right leading-snug"
                style={{ whiteSpace: 'normal' }}
              >
                {attachments || ''}
              </span>
            </p>
          )}
        </div>
      </div>
    </header>
  );
}

function PrintFooter({
  footerText,
  showExportDate,
  todayFormatted,
}: {
  footerText?: string;
  showExportDate: boolean;
  todayFormatted: string;
}) {
  return (
    <footer className="pl-footer relative mt-1 z-[2]">
      <div className="px-3 pt-1 pb-0.5">
        <div className="pl-footer-row flex justify-between items-center text-[10px] text-black pt-1">
          <span className="text-right font-extrabold">{footerText}</span>
          {showExportDate && (
            <span className="text-left font-bold" dir="ltr">
              تاريخ الاستخراج: {todayFormatted}
            </span>
          )}
        </div>
      </div>
    </footer>
  );
}

/* ═══════════════════════════════════════════════════════════
   المكون الرئيسي
   ═══════════════════════════════════════════════════════════ */

export const PrintLayout = forwardRef<HTMLDivElement, PrintLayoutProps>(
  (
    {
      reportName,
      reportNumber,
      date,
      attachments,
      children,
      hideFooter = false,
      printOnly = false,
      config: userConfig = {},
    },
    ref
  ) => {
    const cfg = useMemo(() => ({ ...DEFAULT_CONFIG, ...userConfig }), [userConfig]);
    const dateObj = useMemo(() => formatArabicDate(date), [date]);
    const todayFormatted = useMemo(
      () => new Date().toLocaleDateString('ar-YE', { year: 'numeric', month: 'long', day: 'numeric', numberingSystem: 'arab' }),
      []
    );

    return (
      <div
        ref={ref}
        dir="rtl"
        className={cn(
          'pl-root',
          printOnly ? 'hidden print:block' : 'bg-surface-print min-h-screen py-10',
          'print:bg-white print:py-0 w-full'
        )}
      >
        <PrintStyles
          pageSize={cfg.pageSize!}
          orientation={cfg.orientation!}
          watermarkSize={cfg.watermarkSize ?? 0.75}
          watermarkOpacity={cfg.watermarkOpacity ?? 0.07}
          showWatermark={cfg.showWatermark ?? true}
        />

        <div className="pl-page">
          {/* الإطار الظاهر عند الطباعة */}
          <div className="pl-border-print hidden print:block" aria-hidden="true" />

          {/* العلامة المائية */}
          <Watermark
            image={cfg.watermarkImage!}
            position={cfg.watermarkPosition ?? 'center'}
            offsetX={cfg.watermarkOffsetX ?? 0}
            offsetY={cfg.watermarkOffsetY ?? 0}
          />

          {/* المحتوى */}
          <div className="pl-wrapper relative z-[2] flex-1 border-4 border-double border-black pb-4">
            <PrintHeader
              config={cfg}
              dateObj={dateObj}
              reportNumber={reportNumber}
              attachments={attachments}
            />

            {/* عنوان التقرير */}
            {reportName && (
              <h1 className="pl-subject text-xl font-black text-center my-2 mb-8 tracking-wide">
                {reportName}
              </h1>
            )}

            {/* المحتوى الرئيسي */}
            <main className="flex-1 w-full text-black px-[30px] box-border">
              {children}
            </main>
          </div>

          {/* التذييل */}
          {!hideFooter && cfg.showFooter && (
            <PrintFooter
              footerText={cfg.footerText}
              showExportDate={cfg.showExportDate ?? true}
              todayFormatted={todayFormatted}
            />
          )}
        </div>
      </div>
    );
  }
);

PrintLayout.displayName = 'PrintLayout';