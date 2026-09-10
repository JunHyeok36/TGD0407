using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDG0407._prototype.Editor
{
    [Overlay(typeof(SceneView), "IsometricCameraOverlay", "Isometric Camera Tools", true)]
    public class IsometricSceneViewOverlay : Overlay
    {
        // 씬 뷰 카메라가 내려다보는 각도 (Pitch)
        private const float PITCH_ANGLE = 45f;
        
        public override VisualElement CreatePanelContent()
        {
            // UI 레이아웃 설정
            var root = new VisualElement 
            { 
                style = 
                { 
                    flexDirection = FlexDirection.Row,
                    paddingTop = 2, paddingBottom = 2, paddingLeft = 2, paddingRight = 2,
                    backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.8f))
                } 
            };

            var btnIso = new Button(SnapToIso) { text = "ISO Snap" };
            btnIso.style.unityFontStyleAndWeight = FontStyle.Bold;
            btnIso.style.marginRight = 5;

            var btnRotLeft = new Button(() => RotateYaw(-45f)) { text = "Q (-45°)" };
            var btnRotRight = new Button(() => RotateYaw(45f)) { text = "E (+45°)" };

            root.Add(btnIso);
            root.Add(btnRotLeft);
            root.Add(btnRotRight);

            return root;
        }

        private void SnapToIso()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // 2D 모드 해제
                sceneView.in2DMode = false;
                
                // 직교 투영(Orthographic) 및 아이소메트릭 각도(Pitch 45, Yaw 45)로 카메라 정렬
                sceneView.LookAt(sceneView.pivot, Quaternion.Euler(PITCH_ANGLE, 45f, 0f), sceneView.size, true);
                sceneView.Repaint();
            }
        }

        private void RotateYaw(float angleDelta)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // 2D 모드 해제
                sceneView.in2DMode = false;

                Vector3 currentEuler = sceneView.rotation.eulerAngles;
                
                // 현재 각도를 45도 단위로 반올림하여 스냅
                float currentYaw = Mathf.Round(currentEuler.y / 45f) * 45f;
                float newYaw = currentYaw + angleDelta;
                
                // 회전 적용 (현재의 피벗, 사이즈, 투영 방식 유지하되 Pitch는 45도로 고정)
                sceneView.LookAt(sceneView.pivot, Quaternion.Euler(PITCH_ANGLE, newYaw, 0f), sceneView.size, sceneView.orthographic);
                sceneView.Repaint();
            }
        }
    }
}
