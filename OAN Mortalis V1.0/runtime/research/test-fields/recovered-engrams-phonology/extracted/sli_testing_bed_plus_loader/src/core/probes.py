import json, re
from typing import Dict, List
from ..core.schema import Fragment

# Simple template-based probes; replace/extend with LLM calls as needed.
DEFAULT_PROBES = [
  {"id":"probe_leap", "question":"State the leap of the argument in one short sentence."},
  {"id":"probe_inversion", "question":"Which line performs the inversion and how does it reframe the origin?"},
  {"id":"probe_gloss", "question":"Paraphrase the master gloss in 12 words or fewer."}
]

def build_probe_set(fragment: Fragment) -> List[Dict]:
    return DEFAULT_PROBES

def evaluate_stub(fragment: Fragment, answers: Dict[str,str]) -> Dict[str,float]:
    # Very light heuristic scoring against fragment text / master_gloss
    scores = {}
    gloss = fragment.master_gloss.lower()
    for pid, ans in answers.items():
        a = (ans or "").lower()
        # lexical overlap proxy
        overlap = len(set(a.split()) & set(gloss.split())) / max(1, len(set(gloss.split())))
        scores[pid] = round(overlap, 3)
    return scores
