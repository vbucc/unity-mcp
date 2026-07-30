"""End-to-end-ish smoke tests for transport routing paths."""
from __future__ import annotations

import pytest

from core.config import config
from transport import unity_transport


@pytest.mark.asyncio
async def test_http_local_smoke(monkeypatch):
    """HTTP local should route through PluginHub without requiring user_id."""
    monkeypatch.setattr(config, "http_remote_hosted", False)

    async def fake_send_command_for_instance(_instance, _command, _params, **_kwargs):
        return {"status": "success", "result": {"message": "ok", "data": {"via": "http"}}}

    monkeypatch.setattr(
        unity_transport.PluginHub,
        "send_command_for_instance",
        fake_send_command_for_instance,
    )

    result = await unity_transport.send_with_unity_instance(
        None, "ping", {}
    )

    assert result["success"] is True
    assert result["data"] == {"via": "http"}


@pytest.mark.asyncio
async def test_http_remote_smoke(monkeypatch):
    """HTTP remote-hosted should route through PluginHub when user_id is provided."""
    monkeypatch.setattr(config, "http_remote_hosted", True)

    async def fake_send_command_for_instance(_instance, _command, _params, **_kwargs):
        return {"status": "success", "result": {"data": {"via": "http-remote"}}}

    monkeypatch.setattr(
        unity_transport.PluginHub,
        "send_command_for_instance",
        fake_send_command_for_instance,
    )

    result = await unity_transport.send_with_unity_instance(
        None, "ping", {}, user_id="user-1"
    )

    assert result["success"] is True
    assert result["data"] == {"via": "http-remote"}


@pytest.mark.asyncio
async def test_http_forwards_retry_on_reload(monkeypatch):
    """HTTP transport should pass retry_on_reload through to PluginHub."""
    monkeypatch.setattr(config, "http_remote_hosted", False)

    captured: dict[str, object] = {}

    async def fake_send_command_for_instance(_instance, _command, _params, **kwargs):
        captured.update(kwargs)
        return {"status": "success", "result": {"data": {"via": "http"}}}

    monkeypatch.setattr(
        unity_transport.PluginHub,
        "send_command_for_instance",
        fake_send_command_for_instance,
    )

    result = await unity_transport.send_with_unity_instance(
        None,
        "manage_script",
        {"action": "edit"},
        retry_on_reload=False,
    )

    assert result["success"] is True
    assert captured.get("retry_on_reload") is False

