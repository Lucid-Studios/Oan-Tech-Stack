
## Ingesting your gold set (B)
Use the bulk loader to validate and import fragments into `gold/`:

```bash
python -m src.tools.gold_loader --in /path/to/your/fragments_folder --out gold --index index.json
```

- Input accepts a single JSON file or a folder tree; only files matching the schema are ingested.
- An index file is written with counts and any validation errors for quick triage.

## Probe QA (C)
Once your fragments are ingested, you can generate/evaluate basic probe sets:

- Implement/extend probe logic in `src/core/probes.py` (the default is a stub).
- Tie probes into your run pipeline (e.g., compute TC by comparing answers to master gloss / acceptable paraphrases).
