using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyAsyncBasicsManager : MonoBehaviour
{
    #region Section 1: 동기 vs 비동기
    [Header("=== 1. Sync vs Async ===")]
    public Button syncDownloadButton;
    public Button asyncDownloadButton;
    public TextMeshProUGUI section1StatusText;
    #endregion

    #region Section 2: Task.Delay
    [Header("=== 2. Task.Delay ===")]
    public Button delay1SecButton;
    public Button delay3SecButton;
    public Button cancellableDelayButton;
    public Button cancelDelayButton;
    public TextMeshProUGUI section2TimerText;

    private CancellationTokenSource delayCts;
    #endregion

    #region Section 3: Task.WhenAll (병렬 실행)
    [Header("=== 3. Task.WhenAll (Parallel) ===")]
    public Button sequentialDownloadButton;
    public Button parallelDownloadButton;
    public Slider progressBar1;
    public Slider progressBar2;
    public Slider progressBar3;
    public TextMeshProUGUI section3TimeText;
    #endregion

    #region Section 4: Task.WhenAny (타임아웃)
    [Header("=== 4. Task.WhenAny (Timeout) ===")]
    public Button timeoutDownloadButton;
    public Slider timeoutSlider;
    public TextMeshProUGUI timeoutValueText;
    public TextMeshProUGUI section4ResultText;
    #endregion

    #region Section 5: 메인 스레드 안전성
    [Header("=== 5. Thread Safety ===")]
    public Button safeCodeButton;
    public Button unsafeCodeButton;
    public Transform movingCube;
    public TextMeshProUGUI section5LogText;
    #endregion

    #region Section 6: CancellationToken
    [Header("=== 6. CancellationToken ===")]
    public Button startLongTaskButton;
    public Button cancelTaskButton;
    public Slider section6ProgressBar;
    public TextMeshProUGUI section6StatusText;

    private CancellationTokenSource longTaskCts;
    #endregion

    #region Unity 생명주기
    private void Start()
    {
        InitializeButtons();
        InitializeTimeoutSlider();
    }

    private void OnDestroy()
    {
        delayCts?.Cancel();
        delayCts?.Dispose();

        longTaskCts?.Cancel();
        longTaskCts?.Dispose();
    }
    #endregion

    #region 초기화
    private void InitializeButtons()
    {
        syncDownloadButton.onClick.AddListener(OnSyncDownloadClicked);
        asyncDownloadButton.onClick.AddListener(OnAsyncDownloadClicked);

        delay1SecButton.onClick.AddListener(() => OnDelayClicked(1));
        delay3SecButton.onClick.AddListener(() => OnDelayClicked(3));
        cancellableDelayButton.onClick.AddListener(OnCancellableDelayClicked);
        cancelDelayButton.onClick.AddListener(OnCancelDelayClicked);

        sequentialDownloadButton.onClick.AddListener(OnSequentialDownloadClicked);
        parallelDownloadButton.onClick.AddListener(OnParallelDownloadClicked);

        timeoutDownloadButton.onClick.AddListener(OnTimeoutDownloadClicked);

        safeCodeButton.onClick.AddListener(OnSafeCodeClicked);
        unsafeCodeButton.onClick.AddListener(OnUnsafeCodeClicked);

        startLongTaskButton.onClick.AddListener(OnStartLongTaskClicked);
        cancelTaskButton.onClick.AddListener(OnCancelTaskClicked);
    }

    private void InitializeTimeoutSlider()
    {
        if (timeoutSlider != null)
        {
            timeoutSlider.minValue = 1;
            timeoutSlider.maxValue = 5;
            timeoutSlider.value = 3;
            timeoutSlider.onValueChanged.AddListener(OnTimeoutSliderChanged);
            OnTimeoutSliderChanged(timeoutSlider.value);
        }
    }

    private void OnTimeoutSliderChanged(float value)
    {
        if (timeoutValueText != null)
        {
            timeoutValueText.text = $"{value:F1}초";
        }
    }
    #endregion

    #region Section 1: 동기 vs 비동기 구현
    private void OnSyncDownloadClicked()
    {
        UpdateSection1Status("동기 다운로드 시작... (3초 멈춤)");

        Thread.Sleep(3000);

        UpdateSection1Status("동기 다운로드 완료! (큐브가 멈췄나요?)");
    }

    private async void OnAsyncDownloadClicked()
    {
        UpdateSection1Status("비동기 다운로드 시작... (3초 대기)");

        await Task.Delay(3000);

        UpdateSection1Status("비동기 다운로드 완료! (큐브가 계속 돌았나요?)");
    }

    private void UpdateSection1Status(string message)
    {
        if (section1StatusText != null)
        {
            section1StatusText.text = message;
        }
        Debug.Log($"[Section1] {message}");
    }
    #endregion

    #region Section 2: Task.Delay 구현
    private async void OnDelayClicked(int seconds)
    {
        UpdateSection2Timer($"{seconds}초 대기 중...");

        for (int i = seconds; i > 0; i--)
        {
            UpdateSection2Timer($"남은 시간: {i}초");
            await Task.Delay(1000);
        }

        UpdateSection2Timer("대기 완료!");
    }

    private async void OnCancellableDelayClicked()
    {
        delayCts?.Cancel();
        delayCts?.Dispose();
        delayCts = new CancellationTokenSource();

        try
        {
            UpdateSection2Timer("취소 가능한 10초 대기 시작...");

            for (int i = 10; i > 0; i--)
            {
                delayCts.Token.ThrowIfCancellationRequested();

                UpdateSection2Timer($"남은 시간: {i}초 (취소 가능)");
                await Task.Delay(1000, delayCts.Token);
            }

            UpdateSection2Timer("10초 대기 완료!");
        }
        catch (OperationCanceledException)
        {
            UpdateSection2Timer("취소 완료!");
            Debug.Log("[Section2] Catch block executed - task was cancelled");
        }
    }

    private void OnCancelDelayClicked()
    {
        if (delayCts != null && !delayCts.IsCancellationRequested)
        {
            delayCts.Cancel();
        }
        else
        {
            UpdateSection2Timer("취소할 작업이 없습니다.");
        }
    }

    private void UpdateSection2Timer(string message)
    {
        if (section2TimerText != null)
        {
            section2TimerText.text = message;
        }
        Debug.Log($"[Section2] {message}");
    }
    #endregion

    #region Section 3: Task.WhenAll 구현
    private async void OnSequentialDownloadClicked()
    {
        ResetProgressBars();
        UpdateSection3Time("순차 다운로드 시작...");

        float startTime = Time.time;

        await FakeDownloadWithProgressAsync(progressBar1, 1, 2000);
        await FakeDownloadWithProgressAsync(progressBar2, 2, 2000);
        await FakeDownloadWithProgressAsync(progressBar3, 3, 2000);

        float elapsed = Time.time - startTime;
        UpdateSection3Time($"순차 완료! 시간: {elapsed:F1}초");
    }

    private async void OnParallelDownloadClicked()
    {
        ResetProgressBars();
        UpdateSection3Time("병렬 다운로드 시작...");

        float startTime = Time.time;

        Task task1 = FakeDownloadWithProgressAsync(progressBar1, 1, 2000);
        Task task2 = FakeDownloadWithProgressAsync(progressBar2, 2, 2000);
        Task task3 = FakeDownloadWithProgressAsync(progressBar3, 3, 2000);

        await Task.WhenAll(task1, task2, task3);

        float elapsed = Time.time - startTime;
        UpdateSection3Time($"병렬 완료! 시간: {elapsed:F1}초");
    }

    private async Task FakeDownloadWithProgressAsync(Slider progressBar, int fileNumber, int durationMs)
    {
        if (progressBar == null)
            return;

        int steps = 20;
        int delayPerStep = durationMs / steps;

        for (int i = 0; i <= steps; i++)
        {
            progressBar.value = (float)i / steps;
            await Task.Delay(delayPerStep);
        }

        Debug.Log($"[Section3] File {fileNumber} download complete");
    }

    private void ResetProgressBars()
    {
        if (progressBar1 != null)
            progressBar1.value = 0;
        if (progressBar2 != null)
            progressBar2.value = 0;
        if (progressBar3 != null)
            progressBar3.value = 0;
    }

    private void UpdateSection3Time(string message)
    {
        if (section3TimeText != null)
        {
            section3TimeText.text = message;
        }
        Debug.Log($"[Section3] {message}");
    }
    #endregion

    #region Section 4: Task.WhenAny (타임아웃) 구현
    private async void OnTimeoutDownloadClicked()
    {
        if (timeoutSlider == null)
        {
            UpdateSection4Result("타임아웃 슬라이더가 설정되지 않았습니다.");
            return;
        }

        float timeoutSeconds = timeoutSlider.value;
        UpdateSection4Result($"다운로드 시작... (타임아웃: {timeoutSeconds:F1}초)");

        Task downloadTask = Task.Delay(4000);
        Task timeoutTask = Task.Delay((int)(timeoutSeconds * 1000));

        Task completedTask = await Task.WhenAny(downloadTask, timeoutTask);

        if (completedTask == downloadTask)
        {
            UpdateSection4Result($"성공! (타임아웃: {timeoutSeconds:F1}초)");
        }
        else
        {
            UpdateSection4Result($"타임아웃! ({timeoutSeconds:F1}초 초과)");
        }
    }

    private void UpdateSection4Result(string message)
    {
        if (section4ResultText != null)
        {
            section4ResultText.text = message;
        }
        Debug.Log($"[Section4] {message}");
    }
    #endregion

    #region Section 5: 메인 스레드 안전성 구현
    private async void OnSafeCodeClicked()
    {
        UpdateSection5Log("안전한 코드 실행 중...");

        await Task.Delay(1000);

        if (movingCube != null)
        {
            movingCube.position += Vector3.up * 0.5f;
            UpdateSection5Log("안전: 큐브가 위로 이동했습니다.");
        }
    }

    private async void OnUnsafeCodeClicked()
    {
        UpdateSection5Log("위험한 코드 실행 중...");

        try
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1000);

                if (movingCube != null)
                {
                    movingCube.position += Vector3.up * 0.5f;
                }
            });

            UpdateSection5Log("이 메시지는 표시되지 않습니다.");
        }
        catch (Exception ex)
        {
            UpdateSection5Log($"예외: Unity API는 메인 스레드에서만 사용 가능!\n{ex.GetType().Name}");
            Debug.LogError($"[Section5] Exception: {ex.Message}");
        }
    }

    private void UpdateSection5Log(string message)
    {
        if (section5LogText != null)
        {
            section5LogText.text = message;
        }
        Debug.Log($"[Section5] {message}");
    }
    #endregion

    #region Section 6: CancellationToken 구현
    private async void OnStartLongTaskClicked()
    {
        longTaskCts?.Cancel();
        longTaskCts?.Dispose();
        longTaskCts = new CancellationTokenSource();

        try
        {
            await LongRunningTaskAsync(longTaskCts.Token);
        }
        catch (OperationCanceledException)
        {
            UpdateSection6Status("취소 완료!");
            Debug.Log("[Section6] Catch block executed - task was cancelled");
        }
    }

    private void OnCancelTaskClicked()
    {
        if (longTaskCts != null && !longTaskCts.IsCancellationRequested)
        {
            longTaskCts.Cancel();
        }
        else
        {
            UpdateSection6Status("취소할 작업이 없습니다.");
        }
    }

    private async Task LongRunningTaskAsync(CancellationToken cancellationToken)
    {
        UpdateSection6Status("긴 작업 시작... (10초)");

        if (section6ProgressBar != null)
        {
            section6ProgressBar.value = 0;
        }

        for (int i = 0; i <= 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (section6ProgressBar != null)
            {
                section6ProgressBar.value = i / 100f;
            }

            UpdateSection6Status($"진행률: {i}% (취소 가능)");

            await Task.Delay(100, cancellationToken);
        }

        UpdateSection6Status("작업 완료! (100%)");
    }

    private void UpdateSection6Status(string message)
    {
        if (section6StatusText != null)
        {
            section6StatusText.text = message;
        }
        Debug.Log($"[Section6] {message}");
    }
    #endregion
}
