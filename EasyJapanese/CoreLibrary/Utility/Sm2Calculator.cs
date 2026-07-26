namespace CoreLibrary.Utility
{
    /// <summary>
    /// SM-2 spaced repetition. Quality: 0 = Quên hẳn, 3 = Khó, 4 = Tốt, 5 = Dễ.
    /// </summary>
    public static class Sm2Calculator
    {
        public static (decimal NewEFactor, int NewReviewCount, DateTime NextReviewAt) Calculate(
            decimal currentEFactor, int currentReviewCount, int quality)
        {
            if (quality < 3)
            {
                // Trả lời sai/quên → học lại từ đầu, hẹn ôn lại sau 1 ngày
                var resetEf = AdjustEFactor(currentEFactor, quality);
                return (resetEf, 0, DateTime.UtcNow.AddDays(1));
            }

            var newReviewCount = currentReviewCount + 1;

            int intervalDays = newReviewCount switch
            {
                1 => 1,
                2 => 6,
                _ => (int)Math.Round(6 * Math.Pow((double)currentEFactor, newReviewCount - 2))
            };

            var newEf = AdjustEFactor(currentEFactor, quality);
            return (newEf, newReviewCount, DateTime.UtcNow.AddDays(intervalDays));
        }

        private static decimal AdjustEFactor(decimal ef, int quality)
        {
            var delta = 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02);
            var newEf = ef + (decimal)delta;
            return newEf < 1.3m ? 1.3m : newEf;
        }
    }
}