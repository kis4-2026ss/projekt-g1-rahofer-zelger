# Avalonia UI Subsystem

## 📋 Overview

The **Avalonia UI** subsystem provides the graphical user interface for the Factorio Architect application. It implements all visual components including node rendering, throughput displays, and user interactions.

## 🎯 Core Responsibilities

- Visual representation of production chains
- Node creation and editing
- Connection management (splitters, merge splitters)
- Throughput calculation displays
- Error message presentation
- Tool palette and recipe browser

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Avalonia UI Layer                      │
├─────────────────────────────────────────────────────────┤
│  ┌───────────┐  ┌───────────┐  ┌─────────────────┐      │
│  │ MainWindow │  │ NodeView  │  │ ConnectionView  │      │
│  └───────────┘  └───────────┘  └─────────────────┘      │
│         │              │                │                │
│  ┌─────────────────────────────────────────────────┐    │
│  │                  Themes / Styles                  │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

## 📁 File Structure

```
AvaloniaUI/
├── README.md                          # This file
├── Views/
│   ├── MainWindow.axaml               # Main application window
│   ├── NodeView.axaml                 # Individual node rendering
│   ├── ConnectionView.axaml           # Splitter/merge connections
│   ├── ToolPalette.axaml              # Recipe selection UI
│   └── ThroughputCard.axaml           # Production rate display
├── Models/
│   ├── NodeViewModel.cs               # Node view model for MVPVM
│   ├── RecipeSelectorViewModel.cs     # Recipe browser view model
│   └── ViewModels.cs                  # Base view models
├── Controls/
│   ├── RecipeSelectorControl.axaml    # Custom recipe picker
│   ├── ThroughputDisplayControl.axaml # Rate display widget
│   └── NodeCardControl.axaml          # Node display component
├── Resources/
│   ├── Avalonia.themexml              # Application theme
│   └── Styles/
│       ├── NodeStyles.xaml            # Node visual styles
│       └── ConnectionStyles.xaml      # Line/connector styles
└── App.axaml                          # Application startup
```

## 🧪 Gherkin Acceptance Criteria

### Feature: Node Creation

```gherkin
Feature: Node Creation
  Background:
    Given the application is initialized
    And the simulation engine is loaded
    And the recipe database is available

  Scenario: Create a new production node
    When the user selects a recipe from the palette
    And clicks an empty area on the canvas
    Then a node is created at the click location
    And the node displays its emoji and label
    And the node shows its throughput calculation
    And the node is added to the simulation graph

  Scenario: Create with multiple recipes
    When multiple recipes exist in the database
    When the user selects different recipes
    Then each recipe creates a distinct node type
    And nodes are differentiated by emoji/label
```

### Feature: Emoji Rendering

```gherkin
Feature: Emoji Rendering
  Background:
    Given the application has loaded emoji assets

  Scenario: Display recipe emoji
    When a node is created with a recipe
    And the recipe has an icon/emoji
    Then the emoji is rendered centered on the node
    And the emoji is scalable for different node sizes

  Scenario: Fallback for missing emoji
    When a recipe has no emoji
    Then a default icon is displayed
    And a warning is logged to console
```

### Feature: Throughput Display

```gherkin
Feature: Throughput Display
  Background:
    Given the simulation engine is calculating

  Scenario: Show throughput on node
    When the simulation is running
    And the node has output rate > 0
    Then the throughput is displayed on the node
    And the display updates in real-time (≤ 100ms)
    And the value is formatted as "items/minute"

  Scenario: Show bottleneck status
    When a node is a bottleneck
    Then the node appears red/orange
    And a bottleneck indicator is shown
    And the severity is displayed
```

## 🔄 Git Workflow

### Conventional Commits for UI Changes

```
feat(ui): add throughput card component
fix(ui): fix node emoji rendering on macOS
docs(ui): document theme customization
refactor(ui): extract node view to separate component
```

### Branch Strategy

```
main                          # Stable release branch
├── develop                    # Integration branch
│   ├── feature/ui-1           # New feature branches
│   ├─ feature/ui-2
│   └── hotfix/ui-1
└── feature/ui-next-version   # Next release prep
```

## 📐 Technical Specifications

### Node Rendering

- **Emoji Rendering**: Uses Avalonia's built-in emoji handling
- **Scale**: Nodes scale 1:1 with canvas
- **Z-Index**: Connections drawn behind nodes
- **Highlight**: Selected nodes have 3px glow border
- **Hover**: 2px raised border with tooltip

### Connection Lines

- **Type**: Bezier curves with control points
- **Rendering**: Stroke with 1.5px width
- **Tooltip**: Shows connection ratio on hover
- **Snap**: 4px snap when drawing new connections

### Throughput Card

- **Layout**: Vertical stack (icon, label, rate)
- **Color Coding**:
  - Green: Optimal (>90% efficiency)
  - Yellow: Warning (70-90%)
  - Red: Bottleneck (<70%)
- **Update Rate**: Every 100ms when simulation active

## 🔧 Configuration

### Visual Settings (appsettings.json)

```json
{
  "AvaloniaUI": {
    "Theme": "Dark",
    "FontSize": 12,
    "NodeBorderRadius": 8,
    "ThroughputUpdateInterval": 100,
    "ConnectionSnapDistance": 4,
    "HighlightGlowColor": "#00ffcc",
    "TooltipPadding": 8
  }
}
```

## 🧰 Dependencies

- Avalonia 11.0.x
- Avalonia.Themes.Simple
- CommunityToolkit.Mvvm
- LiveChartsCore.SkiaSharpView (for charts)

## 🔐 Security

- No sensitive data in UI (offline mode)
- XSS prevention via ContentSecurityPolicy
- Input sanitization on all user controls

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 0.1.0-alpha | 2024-01 | Initial UI scaffolding |

---

**Owner**: Developer Team  
**Review By**: Product Owner  
**Last Updated**: 2024-01  
