from typing import Any
from datetime import datetime
from pydantic import BaseModel, Field


class MCPResponse(BaseModel):
    success: bool
    message: str | None = None
    error: str | None = None
    data: Any | None = None
    # Optional hint for clients about how to handle the response.
    # Supported values:
    #   - "retry": Unity is temporarily reloading; call should be retried politely.
    hint: str | None = None


class ToolParameterModel(BaseModel):
    name: str
    description: str | None = None
    type: str = Field(default="string")
    required: bool = Field(default=True)
    default_value: str | None = None


class ToolDefinitionModel(BaseModel):
    name: str
    description: str | None = None
    structured_output: bool | None = True
    requires_polling: bool | None = False
    poll_action: str | None = "status"
    max_poll_seconds: int = 0
    parameters: list[ToolParameterModel] = Field(default_factory=list)

