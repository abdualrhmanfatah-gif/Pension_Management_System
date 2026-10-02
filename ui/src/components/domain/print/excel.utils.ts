import ExcelJS from "exceljs";

import {
  HEADER_FONT,
  HEADER_FRAME,
  REPORT_HEADER_TEXT,
} from "./reportHeader.shared";

/* ═══════════════════════════════════════════════════════════
   Excel Professional Template — قالب احترافي موحد
   ═══════════════════════════════════════════════════════════ */

// ── ألوان الرسم ──
const COLORS = {
  primary: "FF1B3A5C",       // كحلي غامق (للترويسة فقط)
  primaryLight: "FF2E5C8A",  // كحلي فاتح
  accent: "FFD4A843",        // ذهبي
  white: "FFFFFFFF",
  offWhite: "FFF8F9FA",
  lightGray: "FFE9ECEF",
  headerBg: "FFE8EDF2",      // رمادي فاتح جداً لرؤوس الجدول
  medGray: "FF6C757D",
  darkText: "FF212529",
  totalBg: "FFD6E4F0",       // أزرق فاتح للإجمالي
  altRow: "FFF5F7FA",        // صف بديل
  border: "FFBDC3CE",
  borderDark: "FF8E99A4",
};

// ── ثوابت الخطوط ──
const FONTS = {
  republicLine: { bold: true, size: 13, name: "Arial", color: { argb: COLORS.primary } },
  ministryLine: { bold: true, size: 11, name: "Arial", color: { argb: COLORS.medGray } },
  titleLine: { bold: true, size: 16, name: "Arial", color: { argb: COLORS.primary } },
  metaLine: { bold: false, size: 10, name: "Arial", color: { argb: COLORS.medGray } },
  headerCell: { bold: true, size: 10, name: "Arial", color: { argb: COLORS.primary } },
  dataCell: { size: 10, name: "Arial", color: { argb: COLORS.darkText } },
  dataCellBold: { bold: true, size: 10, name: "Arial", color: { argb: COLORS.darkText } },
  totalCell: { bold: true, size: 11, name: "Arial", color: { argb: COLORS.primary } },
  footerLine: { size: 8, name: "Arial", color: { argb: COLORS.medGray } },
};

// ── ثوابت الحدود ──
const THIN_BORDER: Partial<ExcelJS.Borders> = {
  top: { style: "thin", color: { argb: COLORS.border } },
  left: { style: "thin", color: { argb: COLORS.border } },
  bottom: { style: "thin", color: { argb: COLORS.border } },
  right: { style: "thin", color: { argb: COLORS.border } },
};

const HEADER_BORDER: Partial<ExcelJS.Borders> = {
  top: { style: "medium", color: { argb: COLORS.border } },
  left: { style: "thin", color: { argb: COLORS.border } },
  bottom: { style: "medium", color: { argb: COLORS.border } },
  right: { style: "thin", color: { argb: COLORS.border } },
};

const TOTAL_BORDER: Partial<ExcelJS.Borders> = {
  top: { style: "double", color: { argb: COLORS.primary } },
  left: { style: "thin", color: { argb: COLORS.border } },
  bottom: { style: "medium", color: { argb: COLORS.primary } },
  right: { style: "thin", color: { argb: COLORS.border } },
};

/* ═══════════════════════════════════════════════════════════
   Types
   ═══════════════════════════════════════════════════════════ */

export interface HeaderField {
  label: string;
  value: string;
}

export interface StyleOptions {
  bold?: boolean;
  color?: string;
  numFmt?: string;
  fill?: ExcelJS.Fill;
  align?: "start" | "center" | "end";
}

export interface TotalsCell {
  value: number | string;
  numFmt?: string;
  bold?: boolean;
  color?: string;
  colSpan?: number;
}

/* ═══════════════════════════════════════════════════════════
   Workbook
   ═══════════════════════════════════════════════════════════ */

