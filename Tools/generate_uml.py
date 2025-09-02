#!/usr/bin/env python3
import os
import re
import sys
from typing import Dict, List, Set, Tuple

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
OUT_PATH = os.path.join(ROOT, 'ClassDiagram.puml')

# Regex patterns
namespace_re = re.compile(r"^\s*namespace\s+([\w\.]+)\s*(?:\{|;)\s*$")
# captures: kind, name, bases (optional)
type_re = re.compile(r"^\s*(?:public|internal|protected|private)?\s*(?:abstract|static|sealed|partial\s+)*\s*(class|interface|record)\s+(?<name>[A-Za-z_][\w]*)\s*(?::\s*(?<bases>[^\{]+))?\s*\{?")
# Python doesn't support .NET named groups in this style; fix with indices below

type_re = re.compile(r"^\s*(?:public|internal|protected|private)?\s*(?:abstract|static|sealed|partial\s+)*\s*(class|interface|record)\s+([A-Za-z_][\w]*)\s*(?::\s*([^\{]+))?\s*\{?")
member_re = re.compile(r"^\s*(?:public|internal|protected|private)\s+(?:static\s+)?([A-Za-z_][\w\.<>,\[\]\? ]*)\s+([A-Za-z_][\w]*)\s*(?:\{|=|;)")
property_re = re.compile(r"^\s*(?:public|internal|protected|private)\s+(?:static\s+)?([A-Za-z_][\w\.<>,\[\]\? ]*)\s+([A-Za-z_][\w]*)\s*\{[\s\w;=]*\}")
using_alias_re = re.compile(r"^\s*using\s+([\w\.]+)\s*=\s*([\w\.]+)\s*;\s*$")
comment_line_re = re.compile(r"^\s*//")

# Helpers to clean type names
GENERIC_INNER_RE = re.compile(r"[A-Za-z_][\w\.]*\s*<([^>]+)>")
ARRAY_RE = re.compile(r"(.+)\[\s*\]")

PRIMITIVES = {
    'bool','byte','sbyte','char','decimal','double','float','int','uint','nint','nuint','long','ulong','short','ushort','string','object','void','DateTime','TimeSpan'
}

class TypeInfo:
    def __init__(self, kind: str, name: str, namespace: str, bases: List[str]):
        self.kind = kind  # class/interface/record
        self.name = name
        self.namespace = namespace
        self.fqn = f"{namespace}.{name}" if namespace else name
        self.bases = [b.strip() for b in bases if b.strip()]
        self.members: List[Tuple[str, str]] = []  # (name, type)

    @property
    def plant_kind(self) -> str:
        if self.kind == 'interface':
            return 'interface'
        return 'class'


def norm_type(t: str) -> str:
    t = t.strip()
    # Remove nullable ?
    if t.endswith('?'):
        t = t[:-1]
    # Remove ref/out/in modifiers if present (rare in fields)
    t = re.sub(r"^(ref|out|in)\s+", "", t)
    # Handle arrays
    m = ARRAY_RE.match(t)
    if m:
        t = m.group(1).strip()
    # Strip generic wrappers like List<T>, Dictionary<K,V>
    # Return first inner type for associations; keep outer for display
    # We'll return the cleaned outer for display elsewhere; here we want inner candidates
    return t


def extract_candidate_types(t: str) -> List[str]:
    t = t.strip()
    # For generic types, collect all inner type parameters as candidates
    candidates: List[str] = []
    # Recurse through nested generics by splitting commas at top level
    def split_generic_args(s: str) -> List[str]:
        parts = []
        depth = 0
        cur = []
        for ch in s:
            if ch == '<':
                depth += 1
                cur.append(ch)
            elif ch == '>':
                depth -= 1
                cur.append(ch)
            elif ch == ',' and depth == 0:
                parts.append(''.join(cur).strip())
                cur = []
            else:
                cur.append(ch)
        if cur:
            parts.append(''.join(cur).strip())
        return parts

    # Find all generic wrappers and their inner types
    while True:
        m = GENERIC_INNER_RE.search(t)
        if not m:
            break
        inner = m.group(1)
        for arg in split_generic_args(inner):
            candidates.extend(extract_candidate_types(arg))
        # Strip one level to continue
        t = GENERIC_INNER_RE.sub('T', t, count=1)
    # After removing generics, handle arrays and nullable
    t = norm_type(t)
    # Remove namespaces for later resolution? Keep as is; we'll try both
    candidates.append(t)
    return [c.strip() for c in candidates if c.strip()]


def is_primitive(t: str) -> bool:
    base = t.split('.')[-1]
    # Nullable<T> -> T
    if base.startswith('Nullable'):
        return True
    return base in PRIMITIVES


