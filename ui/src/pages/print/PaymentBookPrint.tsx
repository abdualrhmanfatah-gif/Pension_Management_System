import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer, User } from "lucide-react";
import { useEffect,useMemo, useState } from "react";
import { useNavigate,useParams } from "react-router-dom";

import api from "@/api/axios";
import { financialEntitlementsApi } from "@/api/financial-entitlements";
import { salaryRecipientApi } from "@/api/salary-recipients";
import { Button } from "@/components/ui/Button";
import { LazyQRCode as QRCodeSVG } from "@/components/ui/LazyQRCode";
import { formatDate, getDocumentUrl } from "@/lib/utils";
import type { PaymentBookItem,RetireeDetail, RetirementFileDetail } from "@/types/retirement";

// ═══════════════════════════════════════════════════
// Helpers
// ═══════════════════════════════════════════════════

const getUrl = (path: string | null | undefined): string => {
  if (!path) return "";
  return getDocumentUrl(path) ?? "";
};

// Anti-forgery: cyrb53-based verification hash
function generateVerificationHash(...fields: string[]): string {
  const data = fields.join("|");
  let h1 = 0xdeadbeef;
  let h2 = 0x41c6ce57;
  for (let i = 0; i < data.length; i++) {
    const ch = data.charCodeAt(i);
    h1 = Math.imul(h1 ^ ch, 2654435761);
    h2 = Math.imul(h2 ^ ch, 1597334677);
  }
  h1 = Math.imul(h1 ^ (h1 >>> 16), 2246822507);
  h1 ^= Math.imul(h2 ^ (h2 >>> 13), 3266489909);
  h2 = Math.imul(h2 ^ (h2 >>> 16), 2246822507);
  h2 ^= Math.imul(h1 ^ (h1 >>> 13), 3266489909);
  const hash = 4294967296 * (2097151 & h2) + (h1 >>> 0);
  return hash.toString(16).toUpperCase().padStart(12, "0").slice(0, 8);
}

// ═══════════════════════════════════════════════════
// Component
// ═══════════════════════════════════════════════════

