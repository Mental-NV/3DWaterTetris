using Floodline.Core.Levels;

namespace Floodline.Core.Tests;

public class LevelLoaderTests
{
    [Fact]
    public void LoadValidJsonReturnsLevel()
    {
        // Arrange
        string path = System.IO.Path.Combine("fixtures", "minimal_level.json");
        string json = System.IO.File.ReadAllText(path);

        // Act
        Level level = LevelLoader.Load(json);

        // Assert
        Assert.NotNull(level);
        Assert.Equal("test-level", level.Meta.Id);
        Assert.Equal(10, level.Bounds.X);
        _ = Assert.Single(level.InitialVoxels);
        Assert.Equal(OccupancyType.Bedrock, level.InitialVoxels[0].Type);
    }

    [Fact]
    public void LoadFloatingPointDurationThrowsArgumentException()
    {
        // Arrange
        string json = @"
{
  ""meta"": { ""id"": ""fail"", ""title"": ""fail"", ""schemaVersion"": 1, ""seed"": 1 },
  ""bounds"": { ""x"": 10, ""y"": 10, ""z"": 10 },
  ""initialVoxels"": [],
  ""objectives"": [],
  ""rotation"": { ""cooldownTicks"": 60.5 },
  ""bag"": { ""type"": ""Fixed"" },
  ""hazards"": []
}";

        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => LevelLoader.Load(json));
        Assert.Contains("Floating point number found", ex.Message);
    }

    [Fact]
    public void LoadEmptyJsonThrowsArgumentException()
    {
        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => LevelLoader.Load(""));
    }

    [Fact]
    public void LoadMissingMetaThrowsArgumentException()
    {
        // Arrange
        string json = @"{ ""bounds"": { ""x"": 10, ""y"": 10, ""z"": 10 } }";

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => LevelLoader.Load(json));
    }
}
