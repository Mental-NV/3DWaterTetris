using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Floodline.Core.Movement;

namespace Floodline.Core.Replay;

/// <summary>
/// Serialize and deserialize replay files in a stable JSON format.
/// </summary>
public static class ReplaySerializer
{
    public static string Serialize(ReplayFile replay)
    {
        if (replay is null)
        {
            throw new ArgumentNullException(nameof(replay));
        }

        ValidateReplay(replay);

        ReplayInput[] orderedInputs =
        [
            .. replay.Inputs.OrderBy(input => input.Tick)
        ];

        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });

        writer.WriteStartObject();
        writer.WritePropertyName("meta");
        writer.WriteStartObject();
        WriteMeta(writer, replay.Meta);
        writer.WriteEndObject();

        writer.WritePropertyName("inputs");
        writer.WriteStartArray();
        foreach (ReplayInput input in orderedInputs)
        {
            writer.WriteStartObject();
            writer.WriteNumber("tick", input.Tick);
            writer.WriteString("command", input.Command.ToString());
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static ReplayFile Deserialize(string json)
    {
        if (json is null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        using JsonDocument doc = JsonDocument.Parse(json);
        return Parse(doc.RootElement);
    }

    private static ReplayFile Parse(JsonElement root)
    {
        if (!root.TryGetProperty("meta", out JsonElement metaElement) || metaElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Replay meta section is missing or invalid.");
        }

        ReplayMeta meta = new(
            ReadString(metaElement, "replayVersion"),
            ReadString(metaElement, "rulesVersion"),
            ReadString(metaElement, "levelId"),
            ReadString(metaElement, "levelHash"),
            ReadInt(metaElement, "seed"),
            ReadInt(metaElement, "tickRate"),
            ReadString(metaElement, "platform"),
            ReadString(metaElement, "inputEncoding"));

        if (!root.TryGetProperty("inputs", out JsonElement inputsElement) || inputsElement.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("Replay inputs section is missing or invalid.");
        }

        List<ReplayInput> inputs = [];
        HashSet<int> seenTicks = [];
        foreach (JsonElement element in inputsElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Replay input entry must be an object.");
            }

            int tick = ReadInt(element, "tick");
            string commandText = ReadString(element, "command");

            if (tick < 0)
            {
                throw new ArgumentException("Replay input tick must be non-negative.");
            }

            if (!Enum.TryParse(commandText, ignoreCase: false, out InputCommand command))
            {
                throw new ArgumentException($"Unknown replay command '{commandText}'.");
            }

            if (!seenTicks.Add(tick))
            {
                throw new ArgumentException($"Duplicate replay input for tick {tick}.");
            }

            inputs.Add(new ReplayInput(tick, command));
        }

        inputs.Sort((left, right) => left.Tick.CompareTo(right.Tick));
        ReplayFile replay = new(meta, inputs);
        ValidateReplay(replay);
        return replay;
    }

    private static void ValidateReplay(ReplayFile replay)
    {
        ValidateMeta(replay.Meta);

        if (replay.Inputs is null)
        {
            throw new ArgumentException("Replay inputs cannot be null.", nameof(replay));
        }

        HashSet<int> ticks = [];
        foreach (ReplayInput input in replay.Inputs)
        {
            if (input.Tick < 0)
            {
                throw new ArgumentException("Replay input tick must be non-negative.", nameof(replay));
            }

            if (!ticks.Add(input.Tick))
            {
                throw new ArgumentException($"Duplicate replay input for tick {input.Tick}.", nameof(replay));
            }
        }
    }

    private static void ValidateMeta(ReplayMeta meta)
    {
        if (meta is null)
        {
            throw new ArgumentNullException(nameof(meta));
        }

        RequireText(meta.ReplayVersion, nameof(meta.ReplayVersion));
        RequireText(meta.RulesVersion, nameof(meta.RulesVersion));
        RequireText(meta.LevelId, nameof(meta.LevelId));
        RequireText(meta.LevelHash, nameof(meta.LevelHash));
        RequireText(meta.Platform, nameof(meta.Platform));
        RequireText(meta.InputEncoding, nameof(meta.InputEncoding));

        if (meta.TickRate <= 0)
        {
            throw new ArgumentException("Replay tickRate must be positive.", nameof(meta));
        }
    }

    private static void WriteMeta(Utf8JsonWriter writer, ReplayMeta meta)
    {
        writer.WriteString("replayVersion", meta.ReplayVersion);
        writer.WriteString("rulesVersion", meta.RulesVersion);
        writer.WriteString("levelId", meta.LevelId);
        writer.WriteString("levelHash", meta.LevelHash);
        writer.WriteNumber("seed", meta.Seed);
        writer.WriteNumber("tickRate", meta.TickRate);
        writer.WriteString("platform", meta.Platform);
        writer.WriteString("inputEncoding", meta.InputEncoding);
    }

    private static string ReadString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException($"Replay field '{name}' must be a string.");
        }

        string? text = value.GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException($"Replay field '{name}' must be a non-empty string.");
        }

        return text;
    }

    private static int ReadInt(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.Number)
        {
            throw new ArgumentException($"Replay field '{name}' must be an integer.");
        }

        try
        {
            return value.GetInt32();
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException or OverflowException)
        {
            throw new ArgumentException($"Replay field '{name}' must be an integer.", ex);
        }
    }

    private static void RequireText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Replay field '{name}' must be a non-empty string.");
        }
    }
}
