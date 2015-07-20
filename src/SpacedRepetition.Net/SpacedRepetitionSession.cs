using System.Collections;
using System.Collections.Generic;
using SpacedRepetition.Net.ReviewStrategies;

namespace SpacedRepetition.Net
{
    public class SpacedRepetitionSession<T>  : IEnumerable<T> 
        where T : ISpacedRepetitionItem
    {
        private readonly IEnumerator<T> _enumerator;
        private int _newCardsReturned;
        private int _existingCardsReturned;


        public IReviewStrategy ReviewStrategy { get; set; }
        public int MaxNewCards { get; set; }
        public int MaxExistingCards { get; set; }
        public IClock Clock { get; set; }


        public SpacedRepetitionSession(IEnumerable<T> items)
        {
            _enumerator = items.GetEnumerator();

            ReviewStrategy = new SuperMemo2SrsStrategy();
            Clock = new Clock();
            MaxNewCards = 25;
            MaxExistingCards = 100;
        }

        public void Answer(SpacedRepetitionItem item, SrsAnswer answer)
        {
            if (answer != SrsAnswer.Incorrect)
            {
                item.CorrectReviewStreak++;
            }
            else
            {
                item.CorrectReviewStreak = 0;
            }

            item.LastReviewDate = Clock.Now();
            item.DifficultyRating = ReviewStrategy.AdjustDifficulty(item, answer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (_enumerator.MoveNext())
            {
                var item = _enumerator.Current;
                var nextReview = ReviewStrategy.NextReview(item);
                if (nextReview <= Clock.Now())
                {
                    if (item.IsNewItem)
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
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}