import pytest
import sys
import types
from types import SimpleNamespace

from .test_helpers import DummyContext
from core.config import config


class DummyMiddlewareContext:
    def __init__(self, ctx):
        self.fastmcp_context = ctx


@pytest.mark.asyncio
async def test_auto_selects_single_instance_via_pluginhub(monkeypatch):
    plugin_hub = types.ModuleType("transport.plugin_hub")

    class PluginHub:
        @classmethod
        def is_configured(cls) -> bool:
            return True

        @classmethod
        async def get_sessions(cls):
            raise AssertionError("get_sessions should be stubbed in test")

    plugin_hub.PluginHub = PluginHub
    monkeypatch.setitem(sys.modules, "transport.plugin_hub", plugin_hub)
    monkeypatch.delitem(sys.modules, "transport.unity_instance_middleware", raising=False)

    from transport.unity_instance_middleware import UnityInstanceMiddleware, PluginHub as ImportedPluginHub
    assert ImportedPluginHub is plugin_hub.PluginHub


    middleware = UnityInstanceMiddleware()
    ctx = DummyContext()
    ctx.client_id = "client-1"
    middleware_context = DummyMiddlewareContext(ctx)

    call_count = {"sessions": 0}

    async def fake_get_sessions():
        call_count["sessions"] += 1
        return SimpleNamespace(
            sessions={
                "session-1": SimpleNamespace(project="Ramble", hash="deadbeef"),
            }
        )

    monkeypatch.setattr(plugin_hub.PluginHub, "get_sessions", fake_get_sessions)

    selected = await middleware._maybe_autoselect_instance(ctx)

    assert selected == "Ramble@deadbeef"
    assert await middleware.get_active_instance(ctx) == "Ramble@deadbeef"
    assert call_count["sessions"] == 1

    await middleware._inject_unity_instance(middleware_context)

    assert await ctx.get_state("unity_instance") == "Ramble@deadbeef"
    assert call_count["sessions"] == 1



