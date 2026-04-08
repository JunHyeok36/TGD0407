using System;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407.Utils.Extensions
{

    public static class IEnumerableExtension
    {

        /// <summary>
        /// 리스트에서 랜덤한 원소를 반환합니다.
        /// </summary>
        /// <typeparam name="T">IEnumerable을 상속받는 리스트여야 합니다.</typeparam>
        /// <param name="src">랜덤한 원소를 반환받을 리스트를 나타냅니다. (1개 이상의 원소를 가져야 합니다.)</param>
        /// <returns>랜덤한 원소를 T 형태로 반환합니다.</returns>
        public static T ChoiceOne<T>(this IEnumerable<T> src)
        {
            int count = src.Count();
            if(count == 0) throw new ArgumentException($"{nameof(src)}의 원소는 최소 한 개 이상 존재해야 합니다.");

            return src.ElementAt(UnityEngine.Random.Range(0, count));
        }
        
    }
    
}