# MCP Server Subsystem

## 📋 Overview

The **MCP Server** (Model Context Protocol) subsystem provides the integrated tool server for the Factorio Architect application. It exposes production chain operations as tools that can be invoked by AI assistants or external systems.

## 🎯 Core Responsibilities

- Tool definition registration (`add_node`, `connect_nodes`, `get_bottlenecks`)
- Tool invocation and parameter validation
- State management for tool calls
- Error handling and response formatting
- Rate limiting and concurrency control

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    MCP Server Layer                       │
├─────────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────────┐   │
│  │                  ToolRegistry                      │   │
│  │  ┌─────────┐ ┌─────────┐ ┌────────────────────┐   │   │
│  │  │ add_node│ │connect_ │ │  get_bottlenecks   │   │   │
│  │  └─────────┘ │ nodes   │ └────────────────────┘   │   │
│  └───────────────────────────────────────────────────┘   │
│           │                                                │
│  ┌───────────────────────────────────────────────────┐   │
│  │                  State Manager                      │   │
│  │  ┌─────────┐ ┌─────────┐ ┌────────────────────┐   │   │
│  │  │  Tools  │ │  State  │ │   Simulation Engine  │   │   │
│  │  │          │ │  Graph  │ │   (Dependency Ref)  │   │   │
│  │  └─────────┘ └─────────┘ └────────────────────┘   │   │
│  └───────────────────────────────────────────────────┘   │
└───────────────────────────────────────────────────────────┘
```

## 📁 File Structure

```
MCPServer/
├── README.md                                        # This file
├── Tools/
│   ├── AddNodeTool.cs
│   ├── ConnectNodesTool.cs
│   ├── GetBottlenecksTool.cs
│   ├── ToolDefinitions.cs
│   └── ToolRegistry.cs
├── State/
│   ├── ToolStateManager.cs
│   ├── ProductionGraph.cs
│   └── SessionStorage.cs
├── Interfaces/
│   ├── ITool.cs
│   ├── IToolRegistry.cs
│   └── IStateManager.cs
├── Services/
│   └── MCPProtocolHandler.cs
├── Models/
│   ├── ToolParameter.cs
│   ├── ToolResponse.cs
│   └── BottleneckResult.cs
└── Tests/
    └── MCPServer.Tests/
```

## 🧪 Gherkin Acceptance Criteria

### Feature: add_node Tool

```gherkin
Feature: add_node Tool
  Background:
    Given the MCP server is running
    And the simulation engine is loaded
    And the tool registry is initialized

  Scenario: Add a production node
    Given parameters:
      | node_id | emoji | label | recipe_id |
      | "cb-rack" | "📟" | "Adv Circuit Board" | "circuit-board" |
    When calling add_node
    Then the node is created in the graph
    And the node appears in the UI
    And parameters are validated

  Scenario: Validate recipe_id
    When calling with invalid recipe_id
    Then the tool returns an error
    And the error message indicates recipe not found
```

### Feature: connect_nodes Tool

```gherkin
Feature: connect_nodes Tool
  Background:
    Given two nodes exist in the graph

  Scenario: Connect nodes with split ratio
    Given parameters:
      | from | to | ratio |
      | "storage" | "crafter" | 1.0 |
    When calling connect_nodes
    Then the connection is created
    And the ratio is stored
    And the UI displays a line between nodes

  Scenario: Validate source exists
    When calling with non-existent source node
    Then the tool returns a validation error
    And suggests creating the source node first

  Scenario: Validate target exists
    When calling with non-existent target node
    Then the tool returns a validation error
    And suggests creating the target node first
```

### Feature: get_bottlenecks Tool

```gherkin
Feature: get_bottlenecks Tool
  Background:
    Given a production chain is simulated

  Scenario: Retrieve bottleneck list
    When calling get_bottlenecks
    Then it returns a list of bottlenecks
    And each bottleneck includes:
      - Machine ID
      - Severity rating
      - Throughput
      - Recommendations
    And bottlenecks are sorted by severity

  Scenario: No bottlenecks
    When the chain is balanced
    Then the tool returns an empty list
    Or returns a status message "No bottlenecks detected"
