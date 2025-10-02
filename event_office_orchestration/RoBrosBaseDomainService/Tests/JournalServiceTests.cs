using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoBrosBaseDomainService.Data;
using RoBrosBaseDomainService.DTOs;
using RoBrosBaseDomainService.Services;
using Xunit;

namespace RoBrosBaseDomainService.Tests;

public class JournalServiceTests : IDisposable
{
    private readonly JournalDbContext _context;
    private readonly JournalService _service;
    private readonly Mock<ILogger<JournalService>> _loggerMock;

    public JournalServiceTests()
    {
        var options = new DbContextOptionsBuilder<JournalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new JournalDbContext(options);
        _loggerMock = new Mock<ILogger<JournalService>>();
        _service = new JournalService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_FirstEntry_CreatesNewJournalEntry()
    {
        // Arrange
        var request = new JournalRequest
        {
            EntityId = "test-123",
            EntityType = "TestEntity",
            EntityPayload = new { Name = "Test", Value = 42 },
            UserId = "user1"
        };

        // Act
        var response = await _service.CreateOrUpdateAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.EntityId.Should().Be("test-123");
        response.CreatedBy.Should().Be("user1");
        response.UpdatedBy.Should().Be("user1");
        response.Version.Should().Be(1);
        
        var journalEntry = await _context.EntityJournals.FirstOrDefaultAsync();
        journalEntry.Should().NotBeNull();
        journalEntry!.EntityId.Should().Be("test-123");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_SecondEntry_IncrementsVersion()
    {
        // Arrange
        var firstRequest = new JournalRequest
        {
            EntityId = "test-456",
            EntityType = "TestEntity",
            EntityPayload = new { Name = "Test", Value = 42 },
            UserId = "user1"
        };

        var secondRequest = new JournalRequest
        {
            EntityId = "test-456",
            EntityType = "TestEntity",
            EntityPayload = new { Name = "Updated", Value = 100 },
            UserId = "user2"
        };

        // Act
        var firstResponse = await _service.CreateOrUpdateAsync(firstRequest);
        var secondResponse = await _service.CreateOrUpdateAsync(secondRequest);

        // Assert
        firstResponse.Version.Should().Be(1);
        secondResponse.Version.Should().Be(2);
        secondResponse.CreatedBy.Should().Be("user1"); // Original creator
        secondResponse.UpdatedBy.Should().Be("user2"); // Current updater
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsLatestVersion()
    {
        // Arrange
        await _service.CreateOrUpdateAsync(new JournalRequest
        {
            EntityId = "test-789",
            EntityType = "TestEntity",
            EntityPayload = new { Version = 1 },
            UserId = "user1"
        });

        await _service.CreateOrUpdateAsync(new JournalRequest
        {
            EntityId = "test-789",
            EntityType = "TestEntity",
            EntityPayload = new { Version = 2 },
            UserId = "user1"
        });

        // Act
        var latest = await _service.GetLatestAsync("test-789", "TestEntity");

        // Assert
        latest.Should().NotBeNull();
        latest!.Version.Should().Be(2);
        latest.Entity.Should().Contain("\"version\":2");
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsAllVersions()
    {
        // Arrange
        var entityId = "test-history";
        for (int i = 1; i <= 5; i++)
        {
            await _service.CreateOrUpdateAsync(new JournalRequest
            {
                EntityId = entityId,
                EntityType = "TestEntity",
                EntityPayload = new { Version = i },
                UserId = $"user{i}"
            });
        }

        // Act
        var history = await _service.GetHistoryAsync(new JournalHistoryRequest
        {
            EntityId = entityId,
            EntityType = "TestEntity"
        });

        // Assert
        history.Should().HaveCount(5);
        history.Should().BeInDescendingOrder(h => h.Version);
        history.First().Version.Should().Be(5);
        history.Last().Version.Should().Be(1);
    }

    [Fact]
    public async Task GetHistoryAsync_WithLimit_ReturnsLimitedResults()
    {
        // Arrange
        var entityId = "test-limit";
        for (int i = 1; i <= 10; i++)
        {
            await _service.CreateOrUpdateAsync(new JournalRequest
            {
                EntityId = entityId,
                EntityType = "TestEntity",
                EntityPayload = new { Version = i },
                UserId = "user1"
            });
        }

        // Act
        var history = await _service.GetHistoryAsync(new JournalHistoryRequest
        {
            EntityId = entityId,
            EntityType = "TestEntity",
            Limit = 3
        });

        // Assert
        history.Should().HaveCount(3);
        history.First().Version.Should().Be(10); // Latest versions
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSpecificVersion()
    {
        // Arrange
        var response = await _service.CreateOrUpdateAsync(new JournalRequest
        {
            EntityId = "test-byid",
            EntityType = "TestEntity",
            EntityPayload = new { Name = "Specific" },
            UserId = "user1"
        });

        // Act
        var result = await _service.GetByIdAsync(response.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(response.Id);
        result.EntityId.Should().Be("test-byid");
    }

    [Fact]
    public async Task GetLatestAsync_NonExistentEntity_ReturnsNull()
    {
        // Act
        var result = await _service.GetLatestAsync("nonexistent", "TestEntity");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrUpdateAsync_PreservesCreatedTimestamp()
    {
        // Arrange
        var entityId = "test-timestamp";
        var firstResponse = await _service.CreateOrUpdateAsync(new JournalRequest
        {
            EntityId = entityId,
            EntityType = "TestEntity",
            EntityPayload = new { Data = "First" },
            UserId = "user1"
        });

        await Task.Delay(100); // Small delay to ensure different timestamps

        // Act
        var secondResponse = await _service.CreateOrUpdateAsync(new JournalRequest
        {
            EntityId = entityId,
            EntityType = "TestEntity",
            EntityPayload = new { Data = "Second" },
            UserId = "user2"
        });

        // Assert
        secondResponse.CreatedAt.Should().Be(firstResponse.CreatedAt);
        secondResponse.UpdatedAt.Should().BeAfter(firstResponse.UpdatedAt);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}