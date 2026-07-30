"""
Characterization tests for Models & Data Structures domain.

Tests capture CURRENT behavior of models in:
  - Server/src/models/models.py (MCPResponse, ToolParameterModel, ToolDefinitionModel)
  - Server/src/models/unity_response.py (normalize_unity_response function)

Domain Overview:
  - Purpose: Request/response structures, configuration schemas
  - Pattern: Shared data definitions across Python/C# with duplications noted
  - Key Issue: Duplicate session models should be consolidated (PluginSession vs SessionDetails)

These tests verify:
  - Model instantiation with valid/invalid data
  - Serialization and deserialization
  - Validation logic and error messages
  - Default value application
  - Schema consistency
  - Request/response contract verification

DUPLICATION NOTES:
  - NOTE: PluginSession (Python) and SessionDetails (C# likely) represent the same concept
    These should be consolidated in refactor P1-4
  - NOTE: McpClient (C#) has many configuration flags that could be simplified via builder pattern
    This relates to refactor P2-3
"""
import json
import pytest
from datetime import datetime
from typing import Any, Dict

from models.models import (
    MCPResponse,
    ToolParameterModel,
    ToolDefinitionModel,
)
from models.unity_response import normalize_unity_response


class TestMCPResponseModel:
    """Test MCPResponse model instantiation, validation, and serialization."""

    def test_mcp_response_minimal_required_fields(self):
        """Test MCPResponse with only required field (success)."""
        response = MCPResponse(success=True)

        assert response.success is True
        assert response.message is None
        assert response.error is None
        assert response.data is None
        assert response.hint is None

    def test_mcp_response_all_fields(self):
        """Test MCPResponse with all fields specified."""
        response = MCPResponse(
            success=True,
            message="Operation completed successfully",
            error=None,
            data={"key": "value"},
            hint="retry"
        )

        assert response.success is True
        assert response.message == "Operation completed successfully"
        assert response.error is None
        assert response.data == {"key": "value"}
        assert response.hint == "retry"

    def test_mcp_response_success_false_with_error(self):
        """Test MCPResponse with success=False and error message."""
        response = MCPResponse(
            success=False,
            message=None,
            error="Failed to execute command",
            data=None
        )

        assert response.success is False
        assert response.error == "Failed to execute command"
        assert response.message is None

    def test_mcp_response_serialization_to_json(self):
        """Test MCPResponse can be serialized to JSON."""
        response = MCPResponse(
            success=True,
            message="Success",
            data={"count": 5}
        )

        json_str = response.model_dump_json()
        assert isinstance(json_str, str)

        data = json.loads(json_str)
        assert data["success"] is True
        assert data["message"] == "Success"
        assert data["data"]["count"] == 5

    def test_mcp_response_deserialization_from_json(self):
        """Test MCPResponse can be deserialized from JSON."""
        json_str = json.dumps({
            "success": True,
            "message": "All good",
            "error": None,
            "data": {"result": "ok"}
        })

        response = MCPResponse.model_validate_json(json_str)

        assert response.success is True
        assert response.message == "All good"
        assert response.data == {"result": "ok"}

    def test_mcp_response_hint_values(self):
        """Test MCPResponse with various hint values."""
        hints = ["retry", "other_hint", None]

        for hint in hints:
            response = MCPResponse(success=True, hint=hint)
            assert response.hint == hint

    def test_mcp_response_complex_data_structure(self):
        """Test MCPResponse with nested data structures."""
        complex_data = {
            "items": [
                {"id": 1, "name": "Item 1"},
                {"id": 2, "name": "Item 2"}
            ],
            "metadata": {
                "total": 2,
                "page": 1,
                "nested": {
                    "deep": {
                        "value": "here"
                    }
                }
            }
        }

        response = MCPResponse(success=True, data=complex_data)

        assert response.data == complex_data
        json_str = response.model_dump_json()
        restored = MCPResponse.model_validate_json(json_str)
        assert restored.data == complex_data

    @pytest.mark.parametrize("success,message,error", [
        (True, "OK", None),
        (False, None, "Error occurred"),
        (True, "Completed", "Old error"),
        (False, "Message", "Error"),
    ])
    def test_mcp_response_various_combinations(self, success, message, error):
        """Parametrized test for various field combinations."""
        response = MCPResponse(success=success, message=message, error=error)

        assert response.success == success
        assert response.message == message
        assert response.error == error

        # Round-trip through JSON
        json_str = response.model_dump_json()
        restored = MCPResponse.model_validate_json(json_str)
        assert restored.success == success