export function createWorkbook(sheetName = "تقرير") {
  const wb = new ExcelJS.Workbook();
  wb.creator = "نظام إدارة الصندوق التقاعد";
  wb.created = new Date();
  const ws = wb.addWorksheet(sheetName, {
    views: [{ rightToLeft: true, state: "frozen", ySplit: 7 }],
  });

  // ── إعداد الصفحة للطباعة ──
  ws.pageSetup = {
    orientation: "landscape",
    fitToPage: true,
    fitToWidth: 1,
    fitToHeight: 0,
    paperSize: 8 as any, // A3
    margins: { left: 0.4, right: 0.4, top: 0.5, bottom: 0.5, header: 0.3, footer: 0.3 },
  };

  return { wb, ws };
}

/* ═══════════════════════════════════════════════════════════
   Report Header — الرأس الرسمي الاحترافي
   ═══════════════════════════════════════════════════════════ */

/* ═══════════════════════════════════════════════════════════
   Logo loader — يقرأ شعار الصندوق من public/ (نفس ملف PrintLayout)
   ═══════════════════════════════════════════════════════════ */

let logoCache: { base64: string; ext: "png" | "jpeg" } | null = null;

async function loadCenterLogo(): Promise<{ base64: string; ext: "png" | "jpeg" } | null> {
  if (logoCache) return logoCache;
  const candidates = ["/pension-logo.png", "/logo_republic_color.png", "/yemen-republic-logo.png"];
  for (const url of candidates) {
    try {
      const res = await fetch(url);
      if (!res.ok) continue;
      const buf = new Uint8Array(await res.arrayBuffer());
      let binary = "";
      for (let i = 0; i < buf.length; i++) binary += String.fromCharCode(buf[i]);
      logoCache = { base64: btoa(binary), ext: "png" };
      return logoCache;
    } catch {
      continue;
    }
  }
  return null;
}

