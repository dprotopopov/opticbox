using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace fzp
{
    public class FzpBuilder : IDisposable
    {
        public enum FzpMode
        {
            Binary = 0,
            Sinusoidal
        }

        private const double MmPerInch = 25.4;

        /// <summary>
        ///     Unlike a standard lens, a binary zone plate produces intensity maxima along the axis of the plate at odd fractions
        ///     (f/3, f/5, f/7, etc.). Although these contain less energy (counts of the spot) than the principal focus (because it
        ///     is wider), they have the same maximum intensity (counts/m^2).
        ///     However, if the zone plate is constructed so that the opacity varies in a gradual, sinusoidal manner, the resulting
        ///     diffraction causes only a single focal point to be formed. This type of zone plate pattern is the equivalent of a
        ///     transmission hologram of a converging lens.
        /// </summary>
        [Description("FZP Mode")]
        public FzpMode PlateMode { get; set; }


        /// <summary>
        ///     the distance in inches
        /// </summary>
        [Description("Distance (in)")]
        public double Distance { get; set; }

        /// <summary>
        ///     Dots-Per-Inch
        ///     X - axe
        /// </summary>
        public double DpiX { get; set; }

        /// <summary>
        ///     Dots-Per-Inch
        ///     Y - axe
        /// </summary>
        public double DpiY { get; set; }

        /// <summary>
        ///     the wavelength of the light the zone plate is meant to focus
        /// </summary>
        [Description("Wavelength (nm)")]
        public double WaveLength { get; set; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Fill array with FZP values
        /// </summary>
        /// <param name="data">FZP values</param>
        public void Fill(float[,,] data)
        {
            long height = data.GetLength(0);
            long width = data.GetLength(1);
            int channels = data.GetLength(2);

            double yCenter = MmPerInch*(height - 1L)/DpiY/2; // mm
            double[] xCenter =
            {
                MmPerInch*(width - 1L)*1.5/DpiX/6, // mm
                MmPerInch*(width - 1L)*2/DpiX/6, // mm
                MmPerInch*(width - 1L)*3/DpiX/6, // mm
                MmPerInch*(width - 1L)*4/DpiX/6, // mm
                MmPerInch*(width - 1L)*4.5/DpiX/6 // mm
            };

            double[] zFocus =
            {
                MmPerInch*Distance, // mm
                MmPerInch*Distance/2, // mm
                MmPerInch*Distance/2, // mm
                MmPerInch*Distance/2, // mm
                MmPerInch*Distance // mm
            };

            long[,] xBound =
            {
                {0L, (long) Math.Floor(1.5*(width - 1L)/6)},
                {(long) Math.Ceiling(1.5*(width - 1L)/6), (long) Math.Floor(2.5*(width - 1L)/6)},
                {(long) Math.Ceiling(2.5*(width - 1L)/6), (long) Math.Floor(3.5*(width - 1L)/6)},
                {(long) Math.Ceiling(3.5*(width - 1L)/6), (long) Math.Floor(4.5*(width - 1L)/6)},
                {(long) Math.Ceiling(4.5*(width - 1L)/6), width - 1L}
            };

            switch (PlateMode)
            {
                case FzpMode.Binary:
                    Parallel.ForEach(Enumerable.Range(0, 5), k =>
                    {
                        double x0 = xCenter[k];
                        double y0 = yCenter;
                        double f = zFocus[k];
                        double f2 = f*f;
                        long xBegin = xBound[k, 0];
                        long xEnd = xBound[k, 1];

                        for (long index = xBegin*height; index <= xEnd*height; index++)
                        {
                            var i = (int) (index/height);
                            var j = (int) (index%height);
                            double x = MmPerInch*i/DpiX;
                            double y = MmPerInch*j/DpiY;
                            double dr = Math.Sqrt(f2 + (x - x0)*(x - x0) + (y - y0)*(y - y0)) - f;
                            var opacity =
                                (float) ((1.0 + Math.Sign(Math.Cos(2000000.0*Math.PI*dr/WaveLength)))*255/2);
                            for (int channel = 0; channel < channels; channel++) data[j, i, channel] = opacity;
                        }
                    });
                    break;
                case FzpMode.Sinusoidal:
                    Parallel.ForEach(Enumerable.Range(0, 5), k =>
                    {
                        double x0 = xCenter[k];
                        double y0 = yCenter;
                        double f = zFocus[k];
                        double f2 = f*f;
                        long xBegin = xBound[k, 0];
                        long xEnd = xBound[k, 1];

                        for (long index = xBegin*height; index <= xEnd*height; index++)
                        {
                            var i = (int) (index/height);
                            var j = (int) (index%height);
                            double x = MmPerInch*i/DpiX;
                            double y = MmPerInch*j/DpiY;
                            double dr = Math.Sqrt(f2 + (x - x0)*(x - x0) + (y - y0)*(y - y0)) - f;
                            var opacity =
                                (float) ((1.0 + Math.Cos(2000000.0*Math.PI*dr/WaveLength))*255/2);
                            for (int channel = 0; channel < channels; channel++) data[j, i, channel] = opacity;
                        }
                    });
                    break;
                default:
                    throw new NotImplementedException();
            }

            for (int k = 0; k < 5; k++)
            {
                long xBegin = xBound[k, 0];
                long xEnd = xBound[k, 1];
                for (int j = 0; j < height; j++)
                    for (int channel = 0; channel < channels; channel++)
                        data[j, xBegin, channel] = data[j, xEnd, channel] = 0;
            }
        }
    }
}