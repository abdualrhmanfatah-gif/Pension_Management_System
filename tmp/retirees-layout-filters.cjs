const fs = require('node:fs');
const file = 'ui/src/pages/retirees/RetireesPage.tsx';
const source = fs.readFileSync(file, 'utf8');
const marker = source.indexOf('Filters Bar');
const dialog = source.indexOf('<BaseFormDialog', marker);
if (marker < 0 || dialog < marker) throw new Error('Filters section anchors not found');
const start = source.lastIndexOf('\n', marker) + 1;
const end = source.lastIndexOf('\n', dialog) + 1;
const replacement = `            <Accordion type="single" collapsible className="sm:hidden">
                <AccordionItem value="retiree-filters" className="border-0">
                    <AccordionTrigger density="compact">
                        <span className="flex items-center gap-2">
                            الفلاتر
                            {hasActiveFilters && (
                                <Badge variant="secondary">{[categoryId, rankId, gender, governorate].filter(Boolean).length} مفعّل</Badge>
                            )}
                        </span>
                    </AccordionTrigger>
                    <AccordionContent className="space-y-1 pb-0">
                        {filterFields}
                        {clearFiltersButton}
                    </AccordionContent>
                </AccordionItem>
            </Accordion>
            <div className="hidden space-y-1 sm:block">
                <div className="flex min-h-11 items-center justify-between gap-2">
                    <h3 className="text-sm font-semibold text-foreground">الفلاتر</h3>
                    {clearFiltersButton}
                </div>
                {filterFields}
            </div>
`;
fs.writeFileSync(file, source.slice(0,start) + replacement + source.slice(end));
console.log('Replaced the retirees filters section.');
