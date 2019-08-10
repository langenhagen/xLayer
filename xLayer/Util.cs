using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xLayer
{
    class Util
    {
        // frame rate dependent members
        private static int __lastTick;
        private static int __lastFrameRate;
        private static int __frameRate;

        /// <summary>
        /// Calculates the frame rate and retrieves it.
        /// </summary>
        /// <returns>An approximation of the frame rate in frames per second.</returns>
        public static int GetFrameRate()
        {
            CalculateFrameRate();

            return __lastFrameRate;
        }


        /// <summary>
        /// Calculates the frame rate.
        /// </summary>
        public static void CalculateFrameRate()
        {
            if (System.Environment.TickCount - __lastTick >= 1000)
            {
                __lastFrameRate = __frameRate;
                __frameRate = 0;
                __lastTick = System.Environment.TickCount;
            }
            __frameRate++;
        }

    }
}
