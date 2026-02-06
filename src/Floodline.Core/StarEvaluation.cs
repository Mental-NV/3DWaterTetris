using System;
using System.Collections.Generic;
using Floodline.Core.Levels;

namespace Floodline.Core;

/// <summary>
/// Represents the star achievement for a level.
/// </summary>
/// <param name="StarCount">Number of stars achieved (1, 2, or 3).</param>
/// <param name="Star2Conditions">Evaluation results for star 2 conditions (if any).</param>
/// <param name="Star3Conditions">Evaluation results for star 3 conditions (if any).</param>
public sealed record StarEvaluation(
    int StarCount,
    IReadOnlyList<StarConditionEvaluation> Star2Conditions,
    IReadOnlyList<StarConditionEvaluation> Star3Conditions)
{
    /// <summary>
    /// An empty star evaluation (no stars).
    /// </summary>
    public static StarEvaluation Empty { get; } = new(0, [], []);
}

/// <summary>
/// Represents evaluation of a single star condition.
/// </summary>
/// <param name="Type">Condition type identifier.</param>
/// <param name="Current">Current value.</param>
/// <param name="Target">Target value.</param>
/// <param name="Completed">Whether the condition is completed.</param>
public sealed record StarConditionEvaluation(string Type, int Current, int Target, bool Completed);

/// <summary>
/// Evaluates star achievement based on objectives and star conditions.
/// </summary>
public static class StarEvaluator
{
    public static StarEvaluation Evaluate(
        ObjectiveEvaluation objectives,
        Level level,
        int piecesLocked,
        int rotationsExecuted,
        int shiftVoxelsTotal,
        int lostVoxelsTotal)
    {
        if (objectives is null)
        {
            throw new ArgumentNullException(nameof(objectives));
        }

        if (level is null)
        {
            throw new ArgumentNullException(nameof(level));
        }

        // Star 1: all primary objectives completed
        int starCount = objectives.AllCompleted ? 1 : 0;

        // Star 2: all star2 conditions met
        List<StarConditionEvaluation> star2Evals = [];
        if (level.Stars?.Star2 is not null && level.Stars.Star2.Count > 0)
        {
            foreach (StarConditionConfig condition in level.Stars.Star2)
            {
                star2Evals.Add(EvaluateCondition(
                    condition,
                    piecesLocked,
                    rotationsExecuted,
                    shiftVoxelsTotal,
                    lostVoxelsTotal));
            }

            bool allStar2Met = star2Evals.TrueForAll(c => c.Completed);
            if (allStar2Met)
            {
                starCount = 2;
            }
        }

        // Star 3: all star3 conditions met (in addition to star 2)
        List<StarConditionEvaluation> star3Evals = [];
        if (starCount >= 2 && level.Stars?.Star3 is not null && level.Stars.Star3.Count > 0)
        {
            foreach (StarConditionConfig condition in level.Stars.Star3)
            {
                star3Evals.Add(EvaluateCondition(
                    condition,
                    piecesLocked,
                    rotationsExecuted,
                    shiftVoxelsTotal,
                    lostVoxelsTotal));
            }

            bool allStar3Met = star3Evals.TrueForAll(c => c.Completed);
            if (allStar3Met)
            {
                starCount = 3;
            }
        }

        return new StarEvaluation(starCount, star2Evals, star3Evals);
    }

    private static StarConditionEvaluation EvaluateCondition(
        StarConditionConfig condition,
        int piecesLocked,
        int rotationsExecuted,
        int shiftVoxelsTotal,
        int lostVoxelsTotal)
    {
        if (condition.Params is null)
        {
            throw new ArgumentException($"Star condition '{condition.Type}' is missing params.");
        }

        string type = condition.Type ?? string.Empty;
        string normalized = NormalizeType(type);

        return normalized switch
        {
            "MAXPIECESUSED" => EvaluateMaxPiecesUsed(type, condition.Params, piecesLocked),
            "MAXROTATIONSUSED" => EvaluateMaxRotationsUsed(type, condition.Params, rotationsExecuted),
            "MAXSHIFTVOXELSTOTAL" => EvaluateMaxShiftVoxelsTotal(type, condition.Params, shiftVoxelsTotal),
            "MAXLOSTVOXELSTOTAL" => EvaluateMaxLostVoxelsTotal(type, condition.Params, lostVoxelsTotal),
            _ => throw new ArgumentException($"Unknown star condition type '{condition.Type}'.")
        };
    }

    private static StarConditionEvaluation EvaluateMaxPiecesUsed(
        string type,
        Dictionary<string, object> parameters,
        int piecesLocked)
    {
        int maxPieces = GetRequiredInt(parameters, "count");
        bool completed = piecesLocked <= maxPieces;
        return new StarConditionEvaluation(type, piecesLocked, maxPieces, completed);
    }

    private static StarConditionEvaluation EvaluateMaxRotationsUsed(
        string type,
        Dictionary<string, object> parameters,
        int rotationsExecuted)
    {
        int maxRotations = GetRequiredInt(parameters, "count");
        bool completed = rotationsExecuted <= maxRotations;
        return new StarConditionEvaluation(type, rotationsExecuted, maxRotations, completed);
    }

    private static StarConditionEvaluation EvaluateMaxShiftVoxelsTotal(
        string type,
        Dictionary<string, object> parameters,
        int shiftVoxelsTotal)
    {
        int maxShift = GetRequiredInt(parameters, "count");
        bool completed = shiftVoxelsTotal <= maxShift;
        return new StarConditionEvaluation(type, shiftVoxelsTotal, maxShift, completed);
    }

    private static StarConditionEvaluation EvaluateMaxLostVoxelsTotal(
        string type,
        Dictionary<string, object> parameters,
        int lostVoxelsTotal)
    {
        int maxLost = GetRequiredInt(parameters, "count");
        bool completed = lostVoxelsTotal <= maxLost;
        return new StarConditionEvaluation(type, lostVoxelsTotal, maxLost, completed);
    }

    private static int GetRequiredInt(Dictionary<string, object> parameters, string key)
    {
        if (parameters is null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        if (!parameters.TryGetValue(key, out object? value))
        {
            throw new ArgumentException($"Required parameter '{key}' not found.");
        }

        if (value is int intValue)
        {
            return intValue;
        }

        if (value is long longValue)
        {
            return (int)longValue;
        }

        if (double.TryParse(value?.ToString() ?? string.Empty, out double dblValue))
        {
            return (int)dblValue;
        }

        throw new ArgumentException($"Parameter '{key}' is not a valid integer.");
    }

    private static string NormalizeType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return string.Empty;
        }

        return type.ToUpperInvariant().Replace(" ", string.Empty);
    }
}
