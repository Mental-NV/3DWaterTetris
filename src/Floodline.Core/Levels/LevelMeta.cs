namespace Floodline.Core.Levels;

/// <summary>
/// Metadata for a level.
/// </summary>
public record LevelMeta(
    string Id,
    string Title,
    int SchemaVersion,
    uint Seed
);
