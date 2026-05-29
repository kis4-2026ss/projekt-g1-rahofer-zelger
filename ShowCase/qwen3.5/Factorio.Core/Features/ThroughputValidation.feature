Feature: Advanced Circuit Production Verification

  Background:
    Given a Factorio save file containing circuit network configurations
    And the modeler is initialized with recipe data from /app/agent_workspace/src/factorio_recipes_and_machines.json

  Scenario: Verify high-volume assembly line
    When I configure a production line ending in "advanturite-pipe"
    And activate the "adv-circuit-9x" recipe variant
    And set machine speeds to 100% capacity
    Then the calculated throughput MUST be exactly 10 items per minute
    And the formula validation shows: 
      T = (RecipeOutput/RecipeTime) * MachineSpeed * 60 == 10
    And no bottlenecks are reported at any stage