class TestToolParameterModel:
    """Test ToolParameterModel for parameter schema validation."""

    def test_tool_parameter_minimal(self):
        """Test ToolParameterModel with minimal required fields."""
        param = ToolParameterModel(name="input")

        assert param.name == "input"
        assert param.description is None
        assert param.type == "string"
        assert param.required is True
        assert param.default_value is None

    def test_tool_parameter_full_specification(self):
        """Test ToolParameterModel with all fields specified."""
        param = ToolParameterModel(
            name="count",
            description="Number of items",
            type="integer",
            required=False,
            default_value="10"
        )

        assert param.name == "count"
        assert param.description == "Number of items"
        assert param.type == "integer"
        assert param.required is False
        assert param.default_value == "10"

    def test_tool_parameter_type_defaults_to_string(self):
        """Test that parameter type defaults to 'string'."""
        param = ToolParameterModel(name="text")
        assert param.type == "string"

    def test_tool_parameter_required_defaults_to_true(self):
        """Test that required defaults to True."""
        param = ToolParameterModel(name="mandatory")
        assert param.required is True

    def test_tool_parameter_various_types(self):
        """Test ToolParameterModel with various type specifications."""
        types = ["string", "integer", "float", "boolean", "array", "object"]

        for param_type in types:
            param = ToolParameterModel(name="test", type=param_type)
            assert param.type == param_type

    def test_tool_parameter_serialization(self):
        """Test ToolParameterModel serialization to JSON."""
        param = ToolParameterModel(
            name="search_term",
            description="What to search for",
            type="string",
            required=True
        )

        json_str = param.model_dump_json()
        data = json.loads(json_str)

        assert data["name"] == "search_term"
        assert data["description"] == "What to search for"
        assert data["type"] == "string"
        assert data["required"] is True

    def test_tool_parameter_deserialization(self):
        """Test ToolParameterModel deserialization from JSON."""
        json_str = json.dumps({
            "name": "filepath",
            "description": "Path to file",
            "type": "string",
            "required": True,
            "default_value": None
        })

        param = ToolParameterModel.model_validate_json(json_str)

        assert param.name == "filepath"
        assert param.type == "string"

    def test_tool_parameter_with_default_value(self):
        """Test ToolParameterModel with default values."""
        param = ToolParameterModel(
            name="timeout",
            type="integer",
            required=False,
            default_value="30"
        )

        assert param.default_value == "30"
        assert param.required is False

    @pytest.mark.parametrize("name,param_type,required", [
        ("api_key", "string", True),
        ("limit", "integer", False),
        ("enabled", "boolean", True),
        ("data", "object", False),
        ("items", "array", True),
    ])
    def test_tool_parameter_combinations(self, name, param_type, required):
        """Parametrized test for various parameter specifications."""
        param = ToolParameterModel(
            name=name,
            type=param_type,
            required=required
        )

        assert param.name == name
        assert param.type == param_type
        assert param.required == required


