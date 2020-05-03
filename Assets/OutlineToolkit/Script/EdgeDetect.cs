using UnityEngine;

public class EdgeDetect : MonoBehaviour {

    [Range(0f, 10f)]
    public float depthSensitivity = 1;
    float prevDepthSensitivity;
    [Range(0f, 10f)]
    public float depthSensitivity2 = 1;
    float prevDepth2Sensitivity;
    [Range(0f, 10f)]
    public float normalsSensitivity = 1;
    float prevNormalsSensitivity;

    public DebugMode debugMode = DebugMode.none;
    DebugMode prevDebugMode;

    public Color outlineColor = Color.black;
    Color prevOutlineColor;

    const float farClipPlaneMultiplicand = 1.75f;

    RenderTexture depthTexture;
    RenderTexture normalsTexture;
    Camera cam;
    [HideInInspector]
    public Shader encodedDepthShader;
    Material edgeDetectMat;
    [HideInInspector]
    public Shader edgeDetectShader;
    public Camera depthCapturingCamera;
    [HideInInspector]
    public Shader edgeCombineShader;
    RenderTexture edgeTexture;
    Material edgeCombineMat;

    [HideInInspector]
    public Shader normalsShader;
    public Camera normalsCapturingCamera;

    int width = -1;
    int height = -1;

    void LateUpdate() {

        if (cam == null && width != -1) {
            cam = GetComponent<Camera>();

            depthTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            normalsTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            edgeTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32); //just need boolean

            depthTexture.filterMode = FilterMode.Point;
            normalsTexture.filterMode = FilterMode.Point;
            edgeTexture.filterMode = FilterMode.Point;

            normalsCapturingCamera.targetTexture = normalsTexture;
            normalsCapturingCamera.gameObject.SetActive(true);
            normalsCapturingCamera.SetReplacementShader(normalsShader, "RenderType");

            edgeDetectMat = new Material(edgeDetectShader);
            edgeDetectMat.SetTexture("_NormalsTex", normalsTexture);
            edgeDetectMat.SetVector("_SensitivityAndWidthArgs", new Vector4(depthSensitivity, normalsSensitivity, 0, 0));
            prevNormalsSensitivity = normalsSensitivity;
            prevDepthSensitivity = depthSensitivity;

            Shader.SetGlobalInt("_EdgedetectDebugMode", (int)debugMode);
            prevDebugMode = debugMode;

            edgeCombineMat = new Material(edgeCombineShader);
            edgeCombineMat.SetTexture("_EdgeTex", edgeTexture);
            edgeCombineMat.color = outlineColor;
            prevOutlineColor = outlineColor;

            depthCapturingCamera.SetReplacementShader(encodedDepthShader, "RenderType");
            depthCapturingCamera.targetTexture = depthTexture;
            depthCapturingCamera.gameObject.SetActive(true);
        }

        if (cam != null) {
            Vector3 a = -cam.transform.InverseTransformPoint(cam.ScreenToWorldPoint(Vector3.forward));
            a.z = cam.farClipPlane * farClipPlaneMultiplicand;

            edgeDetectMat.SetMatrix("_Cam2World", Matrix4x4.TRS(Vector3.zero, cam.transform.rotation, Vector3.one));
            Shader.SetGlobalVector("_EdgeDetectDepthArgs", a);

            normalsCapturingCamera.fieldOfView = depthCapturingCamera.fieldOfView = cam.fieldOfView;
            normalsCapturingCamera.farClipPlane = depthCapturingCamera.farClipPlane = cam.farClipPlane;

            if (prevNormalsSensitivity != normalsSensitivity || prevDepthSensitivity != depthSensitivity || prevDepth2Sensitivity != depthSensitivity2) {
                edgeDetectMat.SetVector("_SensitivityAndWidthArgs", new Vector4(normalsSensitivity, depthSensitivity, depthSensitivity2, 0));
                prevNormalsSensitivity = normalsSensitivity;
                prevDepthSensitivity = depthSensitivity;
                prevDepth2Sensitivity = depthSensitivity2;
            }
            if (debugMode != prevDebugMode) {
                Shader.SetGlobalInt("_EdgedetectDebugMode", (int)debugMode);
                prevDebugMode = debugMode;
            }
            if (prevOutlineColor != outlineColor) {
                edgeCombineMat.color = outlineColor;
                prevOutlineColor = outlineColor;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (cam == null) {
            Graphics.Blit(source, destination);
            width = source.width;
            height = source.height;
        } else {
            Graphics.Blit(depthTexture, edgeTexture, edgeDetectMat);
            Graphics.Blit(source, destination, edgeCombineMat);
        }
    }

    public enum DebugMode {
        none,
        outlines,
        normals,
        simpleDepth,
        depthCompression,
        worldSpace
    }
}
