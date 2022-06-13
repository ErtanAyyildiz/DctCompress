namespace Dct.Core
{
    public sealed class RedGreenBlue
    {
        public RedGreenBlue()
        {
        }

        private short red;
        private short blue;
        private short green;

        public RedGreenBlue(short red, short blue, short green)
        {
            this.red = red;
            this.blue = blue;
            this.green = green;
        }

        public short Red
        {
            get => red; set
            {
                if (value < 0) value = 0;
                if (value <= 255)
                    red = value;
                else
                    red = 255;
            }
        }
        public short Blue
        {
            get => blue; set
            {
                if (value < 0) value = 0;
                if (value <= 255)
                    blue = value;
                else
                    blue = 255;
            }
        }
        public short Green
        {
            get => green; set
            {
                if (value < 0) value = 0;
                if (value <= 255)
                    green = value;
                else
                    green = 255;
            }
        }
    }
}
