using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Dictionary<string, IView> _views = new Dictionary<string, IView>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterView(string viewName, IView view)
        {
            if (!_views.ContainsKey(viewName))
            {
                _views[viewName] = view;
            }
        }

        public void UnregisterView(string viewName)
        {
            _views.Remove(viewName);
        }

        public void ShowView(string viewName)
        {
            if (_views.TryGetValue(viewName, out var view))
            {
                view.Show();
            }
            else
            {
                Debug.LogWarning($"View '{viewName}' not found in UIManager");
            }
        }

        public void HideView(string viewName)
        {
            if (_views.TryGetValue(viewName, out var view))
            {
                view.Hide();
            }
        }

        public void ToggleView(string viewName)
        {
            if (_views.TryGetValue(viewName, out var view))
            {
                view.Toggle();
            }
        }

        public void HideAllViews()
        {
            foreach (var view in _views.Values)
            {
                view.Hide();
            }
        }

        public IView GetView(string viewName)
        {
            return _views.TryGetValue(viewName, out var view) ? view : null;
        }
    }
}
