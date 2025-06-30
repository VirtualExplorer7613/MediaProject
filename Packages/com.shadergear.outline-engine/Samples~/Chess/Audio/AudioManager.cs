using UnityEngine;

namespace SHADERGEAR.OutlineEngine.Samples.Chess
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioSource source;
        [SerializeField] AudioClip startClip;
        [SerializeField] AudioClip clickClip;
        [SerializeField] AudioClip slideClip;

        bool didPlayStart;

        public void PlayClick()
        {
            source.PlayOneShot(clickClip);
        }

        public void PlaySlide()
        {
            if (!source.isPlaying)
            {
                source.PlayOneShot(slideClip, 0.3f);
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!didPlayStart)
                {
                    source.PlayOneShot(startClip);
                    didPlayStart = true;
                }
            }
        }
    }
}