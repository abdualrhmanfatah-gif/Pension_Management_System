import fs from 'node:fs';
import crypto from 'node:crypto';
import path from 'node:path';

const root = process.cwd();
const output = path.join(root, 'plan/retirement-lifecycle-implementation');
const baseline = JSON.parse(fs.readFileSync(path.join(root, 'plan/retirement-lifecycle-study-2026-10-20/source-manifest.json'), 'utf8').replace(/^\uFEFF/, ''));
const entities = baseline.filter(f => f.path.startsWith('PensionMS.Core/Entities/') && f.path.endsWith('.cs'));
const hash = file => crypto.createHash('sha256').update(fs.readFileSync(path.join(root, file))).digest('hex');
const changedEntities = entities.filter(f => !fs.existsSync(path.join(root, f.path)) || hash(f.path) !== f.sha256).map(f => f.path);
const changedSources = baseline.filter(f => /\.(cs|ts|tsx)$/.test(f.path) && !f.path.startsWith('.artifacts/'))
    .filter(f => !fs.existsSync(path.join(root, f.path)) || hash(f.path) !== f.sha256).map(f => f.path);

const lintBytes = fs.readFileSync(path.join(output, 'ui-lint-before.txt'));
const lintBefore = lintBytes.toString(lintBytes[0] === 255 ? 'utf16le' : 'utf8').replace(/^\uFEFF/, '');
const before = {};
let file = '';
for (const line of lintBefore.split(/\r?\n/)) {
    if (/^[A-Za-z]:\\.*\.(tsx?|mjs|js)$/.test(line.trim())) file = line.trim();
    const match = line.match(/^\s*\d+:\d+\s+(error|warning)\s+/);
    if (match) {
        const key = `${file}|${match[1]}`;
        before[key] = (before[key] || 0) + 1;
    }
}
const rows = JSON.parse(fs.readFileSync(path.join(output, 'ui-lint-final.json'), 'utf8').replace(/^\uFEFF/, ''));
const after = {};
for (const row of rows) for (const message of row.messages) {
    const key = `${row.filePath}|${message.severity === 2 ? 'error' : 'warning'}`;
    after[key] = (after[key] || 0) + 1;
}
const added = Object.entries(after).filter(([key, count]) => count > (before[key] || 0))
    .map(([key, count]) => ({ key, added: count - (before[key] || 0) }));
const result = {
    entityFilesChecked: entities.length, changedEntities, changedExistingSourcesSinceStudy: changedSources,
    lint: {
        baseline: {
            errors: Object.entries(before).filter(([k]) => k.endsWith('|error')).reduce((s, [, n]) => s + n, 0),
            warnings: Object.entries(before).filter(([k]) => k.endsWith('|warning')).reduce((s, [, n]) => s + n, 0),
        },
        current: { errors: rows.reduce((s, r) => s + r.errorCount, 0), warnings: rows.reduce((s, r) => s + r.warningCount, 0) },
        addedProblemsByFileAndSeverity: added,
    },
};
fs.writeFileSync(path.join(output, 'source-verification.json'), JSON.stringify(result, null, 2));
console.log(JSON.stringify({ entityFilesChecked: entities.length, changedEntities, lint: result.lint }, null, 2));
