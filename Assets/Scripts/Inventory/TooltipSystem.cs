using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text contentText;

        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show(string title, string content)
        {
            titleText.text = title;
            contentText.text = content;
            tooltipPanel.SetActive(true);
        }

        public void Hide()
        {
            tooltipPanel.SetActive(false);
        }

        private void Update()
        {
            if (tooltipPanel.activeSelf)
            {
                tooltipPanel.transform.position = Input.mousePosition;
            }
        }
    }
}