```

## 🔄 Git Workflow

### Conventional Commits for MCP Changes

```
feat(mcp): add new tool endpoint
fix(mcp): fix parameter validation logic
docs(mcp): document new tool parameters
refactor(mcp): extract tool response model
```

### Branch Strategy

```
main                          # Stable release branch
├── develop                    # Integration branch
│   ├── feature/mcp-1          # New tool features
│   └── hotfix/mcp-1
└── feature/mcp-next          # Next release prep
```

## 📐 Technical Specifications

### Tool Definitions

```json
{
  "add_node": {
    "name": "add_node",
    "description": "Create a new production node in the simulation graph",
    "parameters": {
      "type": "object",
      "properties": {
        "node_id": {
          "type": "string",
          "description": "Unique identifier for the node"
        },
        "emoji": {
          "type": "string",
          "description": "Emoji icon for the node",
          "pattern": "^\\P{L}\\P{L}*$"
        },
        "label": {
          "type": "string",
          "description": "Display label for the node"
        },
        "recipe_id": {
          "type": "string",
          "description": "Recipe identifier from the database"
        }
      },
      "required": ["node_id", "emoji", "label", "recipe_id"]
    }
  },
  "connect_nodes": {
    "name": "connect_nodes",
    "description": "Connect two nodes with a production line",
    "parameters": {
      "type": "object",
      "properties": {
        "from": {
          "type": "string",
          "description": "Source node ID"
        },
        "to": {
          "type": "string",
          "description": "Target node ID"
        },
        "ratio": {
          "type": "number",
          "description": "Production ratio (amount per 1 item)",
          "minimum": 0.0,
          "maximum": 100.0
        }
      },
      "required": ["from", "to", "ratio"]
    }
  },
  "get_bottlenecks": {
    "name": "get_bottlenecks",
    "description": "Analyze and return bottleneck information",
    "parameters": {
      "type": "object",
      "properties": {},
      "required": []
    }
  }
}
```

### Tool Response Format

```csharp
public class ToolResponse
{
    public string ToolName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    // For bottleneck results only
    public List<BottleneckInfo>? Bottlenecks { get; set; }
    
    // For node/connection ops
    public List<string>? CreatedItems { get; set; }
    
    public long Timestamp { get; set; }
}

public class BottleneckInfo
{
    public string MachineId { get; set; }
    public BottleneckSeverity Severity { get; set; }
    public double CurrentThroughput { get; set; }
    public double RequiredThroughput { get; set; }
    public double Efficiency { get; set; }
    public List<string> Recommendations { get; set; }
}
```

### Parameter Validation

```csharp
public class ToolValidator
{
    public bool ValidateAddNodeParameters(object parameters)
    {
        var dict = parameters as NSDictionary;
        
        if (dict == null) return false;
        
        // Validate node_id
        if (!string.IsNullOrEmpty(dict[node_id] as string))
            return true;
            
        // Validate emoji format
        var emoji = dict[emoji] as string;
        if (!Regex.IsMatch(emoji, @"^\P{L}\P{L}*"))
            return false;
            
        // Validate label
        if (string.IsNullOrEmpty(dict[label] as string))
            return false;
            
        // Validate recipe_id exists
        return recipeDatabase.Contains(dict[recipe_id] as string);
    }
}
```

## 🔧 Configuration

### MCPServer.json

```json
{
  "MCPServer": {
    "Port": 3000,
    "RateLimitPerMinute": 60,
    "TimeoutSeconds": 30,
    "MaxConcurrentRequests": 5,
    "ValidationLevel": "Strict",
    "EnableDebugLogging": true
  },
  "Tools": {
    "add_node": true,
    "connect_nodes": true,
    "get_bottlenecks": true
  }
}
```

## 🧰 Dependencies

- Microsoft.McpSharp (or custom implementation)
- System.Text.Json (serialization)
- Validation libraries (FluentValidation)

## 🔐 Security

- Tool parameter sanitization
- No external network calls (offline)
- Rate limiting prevents DoS
- Input validation prevents injection

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.1.0-alpha | 2024-01 | Initial tool definitions |

---

**Owner**: Developer Team  
**Review By**: Product Owner  
**Last Updated**: 2024-01  
