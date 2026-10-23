import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { execFileSync } from 'node:child_process';
import { createRequire } from 'node:module';

// Read-only analysis of application files; writes evidence only beside this script.
const root = process.cwd();
const out = path.join(root, 'plan/file-audit-2026-10-18');
const require = createRequire(path.join(root, 'ui/package.json'));
const ts = require('typescript');
const slash = value => value.replaceAll('\\', '/');
const relative = value => slash(path.relative(root, value));
const hash = value => crypto.createHash('sha256').update(value).digest('hex');
const listed = [...new Set(execFileSync('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z'], {maxBuffer: 32 * 1024 * 1024}).toString().split('\0').filter(Boolean))];
const generated = /(^|\/)(node_modules|bin|obj|dist|publish|TestResults|\.git|\.artifacts|\.debug-capture)(\/|$)/i;
const excluded = /^(?:\.agents|\.agent|\.claude|\.codex|\.gemini|\.impeccable|\.opencode|\.playwright-mcp|skills|tmp|plan)\//;
const files = listed.filter(f => f !== 'docs/audits/code-duplicates-unused-files-2026-10-18.md' && !generated.test(f) && !excluded.test(f) && fs.existsSync(f) && fs.statSync(f).isFile()).sort();
const buffers = new Map(files.map(f => [f, fs.readFileSync(f)]));
const texts = new Map([...buffers].filter(([f]) => /\.(?:[cm]?[jt]sx?|css|html|cs|csproj|json|md|ps1|sql|ya?ml|py|http|config|svg)$/i.test(f)).map(([f,b]) => [f,b.toString('utf8')]));
const groupBy = (items, key) => {
  const result = new Map();
  for (const item of items) { const k = key(item); if (!result.has(k)) result.set(k, []); result.get(k).push(item); }
  return [...result.values()].filter(group => group.length > 1);
};
const exact = groupBy(files, f => hash(buffers.get(f))).map(group => ({sha256: hash(buffers.get(group[0])), bytes: buffers.get(group[0]).length, files:group}));
const appCode = files.filter(f => /^(ui\/src\/|PensionMS\.[^/]+\/)/.test(f) && /\.(?:cs|tsx?|jsx?)$/.test(f));
const normalized = groupBy(appCode, f => hash(texts.get(f).replace(/\s+/g, ''))).filter(group => new Set(group.map(f => hash(buffers.get(f)))).size > 1);

