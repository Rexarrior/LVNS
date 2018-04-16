using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace JoysticControl
{
    class JsonVec
    {
        public double x;
        public double y;

        public static JsonVec Parse(byte[] data)
        {
            
            BitArray bits = new BitArray(data);

            bool isFin = bits[0];


            bool isMask = bits[0];

            if (isMask)
                throw new NotImplementedException();

            int sum = 0;
            int mn = 1; 
            for (int i = 15; i >= 9; i--)
            {
                if (bits[i])
                    sum += mn;
                mn *= 2;
            }
            int n;
            if (sum < 126)
                n = sum;
            else
               // if( sum == 126)
            {
                throw new NotImplementedException();
                sum = 0;
                mn = 1;
                for (int i = 6; i >= 1; i--)
                {
                    if (bits[i])
                        sum += mn;
                    mn *= 2;
                }
            }

            String str = Encoding.UTF8.GetString(data, 2, n + 2);


            str = str.Replace("\0", "");
            return JsonConvert.DeserializeObject<JsonVec>(str);

        }

        public JsonVec(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
