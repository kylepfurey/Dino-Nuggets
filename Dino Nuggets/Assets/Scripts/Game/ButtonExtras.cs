using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DN
{
    public class ButtonExtras : MonoBehaviour, IPointerEnterHandler
    {
        Button button;
        [SerializeField] AudioClip hoverSfx;
        [SerializeField] AudioClip clickSfx;
        bool wasHovered;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => clickSfx.SpawnSound());
        }
        void Update() => CheckInput();
        void FixedUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                if (!wasHovered)
                {
                    hoverSfx.SpawnSound();
                    wasHovered = true;
                }
            }
            else
                wasHovered = false;
        }

        public void OnPointerEnter(PointerEventData eventData) => button.Select();

        void CheckInput()
        {
            Mouse mouse = Mouse.current;
            if (!Cursor.visible &&
                mouse != null &&
                (mouse.delta.ReadValue().sqrMagnitude > 0.01f ||
                 mouse.leftButton.wasPressedThisFrame))
            {
                Cursor.visible = true;
                if (wasHovered && gameObject.activeInHierarchy)
                    button.Select();
                return;
            }

            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            if (Cursor.visible &&
                (keyboard != null && keyboard.anyKey.wasPressedThisFrame) ||
                (gamepad != null && gamepad.wasUpdatedThisFrame))
            {
                Cursor.visible = false;
                return;
            }
        }
    }
}