class TestToolDefinitionModel:
    """Test ToolDefinitionModel for tool schema validation."""

    def test_tool_definition_minimal(self):
        """Test ToolDefinitionModel with minimal required fields."""
        tool = ToolDefinitionModel(name="read_file")

        assert tool.name == "read_file"
        assert tool.description is None
        assert tool.structured_output is True
        assert tool.requires_polling is False
        assert tool.poll_action == "status"
        assert tool.parameters == []

    def test_tool_definition_full_specification(self):
        """Test ToolDefinitionModel with all fields specified."""
        params = [
            ToolParameterModel(name="path", type="string", required=True),
            ToolParameterModel(name="encoding", type="string", required=False, default_value="utf-8")
        ]

        tool = ToolDefinitionModel(
            name="read_file",
            description="Read contents of a file",
            structured_output=True,
            requires_polling=False,
            poll_action="status",
            parameters=params
        )

        assert tool.name == "read_file"
        assert tool.description == "Read contents of a file"
        assert len(tool.parameters) == 2
        assert tool.parameters[0].name == "path"

    def test_tool_definition_defaults(self):
        """Test ToolDefinitionModel default values."""
        tool = ToolDefinitionModel(name="test_tool")

        assert tool.structured_output is True
        assert tool.requires_polling is False
        assert tool.poll_action == "status"
        assert tool.parameters == []

    def test_tool_definition_with_polling(self):
        """Test ToolDefinitionModel for tool requiring polling."""
        tool = ToolDefinitionModel(
            name="long_running_task",
            requires_polling=True,
            poll_action="check_progress"
        )

        assert tool.requires_polling is True
        assert tool.poll_action == "check_progress"

    def test_tool_definition_with_many_parameters(self):
        """Test ToolDefinitionModel with multiple parameters."""
        params = [
            ToolParameterModel(name=f"param_{i}", type="string")
            for i in range(5)
        ]

        tool = ToolDefinitionModel(name="complex_tool", parameters=params)

        assert len(tool.parameters) == 5
        assert all(p.name.startswith("param_") for p in tool.parameters)

    def test_tool_definition_serialization(self):
        """Test ToolDefinitionModel serialization to JSON."""
        params = [
            ToolParameterModel(name="input", type="string", required=True),
            ToolParameterModel(name="format", type="string", required=False, default_value="json")
        ]

        tool = ToolDefinitionModel(
            name="process_data",
            description="Process input data",
            parameters=params
        )

        json_str = tool.model_dump_json()
        data = json.loads(json_str)

        assert data["name"] == "process_data"
        assert len(data["parameters"]) == 2
        assert data["parameters"][0]["name"] == "input"

    def test_tool_definition_deserialization(self):
        """Test ToolDefinitionModel deserialization from JSON."""
        json_str = json.dumps({
            "name": "analyze",
            "description": "Analyze data",
            "structured_output": True,
            "requires_polling": False,
            "poll_action": "status",
            "parameters": [
                {
                    "name": "data",
                    "type": "string",
                    "required": True,
                    "default_value": None,
                    "description": None
                }
            ]
        })

        tool = ToolDefinitionModel.model_validate_json(json_str)

        assert tool.name == "analyze"
        assert len(tool.parameters) == 1
        assert tool.parameters[0].name == "data"

    @pytest.mark.parametrize("name,requires_polling,poll_action", [
        ("instant_tool", False, "status"),
        ("async_tool", True, "get_result"),
        ("check_tool", True, "check_status"),
        ("simple", False, "status"),
    ])
    def test_tool_definition_polling_combinations(self, name, requires_polling, poll_action):
        """Parametrized test for polling configurations."""
        tool = ToolDefinitionModel(
            name=name,
            requires_polling=requires_polling,
            poll_action=poll_action
        )

        assert tool.requires_polling == requires_polling
        assert tool.poll_action == poll_action



