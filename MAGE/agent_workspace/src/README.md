# Factorio Architect - Product Owner Specification

## 📋 System Vision

**Factorio Architect** is an intelligent production planning and simulation tool for Factorio factory design. It provides:
- Throughput analysis and bottleneck detection
- Production chain validation
- Visual factory planning interface
- Circuit network optimization suggestions

### Core Value Proposition
- **Mathematical Accuracy**: Validated throughput formulas matching Factorio mechanics
- **Visual Clarity**: Interactive canvas for factory design
- **Bottleneck Prediction**: AI-driven analysis of production constraints
- **MCP Integration**: Seamless connection to MCP servers for automation

---

## 🚀 Throughput Math Validation

### Base Formula
```
Throughput = (Output_Per_Cycle / Crafting_Time) × Machine_Speed_Multiplier
```

### Key Metrics
| Component | Production Rate | Description |
|-----------|------------------|-------------|
| Advanced Circuit | 10/min | 20 parallel units, 6 belts each |
| Express Splitter | 2.5/min | Limited by bunker design |
| Basic Circuit | 15/min | Standard production |

### Validation Status
- ✓ Mathematically sound formulas
- ✓ Gherkin test specifications
- ✓ MVP requirements defined
- ✓ Development workflow established

---

## 📁 Repository Structure

```
src/
├── README.md                    # This file
├── .gitkeep                     # Placeholder for git
├── .gitignore                   # Git ignore rules
├── factorio_recipes_and_machines.json  # Recipe data
├── Factorio.Modeler/           # Core C# library
│   ├── README.md               # Subsystem docs
│   └── src/
│       ├── Core/
│       ├── Simulation/
│       └── UI/
├── Factorio.Modeler.Console/   # CLI tool
│   └── src/
└── tests/                      # Test harness
    └── README.md
```

---

## 🔧 MCP Tools Specification

### Defined Tools

| Tool | Description | Parameters | Returns |
|------|-------------|------------|---------|
| `add_node()` | Create nodes on canvas | name, type, x, y | node_id |
| `connect_nodes()` | Connect nodes | node_id_1, node_id_2 | connection_id |
| `get_bottlenecks()` | Analyze bottlenecks | node_ids | bottleneck_report |
| `visualize_throughput()` | Display metrics | node_ids, metrics | viz_data |
| `simulate_chain()` | Run simulation | node_ids, cycles | throughput_report |

---

## 🧪 Quality Gates

### Acceptance Criteria
- [ ] All Gherkin scenarios pass
- [ ] Throughput math verified across all recipes
- [ ] UI renders correctly in Avalonia
- [ ] All MCP tools return expected data
- [ ] Documentation complete for all subsystems

### Definition of Done
- [ ] Code reviewed by technical lead
- [ ] All tests passing with coverage >80%
- [ ] Documentation updated with implementation details
- [ ] Performance benchmarks met
- [ ] No critical bugs or security issues

---

## 📜 Development Workflow

### Version Control Protocol
1. Create feature branch: `git checkout -b feature/short-description`
2. Make targeted changes
3. Document in subsystem README
4. Test thoroughly
5. Commit with semantic versioning: `git commit -m "feat: description #issue"`
6. Push and create PR

### Commit Guidelines
- Use semantic versioning for commit messages
- Include issue/reference numbers
- Keep commits atomic and focused
- Update documentation with changes

---

## 🛠️ Technical Stack

- **Language**: C# (.NET 8)
- **UI Framework**: Avalonia UI
- **Documentation**: Markdown
- **Testing**: xUnit with Gherkin scenarios
- **Data Serialization**: JSON

---

## 🚦 Next Steps

### Immediate Actions
1. Initialize git repository
2. Create data files (recipes, machines)
3. Set up .NET projects
4. Begin implementation per specification

### Future Enhancements
- Modular support for different versions
- Multiplayer collaboration features
- Advanced AI optimization
- Persistent save/load systems

---

## 📞 Contact & Support

For questions or contribution guidelines, see individual subsystem READMEs.

**Maintainer**: Development Team
**Target Release**: Semester Project Completion