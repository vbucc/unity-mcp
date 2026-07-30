# Deterministic bridge E2E

`bridge_smoke.py` drives a **live Unity Editor** through a running MCP server's
`POST /api/command` route — the same PluginHub path the MCP tools use — with a
fixed sequence of tool calls and exact assertions. It is deterministic and free
(no Anthropic API key), so it can gate changes to the Python↔C# contract.

It is **not** collected by `pytest tests/` (the filename is not `test_*.py`), so
the normal unit suite never tries to reach a Unity instance.

## Run locally

Normally you do not run this directly — `tools/local_harness.py --legs smoke`
starts a server on its own port, boots Unity against it, and invokes this driver
with the right `--base-url` and `--instance`.

To run it by hand against a server you already have:

```bash
cd Server
uv run python tests/e2e/bridge_smoke.py --base-url http://127.0.0.1:8123
uv run python tests/e2e/bridge_smoke.py --base-url http://127.0.0.1:8123 --instance MyProject@<hash>
```

`--base-url` defaults to `$UNITY_MCP_HTTP_URL`, else `http://127.0.0.1:8080`.
Pointing it at `8080` targets your everyday server — fine for a read-only poke,
but note the steps create and delete GameObjects in the open scene.

Exit codes: `0` all steps passed · `1` a step failed an assertion (real bridge
regression) · `2` no Unity bridge reachable (setup problem, not a contract bug).

## CI

This fork has no Unity license in CI, so the smoke leg runs locally through
`tools/local_harness.py` (which `/land` invokes as the C# gate). See
`CLAUDE.md` → "Local headless test harness".

## Adding steps

Append a `Step(...)` in `build_steps()` with a `check(resp)` callback that raises
`AssertionError` on failure. Use `_ok()` / `_result()` to stay tolerant of both
Unity response shapes. Keep new objects uniquely named (see the `_RUN` suffix) and
delete anything you create so reruns stay clean.
