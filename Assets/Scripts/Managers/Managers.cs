using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // 유일성이 보장된다
    public static Managers Instance { get { Init();  return s_instance; } } // 유일한 매니저를 가져온다

    ResourceManager _resoruce = new ResourceManager();
    SceneManagerEX _scene = new SceneManagerEX();
    SoundManager _sound = new SoundManager();

    public static ResourceManager Resource { get { return Instance._resoruce; } }
    public static SceneManagerEX Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get {  return Instance._sound; } }

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._sound.Init();

            // FadeUI 프리팹 인스턴스화
            if (GameObject.FindObjectOfType<FadeController>() == null)
            {
                GameObject fadeUI = Resource.Instantiate("UI/FadeUI");
                GameObject.DontDestroyOnLoad(fadeUI);
            }

        }
    }

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
    }
}