class TestNormalizeUnityResponse:
    """Test normalize_unity_response function for response normalization."""

    def test_normalize_empty_dict(self):
        """Test normalizing empty dictionary."""
        result = normalize_unity_response({})

        assert result == {}

    def test_normalize_already_normalized_response(self):
        """Test normalizing already MCPResponse-shaped response."""
        response = {
            "success": True,
            "message": "OK",
            "error": None,
            "data": None
        }

        result = normalize_unity_response(response)

        assert result == response
        assert result["success"] is True

    def test_normalize_status_success_response(self):
        """Test normalizing status='success' response."""
        response = {
            "status": "success",
            "result": {
                "message": "Operation succeeded"
            }
        }

        result = normalize_unity_response(response)

        assert result["success"] is True
        assert result["message"] == "Operation succeeded"

    def test_normalize_status_error_response(self):
        """Test normalizing status='error' response."""
        response = {
            "status": "error",
            "result": {
                "error": "Something went wrong"
            }
        }

        result = normalize_unity_response(response)

        assert result["success"] is False
        assert result["error"] == "Something went wrong"

    def test_normalize_with_data_payload(self):
        """Test normalizing response with data in result."""
        response = {
            "status": "success",
            "result": {
                "message": "Retrieved data",
                "data": {"id": 1, "name": "Test"}
            }
        }

        result = normalize_unity_response(response)

        assert result["success"] is True
        assert result["data"]["id"] == 1

    def test_normalize_non_dict_response(self):
        """Test normalizing non-dict response (should pass through)."""
        response = "plain string response"
        result = normalize_unity_response(response)

        assert result == response

    def test_normalize_none_response(self):
        """Test normalizing None response."""
        result = normalize_unity_response(None)
        assert result is None

    def test_normalize_list_response(self):
        """Test normalizing list response (should pass through)."""
        response = [1, 2, 3]
        result = normalize_unity_response(response)

        assert result == response

    def test_normalize_result_with_nested_dict(self):
        """Test normalizing result field containing nested dict."""
        response = {
            "status": "success",
            "result": {
                "message": "Complex result",
                "nested": {
                    "deep": {
                        "value": "found"
                    }
                }
            }
        }

        result = normalize_unity_response(response)

        assert result["success"] is True
        assert result["data"]["nested"]["deep"]["value"] == "found"

    def test_normalize_no_status_no_success_field(self):
        """Test normalizing response with neither status nor success field."""
        response = {
            "id": 123,
            "name": "Some response"
        }

        result = normalize_unity_response(response)

        # Should pass through unchanged
        assert result == response

    def test_normalize_result_field_as_string(self):
        """Test normalizing when result field is a string."""
        response = {
            "status": "success",
            "result": "simple string result",
            "message": "Operation complete"
        }

        result = normalize_unity_response(response)

        assert result["success"] is True
        assert result["message"] == "Operation complete"

    def test_normalize_error_message_fallback(self):
        """Test error message falls back to message field."""
        response = {
            "status": "error",
            "message": "Command failed",
            "result": {}
        }

        result = normalize_unity_response(response)

        assert result["success"] is False
        assert result["error"] == "Command failed"

    def test_normalize_unknown_status(self):
        """Test normalizing response with unknown status."""
        response = {
            "status": "unknown_status",
            "message": "Unclear what happened"
        }

        result = normalize_unity_response(response)

        # Unknown status != "success" so should be failure
        assert result["success"] is False

    def test_normalize_result_none_value(self):
        """Test normalizing when result field is None."""
        response = {
            "status": "success",
            "result": None,
            "message": "OK but no data"
        }

        result = normalize_unity_response(response)

        assert result["success"] is True
        assert result["data"] is None

    def test_normalize_nested_success_in_result(self):
        """Test normalizing when result itself contains 'success' field."""
        response = {
            "status": "pending",
            "result": {
                "success": True,
                "message": "Inner success",
                "data": {"value": 42}
            }
        }

        result = normalize_unity_response(response)

        # Should extract the inner response
        assert result["success"] is True
        assert result["message"] == "Inner success"

    @pytest.mark.parametrize("status,expected_success", [
        ("success", True),
        ("error", False),
        ("failed", False),
        ("pending", False),
        ("completed", False),
    ])
    def test_normalize_status_to_success_mapping(self, status, expected_success):
        """Parametrized test for status to success field mapping."""
        response = {
            "status": status,
            "result": {"message": f"Status is {status}"}
        }

        result = normalize_unity_response(response)

        assert result["success"] == expected_success

    def test_normalize_preserves_extra_fields_in_result(self):
        """Test that extra fields in result are included in data."""
        response = {
            "status": "success",
            "result": {
                "message": "Done",
                "field1": "value1",
                "field2": 123,
                "field3": True
            }
        }

        result = normalize_unity_response(response)

        # Extra fields should be in data
        assert result["data"]["field1"] == "value1"
        assert result["data"]["field2"] == 123
        assert result["data"]["field3"] is True

    def test_normalize_empty_result_dict(self):
        """Test normalizing response with empty result dict."""
        response = {
            "status": "success",
            "result": {}
        }

        result = normalize_unity_response(response)

        assert result["success"] is True
        assert result["data"] is None

    def test_normalize_status_code_excluded_from_data(self):
        """Test that 'code' and 'status' fields are filtered from data."""
        response = {
            "status": "success",
            "result": {
                "message": "OK",
                "code": 200,
                "status": "ok",
                "data": {"actual": "data"}
            }
        }

        result = normalize_unity_response(response)

        # code and status should not appear in data
        assert "code" not in result["data"]
        assert "status" not in result["data"]
        assert result["data"]["actual"] == "data"


class TestModelValidation:
    """Test model validation and error handling."""

    def test_mcp_response_missing_success_field_required(self):
        """Test that MCPResponse requires success field."""
        with pytest.raises(Exception):  # Pydantic ValidationError
            MCPResponse.model_validate({})

    def test_tool_parameter_missing_name_required(self):
        """Test that ToolParameterModel requires name field."""
        with pytest.raises(Exception):
            ToolParameterModel.model_validate({})

    def test_tool_definition_missing_name_required(self):
        """Test that ToolDefinitionModel requires name field."""
        with pytest.raises(Exception):
            ToolDefinitionModel.model_validate({})




class TestSchemaConsistency:
    """Test schema consistency and inter-model contracts."""

    def test_mcp_response_with_tool_definition_as_data(self):
        """Test MCPResponse containing ToolDefinitionModel as data."""
        tool = ToolDefinitionModel(
            name="test_tool",
            description="A test tool"
        )

        response = MCPResponse(
            success=True,
            data={
                "tool": tool.model_dump()
            }
        )

        assert response.data["tool"]["name"] == "test_tool"

    def test_tool_definition_with_all_parameter_types(self):
        """Test ToolDefinitionModel can represent all parameter types."""
        param_types = ["string", "integer", "float", "boolean", "array", "object"]

        params = [
            ToolParameterModel(name=f"param_{i}", type=ptype)
            for i, ptype in enumerate(param_types)
        ]

        tool = ToolDefinitionModel(name="multi_type_tool", parameters=params)

        for i, param in enumerate(tool.parameters):
            assert param.type == param_types[i]



if __name__ == "__main__":
    pytest.main([__file__, "-v"])
