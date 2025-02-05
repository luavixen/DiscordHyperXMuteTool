using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using DiscordHyperXMuteTool.Properties;

namespace DiscordHyperXMuteTool
{
    internal class DiscordTrayIcon
    {
        public enum ImageType
        {
            Default,
            DefaultPing,
            VoiceInactive,
            VoiceActive,
            Muted,
            Deafened,
        }

        public static readonly IReadOnlyDictionary<ImageType, Bitmap> Images = new Dictionary<ImageType, Bitmap>
        {
            { ImageType.Default, Resources.DiscordTrayIcon_Default },
            { ImageType.DefaultPing, Resources.DiscordTrayIcon_DefaultPing },
            { ImageType.VoiceInactive, Resources.DiscordTrayIcon_VoiceInactive },
            { ImageType.VoiceActive, Resources.DiscordTrayIcon_VoiceActive },
            { ImageType.Muted, Resources.DiscordTrayIcon_Muted },
            { ImageType.Deafened, Resources.DiscordTrayIcon_Deafened },
        };

        public static Bitmap ByteArrayToBitmap(byte[] bitmapBytes)
        {
            using (var stream = new MemoryStream(bitmapBytes))
            {
                return new Bitmap(stream);
            }
        }

        // Compute average hash of an image
        private static ulong ComputeAverageHash(Bitmap image, int hashSize = 8)
        {
            // Resize the image to hashSize x hashSize
            using (var imageResized = new Bitmap(hashSize, hashSize))
            {
                using (var graphics = Graphics.FromImage(imageResized))
                {
                    graphics.DrawImage(image, 0, 0, hashSize, hashSize);
                }

                // Convert to grayscale and compute average value

                byte[] grayValues = new byte[hashSize * hashSize];
                int grayValuesIndex = 0;

                long grayValuesSum = 0;

                for (int y = 0; y < hashSize; y++)
                {
                    for (int x = 0; x < hashSize; x++)
                    {
                        var color = imageResized.GetPixel(x, y);
                        var gray = (byte)((color.R + color.G + color.B) / 3);
                        grayValues[grayValuesIndex++] = gray;
                        grayValuesSum += gray;
                    }
                }

                byte avg = (byte)(grayValuesSum / (hashSize * hashSize));

                // Build the hash, set bit to 1 if pixel is above average

                ulong hash = 0;

                for (int i = 0; i < grayValues.Length; i++)
                {
                    if (grayValues[i] >= avg)
                    {
                        hash |= 1UL << i;
                    }
                }

                return hash;
            }
        }

        // Compute Hamming distance between two hashes
        private static int HammingDistance(ulong hash1, ulong hash2)
        {
            var value = hash1 ^ hash2;
            int distance = 0;
            while (value != 0)
            {
                distance += (int)(value & 1);
                value >>= 1;
            }
            return distance;
        }

        // Compares two images using Hamming distance of their average hashes
        private static int CompareImages(Bitmap image1, Bitmap image2, int hashSize = 8)
        {
            ulong hash1 = ComputeAverageHash(image1, hashSize);
            ulong hash2 = ComputeAverageHash(image2, hashSize);
            return HammingDistance(hash1, hash2);
        }

        // Finds the closest image type to the given image, or null if none are close enough
        public static ImageType? FindClosestImageType(Bitmap image)
        {
            var distances = Images.ToDictionary(pair => pair.Key, pair => CompareImages(image, pair.Value));
            var distanceClosest = distances.OrderBy(pair => pair.Value).First();
            return distanceClosest.Value <= 5 ? distanceClosest.Key : (ImageType?)null;
        }
    }
}
