public static class FibonacciExchange
{
    // exchangeNumber >= 1 -> returns sequence 2,3,5,8,...
    public static int GetExchangeValue(int exchangeNumber)
    {
        if (exchangeNumber <= 1) return 2;
        int a = 2, b = 3;
        for (int i = 2; i <= exchangeNumber; i++)
        {
            int c = a + b;
            a = b;
            b = c;
        }
        return a;
    }
}
