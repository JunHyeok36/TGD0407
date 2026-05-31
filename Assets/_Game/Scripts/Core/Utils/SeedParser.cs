using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace TDG0407.Core.Utils
{

    public static class SeedParser
    {

        /// <summary>
        /// 새로운 16진수-8자리(32비트) 랜덤 시드를 Int형으로 생성합니다.
        /// </summary>
        public static int NewIntSeed()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);

            return BinaryPrimitives.ReadInt32BigEndian(bytes);
        }

        /// <summary>
        /// 새로운 16진수-8자리(32비트) 랜덤 시드를 String형으로 생성합니다.
        /// </summary>
        /// <returns></returns>
        public static string NewStringSeed()
        {
            return ((uint)NewIntSeed()).ToString("X8");
        }

    }

}