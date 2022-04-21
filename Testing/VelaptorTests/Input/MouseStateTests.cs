// <copyright file="MouseStateTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTests.Input
{
    using System.Drawing;
    using Velaptor.Input;
    using Xunit;

    /// <summary>
    /// Tests the <see cref="MouseState"/> struct.
    /// </summary>
    public class MouseStateTests
    {
        #region Method Tests
        [Fact]
        public void SetPosition_WhenInvoked_SetsPosition()
        {
            // Arrange
            var expected = new Point(11, 22);
            var state = default(MouseState);

            // Act
            state.SetPosition(11, 22);
            var actual = state.GetPosition();

            // Arrange
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetX_WhenInvoked_ReturnsCorrectResult()
        {
            // Arrange
            var state = default(MouseState);
            state.SetPosition(123, 0);

            // Act
            var actual = state.GetX();

            // Assert
            Assert.Equal(123, actual);
        }

        [Fact]
        public void GetY_WhenInvoked_ReturnsCorrectResult()
        {
            // Arrange
            var state = default(MouseState);
            state.SetPosition(0, 123);

            // Act
            var actual = state.GetY();

            // Assert
            Assert.Equal(123, actual);
        }

        [Fact]
        public void SetScrollWheelValue_WhenInvoked_SetsValue()
        {
            // Arrange
            var state = default(MouseState);

            // Act
            state.SetScrollWheelValue(123);
            var actual = state.GetScrollWheelValue();

            // Assert
            Assert.Equal(123, actual);
        }

        [Theory]
        [InlineData(MouseButton.LeftButton, true, false, false)]
        [InlineData(MouseButton.MiddleButton, false, true, false)]
        [InlineData(MouseButton.RightButton, false, false, true)]
        public void SetButtonState_WhenInvoked_SetsState(MouseButton mouseButton, bool expectedLeft, bool expectedMiddle, bool expectedRight)
        {
            // Arrange
            var state = default(MouseState);

            // Act
            state.SetButtonState(mouseButton, true);
            var actualLeft = state.GetButtonState(MouseButton.LeftButton);
            var actualMiddle = state.GetButtonState(MouseButton.MiddleButton);
            var actualRight = state.GetButtonState(MouseButton.RightButton);

            // Assert
            Assert.Equal(expectedLeft, actualLeft);
            Assert.Equal(expectedMiddle, actualMiddle);
            Assert.Equal(expectedRight, actualRight);
        }

        [Fact]
        public void IsLeftButtonDown_WhenLeftButtonIsDown_ReturnsTrue()
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(MouseButton.LeftButton, true);

            // Act
            var actual = state.IsLeftButtonDown();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsMiddleButtonDown_WhenLeftButtonIsDown_ReturnsTrue()
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(MouseButton.MiddleButton, true);

            // Act
            var actual = state.IsMiddleButtonDown();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsRightButtonDown_WhenLeftButtonIsDown_ReturnsTrue()
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(MouseButton.RightButton, true);

            // Act
            var actual = state.IsRightButtonDown();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsLeftButtonUp_WhenLeftButtonIsUp_ReturnsTrue()
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(MouseButton.LeftButton, false);

            // Act
            var actual = state.IsLeftButtonUp();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsMiddleButtonUp_WhenLeftButtonIsUp_ReturnsTrue()
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(MouseButton.MiddleButton, false);

            // Act
            var actual = state.IsMiddleButtonUp();

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void IsRightButtonUp_WhenLeftButtonIsUp_ReturnsTrue()
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(MouseButton.RightButton, false);

            // Act
            var actual = state.IsRightButtonUp();

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData(MouseButton.LeftButton, true)]
        [InlineData(MouseButton.MiddleButton, true)]
        [InlineData(MouseButton.RightButton, true)]
        public void GetButtonState_WhenInvoked_ReturnsCorrectResult(MouseButton mouseButton, bool expected)
        {
            // Arrange
            var state = default(MouseState);
            state.SetButtonState(mouseButton, expected);

            // Act
            var actual = state.GetButtonState(mouseButton);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MouseButton.LeftButton, true)]
        [InlineData(MouseButton.MiddleButton, true)]
        [InlineData(MouseButton.RightButton, true)]
        public void GetButtonState_WhenButtonIsDown_ReturnsTrue(MouseButton downButton, bool expected)
        {
            // Arrange
            var state = default(MouseState);

            state.SetButtonState(downButton, true);

            // Act
            var actual = state.GetButtonState(downButton);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetButtonState_WithInvalidMouseButton_ReturnsFalse()
        {
            // Arrange
            var state = default(MouseState);

            // Act
            var actual = state.GetButtonState((MouseButton)1234);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void Equals_WhenUsingSameTypeParamWhenEqual_ReturnsTrue()
        {
            // Arrange
            var stateA = default(MouseState);
            var stateB = default(MouseState);

            // Act
            var actual = stateA.Equals(stateB);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void Equals_WhenUsingObjectParamOfDifferentType_ReturnsFalse()
        {
            // Arrange
            var stateA = default(MouseState);
            var stateB = new object();

            // Act
            var actual = stateA.Equals(stateB);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void Equals_WhenUsingObjectParamOfSameTypeWhenEqual_ReturnsTrue()
        {
            // Arrange
            var stateA = default(MouseState);
            object stateB = default(MouseState);

            // Act
            var actual = stateA.Equals(stateB);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void EqualsOperator_WhenBothOperandsAreEqual_ReturnsTrue()
        {
            // Arrange
            var stateA = default(MouseState);
            var stateB = default(MouseState);

            // Act
            var actual = stateA == stateB;

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void EqualsOperator_WhenBothOperandsAreNotEqual_ReturnsFalse()
        {
            // Arrange
            var stateA = default(MouseState);
            var stateB = default(MouseState);
            stateB.SetPosition(11, 22);

            // Act
            var actual = stateA == stateB;

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void NotEqualsOperator_WhenBothOperandsAreEqual_ReturnsFalse()
        {
            // Arrange
            var stateA = default(MouseState);
            var stateB = default(MouseState);

            // Act
            var actual = stateA != stateB;

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void NotEqualsOperator_WhenBothOperandsAreNotEqual_ReturnsTrue()
        {
            // Arrange
            var stateA = default(MouseState);
            var stateB = default(MouseState);
            stateB.SetPosition(11, 22);

            // Act
            var actual = stateA != stateB;

            // Assert
            Assert.True(actual);
        }
        #endregion
    }
}
