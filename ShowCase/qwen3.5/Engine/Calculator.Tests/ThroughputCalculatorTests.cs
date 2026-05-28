using System;
using System.Collections.Generic;
using Xunit;

namespace Engine.Calculator.Tests
{
    public class ThroughputCalculatorTests
    {
        private readonly ThroughputCalculator _calculator;
        private readonly UnitConverter _converter;
        private readonly MachineSpeedResolver _speedResolver;

        public ThroughputCalculatorTests()
        {
            _calculator = new ThroughputCalculator();
            _converter = new UnitConverter();
            _speedResolver = new MachineSpeedResolver();
        }

        [Fact]
        public void AdvancedCircuit_Throughput_Expected()
        {
            // Arrange
            var recipe = new Recipe 
            { 
                ID = "advanced-circuit",
                OutputQty = 1, 
                CraftingTime = 6 
            };

            // Act
            double result = _calculator.CalculateThroughput(recipe, 3);
            var tolerance = _calculator.GetAccuracyTolerance();

            // Assert: Advanced circuit with level 3 (1.5 speed)
            // Expected: (1/6) × 1.5 × 60 = 15 circuits/minute
            Assert.InRange(result, 14.999, 15.001);
            Assert.InRange(_calculator.Validate(result, 15.0).Accuracy, 99.9, 100.0);
        }

        [Fact]
        public void ExpressSplitter_Throughput_Expected()
        {
            // Arrange
            var expression = new ExpressSplitter 
            { 
                OutputQty = 2, 
                CraftingTime = 30 
            };

            // Act
            double result = _calculator.CalculateThroughput(expression, 3);
            var tolerance = _calculator.GetAccuracyTolerance();

            // Assert: Express splitter with level 3 (1.5 speed)
            // Expected: (2/30) × 1.5 × 60 = 6 units/minute
            Assert.InRange(result, 5.999, 6.001);
        }

        [Fact]
        public void MachineLevel1_Speed_Multiplier()
        {
            // Arrange
            var recipe = new Recipe { OutputQty = 1, CraftingTime = 10 };

            // Act
            var level1 = _calculator.CalculateThroughput(recipe, 1);

            // Assert: Level 1 with 0.67 speed
            // Expected: (1/10) × 0.67 × 60 = 4.02 units/minute
            Assert.InRange(level1 / 60, 0.669, 0.671);
        }

        [Fact]
        public void MachineLevel2_Speed_Multiplier()
        {
            // Arrange
            var recipe = new Recipe { OutputQty = 1, CraftingTime = 10 };

            // Act
            var level2 = _calculator.CalculateThroughput(recipe, 2);

            // Assert: Level 2 with 1.0 speed
            // Expected: (1/10) × 1.0 × 60 = 6 units/minute
            Assert.InRange(level2 / 60, 0.999, 1.001);
        }

        [Fact]
        public void MachineLevel3_Speed_Multiplier()
        {
            // Arrange
            var recipe = new Recipe { OutputQty = 1, CraftingTime = 10 };

            // Act
            var level3 = _calculator.CalculateThroughput(recipe, 3);

            // Assert: Level 3 with 1.5 speed
            // Expected: (1/10) × 1.5 × 60 = 9 units/minute
            Assert.InRange(level3 / 60, 1.499, 1.501);
        }

        [Fact]
        public void Precision_Tolerance_Enforcement()
        {
            // Arrange
            var recipe = new Recipe { OutputQty = 1, CraftingTime = 6 };
            double expected = 15.0;

            // Act
            var tolerance = _calculator.GetAccuracyTolerance();

            // Assert: Tolerance is ±0.001
            Assert.Equal(0.001, tolerance);
        }

        [Fact]
        public void NullRecipe_Validation()
        {
            // Arrange
            var recipe = null as Recipe;

            // Act & Assert
            var validationResult = _calculator.ValidateRecipe(recipe, 2);
            Assert.False(validationResult.IsValid);
            Assert.Contains("null", validationResult.ErrorMessage);
        }

        [Fact]
        public void InvalidCraftingTime_Validation()
        {
            // Arrange
            var recipe = new Recipe { OutputQty = 1, CraftingTime = 0 };

            // Act & Assert
            var validationResult = _calculator.Validate(recipe, 2);
            Assert.False(validationResult.IsValid);
            Assert.Contains("positive", validationResult.ErrorMessage);
        }

        [Fact]
        public void InvalidMachineLevel_Validation()
        {
            // Arrange
            var recipe = new Recipe { OutputQty = 1, CraftingTime = 10 };
            var invalidLevel = 5;

            // Act & Assert
            var validationResult = _calculator.Validate(recipe, invalidLevel);
            Assert.False(validationResult.IsValid);
            Assert.Contains("1-3", validationResult.ErrorMessage);
        }
    }
}