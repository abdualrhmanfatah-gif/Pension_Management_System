/**
 * سلسلة Enter للنماذج: Enter ينقل التركيز للحقل التالي بدل لا شيء.
 *
 * القواعد:
 * - `input[type="date"]` و `type="file"` مستبعدان: الأول مخفي داخل DateInput
 *   ويفتح منتقي النظام، والثاني لا يقبل إدخالًا نصيًا.
 * - `data-enter-skip`: حقول خارج السلسلة (حقل الملاحظات) يبقي سلوكها الافتراضي.
 * - `data-enter-advance`: Enter فيها = الانتقال للخطوة/الإجراء التالي.
 * - `data-enter-chain-target`: زر الإجراء النهائي؛ يُركَّز عند نهاية السلسلة.
 * - RadioGroup = حقل منطقي واحد: Enter يخرج منه إلى الحقل التالي لا داخله.
 */

/** المحدد المشترك لتسلسل Enter، بترتيب DOM. */
export const ENTER_FOCUSABLE_SELECTOR = [
    'input:not([type="hidden"]):not([type="date"]):not([type="file"]):not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[role="radio"]:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
].join(",");

/** الحقول القابلة للتركيز بترتيب DOM، بعد تصفية غير المرئي وغير الصالح للتسلسل. */
export function getEnterFocusables(container: HTMLElement): HTMLElement[] {
    return Array.from(
        container.querySelectorAll<HTMLElement>(ENTER_FOCUSABLE_SELECTOR)
    ).filter((el) => {
        if (el.offsetWidth <= 0 || el.offsetHeight <= 0) return false;
        if (el.closest(".sr-only")) return false;
        if (el.getAttribute("aria-hidden") === "true") return false;
        if (el.hasAttribute("data-enter-skip")) return false;

        const role = el.getAttribute("role");

        // الأزرار العادية خارج السلسلة (Enter ينفّذها natively).
        if ((el.tagName === "BUTTON" || role === "button") && role !== "radio") {
            return false;
        }

        return true;
    });
}

/**
 * ينقل التركيز للحقل رقم index بعد تهدئة التحديث.
 * يُعاد الاستعلام من DOM عند التنفيذ لا من العقدة المخزّنة، لأن re-render
 * بين المفتاح والتنفيذ يفصل العقدة فيضيع التركيز.
 */
export function focusEnterField(container: HTMLElement, index: number): void {
    window.setTimeout(() => {
        const target = getEnterFocusables(container)[index];
        if (!target) return;

        target.focus();
        target.scrollIntoView({ block: "nearest", behavior: "smooth" });
    }, 0);
}

/**
 * يحسب فهرس الحقل التالي لـ Enter داخل container.
 * يعيد -1 عند عدم وجود حقل تالٍ.
 */
export function nextEnterIndex(
    container: HTMLElement,
    target: HTMLElement | null
): number {
    const focusables = getEnterFocusables(container);
    if (focusables.length === 0) return -1;

    const radioGroup = target?.closest<HTMLElement>('[role="radiogroup"]') ?? null;

    if (radioGroup) {
        const radioIndexes = focusables
            .map((element, index) => (radioGroup.contains(element) ? index : -1))
            .filter((index) => index >= 0);

        return radioIndexes.length > 0 ? Math.max(...radioIndexes) + 1 : -1;
    }

    if (!target) return -1;

    // بعض المكونات ترسل الحدث من عنصر داخلي، لذا نتحقق من contains أيضًا.
    const currentIndex = focusables.findIndex(
        (element) => element === target || element.contains(target)
    );

    return currentIndex >= 0 ? currentIndex + 1 : -1;
}
