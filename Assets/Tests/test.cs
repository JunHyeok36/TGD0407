using TDG0407.Core.Grid;
using TDG0407.Core.Value;
using UnityEngine;

public class TEST_SCRIPT : MonoBehaviour
{
    [SerializeField] private Point p;
    [SerializeField] private BoundedValue<float> fv;
    [SerializeField] private BoundedValue<int> iv;
    [SerializeField] private BoundedValue<Point> pv;
}