def parse_cs_file(path: str) -> Tuple[str, List[TypeInfo]]:
    namespace = ''
    types: List[TypeInfo] = []
    cur: TypeInfo | None = None

    try:
        with open(path, 'r', encoding='utf-8') as f:
            for line in f:
                if comment_line_re.match(line):
                    continue
                m = namespace_re.match(line)
                if m:
                    namespace = m.group(1)
                    continue
                m = type_re.match(line)
                if m:
                    kind, name, bases = m.group(1), m.group(2), m.group(3) or ''
                    base_list = [b.strip() for b in bases.split(',')] if bases else []
                    cur = TypeInfo(kind, name, namespace, base_list)
                    types.append(cur)
                    continue
                if cur is not None:
                    # naive scope end detection
                    if line.strip().startswith('}'):  # may end type or namespace; good enough
                        # can't reliably detect which, but ok
                        pass
                    # Members
                    pm = property_re.match(line) or member_re.match(line)
                    if pm:
                        ttype, mname = pm.group(1), pm.group(2)
                        cur.members.append((mname, ttype.strip()))
    except Exception as e:
        print(f"WARN: failed to parse {path}: {e}", file=sys.stderr)
    return namespace, types


def walk_cs_files(root: str) -> List[str]:
    files = []
    for dirpath, dirnames, filenames in os.walk(root):
        # prune
        pruned = []
        for d in list(dirnames):
            if d.lower() in {'bin', 'obj', '.git', '.idea', '.vs'}:
                dirnames.remove(d)
        for f in filenames:
            if f.endswith('.cs'):
                files.append(os.path.join(dirpath, f))
    return files


def build_model(files: List[str]):
    types: Dict[str, TypeInfo] = {}
    by_simple: Dict[str, Set[str]] = {}
    for path in files:
        _, tlist = parse_cs_file(path)
        for t in tlist:
            types[t.fqn] = t
            by_simple.setdefault(t.name, set()).add(t.fqn)
    return types, by_simple


def resolve_type(name: str, by_simple: Dict[str, Set[str]], types: Dict[str, TypeInfo]) -> List[str]:
    # Try as FQN
    if name in types:
        return [name]
    # Try by simple name
    simple = name.split('.')[-1]
    cands = sorted(by_simple.get(simple, []))
    return cands


def generate_plantuml(types: Dict[str, TypeInfo], by_simple: Dict[str, Set[str]]) -> str:
    lines: List[str] = []
    lines.append('@startuml')
    lines.append('set namespaceSeparator none')
    lines.append('hide empty members')
    lines.append('skinparam classAttributeIconSize 0')

    # Group by namespace
    by_ns: Dict[str, List[TypeInfo]] = {}
    for t in types.values():
        by_ns.setdefault(t.namespace, []).append(t)

    # Emit packages and classes
    for ns, tlist in sorted(by_ns.items(), key=lambda x: x[0]):
        pkg = ns or 'Global'
        lines.append(f'package "{pkg}" {{')
        for t in sorted(tlist, key=lambda x: x.name):
            stereotype = ' <<record>>' if t.kind == 'record' else ''
            lines.append(f'  {t.plant_kind} "{t.name}" as {t.fqn.replace(".", "_")}{stereotype} {{')
            # show members (public only guess)
            shown = 0
            for mname, mtype in t.members[:10]:  # cap to keep diagram readable
                lines.append(f'    + {mname} : {mtype}')
                shown += 1
            if len(t.members) > shown:
                lines.append('    .. (more) ..')
            lines.append('  }')
        lines.append('}')

    # Relationships
    rels: Set[Tuple[str, str, str]] = set()  # (src, arrow, dst)

    for t in types.values():
        src = t.fqn.replace('.', '_')
        # Inheritance / interfaces
        for b in t.bases:
            b_clean = b.strip().split('<')[0].strip()
            if not b_clean:
                continue
            targets = resolve_type(b_clean, by_simple, types)
            for dst_fqn in targets:
                arrow = '--|>' if types[dst_fqn].kind != 'interface' else '..|>'
                dst = dst_fqn.replace('.', '_')
                rels.add((src, arrow, dst))
        # Associations via member types
        for mname, mtype in t.members:
            for cand in extract_candidate_types(mtype):
                if is_primitive(cand):
                    continue
                # Try to resolve
                targets = resolve_type(cand, by_simple, types)
                for dst_fqn in targets:
                    dst = dst_fqn.replace('.', '_')
                    rels.add((src, '-->', dst))

    # Emit relationships
    for src, arrow, dst in sorted(rels):
        if src == dst:
            continue
        lines.append(f'{src} {arrow} {dst}')

    lines.append('@enduml')
    return '\n'.join(lines)


def main():
    root = ROOT
    files = walk_cs_files(root)
    if not files:
        print('No C# files found.', file=sys.stderr)
        return 2
    types, by_simple = build_model(files)
    uml = generate_plantuml(types, by_simple)
    with open(OUT_PATH, 'w', encoding='utf-8') as f:
        f.write(uml)
    print(f'Wrote {OUT_PATH} with {len(types)} types.')
    return 0

if __name__ == '__main__':
    sys.exit(main())

