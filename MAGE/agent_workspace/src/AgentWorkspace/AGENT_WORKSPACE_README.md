# Agent Workspace - AI Integration Subsystem

## 🤖 Overview

The **Agent Workspace** is the AI integration layer that enables Model Context Protocol (MCP) communication with AI agents. It provides the interface for AI-driven factory planning and decision-making.

### Core Components

```
┌─────────────────────────────────────────────────┐
│              Agent Workspace                     │
├─────────────────────────────────────────────────┤
│  [MCP Integration]                               │
│    ├─ Tool Registration                          │
│    ├─ Context Management                         │
│    └─ Response Handling                          │
├─────────────────────────────────────────────────┤
│  [Agent Controller]                              │
│    ├─ State Machine                               │
│    ├─ Task Management                            │
│    └─ Decision Logging                           │
├─────────────────────────────────────────────────┤
│  [Tool Definitions]                              │
│    ├─ Node Operations                             │
│    ├─ Connection Management                       │
│    ├─ Bottleneck Analysis                         │
│    └─ Visualization                              │
└─────────────────────────────────────────────────┘
```

---

## 🎯 MCP Tools Specification

### 1. `add_node()` - Create Factory Nodes

```csharp
public async Task<NodeResult> AddNodeAsync(NodeCreateRequest request)
{
    // Creates a new machine placement on canvas
    return await _agent.ExecuteToolAsync("add_node", request);
}
```

**Request Parameters:**
- `nodeType`: Machine type (e.g., "assembling-machine-1")
- `position`: `{ x, y }` canvas coordinates
- `configuration`: Machine settings (speed, belts, etc.)
- `rotation`: Rotation angle in degrees

**Response Schema:**
```json
{
  "nodeId": "string",
  "success": boolean,
  "message": "string",
  "suggestedConnections": [NodeReference]
}
```

### 2. `connect_nodes()` - Create Data Flows

```csharp
public async Task<ConnectionResult> ConnectNodesAsync(ConnectionCreateRequest request)
{
    // Creates connection between nodes
    return await _agent.ExecuteToolAsync("connect_nodes", request);
}
```

**Request Parameters:**
- `sourceNode`: Source node ID
- `targetNode`: Target node ID
- `flowType`: "item", "power", or "circuit"
- `beltCount`: Number of belts (items)
- `wireCount`: Number of wires (circuits)

**Response Schema:**
```json
{
  "connectionId": "string",
  "success": boolean,
  "throughputLimit": number,
  "message": "string"
}
```

### 3. `get_bottlenecks()` - Analyze Production Limits

```csharp
public async Task<BottleneckReport> AnalyzeBottlenecksAsync(AnalyzeRequest request)
{
    // Identifies bottlenecks in production chain
    return await _agent.ExecuteToolAsync("get_bottlenecks", request);
}
```

**Request Parameters:**
- `scope`: Analysis scope ("entire_chain", "selected_nodes", "full_factory")
- `includePowerAnalysis`: Enable power bottleneck detection
- `includeCircuitAnalysis`: Enable circuit logic bottlenecks
- `benchmarkComparison`: Compare against theoretical maximum

**Response Schema:**
```json
{
  "bottlenecks": [
    {
      "type": "belt_limit" | "power_limit" | "circuit_limit" | "machine_limit",
      "location": string,
      "currentThroughput": number,
      "maxThroughput": number,
      "utilization": number,
      "recommendations": [string]
    }
  ],
  "overallEfficiency": number,
  "totalPower": number,
  "totalItemsPerMinute": number
}
```

### 4. `visualize_throughput()` - Real-time Metrics

```csharp
public async Task<VisualizationData> GetThroughputVisualizationAsync(Request request)
{
    // Generates throughput visualization data
    return await _agent.ExecuteToolAsync("visualize_throughput", request);
}
```

**Request Parameters:**
- `metrics`: Which metrics to visualize
- `timeRange`: Simulation duration
- `granularity`: Data sampling rate
- `chartType`: "line" | "bar" | "stacked"

**Response Schema:**
```json
{
  "timeSeries": [
    {
      "timestamp": number,
      "throughput": number,
      "powerUsed": number,
      "itemsProduced": [
        { "itemName": string, "count": number }
      ]
    }
  ],
  "summary": {
    "averageThroughput": number,
    "peakThroughput": number,
    "totalEnergy": number
  }
}
```

