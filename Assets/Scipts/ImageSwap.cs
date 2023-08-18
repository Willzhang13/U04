using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageSwap : MonoBehaviour
{
    private int _curBackgroundIndex;
    public string[] pictureNames = {"Alien1", "Alien2", "Cat", "Dog", "Parrot" };
    private Image _curBackground;

    public enum LoadMethods {Resources, WWW, UnityWebRequest, AssetDatabase, AssetBundle}
    private LoadMethods _curMethod;
    public LoadMethods selectedMethod;

    public bool isAsync;
    public float fadeDuration;

    // Start is called before the first frame update
    void Start()
    {
        isAsync = false;
        selectedMethod = LoadMethods.Resources;
        _curMethod = selectedMethod;
        _curBackgroundIndex = 0;
        _curBackground = GetComponent<Image>();
        RefreshPicture(selectedMethod);

    }

    private void Update()
    {
        if (_curMethod != selectedMethod)
        {
            RefreshPicture(selectedMethod);
            _curMethod = selectedMethod;
            
        }
    }

    private void RefreshPicture(LoadMethods method)
    {
        switch (method)
        {
            case LoadMethods.Resources:
                if (isAsync)
                {
                    StartCoroutine(LoadFromResourcesAsync());
                } else
                {
                    LoadFromResources();
                }
                
                break;
            case LoadMethods.WWW:
                StartCoroutine(LoadFromWWW());
                break;
            case LoadMethods.UnityWebRequest:
                StartCoroutine(LoadFromUnityWebRequest());
                break;
            case LoadMethods.AssetBundle:
                if (isAsync)
                {
                    StartCoroutine(LoadFromAssetBundleAsync());
                }
                else
                {
                    LoadFromAssetBundle();
                }
                break;
            case LoadMethods.AssetDatabase:
                LoadFromAssetDatabase();
                break;
        }
    }


    private void LoadFromResources()
    {
        Sprite loadedAsset = Resources.Load("Images/" + pictureNames[_curBackgroundIndex], typeof(Sprite)) as Sprite;
        StartCoroutine(Fade(loadedAsset));
        Resources.UnloadAsset(loadedAsset);
    }

    private IEnumerator LoadFromResourcesAsync()
    {
        ResourceRequest loadRequest = Resources.LoadAsync<Sprite>("Images/" + pictureNames[_curBackgroundIndex]);
        yield return loadRequest;
        Sprite loadedAsset = loadRequest.asset as Sprite;
        StartCoroutine(Fade(loadedAsset));
        Resources.UnloadAsset(loadedAsset) ;
    }

    private IEnumerator LoadFromWWW()
    {
        WWW www = new WWW(@"file:///" + Application.dataPath + "/Resources/Images/" + pictureNames[_curBackgroundIndex] + ".jpg");
        yield return www;
        Texture2D tex = www.texture;
        StartCoroutine(Fade(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero)));
        www.Dispose();
    }

    private void LoadFromAssetBundle()
    {
        var loadedAssetBundle = AssetBundle.LoadFromFile("Assets/imagebundle/imageassetbundle");
        Sprite loadedAsset = loadedAssetBundle.LoadAsset<Sprite>(pictureNames[_curBackgroundIndex]);
        StartCoroutine(Fade(loadedAsset));
        loadedAssetBundle.Unload(false);
    }

    private IEnumerator LoadFromAssetBundleAsync()
    {
        AssetBundleCreateRequest loadBundleRequest = AssetBundle.LoadFromFileAsync("Assets/imagebundle/imageassetbundle");
        yield return loadBundleRequest;
        AssetBundle loadedAssetBundle = loadBundleRequest.assetBundle;
        AssetBundleRequest loadAssetRequest = loadedAssetBundle.LoadAssetAsync<Sprite>(pictureNames[_curBackgroundIndex]);
        yield return loadAssetRequest;
        StartCoroutine(Fade(loadAssetRequest.asset as Sprite));
        loadedAssetBundle.Unload(false);
    }

    private void LoadFromAssetDatabase()
    {
        Sprite loadedAsset = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Resources/Images/" + pictureNames[_curBackgroundIndex] + ".jpg", typeof(Sprite));
        StartCoroutine(Fade(loadedAsset));
    }

    private IEnumerator LoadFromUnityWebRequest()
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(@"file:///" + Application.dataPath + "/Resources/Images/" + pictureNames[_curBackgroundIndex] + ".jpg");
        yield return uwr.SendWebRequest();
        Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
        StartCoroutine(Fade(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero)));
        uwr.Dispose();
    }

    public void NextPic()
    {
        _curBackgroundIndex = (_curBackgroundIndex + 1) % 5;
        RefreshPicture(_curMethod);
    }

    public void PrevPic()
    {
        int newIndex = _curBackgroundIndex - 1;
        _curBackgroundIndex = newIndex < 0 ? 4 : newIndex;
        RefreshPicture(_curMethod);
    }

    IEnumerator Fade(Sprite newSprite)
    {
        Color color = _curBackground.color;
        float t = 0;
        float fadeHalfDuration = fadeDuration / 2;

        while (t < fadeHalfDuration)
        {
            t += Time.deltaTime;
            float blend = Mathf.Clamp01(t / fadeHalfDuration);

            color.a = Mathf.Lerp(1, 0, blend);

            _curBackground.color = color;

            yield return null;
        }
        _curBackground.sprite = newSprite;
        t = 0;
        while (t < fadeHalfDuration)
        {
            t += Time.deltaTime;
            float blend = Mathf.Clamp01(t / fadeHalfDuration);

            color.a = Mathf.Lerp(0, 1, blend);

            _curBackground.color = color;

            yield return null;
        }

    }
}
 