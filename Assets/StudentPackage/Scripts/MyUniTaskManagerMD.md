using System;
using System.Threading;
using System.Timers;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MyUniTaskManager : MonoBehaviour
{
    #region Section 1: 시간 기반 대기


    [Header("=== 1. Time-based Waiting ===")]
    public Button delayButton;
    public Button delayFrameButton;
    public Button yieldButton;
    public Button nextFrameButton;
    public TextMeshProUGUI section1StatusText;
    #endregion

    #region Section 2: 병렬 실행
    [Header("=== 2. Parallel Execution ===")]
    public Button sequentialButton;
    public Button whenAllButton;
    public Button whenAnyButton;
    public Slider progressBar1;
    public Slider progressBar2;
    public Slider progressBar3;
    public TextMeshProUGUI section2TimeText;
    #endregion

    #region Section 3: Unity AsyncOperation 통합
    [Header("=== 3. Unity AsyncOperation Integration ===")]
    public Button loadResourceButton;
    public Button loadWithProgressButton;
    public Button cancelLoadButton;
    public Slider section3ProgressBar;
    public TextMeshProUGUI section3StatusText;

    private CancellationTokenSource loadCts;
    #endregion

    #region Section 4: PlayerLoopTiming
    [Header("=== 4. PlayerLoopTiming ===")]
    public Button updateTimingButton;
    public Button fixedUpdateTimingButton;
    public Button lateUpdateTimingButton;
    public TextMeshProUGUI section4LogText;
    #endregion

    #region Section 5: CancellationToken 패턴
    [Header("=== 5. CancellationToken Patterns ===")]
    public Button destroyTokenButton;
    public Button timeoutButton;
    public Button linkedTokenButton;
    public Button cancelSection5Button;
    public Slider section5ProgressBar;
    public TextMeshProUGUI section5StatusText;

    private CancellationTokenSource section5Cts;
    #endregion

    #region Section 6: 실전 패턴
    [Header("=== 6. Practical Patterns ===")]
    public Button fadeInButton;
    public Button fadeOutButton;
    public Button animationSequenceButton;
    public Button waitForInputButton;
    public CanvasGroup fadePanel;
    public Transform animatedCube;
    public TextMeshProUGUI section6StatusText;
    #endregion

    #region Unity 생명주기
    private void Start()
    {
        InitializeButtons();
    }

    private void OnDestroy()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();

        section5Cts?.Cancel();
        section5Cts?.Dispose();
    }
    #endregion

    #region 초기화
    private void InitializeButtons()
    {
        delayButton.onClick.AddListener(() => OnDelayClicked().Forget());
        delayFrameButton.onClick.AddListener(() => OnDelayFrameClicked().Forget());
        yieldButton.onClick.AddListener(() => OnYieldClicked().Forget());
        nextFrameButton.onClick.AddListener(() => OnNextFrameClicked().Forget());

        sequentialButton.onClick.AddListener(() => OnSequentialClicked().Forget());
        whenAllButton.onClick.AddListener(() => OnWhenAllClicked().Forget());
        whenAnyButton.onClick.AddListener(() => OnWhenAnyClicked().Forget());

        loadResourceButton.onClick.AddListener(() => OnLoadResourceClicked().Forget());
        loadWithProgressButton.onClick.AddListener(() => OnLoadWithProgressClicked().Forget());
        cancelLoadButton.onClick.AddListener(OnCancelLoadClicked);

        updateTimingButton.onClick.AddListener(() => OnUpdateTimingClicked().Forget());
        fixedUpdateTimingButton.onClick.AddListener(() => OnFixedUpdateTimingClicked().Forget());
        lateUpdateTimingButton.onClick.AddListener(() => OnLateUpdateTimingClicked().Forget());

        destroyTokenButton.onClick.AddListener(() => OnDestroyTokenClicked().Forget());
        timeoutButton.onClick.AddListener(() => OnTimeoutClicked().Forget());
        linkedTokenButton.onClick.AddListener(() => OnLinkedTokenClicked().Forget());
        cancelSection5Button.onClick.AddListener(OnCancelSection5Clicked);

        fadeInButton.onClick.AddListener(() => OnFadeInClicked().Forget());
        fadeOutButton.onClick.AddListener(() => OnFadeOutClicked().Forget());
        animationSequenceButton.onClick.AddListener(() => OnAnimationSequenceClicked().Forget());
        waitForInputButton.onClick.AddListener(() => OnWaitForInputClicked().Forget());
    }
    #endregion

    #region Section 1: 시간 기반 대기 구현
    private async UniTaskVoid OnDelayClicked()
    {
        UpdateSection1Status("2초 대기 시작");
        await UniTask.Delay(2000);

        UpdateSection1Status("2초 대기 끝");;
    }

    private async UniTaskVoid OnDelayFrameClicked()
    {
         UpdateSection1Status("60프레임 대기 시작");

        await UniTask.DelayFrame(60);

        UpdateSection1Status("60프레임 대기 끝");
    }

    private async UniTaskVoid OnYieldClicked()
    {
        UpdateSection1Status("UniTask.Yield() 시작...");

        int startFrame = Time.frameCount;
        await UniTask.Yield();
        int endFrame = Time.frameCount;

        UpdateSection1Status($"Yield 완료! (프레임: {startFrame} → {endFrame})");


        
    }

    private async UniTaskVoid OnNextFrameClicked()
    {
        UpdateSection1Status("UniTask.NextFrame() 시작...");

        int startFrame = Time.frameCount;
        await UniTask.NextFrame();
        int endFrame = Time.frameCount;

        UpdateSection1Status($"NextFrame 완료! (프레임: {startFrame} → {endFrame})");
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

    #region Section 2: 병렬 실행 구현
    private async UniTaskVoid OnSequentialClicked()
    {
        ResetProgressBars();
        UpdateSection2Time("순차 실행 시작...");

        float startTime = Time.time;

        await FakeLoadAsync(progressBar1, 2000);
        await FakeLoadAsync(progressBar2, 2000);
        await FakeLoadAsync(progressBar3, 2000);

        float elapsed = Time.time - startTime;
        UpdateSection2Time($"순차 완료! 시간: {elapsed:F1}초");
    }

    private async UniTaskVoid OnWhenAllClicked()
    {   
        ResetProgressBars();
        UpdateSection2Time("병렬 실행 시작");
        float startTime = Time.time;

        await UniTask.WhenAll(
            FakeLoadAsync(progressBar1, 1000),
            FakeLoadAsync(progressBar2, 2000),
            FakeLoadAsync(progressBar3, 5000)
        );

        float elapsed = Time.time - startTime;
        UpdateSection2Time($"병렬 시간 완료! 시간 : {elapsed:F1}초");
    }

    private async UniTaskVoid OnWhenAnyClicked()
    {
        ResetProgressBars();
        UpdateSection2Time(" WhenAny 실행 시작");
        float startTime = Time.time;

        int winIndex = await UniTask.WhenAny(
            FakeLoadAsync(progressBar1, 1000),
            FakeLoadAsync(progressBar2, 2000),
            FakeLoadAsync(progressBar3, 5000)
        );

        float elapsed = Time.time - startTime;
        UpdateSection2Time($"WhenAny 시간 완료! {winIndex} / 시간 : {elapsed:F1}초");
    }

    private async UniTask FakeLoadAsync(Slider progressBar, int durationMs)
    {
        if (progressBar == null)
            return;

        int steps = 20;
        int delayPerStep = durationMs / steps;

        for (int i = 0; i <= steps; i++)
        {
            progressBar.value = (float)i / steps;
            await UniTask.Delay(delayPerStep);
        }
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

    private void UpdateSection2Time(string message)
    {
        if (section2TimeText != null)
        {
            section2TimeText.text = message;
        }
        Debug.Log($"[Section2] {message}");
    }
    #endregion

    #region Section 3: Unity AsyncOperation 통합 구현
    private async UniTaskVoid OnLoadResourceClicked()
    {
        UpdateSection3Status("리소스 로딩 시작...");

        await UniTask.Delay(2000);

        UpdateSection3Status("리소스 로딩 완료!");
    }

    private async UniTaskVoid OnLoadWithProgressClicked()
    {
        loadCts?.Cancel();
        loadCts?.Dispose();
        loadCts = new CancellationTokenSource();

        try
        {
            UpdateSection3Status("로딩 시작");

            section3ProgressBar.value = 0;
            
            for (int i = 0; i <= 100; i++)
            {
                loadCts.Token.ThrowIfCancellationRequested();

                section3ProgressBar.value = i / 100f;

                await UniTask.Delay(50);
            }

            UpdateSection3Status("로딩 끝");
        }
        catch (OperationCanceledException)
        {
            UpdateSection3Status("로딩 취소됨");
        }
    }

    private void OnCancelLoadClicked()
    {
        if (loadCts != null && !loadCts.IsCancellationRequested)
        {
            loadCts.Cancel();
        }
        else
        { 
            UpdateSection3Status("취소할 작업이 없습니다.");
        }
    }

    private void UpdateSection3Status(string message)
    {
        if (section3StatusText != null)
        {
            section3StatusText.text = message;
        }
        Debug.Log($"[Section3] {message}");
    }
    #endregion

    #region Section 4: PlayerLoopTiming 구현
    private async UniTaskVoid OnUpdateTimingClicked()
    {
        UpdateSection4Log("Update 타이밍 시작");

        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            UpdateSection4Log($"FixedUpdate #{i + 1} (시간: {Time.frameCount:F2})");
        }

        UpdateSection4Log("Update 타이밍 완료");
    }

    private async UniTaskVoid OnFixedUpdateTimingClicked()
    {
        UpdateSection4Log("FixedUpdate 타이밍 시작...");

        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            UpdateSection4Log($"FixedUpdate #{i + 1} (시간: {Time.fixedTime:F2})");
        }

        UpdateSection4Log("FixedUpdate 타이밍 완료!");
    }

    private async UniTaskVoid OnLateUpdateTimingClicked()
    {
        UpdateSection4Log("LateUpdate 타이밍 시작...");

        for (int i = 0; i < 3; i++)
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            UpdateSection4Log($"LateUpdate #{i + 1} (프레임: {Time.frameCount})");
        }

        UpdateSection4Log("LateUpdate 타이밍 완료!");
    }

    private void UpdateSection4Log(string message)
    {
        if (section4LogText != null)
        {
            section4LogText.text = message;
        }
        Debug.Log($"[Section4] {message}");
    }
    #endregion

    #region Section 5: CancellationToken 패턴 구현
    private async UniTaskVoid OnDestroyTokenClicked()
    {
        UpdateSection5Status("OnDestroyTokeClicked 작업 시작...");

        try
        {
            await LongTaskAsync(10000, this.GetCancellationTokenOnDestroy());
            UpdateSection5Status("OnDestroyTokeClicked 작업 끝...");
        }
        catch (OperationCanceledException)
        {
            UpdateSection5Status("GameObject 파괴로 취소 !");
        }
    }
    private async UniTaskVoid OnTimeoutClicked()
    {
        var cts = new CancellationTokenSource();
        var timer = cts.CancelAfterSlim(TimeSpan.FromSeconds(3));

        UpdateSection5Status("OnTimeoutClicked 작업 시작...");

        try
        {
            await LongTaskAsync(10000, cts.Token);
            UpdateSection5Status("OnTimeOutClicked 작업 끝...");
        }
        catch (OperationCanceledException)
        {
            UpdateSection5Status("Time Out");
        }
        finally
        {
            timer.Dispose();
            cts.Dispose();
        }
    }

    private async UniTaskVoid OnLinkedTokenClicked()
    {
        section5Cts?.Cancel();
        section5Cts?.Dispose();
        section5Cts = new CancellationTokenSource();

        var timeoutCts = new CancellationTokenSource();
        var timeoutTimer = timeoutCts.CancelAfterSlim(TimeSpan.FromSeconds(8));

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            section5Cts.Token,
            timeoutCts.Token,
            this.GetCancellationTokenOnDestroy()
        );

        UpdateSection5Status("LinkedToken 작업 시작... (10초, 8초 타임아웃, 수동 취소)");

        try
        {
            await LongTaskAsync(10000, linkedCts.Token);
            UpdateSection5Status("작업 완료!");
        }
        catch (OperationCanceledException)
        {
            if (timeoutCts.IsCancellationRequested)
                UpdateSection5Status("타임아웃으로 취소됨!");
            else if (section5Cts.IsCancellationRequested)
                UpdateSection5Status("수동으로 취소됨!");
            else
                UpdateSection5Status("GameObject 파괴로 취소됨!");
        }
        finally
        {
            timeoutTimer.Dispose();
            timeoutCts.Dispose();
            linkedCts.Dispose();
        }
    }

    private void OnCancelSection5Clicked()
    {
        if (section5Cts != null && !section5Cts.IsCancellationRequested)
        {
            section5Cts.Cancel();
        }
        else
        {
            UpdateSection5Status("취소할 작업이 없습니다.");
        }
    }

    private async UniTask LongTaskAsync(int durationMs, CancellationToken ct)
    {
        if (section5ProgressBar != null)
        {
            section5ProgressBar.value = 0;
        }

        int steps = 100;
        int delayPerStep = durationMs / steps;

        for (int i = 0; i <= steps; i++)
        {
            ct.ThrowIfCancellationRequested();

            if (section5ProgressBar != null)
            {
                section5ProgressBar.value = i / 100f;
            }

            await UniTask.Delay(delayPerStep, cancellationToken: ct);
        }
    }

    private void UpdateSection5Status(string message)
    {
        if (section5StatusText != null)
        {
            section5StatusText.text = message;
        }
        Debug.Log($"[Section5] {message}");
    }
    #endregion

    #region Section 6: 실전 패턴 구현
    private async UniTaskVoid OnFadeInClicked()
    {
        if (fadePanel == null)
        {
            UpdateSection6Status("FadePanel이 설정되지 않았습니다.");
            return;
        }

        UpdateSection6Status("페이드 인 시작...");
        await FadeAsync(fadePanel, 0f, 1f, 0.5f);
        UpdateSection6Status("페이드 인 완료!");
    }

    private async UniTaskVoid OnFadeOutClicked()
    {
        if (fadePanel == null)
        {
            UpdateSection6Status("FadePanel이 설정되지 않았습니다.");
            return;
        }

        UpdateSection6Status("페이드 아웃 시작...");
        await FadeAsync(fadePanel, 1f, 0f, 0.5f);
        UpdateSection6Status("페이드 아웃 완료!");
    }

    private async UniTaskVoid OnAnimationSequenceClicked()
    {
        if (animatedCube == null)
        {
            UpdateSection6Status("AnimatedCube가 설정되지 않았습니다.");
            return;
        }

        UpdateSection6Status("애니메이션 시퀀스 시작...");

        RectTransform rectTransform = animatedCube as RectTransform;
        if (rectTransform == null)
        {
            UpdateSection6Status("AnimatedCube는 RectTransform(UI 요소)이어야 합니다.");
            return;
        }
        Vector2 originalPos = rectTransform.anchoredPosition;

        await MoveToAsync(rectTransform, originalPos + Vector2.up * 50f, 0.5f);
        UpdateSection6Status("1단계: 위로 이동 완료");

        await RotateAsync(rectTransform, 360f, 0.5f);
        UpdateSection6Status("2단계: 회전 완료");

        await MoveToAsync(rectTransform, originalPos, 0.5f);
        UpdateSection6Status("3단계: 원위치로 복귀!");
    }

    private async UniTaskVoid OnWaitForInputClicked()
    {
        UpdateSection6Status("키 입력 대기");

        await UniTask.WaitUntil(() => Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);

        UpdateSection6Status("키 입력 완료");
    }

    private async UniTask FadeAsync(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        canvasGroup.alpha = from;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            await UniTask.Yield();
        }

        canvasGroup.alpha = to;
    }

    private async UniTask MoveToAsync(RectTransform target, Vector2 to, float duration)
    {
        Vector2 from = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.anchoredPosition = Vector2.Lerp(from, to, t);
            await UniTask.Yield();
        }

        target.anchoredPosition = to;
    }

    private async UniTask RotateAsync(Transform target, float degrees, float duration)
    {
        float startAngle = target.eulerAngles.z;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentAngle = startAngle + (degrees * t);
            target.rotation = Quaternion.Euler(0, 0, currentAngle);
            await UniTask.Yield();
        }

        target.rotation = Quaternion.Euler(0, 0, startAngle + degrees);
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
