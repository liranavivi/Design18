using FlowOrchestrator.Abstractions.Common;

namespace FlowOrchestrator.Abstractions.Tests.Common
{
    public class ValidationResultTests
    {
        [Fact]
        public void Valid_ShouldReturnValidResult()
        {
            // Act
            var result = ValidationResult.Valid();

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public void Invalid_ShouldReturnInvalidResult()
        {
            // Arrange
            var errorCode = "ERR001";
            var errorMessage = "Test error message";

            // Act
            var result = ValidationResult.Invalid(errorCode, errorMessage);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal(errorCode, result.Errors[0].Code);
            Assert.Equal(errorMessage, result.Errors[0].Message);
            Assert.Empty(result.Warnings);
        }

        [Fact]
        public void WithWarning_ShouldAddWarning()
        {
            // Arrange
            var warningCode = "WARN001";
            var warningMessage = "Test warning message";
            var result = ValidationResult.Valid();

            // Act
            result.WithWarning(warningCode, warningMessage);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.Single(result.Warnings);
            Assert.Equal(warningCode, result.Warnings[0].Code);
            Assert.Equal(warningMessage, result.Warnings[0].Message);
        }

        [Fact]
        public void WithWarning_ShouldReturnSameInstance()
        {
            // Arrange
            var result = ValidationResult.Valid();

            // Act
            var returnedResult = result.WithWarning("WARN001", "Test warning");

            // Assert
            Assert.Same(result, returnedResult);
        }
    }
}
