# Runtime Log Analysis – Live Demo Companion

This document shows what actually happened when running the multi‑agent system with three different local LLMs (Qwen‑7B, Qwen‑14B, Llama‑3‑8B). All logs are from the same Docker environment, same task prompt, same tools. The architecture and roles were already explained – here we just watch the agents fail in practice.

---

## 1. Product Owner Output

**Observed behavior:**  
The Product Owner often generated surprisingly coherent Gherkin stories and requirement documents. However, role boundaries were repeatedly violated – the PO produced C# code, XAML snippets, and implementation details, which is not allowed.

**Log snippet (Qwen‑7B) – PO writing C# code instead of pure specs:**

> ## Core Subsystem Component Requirements
> 
> ### Backend Engine Core
> ```csharp
> // backend/core/recipe-deserializer.cs
> using System;
> using System.Collections.Generic;
> 
> public class RecipeDeserializer
> {
>     public static Dictionary<string, Ingredient> DeserializeRecipe(string json)
>     {
>         // deserialize the recipe from JSON
>     }
> }
> ```

**Why it matters:**  
The PO should only define *what* (functional specs, metrics, Gherkin), not *how*. This confusion fed invalid, code‑contaminated specs to the Developer, causing further divergence.

---

## 2. Scrum Master Decisions

**Observed behavior:**  
The Scrum Master’s audit logic worked relatively well. It frequently rejected malformed or role‑violating outputs.

**Log snippet (Qwen‑14B) – rejecting a plain JSON tool call instead of execution:**

> [LOG]: SMA: Rejected - {
>   "name": "safe_file_read",
>   "arguments": { "file_path": "/app/agent_workspace/src/factorio_recipes_and_machines.json" }
> }

**Impact:**  
The workflow did not progress because the rejection only triggered a retry of the same broken agent behaviour – no error correction was attempted. Even though the system prompt specifically instructed it to use the `ReAct` pattern.

---

## 3. Developer Behavior

**Observed problems:**  
- Duplicate directory structures (`FactorioArchitectEngine.Models/` vs `FactorioArchitectEngine/Models/`)  
- Inconsistent casing (`Calculator.cs` vs `calculator_module.cs`)  
- Placeholder implementations (empty methods, dummy returns)  
- No cleanup or renaming of existing files

**Log snippet (Llama‑3) – workspace after several cycles:**

> CURRENT WORKSPACE STRUCTURE:
> - Factors\ProductionCalculator.cs
> - app\agent_workspace\src\Calculator.cs
> - app\agent_workspace\src\FactorioArchitectEngine.Models\ProductionCalculator.cs
> - app\agent_workspace\src\FactorioArchitectEngine.Models\User.cs
> - app\agent_workspace\src\FactorioArchitectEngine\Models\User.cs
> - issues (DIR)
> - src (DIR)
>     - factorio_recipes_and_machines.json

**Why it matters:**  
The agent never reconciled or renamed existing files, leading to compilation failures (duplicate definitions, missing references) and confusion about which file was the “real” implementation.

---

## 4. QA Failures

**Observed behavior:**  
The QA agent sometimes produced plausible xUnit test code, but the .NET project was never correctly set up. Every `dotnet build` or `dotnet test` failed because no `.csproj` or `.sln` existed in the working directory.

**Log snippet (Llama‑3) – repeated build failure:**

> dotnet_tool executed with result: MSBUILD : error MSB1003: Specify a project or solution file.

**Observable pattern:**  
QA kept calling `dotnet test` even after multiple build failures – no adaptive behaviour. The agent never attempted to create a project file or navigate to the correct directory.

---

## 5. Retry Loops

**Observed behavior:**  
The workflow entered endless cycles where Developer produced code, QA tried to test it, build failed, Scrum Master approved again, and QA retried the same failing command.

**Log snippet (Qwen‑7B) – repeated “Agent failed to confirm test writing”:**

> [LOG]: QA: Agent failed to confirm test writing.

Each time the QA agent returned a tool call as plain text instead of executing it. The Scrum Master re‑approved, and the loop continued until a crash.

**Why it matters:**  
Retry loops consumed token budgets, flooded logs, and never resulted in progress. No mechanism existed to detect a stuck state and abort or escalate.

---

## 6. Tool‑Calling Failures (Most Critical Issue)

**Observed behavior:**  
All three models frequently printed tool calls as JSON or markdown code blocks instead of actually invoking them. This is visible throughout every log file.

**Clear example (Qwen‑14B – “no‑exec” model)** – the agent outputs the exact tool call it should execute:

> {
>   "name": "safe_file_read",
>   "arguments": { "file_path": "/app/agent_workspace/src/factorio_recipes_and_machines.json" }
> }

**Consequences:**  
- Files were never read, so context remained incomplete.  
- The system interpreted the output as a “final answer” and stopped.  
- Manual restarts were required to continue.  
- The agents never learned to correct this mistake – it happened repeatedly.

---

## 7. Commit Examples

**Observed behavior:**  
The Developer attempted to commit changes, but often used wrong git commands or missing commit messages. The Scrum Master also attempted commits without staging files or configuring user identity.

**Log snippet (Llama‑3) – failed commit due to missing author identity:**

> git_tool executed with result: Author identity unknown
> *** Please tell me who you are.

**Agents almost never configured `user.name` or `user.email`.** Version control became another dead end – commits were barely ever successfully created.

**Another snippet (Qwen‑7B) – Scrum Master trying an invalid commit command:**

> {"name": "git_tool", "arguments": {"command": "commit -m 'feat(README): Update README...'}}

(Command failed because `-m` was not quoted properly in the argument string.)

**Why it matters:**  
Without version control, the system could not checkpoint progress or recover from failures. The entire workflow remained in a volatile, untracked state.

---

## Conclusion (visible from logs)

- **Orchestration logic** (role routing, auditing, tool definitions) worked as designed.
- **Local LLMs** failed to reliably:
  - invoke tools instead of printing them,
  - follow naming and structure policies,
  - recover from build errors,
  - handle feedback from tools,
  - configure a working .NET project from scratch.
- The system never produced a single passing test or a buildable artifact.

The logs directly support the slide’s final claim: **Small‑to‑medium local LLMs are too unreliable for autonomous multi‑agent software engineering.**
