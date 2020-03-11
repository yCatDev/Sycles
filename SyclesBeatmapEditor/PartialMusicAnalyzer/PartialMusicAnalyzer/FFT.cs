using System;

namespace PartialMusicAnalyzer
{
    public class FFT
    {
        double pi;
        ulong fundamental_frequency;
        private float[] vector;

        public FFT()
        {
            pi = 4 * Math.Atan((double) 1);
            vector = null;
        }


        // FFT 1D
        public float[] ComplexFFT(float[] data, ulong number_of_samples, uint sample_rate, int sign)
        {
            try
            {
                //variables for the fft
                ulong n, mmax, m, j, istep, i;
                float wtemp, wr, wpr, wpi, wi, theta, tempr, tempi;

                //the complex array is real+complex so the array 
                //as a size n = 2* number of complex samples
                //real part is the data[index] and 
                //the complex part is the data[index+1]


                vector = new float [2 * sample_rate];

                //put the real array in a complex array
                //the complex part is filled with 0's
                //the remaining vector with no data is filled with 0's
                for (n = 0; n < sample_rate; n++)
                {
                    if (n < number_of_samples)
                        vector[2 * n] = data[n];
                    else
                        vector[2 * n] = 0;
                    vector[2 * n + 1] = 0;
                }

                //binary inversion (note that the indexes 
                //start from 0 witch means that the
                //real part of the complex is on the even-indexes 
                //and the complex part is on the odd-indexes)
                n = sample_rate << 1;
                j = 0;
                for (i = 0; i < n / 2; i += 2)
                {
                    if (j > i)
                    {
                        SWAP(ref vector[j], ref vector[i]);
                        SWAP(ref vector[j + 1], ref vector[i + 1]);
                        if ((j / 2) < (n / 4))
                        {
                            SWAP(ref vector[(n - (i + 2))], ref vector[(n - (j + 2))]);
                            SWAP(ref vector[(n - (i + 2)) + 1], ref vector[(n - (j + 2)) + 1]);
                        }
                    }

                    m = n >> 1;
                    while (m >= 2 && j >= m)
                    {
                        j -= m;
                        m >>= 1;
                    }

                    j += m;
                }
                //end of the bit-reversed order algorithm

                //Danielson-Lanzcos routine
                mmax = 2;
                while (n > mmax)
                {
                    istep = mmax << 1;
                    theta = (float) (sign * (2 * pi / mmax));
                    wtemp = (float) Math.Sin(0.5 * theta);
                    wpr = -2.0f * wtemp * wtemp;
                    wpi = (float) Math.Sin(theta);
                    wr = 1.0f;
                    wi = 0.0f;
                    for (m = 1; m < mmax; m += 2)
                    {
                        for (i = m; i <= n; i += istep)
                        {
                            j = i + mmax;
                            tempr = wr * vector[j - 1] - wi * vector[j];
                            tempi = wr * vector[j] + wi * vector[j - 1];
                            vector[j - 1] = vector[i - 1] - tempr;
                            vector[j] = vector[i] - tempi;
                            vector[i - 1] += tempr;
                            vector[i] += tempi;
                        }

                        wr = (wtemp = wr) * wpr - wi * wpi + wr;
                        wi = wi * wpr + wtemp * wpi + wi;
                    }

                    mmax = istep;
                }
                //end of the algorithm

                //determine the fundamental frequency
                //look for the maximum absolute value in the complex array
                fundamental_frequency = 0;
                for (i = 2; i <= sample_rate; i += 2)
                {
                    if ((Math.Pow(vector[i], 2) + Math.Pow(vector[i + 1], 2)) >
                        (Math.Pow(vector[fundamental_frequency], 2) + Math.Pow(vector[fundamental_frequency + 1], 2)))
                    {
                        fundamental_frequency = i;
                    }
                }

                //since the array of complex has the format [real][complex]=>[absolute value]
                //the maximum absolute value must be ajusted to half
                fundamental_frequency = (ulong) Math.Floor((float) fundamental_frequency / 2);
            }
            catch
            {
                ;
            }
                return vector;
            
        }

        private void SWAP(ref float a, ref float b)
        {
            var t = a;
            a = b;
            b = t;
        }
    }
}