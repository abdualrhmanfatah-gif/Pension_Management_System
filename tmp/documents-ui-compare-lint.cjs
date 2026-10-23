const fs = require('node:fs');
function issues(file) {
  const bytes = fs.readFileSync(file);
  const content = bytes.toString(bytes[0] === 255 && bytes[1] === 254 ? 'utf16le' : 'utf8');
  let current = '';
  const result = [];
  for (const line of content.split(/\r?\n/)) {
    if (line.startsWith('F:') && !/:\d+:\d+$/.test(line.trim())) current = line.trim();
    const match = line.match(/^\s+\d+:\d+\s+(error|warning)\s+(.+)/);
    if (match) result.push(current + '|' + match[1] + '|' + match[2].replace(/\(at line \d+\)/g, '(at line)').replace(/\s+/g, ' ').trim());
  }
  return result;
}
const prefix = process.argv[2] || 'documents-ui';
const before = issues(`plan/${prefix}-lint-before.txt`);
const after = issues(`plan/${prefix}-lint-after.txt`);
const remaining = new Map();
for (const item of before) remaining.set(item, (remaining.get(item) || 0) + 1);
const added = [];
for (const item of after) {
  if (remaining.get(item) > 0) remaining.set(item, remaining.get(item) - 1);
  else added.push(item);
}
const result = { before: before.length, after: after.length, added };
fs.writeFileSync(`plan/${prefix}-lint-comparison.json`, JSON.stringify(result, null, 2));
console.log(result);
if (!before.length || added.length) process.exitCode = 1;
