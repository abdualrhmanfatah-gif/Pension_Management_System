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
    'input:not([type="hidden"]):not([type="date"]):not([type="file"]):not([type="button"]):not([type="submit"]):not([type="reset"]):not([type="image"]):not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[role="radio"]:not([disabled])',
    '[role="combobox"]:not([disabled])',
    '[role="checkbox"]:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
].join(",");

function isAvailableEnterTarget(el: HTMLElement, container: HTMLElement): boolean {
    return el.offsetWidth > 0 && el.offsetHeight > 0 &&
        !el.matches(':disabled, [readonly]') &&
        !el.closest('.sr-only, [hidden], [inert], [aria-hidden="true"], [aria-disabled="true"]') &&
        getComputedStyle(el).visibility === 'visible' &&
        el.closest('[role="dialog"], [role="alertdialog"]') ===
            container.closest('[role="dialog"], [role="alertdialog"]');
}

/** الحقول القابلة للتركيز بترتيب DOM، بعد تصفية غير المرئي وغير الصالح للتسلسل. */
export function getEnterFocusables(container: HTMLElement): HTMLElement[] {
    return Array.from(
        container.querySelectorAll<HTMLElement>(ENTER_FOCUSABLE_SELECTOR)
    ).filter((el) => {
        if (!isAvailableEnterTarget(el, container)) return false;
        if (el.hasAttribute("data-enter-skip")) return false;

        const role = el.getAttribute("role");
        if (el.tabIndex < 0 && role !== "radio") return false;

        // الأزرار العادية خارج السلسلة (Enter ينفّذها natively).
        if ((el.tagName === "BUTTON" || role === "button") &&
            role !== "radio" && role !== "combobox" && role !== "checkbox") {
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
        target.scrollIntoView({ block: "nearest", behavior: "auto" });
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

/** Enter العادي يتنقل داخل النافذة؛ الحفظ يتطلب تفعيل زر الإجراء صراحةً. */
export function handleDialogEnter(container: HTMLElement, event: KeyboardEvent): void {
    if (event.key !== "Enter" || event.ctrlKey || event.altKey || event.metaKey ||
        event.shiftKey || event.isComposing || event.keyCode === 229) return;

    const target = event.target;
    if (!(target instanceof HTMLElement) || !container.contains(target)) return;
    // المعالج متعدد الخطوات يدير سلسلة Enter بنفسه على document.
    if (target.closest('[data-wizard-root], [data-enter-handled], [data-enter-skip]')) return;
    if (target.closest('[role="dialog"], [role="alertdialog"]') !== container) return;
    const isRadio = target.getAttribute("role") === "radio";
    // Radix يمنع Enter على radio لمنع إرسال النموذج، لكنه لا ينقل التركيز.
    if (event.defaultPrevented && !isRadio) return;
    if (target.isContentEditable || target.tagName === "TEXTAREA" ||
        target.tagName === "SELECT" || target.closest('[role="combobox"], [role="listbox"]')) return;
    if (!isRadio && (!target.matches('input') ||
        target.matches('[type="hidden"], [type="date"], [type="file"], [type="checkbox"], [type="radio"], [type="color"], [type="range"], [type="button"], [type="submit"], [type="reset"], [type="image"]'))) return;
    if (!isAvailableEnterTarget(target, container)) return;

    event.preventDefault();
    event.stopPropagation();
    if (event.repeat) return;

    focusNextEnterField(container, target);
}

/** يُستخدم أيضًا للقائمة الأصلية بعد وجود اختيار، دون تشغيل زر الحفظ. */
export function focusNextEnterField(container: HTMLElement, target: HTMLElement): void {
    const form = target.closest('form');
    const scope = form && container.contains(form) ? form : container;
    const nextIndex = nextEnterIndex(scope, target);
    if (nextIndex >= 0 && nextIndex < getEnterFocusables(scope).length) {
        focusEnterField(scope, nextIndex);
        return;
    }

    const finalAction = Array.from(container.querySelectorAll<HTMLElement>(
        '[data-enter-chain-target], button[type="submit"], input[type="submit"]'
    )).find(el => isAvailableEnterTarget(el, container) &&
        (!form || el.closest('form') === form || el.getAttribute('form') === form.id));
    finalAction?.focus();
}

/** القائمة الفارغة تُفتح، والمختارة تنتقل للحقل التالي داخل النافذة. */
export function handleNativeSelectEnter(select: HTMLSelectElement, event: KeyboardEvent): void {
    if (event.key !== "Enter" || event.defaultPrevented || event.ctrlKey || event.altKey ||
        event.metaKey || event.shiftKey || event.isComposing || event.keyCode === 229) return;
    if (select.disabled || select.multiple || select.size > 1 ||
        select.closest('[data-enter-handled], [data-enter-skip]')) return;

    const container = select.closest<HTMLElement>(
        '[data-wizard-root], [role="dialog"], [role="alertdialog"]'
    );
    if (!container) return;
    // القائمة المفتوحة تستقبل Enter لتأكيد الخيار دون نقل التركيز قبل التأكيد.
    if (CSS.supports('selector(:open)') && select.matches(':open')) return;
    if (event.repeat) {
        event.preventDefault();
        event.stopPropagation();
        return;
    }

    if (select.value !== "" && select.selectedIndex >= 0 && !select.selectedOptions[0]?.disabled) {
        // داخل wizard-root: السلسلة يديرها handleEnterChain على document بنطاق صحيح.
        // لا نمنع الانتشار حتى يصل الحدث إلى document.
        if (select.closest('[data-wizard-root]')) return;

        event.preventDefault();
        event.stopPropagation();
        focusNextEnterField(container, select);
        return;
    }

    event.stopPropagation();
    if (typeof select.showPicker === 'function') {
        try {
            select.showPicker();
            event.preventDefault();
        } catch {
            // إبقاء سلوك Enter الأصلي إذا منع المتصفح فتح المنتقي برمجيًا.
        }
    }
}