export function PaymentBookPrint() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const {
    data: paymentBook,
    isLoading: loadingBook,
    isError: errorBook,
  } = useQuery({
    queryKey: ["payment-book-print", id],
    queryFn: async () => {
      const res = await api.get<{
        data: PaymentBookItem & {
          fileID: number;
          changeReason?: string;
          recipientType?: string;
          recipientName?: string;
          bookType?: string;
          legacyColor?: string;
          issuingBranchName?: string;
        };
      }>(`/payment-books/${id}`);
      return res.data.data;
    },
    enabled: !!id,
    retry: 1,
  });

  const fileId = paymentBook?.fileID;

  const {
    data: fileDetail,
    isLoading: loadingFile,
    isError: errorFile,
  } = useQuery({
    queryKey: ["retirement-file-print", fileId],
    queryFn: async () => {
      if (!fileId) return null;
      const res = await api.get<{ data: RetirementFileDetail }>(
        `/retirement-files/${fileId}`
      );
      return res.data.data;
    },
    enabled: !!fileId,
    retry: 1,
  });

  const retireeId = fileDetail?.retireeID;

  const {
    data: retiree,
    isLoading: loadingRetiree,
    isError: errorRetiree,
  } = useQuery({
    queryKey: ["retiree-print", retireeId],
    queryFn: async () => {
      if (!retireeId) return null;
      const res = await api.get<{ data: RetireeDetail }>(`/retirees/${retireeId}`);
      return res.data.data;
    },
    enabled: !!retireeId,
    retry: 1,
  });

  // Fetch active salary recipient to get the recipient's photo if they are not the retiree
  const {
    data: activeRecipient,
    isLoading: loadingRecipient,
    isError: recipientError,
  } = useQuery({
    queryKey: ["active-salary-recipient-print", fileId],
    queryFn: async () => {
      if (!fileId) return null;
      const res = await salaryRecipientApi.getByFile(fileId);
      return res.data.find((r: { isActive: boolean }) => r.isActive) || null;
    },
    enabled: !!fileId,
  });

  // Fetch active financial entitlement for the pension amount
  const {
    data: activeEntitlement,
    isLoading: loadingEntitlement,
    isError: entitlementError,
  } = useQuery({
    queryKey: ["active-entitlement-print", retireeId],
    queryFn: async () => {
      if (!retireeId) return null;
      const res = await financialEntitlementsApi.getActive(retireeId);
      return res.data;
    },
    enabled: !!retireeId,
  });

  const {
    fileNumber,
    rankName,
    retireeFullName,
    retireeMilitaryNumber,
    retireePhotoPath,
    recipientPhotoPath,
    branchName,
    retirementDate,
    unitName,
    categoryName,
    pensionAmount,
    isRetireeAlive,
  } = useMemo(() => {
    const fd = fileDetail as unknown as Record<string, unknown> | undefined;

    // Retiree's photo
    const rPhoto =
      retiree?.personalPhotoPath ||
      fileDetail?.personalPhotoPath ||
      (fd?.PersonalPhotoPath as string | undefined);

    // Recipient's photo (fallback to retiree if not set)
    const recPhoto = activeRecipient?.recipientPhotoPath || rPhoto;

    return {
      fileNumber:
        fileDetail?.fileNumber || (fd?.FileNumber as string | undefined) || "ـــ",
      rankName:
        fileDetail?.rankName || (fd?.RankName as string | undefined) || "ـــ",
      retireeFullName:
        retiree?.fullName ||
        fileDetail?.retireeFullName ||
        (fd?.RetireeFullName as string | undefined) ||
        "ـــ",
      retireeMilitaryNumber: retiree?.militaryNumber || "ـــ",
      retireePhotoPath: rPhoto,
      recipientPhotoPath: recPhoto,
      branchName: fileDetail?.branchName || (fd?.BranchName as string | undefined) || "ـــ",
      retirementDate: fileDetail?.retirementDate ? formatDate(fileDetail.retirementDate) : "ـــ",
      unitName: retiree?.unitName || "ـــ",
      categoryName: retiree?.categoryName || "ـــ",
      pensionAmount: activeEntitlement?.amount
        ? `${activeEntitlement.amount.toLocaleString("ar-YE")} ر.ي`
        : "ـــ",
      isRetireeAlive: retiree?.isAlive ?? true,
    };
  }, [fileDetail, retiree, activeRecipient, activeEntitlement]);

  const loading = loadingBook || loadingFile || loadingRetiree || loadingRecipient || loadingEntitlement;

  // Anti-forgery verification code (must be before early returns — Rules of Hooks)
  const verificationCode = useMemo(
    () => generateVerificationHash(fileNumber, retireeMilitaryNumber, retireeFullName, paymentBook?.issueDate || ""),
    [fileNumber, retireeMilitaryNumber, retireeFullName, paymentBook?.issueDate]
  );
  const qrContent = `PMS:${fileNumber}:${verificationCode}`;

  if (loading) {
    return (
      <div className="flex justify-center py-20 text-muted-foreground italic text-sm">
        جاري تحميل بيانات الكرت...
      </div>
    );
  }

  if (errorBook || errorFile || errorRetiree || recipientError || entitlementError) {
    return (
      <div className="flex justify-center py-20 text-destructive font-bold">
        عذراً، حدث خطأ أثناء تحميل البيانات. يرجى المحاولة مرة أخرى.
      </div>
    );
  }

  if (!paymentBook || !fileDetail) {
    return (
      <div className="flex justify-center py-20 text-destructive font-bold">
        عذراً، تعذر تحميل بيانات الكرت الرسمي.
      </div>
    );
  }

  // ── Legacy book: show simple info instead of CR80 card ──
  if (paymentBook.bookType === "يدوي") {
    return <LegacyBookPrint paymentBook={paymentBook} fileDetail={fileDetail} retireeFullName={retireeFullName} />;
  }

  const recipientName = paymentBook.recipientName || activeRecipient?.recipientName || retireeFullName;
  const recipientType = paymentBook.recipientType || activeRecipient?.recipientType || "متقاعد";



  return (
    <div className="min-h-screen bg-muted py-8 print:py-0 print:bg-white flex flex-col items-center print:block">
      <InjectPrintStyles />

      {/* ── Screen Controls ── */}
      <div className="print:hidden flex items-center justify-between mb-6 p-4 w-full max-w-2xl bg-white rounded-xl border border-border">
        <Button variant="ghost" onClick={() => navigate(-1)} className="gap-2">
          <ArrowRight className="w-4 h-4" />
          عودة
        </Button>
<<<<<<< HEAD
        <h2 className="font-bold text-foreground text-sm">معاينة كرت الصرف</h2>
=======
        <h2 className="font-bold text-foreground/70 text-sm">معاينة كرت الصرف</h2>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        <Button
          onClick={() => window.print()}
          className="bg-emerald-700 hover:bg-emerald-800 gap-2"
        >
          <Printer className="w-4 h-4" />
          طباعة الكرت
        </Button>
      </div>

      {/* ═══════════════════════════════════════════════
          CR80 Card Front — 85.6 × 54 mm
      ═══════════════════════════════════════════════ */}
      <div
        className="cr80-card relative overflow-hidden"
        style={{
          width: "85.6mm",
          height: "54mm",
          direction: "rtl",
          background: "linear-gradient(180deg, #e3effc 0%, #c9dffa 40%, #b0d0f5 100%)",
          boxSizing: "border-box",
        }}
      >
        {/* ── Background Watermark ── */}
        <div
          className="absolute inset-0 pointer-events-none"
          style={{
            backgroundImage: `url('/pension-logo.png')`,
            backgroundPosition: "center",
            backgroundSize: "38mm 38mm",
            backgroundRepeat: "no-repeat",
            opacity: 0.10,
          }}
        />

        {/* ── Anti-forgery: Guilloché Pattern ── */}
        <GuillochePattern variant="front" />

        {/* ═════ HEADER ═════ */}
        <header className="relative z-10 flex items-start justify-between px-[1.5mm] pt-[2mm] w-full">
          {/* Right: Arabic (first element in RTL = physical right) */}
          <div className="flex flex-col text-center items-center shrink-0">
            <span className="text-[11px] font-black text-black leading-tight tracking-wide whitespace-nowrap">
              الجمهورية اليمنية
            </span>
            <span className="text-[9px] font-extrabold text-black leading-tight mt-[0.5mm] whitespace-nowrap">
              وزارة الداخلية
            </span>
            <span className="text-[8.5px] font-extrabold text-neutral-800 leading-tight mt-[0.3mm] whitespace-nowrap">
              صندوق التقاعد الأمني
            </span>
          </div>

          {/* Center: Emblem + Title */}
          <div className="flex flex-col items-center justify-start min-w-0 -mt-[3.0mm]">
            <img
              src="/yemen-republic-logo.png"
              alt="شعار الجمهورية"
              className="w-[18mm] h-[16mm] object-contain"
              draggable={false}
            />
            <span className="text-[9.5px] font-black text-black mt-[-3mm] tracking-wide whitespace-nowrap">
              بطاقة صرف معاش متقاعد
            </span>
          </div>

          {/* Left: English (last element in RTL = physical left) */}
          <div
            className="flex flex-col text-center items-center shrink-0"
            style={{ direction: "ltr" }}
          >
            <span className="text-[9px] font-black text-black leading-tight whitespace-nowrap">
              Republic of Yemen
            </span>
            <span className="text-[8px] font-extrabold text-black leading-tight mt-[0.5mm] whitespace-nowrap">
              Ministry of Interior
            </span>
            <span className="text-[7.5px] font-extrabold text-neutral-800 leading-tight mt-[0.3mm] whitespace-nowrap">
              Security Pension Fund
            </span>
          </div>
        </header>

        {/* ═════ BODY ═════ */}
        <div className="relative z-10 flex w-full px-[2.5mm] mt-[0.5mm] gap-[.5mm]">
          {/* ═══════════════════════════════════════════
              RIGHT SIDE: Data Fields 
              (first in RTL = physical right side)
          ═══════════════════════════════════════════ */}
          <div className="flex-1 grid grid-cols-[1fr_1.2fr] gap-x-[1mm] gap-y-[1.1mm] content-start pt-[1.5mm]">
            <FieldBox label="اسم المتقاعد" value={retireeFullName} fullWidth />
            <FieldBox label="اسم المستلم" value={`${recipientName} (${recipientType})`} fullWidth fitLong />

            <FieldBox label="الرقم العسكري" value={retireeMilitaryNumber} mono />
            <FieldBox label="الرتبة" value={rankName} />

            <FieldBox label="الفئة" value={categoryName} />
            <FieldBox label="تاريخ الإحالة" value={retirementDate} />

            <FieldBox label="الفرع" value={branchName} />
            <FieldBox label="مبلغ المعاش" value={pensionAmount} mono />

            <FieldBox label="الوحدة" value={unitName} fullWidth />

            <FieldBox label="جهة الإصدار" value={paymentBook?.issuingBranchName || "ـــ"} fullWidth />
          </div>

          {/* ═══════════════════════════════════════════
              LEFT SIDE: Photos
              (second in RTL = physical left side)
          ═══════════════════════════════════════════ */}
          <div className="flex flex-col items-center justify-start shrink-0 gap-[0.5mm] -mt-[1.0mm]">
            <div className="flex gap-[1mm] items-start relative">
              {/* Retiree Photo Container */}
              {(!isRetireeAlive && !retireePhotoPath) ? null : (
                <div className="flex flex-col items-center gap-[0.5mm]">
                  <span className="text-[6px] font-black text-black leading-none bg-slate-200/50 px-1 rounded-sm">المتقاعد</span>
                  <div
                    className="w-[11.0mm] h-[14.0mm] bg-white rounded-sm overflow-hidden"
                  >
                    <RetireePhoto path={retireePhotoPath} alt="المتقاعد" />
                  </div>
                </div>
              )}

              {/* Recipient Photo Container */}
              <div className="flex flex-col items-center gap-[0.5mm] relative">
                <span className="text-[6px] font-black text-black leading-none bg-primary/10 px-1 rounded-sm">المستلم</span>
                <div
                  className="w-[11.0mm] h-[14.0mm] bg-white rounded-sm overflow-hidden"
                >
                  <RetireePhoto path={recipientPhotoPath} alt="المستلم" />
                </div>
              </div>

              {/* Official Stamp Overlay */}
              <img
                src="/Stamp.png"
                alt="ختم رسمي"
                className="absolute object-contain pointer-events-none z-10"
                style={{
                  width: "10mm",
                  height: "10mm",
                  opacity: 0.8,
                  filter: "saturate(1.8) contrast(1.2)",
                  mixBlendMode: "multiply",
                  // للتحكم بموضع الختم، عدّل القيم التالية:
                  top: "80%",                 // المسافة من الأعلى
                  right: "50%",               // المسافة من اليمين (لأن الصفحة RTL)
                  transform: "translate(50%, -50%) rotate(15deg)" // للحفاظ على مركز الختم
                }}
                draggable={false}
              />
            </div>

            <div className="relative flex flex-col items-center mt-[9.0mm] w-full">

              <span className="relative z-20 text-[6.5px] font-extrabold text-black whitespace-nowrap leading-none">
                المدير العام التنفيذي
              </span>

              {/* حاوية التوقيع بارتفاع أقل لتقريب النصين */}
              <div className="relative w-full h-[2mm] flex items-center justify-center">
                <img
                  src="/Director's Signature.png"
                  alt="توقيع المدير العام التنفيذي"
                  className="absolute w-[18mm] h-[10mm] object-contain pointer-events-none z-0"
                  style={{
                    opacity: 0.95,
                    top: "-4mm"
                  }}
                  draggable={false}
                />
              </div>

              <span className="relative z-20 text-[6.5px] font-extrabold text-black mt-[1.5mm] whitespace-nowrap leading-none">
                عميد ركن/ جمال فضل القطيبي
              </span>

            </div>

          </div>

        </div>
      </div>

      {/* Spacer for screen preview */}
      <div className="h-6 print:hidden w-full" />

      {/* ══════════════════════════════════════════
          CARD BACK — printed on second page
      ══════════════════════════════════════════ */}
      <div
        className="cr80-card relative overflow-hidden flex flex-col mt-4 print:mt-8 rounded-[3.18mm]"
        style={{
          width: "85.6mm",
          height: "54mm",
          background: "linear-gradient(135deg, #e8f0fc 0%, #d4e4f8 50%, #bfd6f2 100%)",
          boxSizing: "border-box",
        }}
      >
        {/* Watermark */}
        <div
<<<<<<< HEAD
          className="absolute inset-0 z-0 opacity-100 pointer-events-none"
=======
          className="absolute inset-0 z-0 opacity-[0.10] pointer-events-none"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
          style={{
            backgroundImage: "url('/pension-logo.png')",
            backgroundPosition: "center",
            backgroundSize: "38mm 38mm",
            backgroundRepeat: "no-repeat",
          }}
        />

        {/* ── Anti-forgery: Guilloché Pattern ── */}
        <GuillochePattern variant="back" />

        {/* Back content */}
        <div className="relative z-10 flex flex-col flex-1 justify-between px-[4mm] py-[2.5mm]">
          {/* ─── Instructions ─── */}
          <div className="w-full mt-[0.5mm]">
            <h3 className="text-[11px] font-black text-red-600 mb-[1mm] text-center">
              تنبيه
            </h3>
            <ul className="text-[7px] text-black leading-[1.1] list-disc pr-[3.5mm] space-y-[0.6mm] text-start font-extrabold">
              <li>لا يصرف المعاش إلا بموجب هذا الكرت والمطابقة بالبطاقة الشخصية الذكية.</li>
              <li>المحافظة على كرت المعاش سليماً مالم سيتحمل المستفيد مسئولية إلغاء الكرت.</li>
              <li>يتحمل الوكيل الشرعي أو المستحقين مسئولية إعادة المبالغ المستلمة دون حق إلى الإدارة العامة طبقاً للقانون.</li>
              <li>إذا انقطع المتقاعد أو المستحق للمعاش أكثر من سنتين يحال المعاش إلى الصندوق ولا يعاد إلا إذا كان إنقطاعه لعذر شرعي ويصرف من تاريخ المطالبة للمعاش.</li>
              <li>يلتزم المستحق للمعاش أو الوكيل بتقديم استمارة بحث الجماعي نهاية كل عامين.</li>
              <li>يلتزم الوكيل والمستحقين  إبلاغ الإدارة في الأحوال التالية:
                <ul className="list-[circle] pr-[3mm] mt-[0.4mm] space-y-[0.3mm]">
                  <li>استبدال الوكيل أو عند عدم تسليم المعاش المستحقين.</li>
                  <li>وفاة أحد المستحقين.</li>
                  <li>التحاق أحد المستحقين بأي عمل أو زواج الأرمل أو الأخت أو الابنة.</li>
                </ul>
              </li>
            </ul>
          </div>

          {/* ── Anti-forgery: QR Code + Verification Hash + Dates ── */}
          <div className="w-full flex items-end justify-between mt-[1mm]">
            {/* QR Code */}
            <div className="flex flex-col items-center gap-[0.5mm]">
              <QRCodeSVG
                value={qrContent}
                size={52}
                level="H"
                bgColor="transparent"
                fgColor="#0f172a"
                style={{ width: "13mm", height: "13mm" }}
              />

            </div>

            {/* Center: Dates + File Number */}
            <div className="flex flex-col items-center gap-[1mm]">
              <div className="flex items-center gap-[4mm]">
                <div className="flex items-center gap-[1mm]">
                  <span className="text-[6.5px] font-extrabold text-neutral-700">الإصدار:</span>
                  <span className="text-[7px] font-mono font-extrabold text-black">{formatDate(paymentBook.issueDate)}</span>
                </div>
                <div className="flex items-center gap-[1mm]">
                  <span className="text-[6.5px] font-extrabold text-neutral-700">الانتهاء:</span>
                  <span className="text-[7px] font-mono font-extrabold text-red-800">{formatDate(paymentBook.expiryDate)}</span>
                </div>
              </div>
              <span className="text-[8px] font-mono font-black tracking-[1.5px] text-black">
                {fileNumber}
              </span>
            </div>

            {/* Verification Hash */}
            <div className="flex flex-col items-center gap-[0.3mm]">
              <span
                className="font-mono font-black tracking-[1px] text-slate-800 border border-slate-300 px-[1.5mm] py-[0.3mm] rounded-sm bg-white/60"
                style={{ fontSize: "8px", letterSpacing: "1.5px" }}
              >
                {verificationCode}
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* ── Print Instructions ── */}
      <div className="print:hidden mt-6 text-center text-xs text-slate-500 max-w-md space-y-1">
        <p>
          💡 <strong>تلميح:</strong> تأكد من تفعيل خيار «الرسومات الخلفية» في
          إعدادات الطباعة
        </p>
        <p className="text-[10px] text-slate-400">
          مقاس الكرت: 85.6 × 54 ملم (معيار CR80) • دقة موصى بها: 300 DPI
        </p>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════
