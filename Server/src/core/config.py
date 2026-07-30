"""
Configuration settings for the MCP for Unity Server.
This file contains all configurable parameters for the server.
"""

from dataclasses import dataclass


@dataclass
class ServerConfig:
    """Main configuration class for the MCP server."""

    # HTTP transport behaviour
    http_remote_hosted: bool = False

    # API key authentication (required when http_remote_hosted=True)
    api_key_validation_url: str | None = None  # POST endpoint to validate keys
    api_key_login_url: str | None = None       # URL for users to get/manage keys
    # Cache TTL in seconds (5 min default)
    api_key_cache_ttl: float = 300.0
    # Optional service token for authenticating to the validation endpoint
    api_key_service_token_header: str | None = None  # e.g. "X-Service-Token"
    api_key_service_token: str | None = None         # The token value

    # Logging settings
    log_level: str = "INFO"
    log_format: str = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"

    # Telemetry settings
    telemetry_enabled: bool = True
    # Align with telemetry.py default Cloud Run endpoint
    telemetry_endpoint: str = "https://api-prod.coplay.dev/telemetry/events"


# Create a global config instance
config = ServerConfig()
