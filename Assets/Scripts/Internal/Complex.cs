namespace GanglionUnity.Internal
{
    public class Complex
    {
        public readonly float re;
        public readonly float im;

        public Complex(float re)
        {
            this.re = re;
            this.im = 0;
        }

        public Complex(double re, double im)
        {
            this.re = (float)re;
            this.im = (float)im;
        }

        public Complex(float re, float im)
        {
            this.re = re;
            this.im = im;
        }

        public static implicit operator Complex(double i)
        {
            Complex c = new Complex(i, 0);
            return c;
        }


        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.re + b.re, a.im + b.im);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.re - b.re, a.im - b.im);
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re);
        }


    }
}
