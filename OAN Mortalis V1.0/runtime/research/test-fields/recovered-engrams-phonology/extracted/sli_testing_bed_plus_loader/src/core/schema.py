from dataclasses import dataclass, field
from typing import List, Literal, Dict, Any
import json

Role = Literal["origin","bridge","inversion","leap","reflection"]

@dataclass
class Node:
    id: str
    text: str
    role: Role
    critical: bool = False
    deps: List[str] = field(default_factory=list)

@dataclass
class Fragment:
    id: str
    title: str
    nodes: List[Node]
    master_gloss: str
    acceptable_paraphrases: List[str] = field(default_factory=list)

def validate_fragment(obj: Dict[str, Any]) -> Fragment:
    required = ["id","title","nodes","master_gloss"]
    for k in required:
        if k not in obj:
            raise ValueError(f"Missing key: {k}")
    nodes = []
    for i, n in enumerate(obj["nodes"]):
        for key in ["id","text","role"]:
            if key not in n:
                raise ValueError(f"Node {i} missing '{key}'")
        role = n["role"]
        if role not in ["origin","bridge","inversion","leap","reflection"]:
            raise ValueError(f"Node {n['id']} invalid role: {role}")
        deps = n.get("deps",[])
        if not isinstance(deps, list):
            raise ValueError(f"Node {n['id']} deps must be a list")
        nodes.append(Node(id=n["id"], text=n["text"], role=role,
                          critical=bool(n.get("critical", False)),
                          deps=deps))
    # sanity: dep ids must appear among node ids
    node_ids = {n.id for n in nodes}
    for n in nodes:
        for d in n.deps:
            if d not in node_ids:
                raise ValueError(f"Node {n.id} depends on missing id {d}")
    return Fragment(
        id=obj["id"],
        title=obj["title"],
        nodes=nodes,
        master_gloss=obj["master_gloss"],
        acceptable_paraphrases=obj.get("acceptable_paraphrases", [])
    )
