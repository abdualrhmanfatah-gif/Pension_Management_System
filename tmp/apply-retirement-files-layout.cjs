const fs = require('node:fs');
const file = 'ui/src/pages/retirement-files/RetirementFilesPage.tsx';
let source = fs.readFileSync(file, 'utf8');
function between(start, end, replacement) {
    const a = source.indexOf(start), b = source.indexOf(end, a);
    if (a < 0 || b < a) throw new Error(`Missing marker: ${start}`);
    source = source.slice(0, a) + replacement + source.slice(b);
}
const filters = `    const filterFields = (
        <div className="grid grid-cols-2 gap-2 lg:grid-cols-5">
            <NativeSelect label="حالة الملف" value={statusId} onChange={setStatusId}
                options={[{ value: "", label: "جميع الحالات" }, ...(statuses ?? []).map(s => ({ value: String(s.statusID), label: s.statusName }))]} />
            <NativeSelect label="الفئة" value={categoryId} onChange={setCategoryId}
                options={[{ value: "", label: "جميع الفئات" }, ...(categories ?? []).map(c => ({ value: String(c.id), label: c.name }))]} />
            <NativeSelect label="الرتبة" value={rankId} onChange={setRankId}
                options={[{ value: "", label: "جميع الرتب" }, ...(ranks ?? []).map(r => ({ value: String(r.rankID), label: r.rankName }))]} />
            <NativeSelect label="الجنس" value={gender} onChange={setGender}
                options={[{ value: "", label: "الكل" }, ...GENDER_OPTIONS.map(g => ({ value: String(g.id), label: g.name }))]} />
            <div className="col-span-2 lg:col-span-1">
                <NativeSelect label="المحافظة" value={governorate} onChange={setGovernorate}
                    options={[{ value: "", label: "جميع المحافظات" }, ...(governorates ?? []).map(g => ({ value: g.name, label: g.name }))]} />
            </div>
        </div>
    );
    const clearFiltersButton = hasActiveFilters ? (
        <Button variant="ghost" size="sm" onClick={clearFilters} className="text-destructive">
            <X className="h-4 w-4" aria-hidden="true" />مسح الفلاتر
        </Button>
    ) : null;

`;
source = source.replace('    // معالجة العرض حسب الصلاحية', filters + '    // معالجة العرض حسب الصلاحية');
source = source.replace('<div className="space-y-6">', '<div className="min-w-0">');
source = source.replace('<CrudPageLayout\n', '<CrudPageLayout\n                density="compact"\n');
source = source.replace('                searchInHeader\n', '                searchInHeader\n                searchLabel="البحث عن ملف"\n                searchPlaceholder="الاسم أو الرقم العسكري أو رقم الملف"\n');
between('                headerActions={', '                renderMobileCard=', '');
between('                renderMobileCard=', '                <RetirementFileDialog', `                rowKey={(row) => row.fileID}
                mobileScrollMode="page"
                rowClassName={retirementFileCardClassName}
                renderMobileCard={(item) => (
                    <RetirementFileCard file={item}>
                        {(item.militaryNumber || item.rankName || item.reviewStatus === 1) && (
                            <div className="flex min-w-0 items-center gap-2 text-xs text-muted-foreground">
                                {item.militaryNumber && <p className="shrink-0 whitespace-nowrap">العسكري: <bdi dir="ltr" className="font-mono font-semibold text-foreground">{item.militaryNumber}</bdi></p>}
                                {item.rankName && <p className="min-w-0 flex-1 truncate" title={item.rankName}>{item.rankName}</p>}
                                {item.reviewStatus === 1 && <Badge variant="warning" className="shrink-0">مراجعة</Badge>}
                            </div>
                        )}
                    </RetirementFileCard>
                )}
            >
                <SearchDialog
                    value={search}
                    onSelect={(val) => { setSearch(val); setPage(1); }}
                    queryKey={["retirement-files", "dialog-search"]}
                    fetchFn={(term) => retirementApi.getFiles({ page: 1, pageSize: 100, search: term || undefined })}
                    getName={(f) => f.retireeFullName}
                    getSubtitle={(f) => f.fileNumber}
                    title="بحث بالاسم"
                    placeholder="اكتب الرقم العسكري أو اسم المتقاعد..."
                    hideTrigger
                />
                <Accordion type="single" collapsible className="sm:hidden">
                    <AccordionItem value="file-filters" className="border-0">
                        <AccordionTrigger density="compact">
                            <span className="flex items-center gap-2">
                                الفلاتر
                                {hasActiveFilters && <Badge variant="secondary">{[statusId, categoryId, rankId, gender, governorate].filter(Boolean).length} مفعّل</Badge>}
                            </span>
                        </AccordionTrigger>
                        <AccordionContent className="space-y-1 pb-0">
                            {filterFields}
                            {clearFiltersButton}
                        </AccordionContent>
                    </AccordionItem>
                </Accordion>
                <div className="hidden space-y-1 sm:block">
                    {filterFields}
                    {clearFiltersButton}
                </div>

`);
fs.writeFileSync(file,source);
