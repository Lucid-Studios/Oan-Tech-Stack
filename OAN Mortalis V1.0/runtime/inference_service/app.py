#!/usr/bin/env python3
"""
Windows-native inference service template for CradleTek runtime.

This file is source code and should be copied to
<CRADLETEK_RUNTIME_ROOT>\\runtime\\inference_service\\app.py
by scripts/windows-bootstrap.ps1.
"""

import hashlib
import json
import os
import subprocess
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, Optional

from flask import Flask, jsonify, request

APP = Flask(__name__)

TERMINAL_STATES = {
    "QUERY",
    "NEEDS_MORE_INFORMATION",
    "UNRESOLVED_CONFLICT",
    "REFUSAL",
    "ERROR",
    "COMPLETE",
    "HALT",
}


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def str_hash(value: str) -> str:
    return hashlib.sha256(value.encode("utf-8")).hexdigest()


def runtime_root() -> Path:
    return Path(os.getenv("CRADLETEK_RUNTIME_ROOT", str(Path.home() / "CradleTek")))


def load_config() -> Dict[str, Any]:
    cfg_path = runtime_root() / "runtime" / "config.json"
    if cfg_path.exists():
        with cfg_path.open("r", encoding="utf-8") as handle:
            cfg = json.load(handle)
    else:
        cfg = {}

    cfg.setdefault("model_path", str(runtime_root() / "models" / "seed.gguf"))
    cfg.setdefault("inference_port", 8181)
    cfg.setdefault("max_context", 2048)
    cfg.setdefault("telemetry_enabled", True)

    if os.getenv("OAN_MODEL_PATH"):
        cfg["model_path"] = os.getenv("OAN_MODEL_PATH")
    if os.getenv("SOULFRAME_API_PORT"):
        cfg["inference_port"] = int(os.getenv("SOULFRAME_API_PORT", "8181"))

    return cfg


CONFIG = load_config()


def emit_telemetry(event_type: str, detail: Dict[str, Any]) -> None:
    if not CONFIG.get("telemetry_enabled", True):
        return

    payload = {
        "event_type": event_type,
        "timestamp": utc_now(),
        "event_hash": str_hash(f"{event_type}|{json.dumps(detail, sort_keys=True)}"),
        "detail": detail,
    }

    telemetry_file = runtime_root() / "telemetry" / "inference_events.ndjson"
    telemetry_file.parent.mkdir(parents=True, exist_ok=True)
    with telemetry_file.open("a", encoding="utf-8") as handle:
        handle.write(json.dumps(payload) + "\n")


def find_llama_cli() -> Optional[Path]:
    candidates = [
        runtime_root() / "runtime" / "llama.cpp" / "bin" / "llama-cli.exe",
        runtime_root() / "runtime" / "llama.cpp" / "bin" / "main.exe",
    ]
    for candidate in candidates:
        if candidate.exists():
            return candidate
    return None


def run_llama(prompt: str, max_tokens: int) -> str:
    model_path = Path(CONFIG["model_path"])
    cli = find_llama_cli()
    if cli is None or not model_path.exists():
        return json.dumps(
            {
                "mode": "stub",
                "reason": "llama-cli or model missing",
                "trace": str_hash(prompt)[:16],
            }
        )

    args = [
        str(cli),
        "-m",
        str(model_path),
        "-p",
        prompt,
        "-n",
        str(max_tokens),
        "--temp",
        "0.2",
    ]

    result = subprocess.run(args, capture_output=True, text=True, check=False, timeout=120)
    output = (result.stdout or "").strip()
    if not output:
        output = (result.stderr or "").strip()
    if not output:
        output = json.dumps({"mode": "stub", "trace": str_hash(prompt)[:16]})
    return output[:4000]


def parse_payload() -> Dict[str, Any]:
    data = request.get_json(silent=True) or {}
    task = data.get("task") or "infer"
    context = data.get("context") or data.get("text") or data.get("prompt") or ""
    constraints = data.get("opal_constraints") or {}
    governance_protocol = data.get("governance_protocol") or {}
    max_tokens = int(constraints.get("max_tokens", CONFIG["max_context"]))
    return {
        "task": task,
        "context": context,
        "max_tokens": max_tokens,
        "governance_protocol": governance_protocol,
    }


