using System;
using System.Collections;
using System.Collections.Generic;
using SpacedRepetition.Net.ReviewStrategies;

namespace SpacedRepetition.Net
{
    public class StudySession<T>  : IEnumerable<T> 
        where T : IReviewItem
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly List<T> _revisionList = new List<T>(); 
        private int _newCardsReturned;
        private int _existingCardsReturned;


        public IReviewStrategy ReviewStrategy { get; set; }
        public int MaxNewCards { get; set; }
        public int MaxExistingCards { get; set; }
        public IClock Clock { get; set; }


        public StudySession(IEnumerable<T> items)
        {
            _enumerator = items.GetEnumerator();

            ReviewStrategy = new SuperMemo2ReviewStrategy();
            Clock = new Clock();
            MaxNewCards = 25;
            MaxExistingCards = 100;
        }

        public void Review(T item, ReviewOutcome outcome)
        {
            if (outcome != ReviewOutcome.Incorrect)
            {
                item.CorrectReviewStreak++;
                item.PreviousCorrectReview = item.ReviewDate;
                _revisionList.Remove(item);
            }
            else
            {
                item.CorrectReviewStreak = 0;
                item.PreviousCorrectReview = DateTime.MinValue;
                if(!_revisionList.Contains(item))
                    _revisionList.Add(item);
            }

            item.ReviewDate = Clock.Now();
            item.DifficultyRating = ReviewStrategy.AdjustDifficulty(item, outcome);
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (_enumerator.MoveNext())
            {
                var item = _enumerator.Current;
                if (IsDue(item))
                {
                    if (IsNewItem(item))
                    {
                        _newCardsReturned++;
                        if (_newCardsReturned <= MaxNewCards)
                            yield return item;
                    }
                    else
                    {
                        _existingCardsReturned++;
                        if (_existingCardsReturned <= MaxExistingCards)
                            yield return item;
                    }
                }
            }

            while (_revisionList.Count > 0)
            {
                yield return _revisionList[0];
            }
        }

        private static bool IsNewItem(IReviewItem item)
        {
            return item.ReviewDate == DateTime.MinValue;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsDue(IReviewItem item)
        {
            var nextReview = ReviewStrategy.NextReview(item);
            return nextReview <= Clock.Now();
        }
    }
}