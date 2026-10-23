import fs from 'node:fs';
import crypto from 'node:crypto';

const categories = new Set(['text-alpha', 'element-opacity', 'border-alpha']);
const inventory = JSON.parse(fs.readFileSync('plan/transparency-inventory.json', 'utf8'));
const findings = inventory.findings.filter(f => f.scope === 'pages-components' && categories.has(f.category));
const hash = source => crypto.createHash('sha256').update(source).digest('hex');
const replace = finding => {
  if (finding.category === 'element-opacity') return finding.token.replace(/opacity-(?:\d+(?:\.\d+)?|\[[^\]]+\])$/, 'opacity-100');
  if (finding.kind === 'arbitrary-alpha') return finding.token.replace(/\/\d*\.?\d+%?(?=\))/, '');
  if (finding.kind === 'legacy-alpha') return finding.token.replace(/opacity-\d+$/, 'opacity-100');
  return finding.token.replace(/\/(?:\d+(?:\.\d+)?|\[[^\]]+\])$/, '');
};
const files = [];
for (const file of [...new Set(findings.map(f => f.file))].sort()) {
  const before = fs.readFileSync(file, 'utf8');
  let after = before;
  const edits = findings.filter(f => f.file === file).sort((a, b) => b.index - a.index).map(f => {
    if (before.slice(f.index, f.index + f.token.length) !== f.token) throw new Error(`Stale scan: ${file}:${f.line}`);
    const replacement = replace(f);
    if (replacement === f.token) throw new Error(`No replacement: ${file}:${f.line} ${f.token}`);
    after = after.slice(0, f.index) + replacement + after.slice(f.index + f.token.length);
    return {line: f.line, category: f.category, before: f.token, after: replacement};
  });
  files.push({file, beforeHash: hash(before), afterHash: hash(after), edits, after});
}
const summary = {
  files: files.length,
  occurrences: findings.length,
  categories: Object.fromEntries([...categories].map(category => [category, findings.filter(f => f.category === category).length])),
};
if (process.argv.includes('--apply')) {
  if (fs.existsSync('plan/transparency-removal-changes.json')) throw new Error('A previous application manifest already exists.');
  fs.copyFileSync('plan/transparency-inventory.json', 'plan/transparency-removal-before.json');
  for (const file of files) {
    if (hash(fs.readFileSync(file.file, 'utf8')) !== file.beforeHash) throw new Error(`Source changed: ${file.file}`);
    fs.writeFileSync(file.file, file.after);
  }
  fs.writeFileSync('plan/transparency-removal-changes.json', JSON.stringify({summary, files: files.map(({after, ...file}) => file)}, null, 2) + '\n');
}
console.log(JSON.stringify(summary, null, 2));
