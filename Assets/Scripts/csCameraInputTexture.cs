using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csCameraInputRenderTexture : MonoBehaviour
{
    public RenderTexture renderTexture1; // 첫 번째 카메라 입력을 받을 RenderTexture
    public RenderTexture renderTexture2; // 두 번째 카메라 입력을 받을 RenderTexture

    private WebCamTexture webCamTexture1; // 첫 번째 카메라 텍스처
    private WebCamTexture webCamTexture2; // 두 번째 카메라 텍스처

    public bool singleCameraForBothTextures = false; // true면 한 대의 카메라로 두 개의 RenderTexture에 텍스처링

    void Start()
    {
        // 디바이스에서 사용할 수 있는 카메라 장치 가져오기
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length >= 1) // 카메라가 최소 한 대는 있어야 함
        {
            // 첫 번째 카메라 사용
            webCamTexture1 = new WebCamTexture(devices[0].name);
            webCamTexture1.Play(); // 첫 번째 카메라 시작

            // 첫 번째 RenderTexture에 WebCamTexture 데이터 초기화
            StartCoroutine(UpdateRenderTexture(webCamTexture1, renderTexture1));

            if (singleCameraForBothTextures)
            {
                // 두 번째 RenderTexture도 동일한 카메라 데이터로 초기화
                StartCoroutine(UpdateRenderTexture(webCamTexture1, renderTexture2));
            }
            else if (devices.Length > 1) // 카메라가 두 대 이상일 때
            {
                // 두 번째 카메라 사용
                webCamTexture2 = new WebCamTexture(devices[1].name);
                webCamTexture2.Play(); // 두 번째 카메라 시작

                // 두 번째 RenderTexture에 WebCamTexture 데이터 초기화
                StartCoroutine(UpdateRenderTexture(webCamTexture2, renderTexture2));
            }
            else
            {
                Debug.LogWarning("Only one camera detected. The second RenderTexture will not receive a texture if 'singleCameraForBothTextures' is false.");
            }
        }
        else
        {
            Debug.LogWarning("No camera devices found.");
        }
    }

    void OnDestroy()
    {
        // 첫 번째 카메라 스트림 중지
        if (webCamTexture1 != null && webCamTexture1.isPlaying)
        {
            webCamTexture1.Stop();
        }

        // 두 번째 카메라 스트림 중지
        if (webCamTexture2 != null && webCamTexture2.isPlaying)
        {
            webCamTexture2.Stop();
        }
    }

    // RenderTexture를 WebCamTexture로 업데이트하는 코루틴
    private IEnumerator UpdateRenderTexture(WebCamTexture webCamTexture, RenderTexture renderTexture)
    {
        while (true)
        {
            if (webCamTexture.didUpdateThisFrame)
            {
                // WebCamTexture 데이터를 RenderTexture로 복사
                Graphics.Blit(webCamTexture, renderTexture);
            }
            yield return null;
        }
    }
}