// Sub-components
// ═══════════════════════════════════════════════════

function RetireePhoto({ path, alt = "صورة" }: { path: string | undefined; alt?: string }) {
  const [hasError, setHasError] = useState(false);

  if (hasError || !path) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center bg-slate-50 text-slate-400">
<<<<<<< HEAD
        <User className="w-5 h-5 opacity-100 mb-1" />
=======
        <User className="w-5 h-5 opacity-40 mb-1" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        <span className="text-[6px]">بدون صورة</span>
      </div>
    );
  }

  return (
    <img
      src={getUrl(path)}
      alt={alt}
      className="w-full h-full object-cover bg-white"
      loading="eager"
      draggable={false}
      onError={() => setHasError(true)}
    />
  );
}

function FieldBox({
  label,
  value,
  mono,
  fullWidth,
  fitLong,
}: {
  label: string;
  value: string;
  mono?: boolean;
  fullWidth?: boolean;
  fitLong?: boolean;
}) {
  const fittedSize = fitLong
    ? Math.max(4.5, Math.min(9, (26 / Math.max(value.length, 1)) * 9))
    : 9;
  return (
    <div className={`flex items-baseline gap-[0.8mm] ${fullWidth ? "col-span-2" : "col-span-1"}`}>
      <span className="text-[7.5px] font-extrabold text-neutral-700 whitespace-nowrap shrink-0">
        {label}:
      </span>
      <span
        className={`font-extrabold text-black leading-tight ${fitLong ? "whitespace-nowrap" : "truncate"} ${mono ? "font-mono tracking-wider" : ""
          }`}
        style={{ fontSize: `${fittedSize}px` }}
        title={value}
      >
        {value}
      </span>
    </div>
  );
}

