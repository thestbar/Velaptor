﻿// <copyright file="TextureGPUBufferTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTests.OpenGL.Buffers;

using System;
using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using Moq;
using Velaptor.Exceptions;
using Velaptor.Graphics;
using Velaptor.NativeInterop.OpenGL;
using Velaptor.OpenGL;
using Velaptor.OpenGL.Buffers;
using Velaptor.OpenGL.Exceptions;
using Velaptor.Reactables.Core;
using Velaptor.Reactables.ReactableData;
using Helpers;
using Xunit;

/// <summary>
/// Tests the <see cref="TextureGPUBuffer"/> class.
/// </summary>
public class TextureGPUBufferTests
{
    private const uint BatchSize = 1000u;
    private const uint VertexArrayId = 111;
    private const uint VertexBufferId = 222;
    private const uint IndexBufferId = 333;
    private readonly Mock<IGLInvoker> mockGL;
    private readonly Mock<IOpenGLService> mockGLService;
    private readonly Mock<IReactable<GLInitData>> mockGLInitReactable;
    private readonly Mock<IReactable<BatchSizeData>> mockBatchSizeReactable;
    private readonly Mock<IReactable<ShutDownData>> mockShutDownReactable;
    private IReactor<GLInitData>? glInitReactor;
    private IReactor<BatchSizeData>? batchSizeReactor;
    private bool vertexBufferCreated;
    private bool indexBufferCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureGPUBufferTests"/> class.
    /// </summary>
    public TextureGPUBufferTests()
    {
        this.mockGL = new Mock<IGLInvoker>();
        this.mockGL.Setup(m => m.GenVertexArray()).Returns(VertexArrayId);
        this.mockGL.Setup(m => m.GenBuffer()).Returns(() =>
        {
            if (!this.vertexBufferCreated)
            {
                this.vertexBufferCreated = true;
                return VertexBufferId;
            }

            if (this.indexBufferCreated)
            {
                return 0;
            }

            this.indexBufferCreated = true;
            return IndexBufferId;
        });

        this.mockGLService = new Mock<IOpenGLService>();

        this.mockGLInitReactable = new Mock<IReactable<GLInitData>>();
        this.mockGLInitReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<GLInitData>>()))
            .Callback<IReactor<GLInitData>>(reactor =>
            {
                if (reactor is null)
                {
                    Assert.True(false, "GL initialization reactable subscription failed.  Reactor is null.");
                }

                this.glInitReactor = reactor;
            });

        this.mockBatchSizeReactable = new Mock<IReactable<BatchSizeData>>();
        this.mockBatchSizeReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<BatchSizeData>>()))
            .Callback<IReactor<BatchSizeData>>((reactor) =>
            {
                if (reactor is null)
                {
                    Assert.True(false, "GL initialization reactable subscription failed.  Reactor is null.");
                }

                this.batchSizeReactor = reactor;
            });

        this.mockShutDownReactable = new Mock<IReactable<ShutDownData>>();
    }

    /// <summary>
    /// Provides sample data to test if the correct data is being sent to the GPU.
    /// </summary>
    /// <returns>The data to test against.</returns>
    public static IEnumerable<object[]> GetGPUUploadTestData()
    {
        yield return new object[]
        {
            RenderEffects.None,
            new[]
            {
                -0.847915947f, 0.916118085f, 0.142857149f, 0.75f, 147f, 112f, 219f, 255f, -0.964588523f,
                0.760554552f, 0.142857149f, 0.25f, 147f, 112f, 219f, 255f, -0.760411441f, 0.79944545f,
                0.571428597f, 0.75f, 147f, 112f, 219f, 255f, -0.877084076f, 0.643881917f, 0.571428597f,
                0.25f, 147f, 112f, 219f, 255f,
            },
        };
        yield return new object[]
        {
            RenderEffects.FlipHorizontally,
            new[]
            {
                -0.760411441f, 0.79944545f, 0.142857149f, 0.75f, 147f, 112f, 219f, 255f, -0.877084076f,
                0.643881917f, 0.142857149f, 0.25f, 147f, 112f, 219f, 255f, -0.847915947f, 0.916118085f,
                0.571428597f, 0.75f, 147f, 112f, 219f, 255f, -0.964588523f, 0.760554552f, 0.571428597f,
                0.25f, 147f, 112f, 219f, 255f,
            },
        };
        yield return new object[]
        {
            RenderEffects.FlipVertically,
            new[]
            {
                -0.964588523f, 0.760554552f, 0.142857149f, 0.75f, 147f, 112f, 219f, 255f, -0.847915947f,
                0.916118085f, 0.142857149f, 0.25f, 147f, 112f, 219f, 255f, -0.877084076f, 0.643881917f,
                0.571428597f, 0.75f, 147f, 112f, 219f, 255f, -0.760411441f, 0.79944545f, 0.571428597f,
                0.25f, 147f, 112f, 219f, 255f,
            },
        };
        yield return new object[]
        {
            RenderEffects.FlipBothDirections,
            new[]
            {
                -0.877084076f, 0.643881917f, 0.142857149f, 0.75f, 147f, 112f, 219f, 255f, -0.760411441f,
                0.79944545f, 0.142857149f, 0.25f, 147f, 112f, 219f, 255f, -0.964588523f, 0.760554552f,
                0.571428597f, 0.75f, 147f, 112f, 219f, 255f, -0.847915947f, 0.916118085f, 0.571428597f,
                0.25f, 147f, 112f, 219f, 255f,
            },
        };
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullBatchSizeReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new TextureGPUBuffer(
                this.mockGL.Object,
                this.mockGLService.Object,
                this.mockGLInitReactable.Object,
                null,
                this.mockShutDownReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'batchSizeReactable')");
    }

    [Fact]
    public void Ctor_WhenBatchSizeReactableEndsNotifications_UnsubscriberInvoked()
    {
        // Arrange
        var mockUnsubscriber = new Mock<IDisposable>();

        this.mockBatchSizeReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<BatchSizeData>>()))
            .Callback<IReactor<BatchSizeData>>((reactor) =>
            {
                if (reactor is null)
                {
                    Assert.True(false, "GL initialization reactable subscription failed.  Reactor is null.");
                }

                this.batchSizeReactor = reactor;
            })
            .Returns<IReactor<BatchSizeData>>(_ => mockUnsubscriber.Object);

        _ = CreateSystemUnderTest();

        // Act
        this.batchSizeReactor.OnCompleted();
        this.batchSizeReactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void UploadVertexData_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act & Assert
        AssertExtensions.ThrowsWithMessage<BufferNotInitializedException>(() =>
        {
            sut.UploadVertexData(It.IsAny<TextureBatchItem>(), It.IsAny<uint>());
        }, "The texture buffer has not been initialized.");
    }

    [Fact]
    public void UploadVertexData_WithInvalidRenderEffects_ThrowsException()
    {
        // Arrange
        var textureQuad = new TextureBatchItem(
            new RectangleF(1, 1, 1, 1),
            new RectangleF(1, 1, 1, 1),
            1,
            1,
            Color.White,
            (RenderEffects)1234,
            default,
            1,
            1);
        var sut = CreateSystemUnderTest();
        this.glInitReactor.OnNext(default);

        // Act & Assert
        AssertExtensions.ThrowsWithMessage<InvalidRenderEffectsException>(() =>
        {
            sut.UploadVertexData(textureQuad, It.IsAny<uint>());
        }, "The 'RenderEffects' value of '1234' is not valid.");
    }

    [Fact]
    public void UploadVertexData_WhenInvoked_CreatesOpenGLDebugGroups()
    {
        // Arrange
        var batchItem = new TextureBatchItem(
            default,
            default,
            1,
            0,
            default,
            RenderEffects.None,
            default,
            1,
            0);

        var sut = CreateSystemUnderTest();

        this.glInitReactor.OnNext(default);

        // Act
        sut.UploadVertexData(batchItem, 0u);

        // Assert
        this.mockGLService.Verify(m => m.BeginGroup("Update Texture Quad - BatchItem(0) Data"), Times.Once);
        this.mockGLService.Verify(m => m.EndGroup(), Times.Exactly(5));
    }

    [Theory]
    [MemberData(nameof(GetGPUUploadTestData))]
    public void UploadVertexData_WhenInvoked_UploadsData(RenderEffects effects, float[] expected)
    {
        // Arrange
        var batchItem = new TextureBatchItem(
            new RectangleF(11, 22, 33, 44),
            new RectangleF(55, 66, 77, 88),
            1.5f,
            45,
            Color.MediumPurple,
            effects,
            new SizeF(800, 600),
            1,
            0);

        var sut = CreateSystemUnderTest();

        this.glInitReactor.OnNext(default);

        // Act
        sut.UploadVertexData(batchItem, 0u);

        // Assert
        this.mockGL.Verify(m
            => m.BufferSubData(GLBufferTarget.ArrayBuffer, 0, 128u, expected));
    }

    [Fact]
    public void PrepareForUpload_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act & Assert
        AssertExtensions.ThrowsWithMessage<BufferNotInitializedException>(() =>
        {
            sut.PrepareForUpload();
        }, "The texture buffer has not been initialized.");
    }

    [Fact]
    public void PrepareForUpload_WhenInvoked_BindsVertexArrayObject()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        this.glInitReactor.OnNext(default);

        // Act
        sut.PrepareForUpload();

        // Assert
        this.mockGLService.Verify(m => m.BindVAO(VertexArrayId), Times.AtLeastOnce);
    }

    [Fact]
    public void GenerateData_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act & Assert
        AssertExtensions.ThrowsWithMessage<BufferNotInitializedException>(() =>
        {
            sut.GenerateData();
        }, "The texture buffer has not been initialized.");
    }

    [Fact]
    public void GenerateData_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        var sut = CreateSystemUnderTest();
        this.glInitReactor.OnNext(default);
        this.batchSizeReactor.OnNext(new BatchSizeData(BatchSize));

        // Act
        var actual = sut.GenerateData();

        // Assert
        Assert.Equal(32_000, actual.Length);
    }

    [Fact]
    public void SetupVAO_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act & Assert
        AssertExtensions.ThrowsWithMessage<BufferNotInitializedException>(() =>
        {
            sut.SetupVAO();
        }, "The texture buffer has not been initialized.");
    }

    [Fact]
    public void SetupVAO_WhenInvoked_SetsUpVertexArrayObject()
    {
        // Arrange
        var unused = CreateSystemUnderTest();

        // Act
        this.glInitReactor.OnNext(default);

        // Assert
        this.mockGLService.Verify(m => m.BeginGroup("Setup Texture Buffer Vertex Attributes"), Times.Once);

        // Assert Vertex Position Attribute
        this.mockGL.Verify(m
            => m.VertexAttribPointer(0, 2, GLVertexAttribPointerType.Float, false, 32, 0), Times.Once);
        this.mockGL.Verify(m => m.EnableVertexAttribArray(0));

        // Assert Texture Coordinate Attribute
        this.mockGL.Verify(m
            => m.VertexAttribPointer(1, 2, GLVertexAttribPointerType.Float, false, 32, 8), Times.Once);
        this.mockGL.Verify(m => m.EnableVertexAttribArray(1));

        // Assert Tint Color Attribute
        this.mockGL.Verify(m
            => m.VertexAttribPointer(2, 4, GLVertexAttribPointerType.Float, false, 32, 16), Times.Once);
        this.mockGL.Verify(m => m.EnableVertexAttribArray(2));

        this.mockGLService.Verify(m => m.EndGroup(), Times.Exactly(4));
    }

    [Fact]
    public void GenerateIndices_WhenNotInitialized_ThrowsException()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act & Assert
        AssertExtensions.ThrowsWithMessage<BufferNotInitializedException>(() =>
        {
            sut.GenerateIndices();
        }, "The texture buffer has not been initialized.");
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="TextureGPUBuffer"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private TextureGPUBuffer CreateSystemUnderTest() => new (
        this.mockGL.Object,
        this.mockGLService.Object,
        this.mockGLInitReactable.Object,
        this.mockBatchSizeReactable.Object,
        this.mockShutDownReactable.Object);
}