const configFile = ts.readConfigFile(path.join(root, 'ui/tsconfig.app.json'), ts.sys.readFile);
const config = ts.parseJsonConfigFileContent(configFile.config, ts.sys, path.join(root, 'ui'));
const modules = files.filter(f => /^ui\//.test(f) && /\.(?:tsx?|jsx?|css)$/.test(f));
const edges = new Map(modules.map(f => [f, new Set()]));
const specifiers = [];
const topLevelFunctions = [];
const unresolvedLocal = [];
const nonLiteralDynamic = [];
function resolve(from, spec) {
  const resolved = ts.resolveModuleName(spec, path.join(root, from), config.options, ts.sys).resolvedModule;
  if (resolved && !resolved.isExternalLibraryImport) return relative(resolved.resolvedFileName);
  const clean = spec.split('?')[0];
  const base = clean.startsWith('@/') ? path.join(root, 'ui/src', clean.slice(2)) : clean.startsWith('.') ? path.resolve(root, path.dirname(from), clean) : null;
  if (base) for (const suffix of ['', '.ts', '.tsx', '.js', '.jsx', '.css', '/index.ts', '/index.tsx']) if (fs.existsSync(base + suffix) && fs.statSync(base + suffix).isFile()) return relative(base + suffix);
  return null;
}
function addEdge(from, spec, kind, line) {
  const target = resolve(from, spec);
  specifiers.push({from, spec, target, kind, line});
  if (target) edges.get(from).add(target);
  else if (spec.startsWith('.') || spec.startsWith('@/')) unresolvedLocal.push({from,spec,kind,line});
}
for (const file of modules) {
  const content = texts.get(file);
  if (file.endsWith('.css')) {
    for (const match of content.matchAll(/@import\s+['"]([^'"]+)['"]/g)) addEdge(file,match[1],'css',content.slice(0,match.index).split('\n').length);
    continue;
  }
  const ast = ts.createSourceFile(file, content, ts.ScriptTarget.Latest, true);
  for (const node of ast.statements) {
    if (ts.isFunctionDeclaration(node) && node.body && node.name && node.getText(ast).split('\n').length >= 12) {
      const printed = ts.createPrinter({removeComments:true}).printNode(ts.EmitHint.Unspecified,node,ast).trim();
      if (printed.length >= 350) topLevelFunctions.push({file,name:node.name.text,line:ast.getLineAndCharacterOfPosition(node.getStart(ast)).line+1,signature:hash(printed)});
    }
  }
  function visit(node) {
    const line = ast.getLineAndCharacterOfPosition(node.getStart(ast)).line + 1;
    if ((ts.isImportDeclaration(node) || ts.isExportDeclaration(node)) && node.moduleSpecifier && ts.isStringLiteralLike(node.moduleSpecifier)) addEdge(file,node.moduleSpecifier.text,'static',line);
    if (ts.isCallExpression(node) && (node.expression.kind === ts.SyntaxKind.ImportKeyword || (ts.isIdentifier(node.expression) && node.expression.text === 'require'))) {
      if (node.arguments[0] && ts.isStringLiteralLike(node.arguments[0])) addEdge(file,node.arguments[0].text,'dynamic',line);
      else nonLiteralDynamic.push({file,line,text:node.getText(ast).slice(0,180)});
    }
    if (ts.isNewExpression(node) && node.expression.getText(ast) === 'URL' && node.arguments?.[0] && ts.isStringLiteralLike(node.arguments[0])) addEdge(file,node.arguments[0].text,'url',line);
    ts.forEachChild(node,visit);
  }
  visit(ast);
}
function walk(roots) {
  const seen = new Set(); const pending = [...roots];
  while (pending.length) {const f = pending.pop(); if (seen.has(f)) continue; seen.add(f); for (const next of edges.get(f) ?? []) pending.push(next);}
  return seen;
}
const html = texts.get('ui/index.html');
const runtimeRoots = [...html.matchAll(/<script[^>]+src=["']([^"']+)["']/g)].map(m => 'ui/' + m[1].replace(/^\//,''));
const testRoots = modules.filter(f => /(?:\.test\.[jt]sx?$|^ui\/tests\/)/.test(f));
const configRoots = modules.filter(f => !f.startsWith('ui/src/') && !f.startsWith('ui/tests/'));
const runtime = walk(runtimeRoots), tests = walk(testRoots), tooling = walk(configRoots);
const source = modules.filter(f => f.startsWith('ui/src/') && !/\.d\.ts$|\.(test|stories)\.[jt]sx?$/.test(f));
const dead = source.filter(f => !runtime.has(f) && !tests.has(f) && !tooling.has(f)).map(file => ({file, lines:texts.get(file).split('\n').length, importedBy:[...edges].filter(([,targets]) => targets.has(file)).map(([f]) => f), imports:[...(edges.get(file) ?? [])]}));
const testOnly = source.filter(f => !runtime.has(f) && tests.has(f));
const imports = new Map();
for (const {spec,from} of specifiers) if (!spec.startsWith('.') && !spec.startsWith('@/')) {
  const dep = spec.startsWith('@') ? spec.split('/').slice(0,2).join('/') : spec.split('/')[0];
  if (!imports.has(dep)) imports.set(dep,new Set()); imports.get(dep).add(from);
}
const pkg = JSON.parse(texts.get('ui/package.json'));
const unimportedDependencies = Object.keys(pkg.dependencies).filter(dep => !imports.has(dep));

const cs = files.filter(f => /^PensionMS\./.test(f) && f.endsWith('.cs'));
// Conservative lexical scan: identifiers in comments/strings may retain a file,
// so zero-reference candidates require subsequent framework/DI review.
const symbolFiles = new Map();
for (const file of cs) for (const symbol of new Set(texts.get(file).match(/\b[A-Za-z_]\w*\b/g) ?? [])) {
  if (!symbolFiles.has(symbol)) symbolFiles.set(symbol,[]); symbolFiles.get(symbol).push(file);
}
const declarations = cs.flatMap(file => [...texts.get(file).matchAll(/\b(?:class|interface|enum|struct|record(?:\s+(?:class|struct))?)\s+(\w+)/g)].map(m => ({file,symbol:m[1],line:texts.get(file).slice(0,m.index).split('\n').length,external:(symbolFiles.get(m[1]) ?? []).filter(f => f !== file)})));
const noExternalSymbols = declarations.filter(d => !d.external.length);
const services = declarations.filter(d => /^PensionMS.Infrastructure\/(?:Services|Jobs)\//.test(d.file) && path.posix.basename(d.file, '.cs') === d.symbol && new RegExp('class\\s+' + d.symbol + '\\b').test(texts.get(d.file))).map(d => {
  const content = texts.get(d.file);
  const match = new RegExp('class\\s+' + d.symbol + '\\s*(?:\\([^)]*\\))?\\s*:\\s*([^\\{]+)').exec(content);
  const bases = match ? (match[1].match(/\bI[A-Z]\w*\b/g) ?? []) : [];
  return {...d, interfaces: bases.map(symbol => ({symbol,external:(symbolFiles.get(symbol) ?? []).filter(f => f !== d.file && !f.startsWith('PensionMS.Core/Interfaces/'))}))};
});
const inactiveServices = services.filter(d => !d.external.some(f => !f.includes('Tests/')) && d.interfaces.every(i => !i.external.some(f => !f.includes('Tests/'))));

// Similarity candidates use 4 consecutive normalized lines; reported percentages
// are Dice overlap over unique blocks, not proof of interchangeable behavior.
const sets = new Map(); const inverted = new Map();
for (const file of appCode) {
  const lines = texts.get(file).split(/\r?\n/).map(l => l.trim().replace(/\s+/g,' ')).filter(l => l.length > 3 && !/^(?:using |import |\/\/|\*|namespace )/.test(l));
  if (lines.length < 24) continue;
  const blocks = new Set();
  for (let i=0;i+3<lines.length;i++) {const block = lines.slice(i,i+4).join('\n'); if (block.length >= 100) blocks.add(hash(block).slice(0,16));}
  sets.set(file,blocks);
  for (const block of blocks) {if (!inverted.has(block)) inverted.set(block,[]); inverted.get(block).push(file);}
}
const shared = new Map();
for (const group of inverted.values()) {
  if (group.length > 25) continue;
  for (let i=0;i<group.length;i++) for(let j=i+1;j<group.length;j++) {
    if (path.extname(group[i]) !== path.extname(group[j])) continue;
    const key = group[i]+'\0'+group[j]; shared.set(key,(shared.get(key)??0)+1);
  }
}
const similar = [...shared].filter(([,n])=>n>=12).map(([key,common])=>{const pair=key.split('\0');const sizes=pair.map(f=>sets.get(f).size);return {files:pair,common,blocks:sizes,dice:Math.round(20000*common/(sizes[0]+sizes[1]))/100,contained:Math.round(10000*common/Math.min(...sizes))/100};}).filter(p=>p.dice>=30 || p.contained>=65).sort((a,b)=>b.dice-a.dice).slice(0,100);

const publicAssets = files.filter(f => f.startsWith('ui/public/'));
const sourceAssets = files.filter(f => f.startsWith('ui/src/') && /\.(?:png|jpe?g|webp|svg|woff2?|ttf|mp3|pdf)$/.test(f));
function findAssetRefs(file) {
  const name = file.replace(/^ui\/(?:public|src)\//,'');
  const basename = path.posix.basename(name);
  const references = [...texts].filter(([f,t]) => !f.startsWith('docs/') && f !== file && (t.includes(name) || t.includes(basename) || t.includes(encodeURI(name)))).map(([file,t]) => ({file,lines:t.split('\n').flatMap((s,i)=>(s.includes(name)||s.includes(basename)||s.includes(encodeURI(name)))?[i+1]:[])}));
  return {file,bytes:buffers.get(file).length,references};
}
const assetRefs = publicAssets.map(findAssetRefs);
const sourceAssetRefs = sourceAssets.map(findAssetRefs);
const exactFunctionClones = groupBy(topLevelFunctions,f=>f.signature).filter(group=>new Set(group.map(f=>f.file)).size>1);
const write = (name, data) => fs.writeFileSync(path.join(out,name), JSON.stringify(data,null,2)+'\n');
const artifacts = ['ui/.debug-capture/', 'PensionMS.API/publish/'].map(prefix => {const group=listed.filter(f=>f.startsWith(prefix)&&fs.existsSync(f)&&fs.statSync(f).isFile());return {prefix,files:group.length,bytes:group.reduce((n,f)=>n+fs.statSync(f).size,0)};});
write('inventory.json',{method:'git ls-files --cached --others --exclude-standard; current existing files; generated/tool/plan/tmp folders excluded',files,artifacts,appCode:appCode.length,counts:Object.fromEntries([...new Set(files.map(f=>f.split('/')[0]))].map(dir=>[dir,files.filter(f=>f.split('/')[0]===dir).length]))});
write('duplicates.json',{exact,normalized,similar,exactFunctionClones});
write('frontend.json',{runtimeRoots,testRoots,configRoots,moduleCount:modules.length,sourceCount:source.length,runtimeCount:source.filter(f=>runtime.has(f)).length,dead,testOnly,nonLiteralDynamic,unresolvedLocal,unimportedDependencies,assetRefs,sourceAssetRefs,specifiers});
write('backend.json',{fileCount:cs.length,noExternalSymbols,inactiveServices,services});
textSummary();
function textSummary(){console.log(JSON.stringify({files:files.length,appCode:appCode.length,exactGroups:exact.length,normalizedGroups:normalized.length,similar:similar.length,frontendSource:source.length,unreachable:dead.length,testOnly,nonLiteralDynamic,unresolvedLocal,unimportedDependencies,inactiveServices:inactiveServices.map(d=>({file:d.file,symbol:d.symbol})),noExternalSymbols:noExternalSymbols.length},null,2));}
