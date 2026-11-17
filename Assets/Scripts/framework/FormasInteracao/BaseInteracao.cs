using UnityEngine;

namespace Ludus.SDK.Framework
{
    public abstract class BaseInteracao : MonoBehaviour
    {
        protected RectTransform rt;
        protected CanvasGroup grupo;
        protected Canvas canvas;
        protected new AudioSource audio;

        public static bool colouCerto;
        public string legenda;

    }
}

