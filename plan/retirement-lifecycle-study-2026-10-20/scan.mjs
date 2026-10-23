// Read-only source inventory; writes artifacts inside this study directory only.
import { createHash } from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { readFileSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const out = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(out, '../..');
const suffixes = new Set(['.cs', '.csproj', '.ts', '.tsx', '.sql', '.md', '.ps1', '.json', '.js', '.mjs']);
const skip = new Set(['node_modules', 'bin', 'obj', '.git', '.agents', '.codex', 'dist', 'coverage', 'skills']);
const patterns = {
  file_state: /FileStatusConstants|\bFILE_STATUS\b|PreviousStatusID|ClosedDate|closedFiles|ClosedCount|UnderReviewFiles|SuspendedFiles/,
  review: /RequestReview|AutoResolveFileReview|PendingReviewStatus|FileReviewStatus|ReviewStatus|reviewStatus|pendingReviewStatus|FileReviewItem|LastReviewedFileState/,
  suspension: /PensionSuspension|SuspensionStatusConstants|SuspensionSource|suspensionSource|suspensionReason|SuspensionReason/,
  payment: /TransactionStatusConstants|BatchStatusConstants|SalaryPaymentItem|PayrollGeneration|SalaryPaymentBatch|RecipientScope|PaymentItemID|paymentItemID/,
  numeric_state: /\b(?:[A-Za-z_]+\.)?(?:StatusID|statusID|FileStatusID|fileStatusID|StatusId|statusId)\s*(?:==|!=|===|!==|=|:|is|switch)|(?:statusID|StatusID)\s*[<>]/,
  arabic_state: /مغلق|مغلقة|المغلق|المغلقة|ملفات موقوفة|ملفات معلقة|ملفات ناقصة|تحت المراجعة|قيد المراجعة|انتهى استحقاق/,
  mutation: /\b(?:public|private|internal|protected)\s+(?:static\s+)?(?:async\s+)?[^;=]+\b(?:Create|Update|Delete|Upload|Assign|Add|Remove|End|Restore|Set|Import|Request|Approve|Reject|Review|Deactivate|Acknowledge|Fulfill|Grant|Lift|Stop|Reactivate|Record|Correct|Handle|Change|Process|Generate|Export)[A-Za-z]*Async\s*\(/,
};
const paths = execFileSync('rg', ['--files', '--hidden', '--no-ignore', '-g', '!.git', '-g', '!node_modules', '-g', '!bin', '-g', '!obj', '-g', '!dist', '-g', '!.agents', '-g', '!.codex', '-g', '!skills'], { cwd: root, encoding: 'utf8', maxBuffer: 32 * 1024 * 1024 }).trim().split(/\r?\n/).sort();
const manifest = [], hits = [];
for (const name of paths) {
  const parts = name.split(/[\\/]/);
  if (!suffixes.has(path.extname(name).toLowerCase()) || parts.some(p => skip.has(p)) || name.replaceAll('\\', '/').startsWith('plan/retirement-lifecycle-study-2026-10-20/') || name.replaceAll('\\', '/') === 'plan/feature-context-retirement-lifecycle.md') continue;
  const data = readFileSync(path.join(root, name));
  const lines = data.toString('utf8').replace(/^\uFEFF/, '').split(/\r?\n/);
  const rel = parts.join('/');
  const area = /^PensionMS\.(?:Tests|IntegrationTests)\//.test(rel) || /^ui\/(?:tests|src\/.*\.test\.)/.test(rel) ? 'tests'
    : /^(?:tmp|plan)\//.test(rel) ? (rel.endsWith('.sql') ? 'database-script' : 'historical-artifact')
    : rel.startsWith('docs/') ? 'documentation' : rel.endsWith('.sql') ? 'database-script'
    : /^PensionMS\.(?:Core|Infrastructure|API)\/|^ui\/src\//.test(rel) ? 'runtime-source' : 'support';
  manifest.push({ path: rel, area, sha256: createHash('sha256').update(data).digest('hex'), lines: lines.length });
  lines.forEach((line, i) => {
    const kinds = Object.entries(patterns).filter(([, regex]) => regex.test(line)).map(([k]) => k);
    if (kinds.length) {
      const text = line.trim();
      hits.push({ path: rel, line: i + 1, kinds, text: text.slice(0, 1200), ...(text.length > 1200 ? { truncated: true, original_length: text.length } : {}) });
    }
  });
}
const save = (name, data) => writeFileSync(path.join(out, name), JSON.stringify(data, null, 2) + '\n', 'utf8');
save('source-manifest.json', manifest);
save('references.json', hits);
const summary = { source_files: manifest.length, matched_files: new Set(hits.map(h => h.path)).size, matched_lines: hits.length, by_kind: {} };
for (const h of hits) for (const kind of h.kinds) summary.by_kind[kind] = (summary.by_kind[kind] || 0) + 1;
save('scan-summary.json', summary);
console.log(JSON.stringify(summary));
