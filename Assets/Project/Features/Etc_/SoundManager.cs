using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip chapterClickSFX;
    [SerializeField] private AudioClip confirmClickSFX;
    [SerializeField] private AudioClip backClickSFX;
    [SerializeField] private AudioClip pageChangeSFX;

    [Header("Interaction Sounds")]
    [SerializeField] private AudioClip interactSFX;

    [Header("기타 효과음")]
    [SerializeField] private AudioClip errorSFX;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private float bgmFadeSpeed = 1f;

    void Start()
    {
        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 1f; // 기본 볼륨
            bgmSource.Play();
        }
    }

    public void FadeOutBGM()
    {
        StartCoroutine(FadeBGMVolume(1f, 0.2f)); // 점점 작아짐
    }

    public void FadeInBGM()
    {
        StartCoroutine(FadeBGMVolume(0.2f, 1f)); // 다시 커짐
    }

    private IEnumerator FadeBGMVolume(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * bgmFadeSpeed;
            bgmSource.volume = Mathf.Lerp(from, to, elapsed);
            yield return null;
        }
        bgmSource.volume = to;
    }


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayChapterClick() => PlaySFX(chapterClickSFX);
    public void PlayConfirmClick() => PlaySFX(confirmClickSFX);
    public void PlayBackClick() => PlaySFX(backClickSFX);
    public void PlayInteract() => PlaySFX(interactSFX);
    public void PlayError() => PlaySFX(errorSFX);
    public void PlayPageChange() => PlaySFX(pageChangeSFX);

    public void PlayConfirmAndLoadScene(string sceneName)
    {
        StartCoroutine(PlayAndLoad(sceneName));
    }

    private System.Collections.IEnumerator PlayAndLoad(string sceneName)
    {
        if (confirmClickSFX != null)
        {
            float waitTime = Mathf.Min(confirmClickSFX.length, 2.5f); // 최대 2.5초까지만 대기
            sfxSource.PlayOneShot(confirmClickSFX);
            yield return new WaitForSeconds(waitTime);
        }

        SceneManager.LoadScene(sceneName);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
