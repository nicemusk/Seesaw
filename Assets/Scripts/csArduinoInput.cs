using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class csArduinoInput : MonoBehaviour
{
    Thread IOThread;
    private static SerialPort sp;
    public static string incomingMsg = " ";
    private static string outgoingMsg = " ";
    [SerializeField] public string realData;

    public Animator seesawAnimator;
    public GameObject seesawObject;

    private const int totalFrames = 29; // 애니메이션의 총 프레임 수를 29로 고정
    private const float maxSerialValue = 75f; // 시리얼 값의 최대/최소 값 설정

    public float serialPower = 1f;
    private float currentSerialValue = 0f;
    public float smoothingFactor = 5f;

    // 키보드 입력 관련 변수
    public float keyboardSpeed = 50f;
    public float maxKeyboardValue = 75f;

    // 루프 애니메이션 제어 변수
    public bool Loop = false; // 애니메이션 자동 루프 여부
    public float loopSpeed = 1f; // 루프 애니메이션 속도

    // 아두이노 사용 여부를 제어하는 변수
    public bool UseArduino = true;

    private float loopDirection = 1f; // 루프 애니메이션 방향 제어

    private static void DataThread()
    {
        try
        {
            sp = new SerialPort("/dev/cu.usbserial-1130", 9600);
            sp.Open();

            while (true)
            {
                if (outgoingMsg != " ")
                {
                    sp.Write(outgoingMsg);
                    outgoingMsg = " ";
                }

                incomingMsg = sp.ReadExisting();
                Thread.Sleep(50);
            }
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Arduino not connected or connection error.");
        }
    }

    private void OnDestroy()
    {
        if (IOThread != null && IOThread.IsAlive)
        {
            IOThread.Abort();
            if (sp != null && sp.IsOpen)
            {
                sp.Close();
            }
        }
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }

    void Start()
    {
        if (UseArduino)
        {
            IOThread = new Thread(() => DataThread());
            IOThread.Start();
        }
    }

    private void Update()
    {
        // Use Arduino가 false일 경우 화살표 키로 제어
        if (!UseArduino || sp == null || !sp.IsOpen)
        {
            HandleKeyboardInput(); // 화살표 키로 제어
        }
        else if (incomingMsg != " ")
        {
            HandleArduinoInput(); // 아두이노 입력 처리
        }

        // 루프 애니메이션이 활성화된 경우
        if (Loop)
        {
            HandleLoopAnimation();
        }

        // 애니메이션 및 오브젝트 회전 처리
        ProcessAnimation();
    }

    // 아두이노 입력 처리 함수
    private void HandleArduinoInput()
    {
        realData = incomingMsg;
        incomingMsg = " ";

        float serialValue;
        if (float.TryParse(realData, out serialValue))
        {
            // 시리얼 값을 -75에서 75로 고정
            serialValue = Mathf.Clamp(serialValue * serialPower, -maxSerialValue, maxSerialValue);
            currentSerialValue = Mathf.Lerp(currentSerialValue, serialValue, Time.deltaTime * smoothingFactor);
        }
    }

    // 키보드 입력 처리 함수
    private void HandleKeyboardInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentSerialValue = Mathf.Lerp(currentSerialValue, -maxKeyboardValue, Time.deltaTime * keyboardSpeed);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            currentSerialValue = Mathf.Lerp(currentSerialValue, maxKeyboardValue, Time.deltaTime * keyboardSpeed);
        }
        else
        {
            currentSerialValue = Mathf.Lerp(currentSerialValue, 0f, Time.deltaTime * keyboardSpeed);
        }
    }

    // 루프 애니메이션 처리 함수
    private void HandleLoopAnimation()
    {
        // currentSerialValue를 maxKeyboardValue와 -maxKeyboardValue 사이로 이동시키는 루프
        currentSerialValue += loopDirection * loopSpeed * Time.deltaTime;

        if (currentSerialValue >= maxKeyboardValue)
        {
            loopDirection = -1f; // 방향을 반대로
        }
        else if (currentSerialValue <= -maxKeyboardValue)
        {
            loopDirection = 1f; // 다시 양수 방향으로
        }
    }

    // 애니메이션과 오브젝트 회전 처리 함수
    private void ProcessAnimation()
    {
        // currentSerialValue의 절대값을 0~1 범위로 매핑하여 애니메이션 프레임 수에 맞춤
        float normalizedTime = Mathf.Abs(currentSerialValue) / maxSerialValue; 

        if (currentSerialValue < 0)
        {
            // 시소 오브젝트를 좌우 회전시킬 필요 없이 애니메이션은 똑같이 재생
            seesawObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // 왼쪽 방향
        }
        else
        {
            seesawObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // 오른쪽 방향
        }

        // 애니메이션의 특정 프레임에 맞추어 재생
        seesawAnimator.Play("SeesawAnimation", 0, normalizedTime);
        seesawAnimator.speed = 0f; // 애니메이션 재생 중지, 현재 위치에 고정
    }
}
