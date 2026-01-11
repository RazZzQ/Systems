using UnityEngine;

public class FocusHighlight : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private string emissionProperty = "_EmissionColor";
    [SerializeField] private Color emissionOn = Color.white;
    [SerializeField] private Color emissionOff = Color.black;

    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        Set(false);
    }

    public void Set(bool on)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(emissionProperty, on ? emissionOn : emissionOff);
            r.SetPropertyBlock(mpb);
        }
    }
}
