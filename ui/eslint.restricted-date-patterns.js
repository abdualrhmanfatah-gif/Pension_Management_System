export const restrictedDatePatterns = [
    {
        selector:
            "MemberExpression[computed=true][property.value=0] > CallExpression[callee.property.name='split'][callee.object.callee.property.name='toISOString']",
        message:
            "لا تستخرج تاريخاً تقويمياً من toISOString() — استخدم todayYemen() أو toDateOnlyYemen() من '@/lib/date' (ADR 0001).",
    },
    {
        selector:
            "CallExpression[callee.property.name=/^(slice|substring)$/][callee.object.callee.property.name='toISOString']",
        message:
            "لا تستخرج تاريخاً تقويمياً من toISOString() — استخدم todayYemen() أو toDateOnlyYemen() من '@/lib/date' (ADR 0001).",
    },
];
