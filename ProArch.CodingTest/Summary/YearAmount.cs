namespace ProArch.CodingTest.Summary
{
    public class YearAmount
    {
        public int Year { get; }
        public decimal Amount { get; }

        public YearAmount(int year, decimal amount)
        {
            Year = year;
            Amount = amount;
        }
    }
}
