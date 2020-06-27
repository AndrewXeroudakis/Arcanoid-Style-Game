using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleSpriteButton : MonoBehaviour {

    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private bool isOn;
    public Slider slider;

    public bool IsOn { get { return isOn; } set { isOn = value; UpdateValue(); } }
    public event Action<bool> OnValueChanged;
    private Button button;

    public void Initialize(bool value)
    {
        isOn = value;
        UpdateValue(false);
    }

    void Start ()
    {
        button = GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(OnClick);
	}

    void Update()
    {
        if (slider.value <= slider.minValue)
        {
            isOn = false;
            UpdateValue();
        }
        else
        {
            isOn = true;
            UpdateValue();
        }
    }

    void OnClick()
    {
        isOn = !isOn;
        UpdateValue();

        if (isOn == true){ slider.value = slider.maxValue; }
        else { slider.value = slider.minValue; }
    }

    private void UpdateValue(bool notify = true)
    {
        if (notify && OnValueChanged != null)
            OnValueChanged(isOn);

        if (targetImage == null)
            return;

        targetImage.sprite = isOn ? onSprite : offSprite;
    }
	
}
