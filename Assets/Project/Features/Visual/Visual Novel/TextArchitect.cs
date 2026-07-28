using System.Collections;
using UnityEngine;
using TMPro;

public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    public string currentText => tmpro.text;
    public string targetText { get; private set; } = "";
    public string preText { get; private set; } = "";
    private int preTextLength = 0;

    public string fullTargetText => preText + targetText;

    public enum BuildMethod { instant, typewriter, fade }
    public BuildMethod buildMethod = BuildMethod.typewriter;

    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    private const float baseSpeed = 1;
    private float speedMultiplier = 1;

    public int charactersPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; } }
    private int characterMultiplier = 1;

    public bool hurryUp = false;

    public bool isCompleted => !isBuilding;





    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }

    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;
    }

    public Coroutine Build(string text)
    {
        Stop(); // 항상 먼저 멈추기

        preText = "";
        targetText = text;

        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;

    public void Stop()
    {
        if (isBuilding)
        {
            tmpro.StopCoroutine(buildProcess);
            buildProcess = null;
        }

        // 클린업
        tmpro.text = fullTargetText;
        tmpro.maxVisibleCharacters = int.MaxValue;
    }

    IEnumerator Building()
    {
        Prepare();

        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }

        OnComplete();
    }

    private void OnComplete()
    {
        buildProcess = null;
        hurryUp = false;
    }

    public void ForceComplete()
    {
        Stop();  // 코루틴 정지부터
        tmpro.text = fullTargetText;

        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.ForceMeshUpdate();
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                tmpro.ForceMeshUpdate();
                break;
        }

        OnComplete();
    }


    private void Prepare()
    {
        switch (buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_TypeWriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }

    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }

    private void Prepare_TypeWriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if (preText != "")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {
        tmpro.text = preText + targetText;
        tmpro.maxVisibleCharacters = int.MaxValue;
        tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpro.textInfo;
        preTextLength = preText.Length;

        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            var meshInfo = textInfo.meshInfo[m];
            for (int i = 0; i < meshInfo.colors32.Length; i++)
            {
                meshInfo.colors32[i].a = 0; // 모든 알파값을 0으로 초기화
            }
        }

        // preText는 바로 보여지게 알파 255
        for (int i = 0; i < preTextLength && i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            for (int v = 0; v < 4; v++)
            {
                textInfo.meshInfo[matIndex].colors32[vertexIndex + v].a = 255;
            }
        }

        tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }


    private IEnumerator Build_Typewriter()
    {
        while (tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp ? charactersPerCycle * 5 : charactersPerCycle;

            yield return new WaitForSeconds(0.015f / speed);
        }
    }

    private IEnumerator Build_Fade()
    {
        TMP_TextInfo textInfo = tmpro.textInfo;
        int charCount = textInfo.characterCount;

        float[] alphaProgress = new float[charCount];
        float fadeDuration = 0.5f / speed; // 글자당 페이드 지속시간
        float delayBetweenCharacters = 0.02f / speed; // 등장 딜레이

        float elapsedTime = 0f;
        bool allCompleted = false;

        while (!allCompleted)
        {
            allCompleted = true;

            for (int i = preTextLength; i < charCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                float startDelay = (i - preTextLength) * delayBetweenCharacters;
                float progress = Mathf.Clamp01((elapsedTime - startDelay) / fadeDuration);
                alphaProgress[i] = progress;

                int matIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                byte alpha = (byte)Mathf.Lerp(0, 255, progress);
                for (int v = 0; v < 4; v++)
                {
                    textInfo.meshInfo[matIndex].colors32[vertexIndex + v].a = alpha;
                }

                if (progress < 1f)
                    allCompleted = false;
            }

            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            elapsedTime += Time.deltaTime;

            // ? 마지막 글자의 알파값이 100 이상이면 강제 완료
            int lastIndex = charCount - 1;
            if (lastIndex >= 0 && textInfo.characterInfo[lastIndex].isVisible)
            {
                var lastChar = textInfo.characterInfo[lastIndex];
                int matIndex = lastChar.materialReferenceIndex;
                int vertexIndex = lastChar.vertexIndex;

                byte lastAlpha = textInfo.meshInfo[matIndex].colors32[vertexIndex].a;

                if (lastAlpha >= 100)
                {
                    break; // 코루틴 종료
                }
            }

            yield return null;
        }

        OnComplete();
    }



}