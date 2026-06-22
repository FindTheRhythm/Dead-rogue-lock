using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class DeveloperSplash : MonoBehaviour
{
    private const string SplashText = "CREATED BY\nFINDTHERHYTHM";
    private const string SplashFontResource = "Fonts/valveoracle-semibold";
    private const string AvatarResource = "Splash/photo_2025-03-24_00-08-16";
    private const float FadeDuration = 0.5f;
    private const float HoldDuration = 3f;

    private CanvasGroup canvasGroup;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var splashObject = new GameObject(nameof(DeveloperSplash));
        DontDestroyOnLoad(splashObject);
        splashObject.AddComponent<DeveloperSplash>();
    }

    private void Awake()
    {
        CreateCanvas();
        StartCoroutine(ShowSplash());
    }

    private void CreateCanvas()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var canvasScaler = gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(transform, false);
        StretchToParent(background.GetComponent<RectTransform>());
        background.GetComponent<Image>().color = Color.black;

        var content = new GameObject("SplashContent", typeof(RectTransform));
        content.transform.SetParent(background.transform, false);
        SetCenteredRect(
            content.GetComponent<RectTransform>(),
            new Vector2(980f, 320f),
            Vector2.zero
        );

        CreateAvatar(content.transform);

        var title = new GameObject("DeveloperName", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(content.transform, false);

        var titleTransform = title.GetComponent<RectTransform>();
        SetCenteredRect(titleTransform, new Vector2(600f, 286f), new Vector2(190f, 0f));

        var text = title.GetComponent<Text>();
        text.text = SplashText;
        text.font = Resources.Load<Font>(SplashFontResource);
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 54;
        text.alignment = TextAnchor.MiddleLeft;
        text.lineSpacing = 0.9f;
        text.color = Color.white;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 18;
        text.resizeTextMaxSize = 54;
    }

    private static void CreateAvatar(Transform parent)
    {
        Sprite avatarSprite = Resources.Load<Sprite>(AvatarResource);
        if (avatarSprite == null)
        {
            Debug.LogWarning($"Developer splash avatar not found at Resources/{AvatarResource}");
            return;
        }

        var shadow = CreateImage(
            "AvatarFrameShadow",
            parent,
            new Color(0.08f, 0.045f, 0.015f, 1f),
            new Vector2(306f, 306f)
        );
        SetCenteredRect(shadow.rectTransform, new Vector2(306f, 306f), new Vector2(-304f, -6f));

        var outerFrame = CreateImage(
            "AvatarFrameBronze",
            parent,
            new Color(0.25f, 0.13f, 0.025f, 1f),
            new Vector2(300f, 300f)
        );
        SetCenteredRect(outerFrame.rectTransform, new Vector2(300f, 300f), new Vector2(-310f, 0f));

        var middleGold = CreateImage(
            "AvatarFrameGold",
            outerFrame.transform,
            new Color(0.72f, 0.43f, 0.055f, 1f),
            new Vector2(290f, 290f)
        );

        var darkGap = CreateImage(
            "AvatarDarkGap",
            middleGold.transform,
            new Color(0.035f, 0.025f, 0.02f, 1f),
            new Vector2(270f, 270f)
        );

        var innerFrame = CreateImage(
            "AvatarGoldInner",
            darkGap.transform,
            new Color(1f, 0.72f, 0.2f, 1f),
            new Vector2(258f, 258f)
        );

        var avatar = CreateImage(
            "Avatar",
            innerFrame.transform,
            Color.white,
            new Vector2(246f, 246f)
        );
        avatar.sprite = avatarSprite;
        avatar.preserveAspect = true;

        CreatePixelCorners(middleGold.transform);
    }

    private static void CreatePixelCorners(Transform frame)
    {
        Vector2[] positions =
        {
            new Vector2(-137f, -137f),
            new Vector2(-137f, 137f),
            new Vector2(137f, -137f),
            new Vector2(137f, 137f)
        };

        foreach (Vector2 position in positions)
        {
            var notch = CreateImage(
                "PixelCornerCut",
                frame,
                new Color(0.25f, 0.13f, 0.025f, 1f),
                new Vector2(18f, 18f)
            );
            SetCenteredRect(notch.rectTransform, new Vector2(18f, 18f), position);

            var highlight = CreateImage(
                "PixelCornerGold",
                frame,
                new Color(1f, 0.77f, 0.22f, 1f),
                new Vector2(10f, 10f)
            );
            SetCenteredRect(
                highlight.rectTransform,
                new Vector2(10f, 10f),
                position * 0.91f
            );
        }
    }

    private static Image CreateImage(
        string objectName,
        Transform parent,
        Color color,
        Vector2 size)
    {
        var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        var image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        SetCenteredRect(image.rectTransform, size, Vector2.zero);
        return image;
    }

    private static void SetCenteredRect(
        RectTransform rectTransform,
        Vector2 size,
        Vector2 position)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;
    }

    private IEnumerator ShowSplash()
    {
        canvasGroup.alpha = 0f;
        yield return null;

        yield return Fade(0f, 1f);

        float elapsed = 0f;
        while (elapsed < HoldDuration && !SkipRequested())
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return Fade(canvasGroup.alpha, 0f);
        Destroy(gameObject);
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / FadeDuration);
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private static bool SkipRequested()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
