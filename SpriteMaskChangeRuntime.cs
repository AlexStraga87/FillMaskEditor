using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace Game
{
    [AddComponentMenu(nameof(Game) + "/" + nameof(SpriteMaskChangeRuntime))]
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteMask))]
    public class SpriteMaskChangeRuntime : MonoBehaviour
    {
        [Tooltip("будет ли анимация сама запускаться при старте")]
        public bool StartSelfRun;

        [Tooltip("будет ли анимация зацикливаться")]
        public bool Loop;

        [Tooltip("Задержка анимации")] [SerializeField]
        private float TimeDelay;
        
        public UnityAction<int> OnAnimationUpdate;

        [SerializeField] private float _speed;

        [Tooltip("Основная анимация")] [SerializeField]
        private Sprite[] _sprites, _masks;

        private SpriteRenderer _render;
        private Coroutine _timer, _timerStart;
        private SpriteMask _mask;
        
        private int _index;
        private bool _isSelfRestart;

        private int index
        {
            get => _index;
            set
            {
                OnAnimationUpdate?.Invoke(index);
                _index = value;
            }
        }


        protected virtual void Awake()
        {
            _render = GetComponent<SpriteRenderer>();
            _mask = GetComponent<SpriteMask>();
        }

        private void Start()
        {
            if (StartSelfRun == true)
                Play();
        }

        private void OnEnable()
        {
            if (StartSelfRun == true)
                Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            Stop();

            _isSelfRestart = false;
            _timer = StartCoroutine(Timer());
        }

        public void Stop()
        {
            if (_timer != null)
                StopCoroutine(_timer);
        }

        // сразу устанавливаем первый спрайт в рендерер и маску
        public void SetFirstSprite()
        {
            _render.sprite = _sprites[0];
            _mask.sprite = _masks[0];
        }

        IEnumerator Timer()
        {
            if(!_isSelfRestart)
                yield return new WaitForSeconds(TimeDelay);

            while (true)
            {
                yield return new WaitForSeconds(_speed / _sprites.Length);
                AnimationUpdate();
            }
        }

        private void AnimationUpdate()
        {
            _render.sprite = _sprites[index];
            _mask.sprite = _masks[index];
            index++;

            if (index < _sprites.Length)
                return;

            //когда анимация подошла к концу

            index = 0;

            OnAnimationEnd();

            if (Loop == true)
            {
                Stop();
                _isSelfRestart = true;
                _timer = StartCoroutine(Timer());
            }
            else
                Stop();
        }

        protected virtual void OnAnimationEnd()
        {
            
        }
    }
}