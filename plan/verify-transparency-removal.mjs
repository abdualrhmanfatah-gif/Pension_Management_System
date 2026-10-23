import fs from 'node:fs';
import crypto from 'node:crypto';

const before = JSON.parse(fs.readFileSync('plan/transparency-removal-before.json', 'utf8'));
const after = JSON.parse(fs.readFileSync('plan/transparency-inventory.json', 'utf8'));
const changes = JSON.parse(fs.readFileSync('plan/transparency-removal-changes.json', 'utf8'));
const targets = new Set(['text-alpha', 'element-opacity', 'border-alpha']);
const hash = source => crypto.createHash('sha256').update(source).digest('hex');
for (const file of changes.files) {
  const current = fs.readFileSync(file.file, 'utf8');
  if (hash(current) !== file.afterHash) throw new Error(`Unexpected file changes: ${file.file}`);
  const originals = before.findings.filter(f => f.file === file.file && targets.has(f.category)).sort((a, b) => a.index - b.index);
  let delta = 0;
  const reversals = originals.map(f => {
    const edit = file.edits.find(e => e.line === f.line && e.category === f.category && e.before === f.token);
    if (!edit) throw new Error(`Missing edit: ${file.file}:${f.line}`);
    const index = f.index + delta;
    delta += edit.after.length - edit.before.length;
    if (current.slice(index, index + edit.after.length) !== edit.after) throw new Error(`Unexpected replacement: ${file.file}:${f.line}`);
    return {index, ...edit};
  });
  let restored = current;
  for (const edit of reversals.reverse()) restored = restored.slice(0, edit.index) + edit.before + restored.slice(edit.index + edit.after.length);
  if (hash(restored) !== file.beforeHash) throw new Error(`Unrelated source edits detected: ${file.file}`);
}
const otherKeys = inventory => inventory.findings.filter(f => !targets.has(f.category) && f.category !== 'full-opacity').map(f => JSON.stringify([f.file, f.line, f.kind, f.token])).sort();
if (JSON.stringify(otherKeys(before)) !== JSON.stringify(otherKeys(after))) throw new Error('Other transparency categories changed.');
const remaining = after.findings.filter(f => f.scope === 'pages-components' && targets.has(f.category));
if (remaining.length) throw new Error(`Targeted transparency remains: ${remaining.length}`);
const result = {files: changes.summary.files, replacements: changes.summary.occurrences, categories: changes.summary.categories, targetedRemaining: remaining.length, onlyTargetedSourceEdits: true, otherTransparencyCategoriesUnchanged: true};
fs.writeFileSync('plan/transparency-removal-verification.json', JSON.stringify(result, null, 2) + '\n');
console.log(JSON.stringify(result, null, 2));
