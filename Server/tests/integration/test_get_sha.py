import pytest

from .test_helpers import DummyContext, setup_script_tools, patch_script_send


@pytest.mark.asyncio
async def test_get_sha_param_shape_and_routing(monkeypatch):
    tools = setup_script_tools()
    get_sha = tools["get_sha"]

    captured = {}

    async def fake_send(_unity_instance, cmd, params, **kwargs):
        captured["cmd"] = cmd
        captured["params"] = params
        return {"success": True, "data": {"sha256": "abc", "lengthBytes": 1, "lastModifiedUtc": "2020-01-01T00:00:00Z", "uri": "mcpforunity://path/Assets/Scripts/A.cs", "path": "Assets/Scripts/A.cs"}}

    patch_script_send(monkeypatch, fake_send)

    resp = await get_sha(DummyContext(), uri="mcpforunity://path/Assets/Scripts/A.cs")
    assert captured["cmd"] == "manage_script"
    assert captured["params"]["action"] == "get_sha"
    assert captured["params"]["name"] == "A"
    assert captured["params"]["path"].endswith("Assets/Scripts")
    assert resp["success"] is True
    assert resp["data"] == {"sha256": "abc", "lengthBytes": 1}
