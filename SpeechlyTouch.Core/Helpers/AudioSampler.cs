using System;

namespace SpeechlyTouch.Core.Helpers
{
    public static class AudioSampler
    {
        public static float[] DownsampleBuffer(float[] buffer, uint sampleRate, uint rate)
        {

            if (rate == sampleRate)
            {
                return buffer;
            }
            if (rate > sampleRate)
            {
                throw new Exception("downsampling rate show be smaller than original sample rate");
            }
            int sampleRateRatio = (int)(sampleRate / rate);
            int newLength = buffer.Length / sampleRateRatio;
            var result = new float[newLength];
            var offsetResult = 0;
            var offsetBuffer = 0;
            while (offsetResult < result.Length)
            {
                var nextOffsetBuffer = (int)((offsetResult + 1) * sampleRateRatio);
                // Use average value of skipped samples
                float accum = 0;
                var count = 0;
                for (var i = offsetBuffer; i < nextOffsetBuffer && i < buffer.Length; i++)
                {
                    accum += buffer[i];
                    count++;
                }
                result[offsetResult] = accum / count;
                // Or you can simply get rid of the skipped samples:
                // result[offsetResult] = buffer[nextOffsetBuffer];
                offsetResult++;
                offsetBuffer = nextOffsetBuffer;
            }

            return result;
        }
    }
}
