Feature: Production Chain Analysis

  As a factory designer
  I want to analyze production chains
  So I can identify bottlenecks and optimize layouts

  Scenario: Simple belt-based production
    Given a factory with iron-plate recipes
    When analyzing the production chain
    Then iron-ore flow should be calculated
    And iron-plate output equals belt capacity

  Scenario: Circuit network analysis
    Given a belt with 100 items
    When calculating circuit needs
    Then advanced-circuits required = 100 * 0.75

  Scenario: Bottleneck detection
    Given overloaded belts from ProductionChainAnalyzer
    When net rate is calculated
    Then upstream bottlenecks are identified

  Scenario: Advanced Circuit Throughput Validation
    Given a Factorio save file containing circuit network configurations
    And the modeler is initialized with recipe data from /app/agent_workspace/src/factorio_recipes_and_machines.json
    When I configure a production line ending in "advanturite-pipe"
    And activate the "adv-circuit-9x" recipe variant
    And set machine speeds to 100% capacity
    Then the calculated throughput MUST be exactly 10 items per minute
    And the formula validation shows:
      T = (RecipeOutput/RecipeTime) * MachineSpeed * 60 == 10
    And no bottlenecks are reported at any stage

  Scenario: Express Splitter Throughput Validation
    Given the modeler is loaded with expression-splitter recipe data
    And circuit network constraints are enabled
    When I configure a production line ending in "express-splitter"
    And activate the "express-splitter-basic" recipe variant
    And optimize machine speeds to 100% capacity
    Then the calculated throughput MUST be exactly 2.5 items per minute
    And the formula validation shows:
      T = (RecipeOutput/RecipeTime) * MachineSpeed * 60 == 2.5
    And all upstream constraints are satisfied
