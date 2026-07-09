using UnityEngine;
using System;

// 인터페이스나 추상 클래스의 자식 클래스들을 인스펙터에서 선택하게 해주는 어트리뷰트
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SubclassSelectorAttribute : PropertyAttribute { }