export async function buildReportHeader(
  ws: ExcelJS.Worksheet,
  title: string,
  cols: number,
  metaFields?: HeaderField[]
): Promise<number> {
  const now = new Date();
  const day = now.toLocaleDateString("ar-YE", { day: "numeric" });
  const month = now.toLocaleDateString("ar-YE", { month: "numeric" });
  const year = now.toLocaleDateString("ar-YE", { year: "numeric" });

  // تقسيم الأعمدة: يمين / وسط / يسار
  const rightEnd = Math.ceil(cols * 0.33);
  const centerEnd = Math.ceil(cols * 0.66);

  // حدود سوداء مزدوجة (من HEADER_FRAME المشترك)
  const DOUBLE_BORDER: Partial<ExcelJS.Borders> = {
    top: { style: HEADER_FRAME.style, color: { argb: HEADER_FRAME.argb } },
    left: { style: HEADER_FRAME.style, color: { argb: HEADER_FRAME.argb } },
    bottom: { style: HEADER_FRAME.style, color: { argb: HEADER_FRAME.argb } },
    right: { style: HEADER_FRAME.style, color: { argb: HEADER_FRAME.argb } },
  };

  const INNER_BORDER: Partial<ExcelJS.Borders> = {
    top: { style: HEADER_FRAME.style, color: { argb: HEADER_FRAME.argb } },
    left: { style: "none" as any, color: { argb: HEADER_FRAME.argb } },
    bottom: { style: HEADER_FRAME.style, color: { argb: HEADER_FRAME.argb } },
    right: { style: "none" as any, color: { argb: HEADER_FRAME.argb } },
  };

  // ── صف ١: بسم الله (مثل basmalah image) ──
  ws.mergeCells(1, 1, 1, cols);
  const basmala = ws.getCell(1, 1);
  basmala.value = REPORT_HEADER_TEXT.basmalah;
  basmala.font = { bold: true, size: HEADER_FONT.basmalahSize, name: "Traditional Arabic, Arial", color: { argb: HEADER_FRAME.argb } };
  basmala.alignment = { horizontal: "center", vertical: "middle" };
  ws.getRow(1).height = 30;

  // ── صف ٢: فاصل ──
  ws.getRow(2).values = [];
  ws.getRow(2).height = 6;

  // ── صف ٣: الترويسة الرئيسية (3 أعمدة) ──
  const row3 = ws.getRow(3);
  row3.height = 80;

  // يمين: الجمهورية + وزارة + الإدارة + القسم
  ws.mergeCells(3, 1, 3, rightEnd);
  const rightCell = ws.getCell(3, 1);
  rightCell.value = [
    REPORT_HEADER_TEXT.republic,
    REPORT_HEADER_TEXT.ministry,
    REPORT_HEADER_TEXT.department,
    REPORT_HEADER_TEXT.section,
  ].join("\n");
  rightCell.font = { bold: true, size: HEADER_FONT.sideSize, name: "Arial", color: { argb: HEADER_FRAME.argb } };
  rightCell.alignment = { horizontal: "center", vertical: "middle", wrapText: true };
  rightCell.border = DOUBLE_BORDER;

  // وسط: شعار (صورة حقيقية مثل PrintLayout)
  ws.mergeCells(3, rightEnd + 1, 3, centerEnd);
  const centerCell = ws.getCell(3, rightEnd + 1);
  centerCell.value = "";
  centerCell.alignment = { horizontal: "center", vertical: "middle" };
  centerCell.border = INNER_BORDER;

  const logo = await loadCenterLogo();
  if (logo) {
    const imageId = ws.workbook.addImage({
      base64: logo.base64,
      extension: logo.ext,
    });
    ws.addImage(imageId, {
      tl: { col: rightEnd + 0.15, row: 2.55 } as any,
      br: { col: centerEnd - 0.15, row: 3.45 } as any,
    });
  } else {
    centerCell.value = "✦";
    centerCell.font = { bold: true, size: 36, name: "Arial", color: { argb: HEADER_FRAME.argb } };
  }

  // يسار: التاريخ + رقم + قيد
  ws.mergeCells(3, centerEnd + 1, 3, cols);
  const leftCell = ws.getCell(3, centerEnd + 1);
  const leftLines: string[] = [];
  leftLines.push(`التاريـــخ  :  ${day}/${month}/${year}  م`);
  if (metaFields) {
    metaFields.forEach((f) => leftLines.push(`${f.label}  :  ${f.value}`));
  }
  leftCell.value = leftLines.join("\n");
  leftCell.font = { bold: true, size: 12, name: "Arial", color: { argb: HEADER_FRAME.argb } };
  leftCell.alignment = { horizontal: "center", vertical: "middle", wrapText: true };
  leftCell.border = DOUBLE_BORDER;

  // ── صف ٤: العنوان الرئيسي (مثل pl-subject) ──
  ws.mergeCells(4, 1, 4, cols);
  const subject = ws.getCell(4, 1);
  subject.value = title;
  subject.font = { bold: true, size: HEADER_FONT.titleSize, name: "Arial", color: { argb: HEADER_FRAME.argb } };
  subject.alignment = { horizontal: "center", vertical: "middle" };
  ws.getRow(4).height = 36;

  // ── صف ٥: فاصل ──
  ws.getRow(5).values = [];
  ws.getRow(5).height = 6;

  return 6; // أول صف بيانات
}

/* ═══════════════════════════════════════════════════════════
   Table Header — رؤوس الجدول
   ═══════════════════════════════════════════════════════════ */

export function buildTableHeader(
  ws: ExcelJS.Worksheet,
  row: number,
  headers: string[]
): void {
  const r = ws.getRow(row);
  r.height = 30;

  headers.forEach((h, i) => {
    const cell = r.getCell(i + 1);
    cell.value = h;
    cell.font = FONTS.headerCell;
    cell.fill = { type: "pattern", pattern: "solid", fgColor: { argb: COLORS.headerBg } };
    cell.border = HEADER_BORDER;
    cell.alignment = { horizontal: "center", vertical: "middle", wrapText: true };
  });
}

