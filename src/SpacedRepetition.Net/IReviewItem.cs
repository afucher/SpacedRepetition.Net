using System;

namespace SpacedRepetition.Net
{
    public interface IReviewItem
    {
        int CorrectReviewStreak { get; set; }
        DateTime LastReviewDate { get; set; }
        DifficultyRating DifficultyRating { get; set; }
        bool IsNewItem { get; }
    }
}