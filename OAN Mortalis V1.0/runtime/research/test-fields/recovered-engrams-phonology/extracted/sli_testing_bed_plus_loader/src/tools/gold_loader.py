import argparse, json, sys
from pathlib import Path
from typing import List
from ..core.schema import validate_fragment

def discover_files(path: Path) -> List[Path]:
    if path.is_file() and path.suffix.lower()==".json":
        return [path]
    return sorted([p for p in path.rglob("*.json") if p.is_file()])

def main():
    ap = argparse.ArgumentParser(description="Bulk ingest & validate gold fragments")
    ap.add_argument("--in", dest="inp", required=True, help="Input file or folder with JSON fragments")
    ap.add_argument("--out", dest="out", required=True, help="Output folder (gold/)")
    ap.add_argument("--index", dest="index", default="index.json", help="Index filename to write")
    args = ap.parse_args()

    inp = Path(args.inp)
    out = Path(args.out); out.mkdir(parents=True, exist_ok=True)
    files = discover_files(inp)
    if not files:
        print("No JSON fragments found.", file=sys.stderr); sys.exit(2)

    index = []
    errors = []
    for fp in files:
        try:
            obj = json.loads(fp.read_text())
            fr = validate_fragment(obj)
            # write normalized copy into out/
            out_fp = out / f"{fr.id}.json"
            out_fp.write_text(json.dumps(obj, indent=2, ensure_ascii=False))
            index.append({"id": fr.id, "title": fr.title, "path": str(out_fp.name), "nodes": len(fr.nodes)})
        except Exception as e:
            errors.append({"file": str(fp), "error": str(e)})
            continue

    (out/args.index).write_text(json.dumps({"fragments": index, "errors": errors}, indent=2, ensure_ascii=False))

    print(f"Ingested {len(index)} fragment(s) with {len(errors)} error(s).")
    if errors:
        print("Errors:")
        for e in errors:
            print(f" - {e['file']}: {e['error']}")

if __name__ == "__main__":
    main()
