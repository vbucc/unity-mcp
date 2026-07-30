import pytest

from .test_helpers import DummyContext, setup_script_tools, patch_script_send


@pytest.mark.asyncio
async def test_validate_script_returns_counts(monkeypatch):
    tools = setup_script_tools()
    validate_script = tools["validate_script"]

    async def fake_send(_unity_instance, cmd, params, **kwargs):
        return {
            "success": True,
            "data": {
                "diagnostics": [
                    {"severity": "warning"},
                    {"severity": "error"},
                    {"severity": "fatal"},
                ]
            },
        }

    patch_script_send(monkeypatch, fake_send)

    resp = await validate_script(DummyContext(), uri="mcpforunity://path/Assets/Scripts/A.cs")
    assert resp == {"success": True, "data": {"warnings": 1, "errors": 2}}
