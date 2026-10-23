import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const roots = ['ui/src/components', 'ui/src/pages'];
const shared = ['ui/src/index.css', 'ui/src/styles', 'ui/tailwind.config.js'];
const walk = entry => fs.statSync(entry).isDirectory()
  ? fs.readdirSync(entry).flatMap(name => walk(path.join(entry, name)))
  : /\.(?:tsx?|jsx?|css)$/.test(entry) ? [entry.replaceAll('\\', '/')] : [];
const files = [...roots.flatMap(walk), ...shared.flatMap(walk)].sort();
const findings = [];
const seen = new Set();
const definitions = [
  ['opacity', /(?<![\w-])(?:(?:[\w-]+|\[[^\]\r\n]+\]):)*opacity-(\d+(?:\.\d+)?|\[[^\]\r\n]+\])/g],
  ['color-alpha', /(?<![\w-])(?:(?:[\w-]+|\[[^\]\r\n]+\]):)*(text|bg|border(?:-[trblesxy])?|ring(?:-offset)?|shadow|outline|decoration|divide(?:-[xy])?|fill|stroke|from|via|to)-[\w.\[\]()%#,-]+\/(\d+(?:\.\d+)?|\[[^\]\r\n]+\])/g],
  ['transparent', /(?<![\w-])(?:(?:[\w-]+|\[[^\]\r\n]+\]):)*(?:text|bg|border(?:-[trblesxy])?|ring|outline|fill|stroke|from|via|to)-transparent\b/g],
  ['animation', /(?<![\w-])(?:(?:[\w-]+|\[[^\]\r\n]+\]):)*fade-(?:in|out)-(\d+)/g],
  ['alpha-color', /\b(?:rgba|hsla)\([^\r\n;]+?\)|#[\da-fA-F]{8}\b|#[\da-fA-F]{4}\b/g],
  ['opacity-property', /\b(opacity|fillOpacity|strokeOpacity|fill-opacity|stroke-opacity|watermarkOpacity|globalAlpha)\s*[:=]\s*([^,\n;}]+)/g],
  ['transparent-keyword', /\btransparent\b/g],
  ['arbitrary-alpha', /(?<![\w-])(?:(?:[\w-]+|\[[^\]\r\n]+\]):)*(text|bg|border(?:-[trblesxy])?|ring|shadow|outline|fill|stroke|from|via|to)-\[[^\]\r\n]*\/(\d*\.?\d+%?)\)[^\]\r\n]*\]/g],
  ['legacy-alpha', /(?<![\w-])(?:(?:[\w-]+|\[[^\]\r\n]+\]):)*(text|bg|border|ring|divide|placeholder)-opacity-(\d+)/g],
];
for (const definition of definitions) {
  definition[1] = new RegExp(definition[1].source.replaceAll('[\\w-]+|', '[\\w-]+(?:\\[[^\\]\\r\\n]+\\])?|'), 'g');
}
const isInScope = file => roots.some(dir => file.startsWith(dir + '/'));
for (const file of files) {
  const source = fs.readFileSync(file, 'utf8');
  const lines = source.split(/\r?\n/);
  for (const [kind, regex] of definitions) {
    for (const match of source.matchAll(regex)) {
      const line = source.slice(0, match.index).split('\n').length;
      const context = lines[line - 1].trim();
      if (/^(?:\/\/|\*|\/\*|<!--)/.test(context)) continue;
      let alpha;
      if (kind === 'opacity') alpha = match[1].startsWith('[') ? Number(match[1].slice(1, -1)) : Number(match[1]) / 100;
      if (kind === 'color-alpha') alpha = match[2].startsWith('[') ? Number(match[2].slice(1, -1)) : Number(match[2]) / 100;
      if (kind === 'legacy-alpha') alpha = Number(match[2]) / 100;
      if (kind === 'arbitrary-alpha') alpha = parseFloat(match[2]) / (match[2].endsWith('%') ? 100 : 1);
      if (kind === 'opacity-property') {
        if (/^(?:number|string|boolean)\b/.test(match[2])) continue;
        if (source.slice(0, match.index).trimEnd().endsWith('?')) continue;
        alpha = Number(match[2].replaceAll(/["']/g, '').trim().replace(/^\{/, ''));
      }
      if (kind === 'transparent-keyword') {
        if (/[\w-]/.test(source[match.index - 1] || '')) continue;
        alpha = 0;
      }
      if (kind === 'transparent') alpha = 0;
      if (kind === 'alpha-color' && match[0].startsWith('#')) {
        const hex = match[0].slice(1);
        alpha = parseInt(hex.length === 4 ? hex[3].repeat(2) : hex.slice(6), 16) / 255;
      }
      if (kind === 'alpha-color' && !match[0].startsWith('#')) {
        const alphaMatch = match[0].match(/[,/]\s*([\d.]+)(%)?\s*\)$/);
        if (alphaMatch) alpha = Number(alphaMatch[1]) / (alphaMatch[2] ? 100 : 1);
      }
      let category;
      if (Number.isFinite(alpha) && alpha >= 1) category = 'full-opacity';
      else if (kind === 'animation') category = 'motion';
      else if (kind === 'opacity-property' && /watermark/i.test(match[1])) category = 'watermark';
      else if (['color-alpha', 'arbitrary-alpha', 'legacy-alpha'].includes(kind)) {
        category = match[1] === 'text' ? 'text-alpha'
          : /^(?:bg|from|via|to)$/.test(match[1]) ? 'background-alpha'
          : /^(?:border|divide)/.test(match[1]) ? 'border-alpha'
          : /ring|outline|shadow/.test(match[1]) ? 'focus-shadow-alpha' : 'icon-alpha';
      } else if (alpha === 0) category = 'hidden-transparent';
      else if (/disabled/.test(match[0]) || /disabled\s*&&/.test(context)) category = 'disabled-opacity';
      else if (kind === 'opacity') category = 'element-opacity';
      else if (kind === 'opacity-property') category = 'style-opacity';
      else category = 'explicit-alpha-color';
      if (match[0].startsWith('placeholderr:')) category = 'invalid-reference';
      const key = `${file}:${match.index}:${kind}`;
      if (seen.has(key)) continue;
      seen.add(key);
      findings.push({file, line, index: match.index, kind, category, token: match[0], alpha: Number.isFinite(alpha) ? alpha : null, context, scope: isInScope(file) ? 'pages-components' : 'shared'});
    }
  }
}
findings.sort((a, b) => a.file.localeCompare(b.file) || a.line - b.line || a.token.localeCompare(b.token));
const actual = findings.filter(f => !['full-opacity', 'invalid-reference'].includes(f.category));
const counts = items => Object.fromEntries([...new Set(items.map(f => f.category))].sort().map(category => [category, {occurrences: items.filter(f => f.category === category).length, files: new Set(items.filter(f => f.category === category).map(f => f.file)).size}]));
const summary = {
  scannedSourceFiles: files.filter(isInScope).length,
  scannedSharedFiles: files.filter(f => !isInScope(f)).length,
  source: {occurrences: actual.filter(f => f.scope === 'pages-components').length, files: new Set(actual.filter(f => f.scope === 'pages-components').map(f => f.file)).size, categories: counts(actual.filter(f => f.scope === 'pages-components'))},
  shared: {occurrences: actual.filter(f => f.scope === 'shared').length, files: new Set(actual.filter(f => f.scope === 'shared').map(f => f.file)).size, categories: counts(actual.filter(f => f.scope === 'shared'))},
  fullOpacityExcluded: findings.filter(f => f.category === 'full-opacity').length,
  invalidReferencesExcluded: findings.filter(f => f.category === 'invalid-reference').length,
};
fs.writeFileSync(path.join(root, 'plan/transparency-inventory.json'), JSON.stringify({summary, findings}, null, 2) + '\n');
const labels = {
  'text-alpha': 'شفافية لون النص أو النص الإرشادي',
  'element-opacity': 'تخفيف العنصر كاملًا أو تغييره عند المرور',
  'disabled-opacity': 'شفافية مرتبطة بحالة التعطيل',
  'background-alpha': 'شفافية الخلفيات والتدرجات',
  'border-alpha': 'شفافية الحدود والفواصل',
  'focus-shadow-alpha': 'شفافية حلقة التركيز والظلال',
  'hidden-transparent': 'إخفاء تام أو لون شفاف صريح',
  'icon-alpha': 'شفافية لون تعبئة أو حد الأيقونة',
  'motion': 'تلاشي مرتبط بحركة الفتح والإغلاق',
  'style-opacity': 'شفافية عبر style أو CSS أو خاصية رسم',
  'watermark': 'إعدادات شفافية العلامة المائية',
  'explicit-alpha-color': 'ألوان rgba أو hsla أو HEX مع قناة شفافية',
};
const sourceFindings = actual.filter(f => f.scope === 'pages-components');
const dimming = sourceFindings.filter(f => ['text-alpha', 'element-opacity'].includes(f.category));
const code = value => '`' + value.replaceAll('`', "'").replaceAll('|', '\\|') + '`';
const fileLink = f => `[${f.file}:${f.line}](/${path.join(root, f.file).replaceAll('\\', '/')}:${f.line})`;
const table = rows => ['| الملف والسطر | الاستخدام |', '|---|---|', ...rows.map(f => `| ${fileLink(f)} | ${code(f.token)} |`)].join('\n');
const report = [
  '# جرد الشفافية في صفحات ومكوّنات الواجهة',
  '',
  'تاريخ الفحص: 2026-10-17 — هذا جرد ساكن للمصدر، ولم يتضمن تعديل ملفات التطبيق أو الحكم على التباين الفعلي في المتصفح.',
  '',
  `فُحص ${summary.scannedSourceFiles} ملفًا في \`ui/src/components\` و\`ui/src/pages\`، إضافة إلى ${summary.scannedSharedFiles} ملفات مشتركة: \`index.css\` و\`styles/print.css\` و\`tailwind.config.js\`.`,
  '',
  `رُصد ${summary.source.occurrences} استخدامًا في ${summary.source.files} ملفًا من الصفحات والمكوّنات، إضافة إلى ${summary.shared.occurrences} استخدامًا في ${summary.shared.files} من الملفات المشتركة. المقصود بالاستخدام ظهور قاعدة أو قيمة في المصدر؛ قد تكون مشروطة أو ضمن فرع غير ظاهر حاليًا. هذه الأعداد ليست عدد أخطاء أو عدد عناصر تظهر للمستخدم.`,
  '',
  `توجد ${dimming.length} إشارة إلى تخفيف لون النص أو العنصر كاملًا في ${new Set(dimming.map(f => f.file)).size} ملفات. تشمل الأخيرة النصوص والأيقونات والزخارف؛ يلزم تمييز العنصر قبل أي إزالة جماعية.`,
  '',
  '## ملخص الصفحات والمكوّنات',
  '',
  '| النوع | مواضع المصدر | ملفات |',
  '|---|---:|---:|',
  ...Object.entries(summary.source.categories).map(([name, value]) => `| ${labels[name]} | ${value.occurrences} | ${value.files} |`),
  '',
  'أعداد الملفات تتداخل بين الأنواع، لذلك لا تُجمع للحصول على عدد الملفات الفريدة.',
  '',
  '## مواضع ذات أولوية عند تحسين وضوح النص',
  '',
  table([
    ['ui/src/components/ui/Input.tsx', 83],
    ['ui/src/components/ui/Input.tsx', 411],
    ['ui/src/components/ui/StatCard.tsx', 80],
    ['ui/src/components/ui/FormField.tsx', 48],
    ['ui/src/components/ui/FormSection.tsx', 109],
    ['ui/src/components/ui/InfoPair.tsx', 36],
    ['ui/src/components/ui/AuditTimeline.tsx', 209],
    ['ui/src/components/ui/DataTable.tsx', 450],
    ['ui/src/pages/accounting/ChartOfAccountsPage.tsx', 293],
    ['ui/src/pages/admin/AuditLogsPage.tsx', 88],
    ['ui/src/pages/notifications/NotificationsPage.tsx', 151],
    ['ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx', 284],
  ].flatMap(([file, line]) => dimming.filter(f => f.file === file && f.line === line))),
  '',
  '## تفسير الأثر قبل التعديل',
  '',
  '- `text-…/60` يخفف لون النص وحده؛ `opacity-60` يخفف العنصر وجميع محتوياته، بما فيها النص والأيقونات والخلفية والحدود.',
  '- اجتماع تخفيف لون النص وشفافية العنصر يضاعف الأثر؛ مثل `opacity-30 text-primary/40` في أيقونات الحالة الفارغة في `DataTable`، فتكون العتامة الفعالة نحو 12% قبل احتساب التنعيم.',
  '- `bg-…/10` و`border-…/40` لا يخففان النص مباشرة. إزالة شفافية الخلفيات تغيّر مظهر الشارات والتنبيهات والبطاقات، ولا تمثل تلقائيًا تحسينًا للنص.',
  '- بعض استخدامات `opacity-0` تخفي عناصر ملفات أو تواريخ أصلية مع الإبقاء على التفاعل؛ وبعضها جزء من الحركة. لا تُعامل على أنها نص باهت.',
  '- توجد حالات تعطيل في مكوّنات مشتركة مثل الأزرار وحقول الإدخال وCheckbox وTabs. يلزم الحفاظ على دلالة التعطيل عند استبدال الشفافية بلون صريح.',
  '- لم تُصنّف القيم الكاملة `opacity-100` على أنها شفافية فعلية، ولم يُحتسب `transition-opacity` وحده لأنه يحدد الخاصية المتحركة دون أن يخففها.',
  '- اكتُشفت إشارتان بالصياغة غير الصحيحة `placeholderr:` في صفحة الدخول؛ لا يولّد Tailwind تنسيق النص الإرشادي المقصود منهما، لذلك فُصلتا عن العدد الأساسي.',
  '- يشمل الجرد صيغ Tailwind والقيم المباشرة في CSS وstyle وSVG وإعدادات الطباعة. لا يقيس شفافية صور أو فيديوهات أو ملفات أصول خارج هذا النطاق، ولا قواعد مكتبات المتصفح أو الأنماط المحسوبة ديناميكيًا من بيانات التشغيل.',
  '',
  '## الجرد التفصيلي للصفحات والمكوّنات',
];
for (const category of Object.keys(labels)) {
  const entries = sourceFindings.filter(f => f.category === category);
  if (entries.length) report.push('', `### ${labels[category]} — ${entries.length} موضعًا`, '', table(entries));
}
report.push('', '## القواعد المشتركة المؤثرة', '', table(actual.filter(f => f.scope === 'shared')));
report.push('', '## إشارات مستبعدة من العدد الأساسي', '', `قيم العتامة الكاملة: ${summary.fullOpacityExcluded}. إشارات Tailwind غير الصحيحة: ${summary.invalidReferencesExcluded}.`, '', table(findings.filter(f => f.category === 'invalid-reference')));
report.push('', '## إعادة إنتاج الجرد', '', 'شغّل `node plan/transparency-scan.mjs` من جذر المشروع. البيانات الكاملة مع سياق كل سطر محفوظة في `plan/transparency-inventory.json`. لم تُشغّل اختبارات التطبيق لأن الفحص لا يعدّل تنفيذه.', '');
fs.writeFileSync(path.join(root, 'docs/audits/ui-transparency-inventory.md'), report.join('\n'));
console.log(JSON.stringify(summary, null, 2));