/* ═══════════════════════════════════════════════════════════
   Data Cell — تنسيق خلية بيانات
   ═══════════════════════════════════════════════════════════ */

export function styleDataCell(
  cell: ExcelJS.Cell,
  opts?: StyleOptions
): void {
  cell.font = opts?.bold ? FONTS.dataCellBold : FONTS.dataCell;
  cell.border = THIN_BORDER;
  const horizontal =
    opts?.align === "end" ? "right"
    : opts?.align === "start" ? "left"
    : (opts?.align ?? "center");
  cell.alignment = { horizontal, vertical: "middle" };
  if (opts?.color) cell.font = { ...cell.font, color: { argb: opts.color } };
  if (opts?.numFmt) cell.numFmt = opts.numFmt;
  if (opts?.fill) cell.fill = opts.fill;
}

/**
 * تطبيق صف بديل على صف بيانات
 */
export function altRowFill(cell: ExcelJS.Cell, rowIndex: number): void {
  if (rowIndex % 2 === 0) {
    cell.fill = { type: "pattern", pattern: "solid", fgColor: { argb: COLORS.altRow } };
  }
}

/* ═══════════════════════════════════════════════════════════
   Report Footer — التذييل الرسمي
   ═══════════════════════════════════════════════════════════ */

export function buildReportFooter(
  ws: ExcelJS.Worksheet,
  cols: number,
  text?: string
): void {
  ws.addRow([]);

  // ── خط فاصل ──
  const decoRow = ws.addRow([]);
  ws.mergeCells(decoRow.number, 1, decoRow.number, cols);
  const deco = ws.getCell(decoRow.number, 1);
  deco.fill = { type: "pattern", pattern: "solid", fgColor: { argb: COLORS.primary } };
  deco.font = { size: 2 };

  // ── نص التذييل ──
  const footerRow = ws.addRow([]);
  ws.mergeCells(footerRow.number, 1, footerRow.number, cols);
  const fc = ws.getCell(footerRow.number, 1);
  fc.value = text ?? REPORT_HEADER_TEXT.footer;
  fc.font = FONTS.footerLine;
  fc.alignment = { horizontal: "center", vertical: "middle" };
}

/* ═══════════════════════════════════════════════════════════
   Totals Row — صف الإجمالي
   ═══════════════════════════════════════════════════════════ */

export function buildTotalsRow(
  ws: ExcelJS.Worksheet,
  row: number,
  cells: (TotalsCell | undefined)[]
): void {
  const r = ws.getRow(row);
  r.height = 28;

  cells.forEach((cell, i) => {
    const c = r.getCell(i + 1);
    if (!cell) {
      c.value = "";
      c.border = TOTAL_BORDER;
      return;
    }
    c.value = cell.value;
    c.font = { ...FONTS.totalCell, color: cell.color ? { argb: cell.color } : FONTS.totalCell.color };
    c.fill = { type: "pattern", pattern: "solid", fgColor: { argb: COLORS.totalBg } };
    c.border = TOTAL_BORDER;
    c.alignment = { horizontal: "center", vertical: "middle" };
    if (cell.numFmt) c.numFmt = cell.numFmt;
  });
}

/* ═══════════════════════════════════════════════════════════
   Export — تحميل الملف
   ═══════════════════════════════════════════════════════════ */

export async function exportWorkbook(
  wb: ExcelJS.Workbook,
  filename: string
): Promise<void> {
  try {
    const buf = await wb.xlsx.writeBuffer();
    const blob = new Blob([buf], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `${filename}.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  } catch (err) {
    console.error("Excel export error:", err);
  }
}

/* ═══════════════════════════════════════════════════════════
   Column Width
   ═══════════════════════════════════════════════════════════ */

export function autoFitColumns(
  ws: ExcelJS.Worksheet,
  widths: Record<number, number>
): void {
  Object.entries(widths).forEach(([col, w]) => {
    ws.getColumn(Number(col)).width = w;
  });
}