// ═══════════════════════════════════════════════════
// Legacy Book Print — simple info display
// ═══════════════════════════════════════════════════

function LegacyBookPrint({
  paymentBook,
  fileDetail,
  retireeFullName,
}: {
  paymentBook: PaymentBookItem & { bookType?: string; legacyColor?: string; changeReason?: string; };
  fileDetail: RetirementFileDetail;
  retireeFullName: string;
}) {
  const navigate = useNavigate();
  const legacyColorBadge = (color?: string) => {
    switch (color) {
      case "أبيض": return <span className="px-2 py-0.5 rounded bg-muted text-foreground text-sm font-bold">أبيض</span>;
      case "أخضر": return <span className="px-2 py-0.5 rounded bg-green-200 text-green-800 text-sm font-bold">أخضر</span>;
      case "أزرق": return <span className="px-2 py-0.5 rounded bg-blue-200 text-blue-800 text-sm font-bold">أزرق</span>;
      default: return null;
    }
  };

  return (
    <div className="min-h-screen bg-muted py-8 flex flex-col items-center">
      <div className="print:hidden flex items-center justify-between mb-6 p-4 w-full max-w-lg bg-white rounded-xl border border-border">
        <Button variant="ghost" onClick={() => navigate(-1)} className="gap-2">
          <ArrowRight className="w-4 h-4" />
          عودة
        </Button>
<<<<<<< HEAD
        <h2 className="font-bold text-foreground text-sm">بيانات دفتر صرف قديم</h2>
=======
        <h2 className="font-bold text-foreground/70 text-sm">بيانات دفتر صرف قديم</h2>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        <Button onClick={() => window.print()} className="gap-2">
          <Printer className="w-4 h-4" />
          طباعة
        </Button>
      </div>

      <div className="w-full max-w-lg bg-white rounded-2xl border border-border p-6 space-y-4">
        <div className="text-center border-b border-border pb-4">
          <h1 className="text-xl font-black text-foreground">دفتر صرف معاش</h1>
          <p className="text-sm text-muted-foreground">دفتر يدوي قديم</p>
          <div className="mt-2 flex items-center justify-center gap-2">
            {legacyColorBadge(paymentBook.legacyColor)}
            <span className="text-xs text-muted-foreground">رقم الملف: {fileDetail.fileNumber}</span>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">رقم الدفتر</p>
            <p className="font-bold font-mono">{paymentBook.bookNumber}</p>
          </div>
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">تاريخ الإصدار</p>
            <p className="font-bold font-mono">{formatDate(paymentBook.issueDate)}</p>
          </div>
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">اسم المتقاعد</p>
            <p className="font-bold">{retireeFullName}</p>
          </div>
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">المستلم</p>
            <p className="font-bold">{paymentBook.recipientName || "—"}</p>
          </div>
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">نوع المستلم</p>
            <p className="font-bold">{paymentBook.recipientType || "—"}</p>
          </div>
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">الحالة</p>
            <p className="font-bold">{paymentBook.status}</p>
          </div>
          <div className="space-y-1">
            <p className="text-muted-foreground text-xs">جهة الإصدار</p>
            <p className="font-bold">{paymentBook.issuingBranchName || "—"}</p>
          </div>
        </div>

        {paymentBook.notes && (
          <div className="border-t border-border pt-3">
            <p className="text-muted-foreground text-xs mb-1">ملاحظات</p>
            <p className="text-sm">{paymentBook.notes}</p>
          </div>
        )}
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════
// Anti-forgery Components
// ═══════════════════════════════════════════════════

function GuillochePattern({ variant }: { variant: "front" | "back" }) {
  const paths = useMemo(() => {
    const w = 324;
    const h = 204;
    const lines: string[] = [];

    // Spirograph rosette removed based on user request

    // Edge wave lines (top + bottom)
    for (let w2 = 0; w2 < 7; w2++) {
      const yTop = 4 + w2 * 5;
      const yBot = h - 4 - w2 * 5;
      const amp = 2.5 + w2;
      const freq = 0.07 + w2 * 0.01;
      let dTop = `M 0 ${yTop}`;
      let dBot = `M 0 ${yBot}`;
      for (let x = 0; x <= w; x += 1) {
        dTop += ` L ${x} ${(yTop + amp * Math.sin(freq * x + w2)).toFixed(1)}`;
        dBot += ` L ${x} ${(yBot + amp * Math.sin(freq * x + w2 + 1)).toFixed(1)}`;
      }

      if (variant !== "front") {
        lines.push(dTop);
      }
      lines.push(dBot);
    }

    return lines;
  }, [variant]);

  const numRosette = 0;

  return (
    <svg
      className="absolute inset-0 w-full h-full pointer-events-none"
      viewBox="0 0 324 204"
      preserveAspectRatio="none"
      style={{ opacity: 0.20, zIndex: 1 }}
    >
      {paths.map((d, i) => (
        <path
          key={i}
          d={d}
          fill="none"
          stroke={i < numRosette ? "#1a3a6b" : "#2563eb"}
          strokeWidth="0.4"
        />
      ))}
    </svg>
  );
}


function InjectPrintStyles() {
  useEffect(() => {
    const styleId = "payment-book-print-styles";
    if (document.getElementById(styleId)) return;

    const style = document.createElement("style");
    style.id = styleId;
    style.textContent = `
      @page {
        size: A4 portrait;
        margin: 10mm;
      }
      
      @media print {
        html, body {
          margin: 0 !important;
          padding: 0 !important;
          background: var(--background) !important;
          -webkit-print-color-adjust: exact !important;
          print-color-adjust: exact !important;
          color-adjust: exact !important;
        }
        
        .cr80-card {
          box-shadow: none !important;
          margin: 0 auto 8mm auto !important;
          border-radius: 3.18mm !important;
          border: none !important;
          page-break-inside: avoid;
          break-inside: avoid;
        }
        
        .cr80-card span,
        .cr80-card li,
        .cr80-card h3 {
          -webkit-print-color-adjust: exact !important;
          print-color-adjust: exact !important;
        }
        
        .page-break {
          page-break-before: always !important;
          break-before: page !important;
        }
        
        .print\\:hidden {
          display: none !important;
        }
      }

      .cr80-card,
      .cr80-card * {
        font-family: var(--font-sans);
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-rendering: optimizeLegibility;
      }
    `;
    document.head.appendChild(style);

    return () => {
      const el = document.getElementById(styleId);
      if (el) el.remove();
    };
  }, []);

  return null;
}