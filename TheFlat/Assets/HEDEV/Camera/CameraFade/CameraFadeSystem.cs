using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Proto.CameraSystem
{
    public class CameraFadeSystem : MonoBehaviour
    {
        private static CameraFadeSystem Instance { get; set; }

        public static Color Color
        {
            get => color;
            set
            {
                color = new Color(value.r, value.g, value.b, alpha);
                material.SetColor(materialColorID, color);
            }
        }

        public static float Alpha
        {
            get => alpha;
            set
            {
                if (!Mathf.Approximately(alpha, value))
                {
                    alpha = value;
                    color.a = value;
                    material.SetColor(materialColorID, color);
                }
            }
        }

        private static Color color = new(0, 0, 0, 0);
        private static float alpha;

        private static Material material;
        private static int materialColorID;
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int ZTest = Shader.PropertyToID("_ZTest");

        private void Awake()
        {
            if (!material)
            {
                materialColorID = Shader.PropertyToID("_Color");

                var shader = Shader.Find("Hidden/Internal-Colored");
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
                // Turn off backface culling, depth writes, depth test.
                material.SetInt(Cull, (int)CullMode.Off);
                material.SetInt(ZWrite, 0);
                material.SetInt(ZTest, (int)CompareFunction.Always);

                Color = color;
            }

            Instance = this;
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        }

        private void OnDestroy()
        {
            Instance = null;
            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        }

        private static void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            OnPostRenderUpdate();
        }

        /// <summary>
        ///     Add a quad in front of camera to fade with color.
        /// </summary>
        public static void OnPostRenderUpdate()
        {
#if UNITY_EDITOR
            //To change fade color and alpha using inspector variables
            if (alpha != color.a || material.GetColor(materialColorID) != color)
            {
                color.a = alpha;
                material.SetColor(materialColorID, color);
            }
#endif

            GL.PushMatrix();
            GL.LoadOrtho();

            // activate the first shader pass (in this case we know it is the only pass)
            material.SetPass(0);
            // draw a quad over whole screen
            GL.Begin(GL.QUADS);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();

            GL.PopMatrix();
        }

        public static void In(float duration = 1, bool restart = false, bool fixedDuration = false)
        {
            if (Instance == null) return;

            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.CameraFadeIn(duration, restart, fixedDuration));
        }

        public static void Out(float duration = 1, bool restart = false, bool fixedDuration = false)
        {
            if (Instance == null) return;

            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.CameraFadeOut(duration, restart, fixedDuration));
        }

        private IEnumerator CameraFadeIn(float duration, bool restart, bool fixedDuration)
        {
            Debug.Log($"[camera fade log] CameraFadeIn ({duration})");
            //Debug.Log("[CameraFade]_______________________ init Fade In");
            //Debug.Log("[CameraFade]init time: " + Time.time);

            if (Mathf.Approximately(duration, 0))
            {
                //Debug.Log("[CameraFade]duration is zero.");
                Alpha = 0;
                yield return null;
                CameraFadeService.OnIn();
                yield break;
            }

            if (restart) Alpha = 1;
            else if (fixedDuration) duration /= Alpha;

            for (var i = Alpha; i - Time.unscaledDeltaTime / duration > 0; i -= Time.unscaledDeltaTime / duration)
            {
                Alpha = i;
                yield return null;
            }
            //Debug.Log("[CameraFade]time end: " + Time.time);

            Alpha = 0;
            yield return null;
            CameraFadeService.OnIn();
        }

        private IEnumerator CameraFadeOut(float duration, bool restart, bool fixedDuration)
        {
            Debug.Log($"[camera fade log] CameraFadeOut ({duration})");
            //Debug.Log("_______________________ init Fade Out");
            //Debug.Log("init time: " + Time.time);
            if (Mathf.Approximately(duration, 0))
            {
                Alpha = 1;
                yield return null;
                CameraFadeService.OnOut();
                yield break;
            }

            if (restart) Alpha = 0;
            else if (fixedDuration) duration /= 1 - Alpha;
            for (var i = Alpha; i + Time.deltaTime / duration < 1; i += Time.deltaTime / duration)
            {
                Alpha = i;
                yield return null;
            }
            //Debug.Log("time end: " + Time.time);

            Alpha = 1;
            yield return null;
            CameraFadeService.OnOut();
        }
    }
}