---

## 🧪 Test Cases

### Scenario 1: Basic Node Creation

```gherkin
Scenario: Create single assembly machine
  Given the agent workspace is initialized
  When calling add_node with assembling-machine-1 at position (0, 0)
  Then node should be created with ID "node_1"
  And suggested connections should include outputs

Scenario: Create multi-node line
  Given two assembly machines exist
  When connecting them with 4 belts
  Then connection should succeed
  And throughput limit should be 4 items per tick
```

### Scenario 2: Bottleneck Detection

```gherkin
Scenario: Detect belt bottleneck
  Given a production line with 20 machines
  When analyzing bottlenecks
  Then identify belt limitations
  And recommend additional belts

Scenario: Detect power bottleneck
  Given a high-power production setup
  When running power analysis
  Then identify energy constraints
  And suggest power distribution optimization
```

---

## 📡 Agent Interaction Protocol

### Message Flow

```
1. USER: "Create copper cable factory"
   ↓
2. WORKSPACE: Parse request to task queue
   ↓
3. AGENT: Generate plan using MCP tools
   ↓
4. WORKSPACE: Execute tools and display results
   ↓
5. USER: Review and modify as needed
```

### Context Data Passing

The agent workspace provides context data to AI:

```csharp
public class AgentContext
{
    public List<Node> Machines { get; set; }
    public List<Connection> Connections { get; set; }
    public Dictionary<string, Benchmark> Benchmarks { get; set; }
    public ThroughputCalculator Calculator { get; set; }
    public BottleneckAnalyzer Analyzer { get; set; }
}
```

---

## 🔒 Security & Validation

### Input Validation

All AI requests must pass through the following validations:

1. **Sandbox Check**: Ensure AI stays within configured tools
2. **Tool Signature Verification**: Validate tool parameters
3. **Resource Limits**: Prevent runaway calculations
4. **Output Filtering**: Sanitize AI responses

### Rate Limiting

- AI requests: 10/minute per agent
- Tool invocations: 5/minute total
- Complex calculations: Timeout after 30s

---

## 🔧 Implementation Notes

### Tool Registration

Tools are registered in `AgentController.cs`:

```csharp
public AgentController(IConfiguration cfg)
{
    var tools = new Dictionary<string, Func<object, Task<object>>>
    {
        ["add_node"] = async (o) => await AddNodeAsync(o as NodeCreateRequest),
        ["connect_nodes"] = async (o) => await ConnectNodesAsync(o as ConnectionCreateRequest),
        ["get_bottlenecks"] = async (o) => await AnalyzeBottlenecksAsync(o as AnalyzeRequest),
        ["visualize_throughput"] = async (o) => await GetThroughputVisualizationAsync(o as Request)
    };

    _agent.ToolRegistry = tools;
}
```

### Error Handling

```csharp
public async Task<object> ExecuteToolAsync(string toolName, object data)
{
    try
    {
        if (!_toolRegistry.ContainsKey(toolName))
            return new ErrorResponse { Success = false, Message = $"Unknown tool: {toolName}" };

        return await _toolRegistry[toolName](data);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Tool execution failed: {ToolName}", toolName);
        return new ErrorResponse { Success = false, Message = ex.Message };
    }
}
```

---

## 📚 Design Decisions

### Why MCP Protocol?

- **Standardized**: Widely adopted AI agent interface
- **Type-Safe**: Strong typing for tool invocations
- **Context-Aware**: Supports rich context passing
- **Extensible**: Easy to add new tools

### Why Structured Context?

- **Deterministic**: Clear input/output contracts
- **Maintainable**: Easy to update AI expectations
- **Testable**: Verifiable behavior
- **Debuggable**: Clear execution path

---

## 🚀 Performance Targets

| Metric | Target | Current |
|--------|--------|---------|
| Tool Response Time | < 100ms | TBA |
| Context Update | < 50ms | TBA |
| AI Planning | < 2s | TBA |
| Bottleneck Analysis | < 50ms | TBA |

---

## 📜 Changelog

### [Unreleased] v0.1.0

- Initial MCP integration setup
- Tool definitions and registration
- Basic validation framework

---

## 📞 Related Documentation

- [Main README](../README.md) - System overview
- [MCP Integration Guide](../Documentation/MCP_INTEGRATION.md) - Deep dive
- [Agent Configuration](../src/agent_configuration.json) - Setup guide
