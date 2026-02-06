using System;
using Floodline.Core.Levels;

namespace Floodline.Core;

/// <summary>
/// Represents the deterministic score calculation result.
/// </summary>
/// <param name="Enabled">Whether scoring is enabled for the level.</param>
/// <param name="Score">Computed score value (if enabled), otherwise 0.</param>
public sealed record ScoreEvaluation(bool Enabled, int Score)
{
    /// <summary>
    /// A score evaluation with scoring disabled.
    /// </summary>
    public static ScoreEvaluation Disabled { get; } = new(false, 0);
}

/// <summary>
/// Computes deterministic score for a level based on configured weights and metrics.
/// </summary>
public static class ScoreEvaluator
{
    public static ScoreEvaluation Evaluate(
        Level level,
        int piecesLocked,
        int waterRemovedTotal,
        int rotationsExecuted,
        int shiftVoxelsTotal,
        int lostVoxelsTotal)
    {
        if (level is null)
        {
            throw new ArgumentNullException(nameof(level));
        }

        if (level.Score is null || !level.Score.Enabled)
        {
            return ScoreEvaluation.Disabled;
        }

        // score = perPiece * PiecesUsed + perWaterRemoved * WaterRemovedTotal
        //         - penaltyShiftVoxel * ShiftVoxelsTotal - penaltyLostVoxel * LostVoxelsTotal
        //         - penaltyRotation * RotationsExecuted

        int score = (level.Score.PerPiece * piecesLocked)
            + (level.Score.PerWaterRemoved * waterRemovedTotal)
            - (level.Score.PenaltyShiftVoxel * shiftVoxelsTotal)
            - (level.Score.PenaltyLostVoxel * lostVoxelsTotal)
            - (level.Score.PenaltyRotation * rotationsExecuted);

        return new ScoreEvaluation(true, Math.Max(0, score));
    }
}