def choose_governed_state(task: str, context: str, max_tokens: int, require_terminal: bool) -> Dict[str, str]:
    normalized = " ".join(str(context).strip().split())
    lowered = normalized.lower()

    if "ready-check" in lowered and not require_terminal:
        return {
            "state": "READY",
            "trace": "runtime-ready",
            "content": "The governed seed runtime is initialized and able to accept work.",
        }

    if "heartbeat-check" in lowered and not require_terminal:
        return {
            "state": "HEARTBEAT",
            "trace": "still-processing",
            "content": "The governed seed runtime is alive and still processing.",
        }

    if not normalized:
        return {
            "state": "NEEDS_MORE_INFORMATION",
            "trace": "missing-context",
            "content": "No context was supplied for governed inference.",
        }

    if len(normalized.split()) < 3 or "underspecified" in lowered:
        return {
            "state": "NEEDS_MORE_INFORMATION",
            "trace": "underspecified-context",
            "content": "More context is required before a governed response can be emitted.",
        }

    conflict_markers = ("contradict", "conflict", "mutually exclusive", "both true and false")
    if any(marker in lowered for marker in conflict_markers):
        return {
            "state": "UNRESOLVED_CONFLICT",
            "trace": "contradictory-constraints",
            "content": "The request contains incompatible constraints and cannot collapse safely.",
        }

    refusal_markers = ("forbidden", "disallowed", "refuse", "policy violation")
    if any(marker in lowered for marker in refusal_markers):
        return {
            "state": "REFUSAL",
            "trace": "policy-refusal",
            "content": "The runtime refused the request under current admissibility rules.",
        }

    halt_markers = ("halt now", "terminate immediately", "emergency stop")
    if any(marker in lowered for marker in halt_markers):
        return {
            "state": "HALT",
            "trace": "halt-requested",
            "content": "The runtime halted in response to an explicit stop request.",
        }

    body = run_llama(normalized, max(1, min(max_tokens, int(CONFIG["max_context"]))))
    return {
        "state": "QUERY",
        "trace": f"{task}-response-ready",
        "content": body,
    }


def build_governed_response(task: str, context: str, max_tokens: int, protocol: Dict[str, Any]) -> Dict[str, Any]:
    require_terminal = bool(protocol.get("require_terminal_state"))
    selected = choose_governed_state(task, context, max_tokens, require_terminal)
    state = selected["state"]
    allowed_states = {
        str(token).strip().upper()
        for token in protocol.get("allowed_states", [])
        if str(token).strip()
    }

    if allowed_states and state not in allowed_states:
        state = "ERROR"
        selected = {
            "state": "ERROR",
            "trace": f"disallowed-state:{selected['state']}",
            "content": "The runtime selected a state outside the caller's allowed governance surface.",
        }

    if require_terminal and state not in TERMINAL_STATES:
        invalid_state = state
        state = "ERROR"
        selected = {
            "state": "ERROR",
            "trace": f"non-terminal-state:{invalid_state}",
            "content": "The runtime cannot return a non-terminal governed state on a final HTTP response.",
        }

    confidence = 0.70 if state in {"QUERY", "COMPLETE"} else 0.15
    decision = {
        "QUERY": f"{task}-ok",
        "NEEDS_MORE_INFORMATION": "needs-more-information",
        "UNRESOLVED_CONFLICT": "unresolved-conflict",
        "REFUSAL": f"{task}-refused",
        "ERROR": f"{task}-error",
        "COMPLETE": f"{task}-complete",
        "HALT": f"{task}-halted",
    }.get(state, f"{task}-pending")

    return {
        "decision": decision,
        "payload": selected["content"],
        "confidence": confidence,
        "governance": {
            "state": selected["state"],
            "trace": selected["trace"],
            "content": selected["content"],
        },
    }


def handle_inference(default_task: str) -> Any:
    payload = parse_payload()
    task = payload["task"] or default_task
    context = payload["context"]
    max_tokens = max(1, min(payload["max_tokens"], int(CONFIG["max_context"])))
    governance_protocol = payload["governance_protocol"]

    emit_telemetry(
        "InferenceRequested",
        {
            "task": task,
            "max_tokens": max_tokens,
            "governed": bool(governance_protocol),
        },
    )

    if governance_protocol:
        response = build_governed_response(task, context, max_tokens, governance_protocol)
        emit_telemetry(
            "InferenceCompleted",
            {
                "task": task,
                "governed": True,
                "state": response["governance"]["state"],
                "trace": response["governance"]["trace"],
            },
        )
        return jsonify(response)

    body = run_llama(context, max_tokens)
    response = {
        "decision": f"{task}-ok",
        "payload": body,
        "confidence": 0.70,
    }
    emit_telemetry("InferenceCompleted", {"task": task, "governed": False})
    return jsonify(response)


@APP.get("/health")
def health() -> Any:
    return jsonify(
        {
            "status": "ok",
            "time": utc_now(),
            "model_path": CONFIG["model_path"],
            "inference_port": CONFIG["inference_port"],
        }
    )


@APP.post("/infer")
def infer() -> Any:
    return handle_inference("infer")


@APP.post("/classify")
def classify() -> Any:
    return handle_inference("classify")


@APP.post("/semantic_expand")
def semantic_expand() -> Any:
    return handle_inference("semantic_expand")


@APP.post("/embedding")
def embedding() -> Any:
    return handle_inference("embedding")


@APP.post("/vm/spawn")
@APP.post("/vm/pause")
@APP.post("/vm/reset")
@APP.post("/vm/destroy")
@APP.post("/vm/upgrade")
def vm_control() -> Any:
    operation = request.path.rsplit("/", 1)[-1]
    emit_telemetry("InferenceRequested", {"task": f"vm-{operation}"})
    emit_telemetry("InferenceCompleted", {"task": f"vm-{operation}"})
    return jsonify({"accepted": True, "operation": operation})


if __name__ == "__main__":
    host = os.getenv("SOULFRAME_API_HOST", "127.0.0.1")
    port = int(os.getenv("SOULFRAME_API_PORT", str(CONFIG["inference_port"])))
    APP.run(host=host, port=port, debug=False)
