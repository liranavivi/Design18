using FlowOrchestrator.Abstractions.Common;
using FlowOrchestrator.Domain.Entities;

namespace FlowOrchestrator.Domain.Tests.Entities
{
    /// <summary>
    /// Tests for the AbstractEntity class.
    /// </summary>
    public class AbstractEntityTests
    {
        /// <summary>
        /// Test implementation of AbstractEntity for testing purposes.
        /// </summary>
        private class TestEntity : AbstractEntity
        {
            public string TestId { get; set; } = "test-id";

            public override string GetEntityId()
            {
                return TestId;
            }

            public override string GetEntityType()
            {
                return "TestEntity";
            }

            public override ValidationResult Validate()
            {
                var result = new ValidationResult { IsValid = true };

                if (string.IsNullOrEmpty(TestId))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Code = "TEST_ID_REQUIRED", Message = "Test ID is required." });
                }

                return result;
            }
        }

        [Fact]
        public void AbstractEntity_DefaultProperties_HaveExpectedValues()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            Assert.Equal("1.0.0", entity.Version);
            Assert.True(DateTime.UtcNow.Subtract(entity.CreatedTimestamp).TotalSeconds < 1);
            Assert.True(DateTime.UtcNow.Subtract(entity.LastModifiedTimestamp).TotalSeconds < 1);
        }

        [Fact]
        public void AbstractEntity_SetProperties_RetainsValues()
        {
            // Arrange
            var createdTimestamp = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var lastModifiedTimestamp = new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            
            var entity = new TestEntity
            {
                Version = "2.0.0",
                CreatedTimestamp = createdTimestamp,
                LastModifiedTimestamp = lastModifiedTimestamp
            };

            // Act & Assert
            Assert.Equal("2.0.0", entity.Version);
            Assert.Equal(createdTimestamp, entity.CreatedTimestamp);
            Assert.Equal(lastModifiedTimestamp, entity.LastModifiedTimestamp);
        }

        [Fact]
        public void GetEntityId_ReturnsExpectedValue()
        {
            // Arrange
            var entity = new TestEntity { TestId = "custom-id" };

            // Act
            var entityId = entity.GetEntityId();

            // Assert
            Assert.Equal("custom-id", entityId);
        }

        [Fact]
        public void GetEntityType_ReturnsExpectedValue()
        {
            // Arrange
            var entity = new TestEntity();

            // Act
            var entityType = entity.GetEntityType();

            // Assert
            Assert.Equal("TestEntity", entityType);
        }

        [Fact]
        public void Validate_WithValidEntity_ReturnsSuccess()
        {
            // Arrange
            var entity = new TestEntity { TestId = "valid-id" };

            // Act
            var result = entity.Validate();

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WithInvalidEntity_ReturnsError()
        {
            // Arrange
            var entity = new TestEntity { TestId = string.Empty };

            // Act
            var result = entity.Validate();

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("TEST_ID_REQUIRED", result.Errors[0].Code);
            Assert.Equal("Test ID is required.", result.Errors[0].Message);
        }
    }
}
