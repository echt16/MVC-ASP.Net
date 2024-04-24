namespace Lab8
{
    public class Fraction
    {
        public int Numerator { get; set; }
        public int Denominator { get; set; }

        private int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                if(temp != 0)
                    a = temp;
            }
            return a;
        }

        public override string ToString()
        {
            if (Denominator != 1)
                return Numerator + "/" + Denominator;
            else
                return Numerator.ToString();
        }

        public void Reduce()
        {
            int gcd = GreatestCommonDivisor(Math.Abs(Numerator), Math.Abs(Denominator));
            Numerator /= gcd;
            Denominator /= gcd;

            if (Denominator < 0)
            {
                Numerator *= -1;
                Denominator *= -1;
            }
        }
        public static Fraction operator +(Fraction fraction1, Fraction fraction2)
        {
            int numerator = fraction1.Numerator * fraction2.Denominator + fraction2.Numerator * fraction1.Denominator;
            int denominator = fraction2.Denominator * fraction1.Denominator;
            return new Fraction() { Denominator = denominator, Numerator = numerator };
        }
        public static Fraction operator -(Fraction fraction1, Fraction fraction2)
        {
            int numerator = fraction1.Numerator * fraction2.Denominator - fraction2.Numerator * fraction1.Denominator;
            int denominator = fraction2.Denominator * fraction1.Denominator;
            return new Fraction() { Denominator = denominator, Numerator = numerator };
        }
        public static Fraction operator *(Fraction fraction1, Fraction fraction2)
        {
            int numerator = fraction1.Numerator * fraction2.Numerator;
            int denominator = fraction2.Denominator * fraction1.Denominator;
            return new Fraction() { Denominator = denominator, Numerator = numerator };
        }
        public static Fraction operator /(Fraction fraction1, Fraction fraction2)
        {
            int numerator = fraction1.Numerator * fraction2.Denominator;
            int denominator = fraction2.Numerator * fraction1.Denominator;
            return new Fraction() { Denominator = denominator, Numerator = numerator };
        }
    }

}
