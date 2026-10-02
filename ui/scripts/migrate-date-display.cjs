const fs = require('node:fs');
const path = require('node:path');
const ts = require('typescript');
for(const f of fs.readdirSync('src',{recursive:true}).filter(f=>/\.tsx?$/.test(f) && !f.startsWith('lib'+path.sep))) {
  const p=path.join('src',f), source=fs.readFileSync(p,'utf8');
  const ast=ts.createSourceFile(p,source,ts.ScriptTarget.Latest,true,p.endsWith('tsx')?ts.ScriptKind.TSX:ts.ScriptKind.TS);
  const edits=[], imports=new Set();
  function visit(n) {
    if(ts.isCallExpression(n) && ts.isPropertyAccessExpression(n.expression)) {
      const receiver=n.expression.expression, method=n.expression.name.text;
      if(ts.isNewExpression(receiver) && receiver.expression.getText(ast)==='Date') {
        const args=receiver.arguments || [];
        if(['toLocaleDateString','toLocaleTimeString','toLocaleString'].includes(method)) {
          const helper=method==='toLocaleDateString'?'formatYemenDate':'formatYemenDateTime';
          const opts=n.arguments[1]?.getText(ast);
          const value=args.length?args[0].getText(ast):'new Date()';
          edits.push({start:n.getStart(ast),end:n.end,text:`${helper}(${value}${opts?', '+opts:method==='toLocaleTimeString'?', { hour: "2-digit", minute: "2-digit" }':''})`});
          imports.add(helper); return;
        }
        if(method==='toISOString' && args.length===1 && /(?:birth|death|issue|expiry|dateOfBirth|retirement|enrollment|serviceStart|serviceEnd)Date|dateOfBirth/i.test(args[0].getText(ast))) {
          edits.push({start:n.getStart(ast),end:n.end,text:`normalizeDateInputValue(${args[0].getText(ast)})`});
          imports.add('normalizeDateInputValue'); return;
        }
      }
    }
    ts.forEachChild(n,visit);
  }
  visit(ast);
  let s=source;
  for(const e of edits.sort((a,b)=>b.start-a.start))s=s.slice(0,e.start)+e.text+s.slice(e.end);
  s=s.replace(/const d = new Date\((\w+\.dateOfBirth)\);\s*if \(!isNaN\(d.getTime\(\)\)\) \{\s*formData.append\("dateOfBirth", d.toISOString\(\)\);\s*\} else \{\s*formData.append\("dateOfBirth", \1\);\s*\}/g, (_,value)=> {
    imports.add('normalizeDateInputValue'); return `formData.append("dateOfBirth", normalizeDateInputValue(${value}));`;
  });
  if(/PaymentBooksTab|PaymentCardsPage|FinancialSection/.test(f)) s=s.replace(/: new Date\(\).toISOString\(\)/g,()=>{imports.add('todayYemen');return ': todayYemen()';});
  if(s===source)continue;
  for(const imp of ast.statements.filter(ts.isImportDeclaration)) {
    if(imp.moduleSpecifier.text==='@/lib/date' && imp.importClause?.namedBindings && ts.isNamedImports(imp.importClause.namedBindings)) {
      for(const el of imp.importClause.namedBindings.elements) imports.delete(el.name.text);
    }
  }
  if(imports.size)s=`import { ${[...imports].join(', ')} } from "@/lib/date";\n`+s;
  fs.writeFileSync(p,s);
}
