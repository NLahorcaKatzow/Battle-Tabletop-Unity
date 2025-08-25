using DG.Tweening;
using UnityEngine;

public static class TweenUtils
{
    // Flash con material instanciado del Renderer (rend.material)
    public static Tween Flash(Renderer rend, string colorProp, Color flashColor, float duration)
    {
        if (!rend) return null;

        // Instancia el material de este renderer
        var mat = rend.material;

        if (!mat || !mat.HasProperty(colorProp))
        {
            Debug.LogWarning($"[Flash] El material no tiene la propiedad '{colorProp}'.");
            return null;
        }

        // (Opcional) habilitar emisión si corresponde
        if (colorProp == "_EmissionColor") mat.EnableKeyword("_EMISSION");

        // Guardamos el color original en una variable local (no global)
        Color original = mat.GetColor(colorProp);

        // Usamos un ID para poder matar cualquier tween previo de este mismo renderer/propiedad
        string id = $"Flash:{rend.GetInstanceID()}:{colorProp}";
        DOTween.Kill(id); // si había uno, lo mata (y disparamos OnKill del tween viejo)

        // Creamos el tween nuevo
        return mat.DOColor(flashColor, colorProp, duration)
                  .SetLoops(2, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine)
                  .SetId(id)
                  .SetLink(rend.gameObject) // se mata si destruyen el GO
                  .OnKill(() => { if (mat) mat.SetColor(colorProp, original); })
                  .OnComplete(() => { if (mat) mat.SetColor(colorProp, original); });
    }

    public static Tween FlashMPB(Renderer rend, string colorProp, Color flashColor, float duration)
    {
        if (!rend) return null;

        var mpb = new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);

        // Tomamos el color actual del MPB; si no existe, cae en negro/0
        Color original = mpb.GetColor(colorProp);

        string id = $"FlashMPB:{rend.GetInstanceID()}:{colorProp}";
        DOTween.Kill(id);

        // Tween manual con MPB
        Color current = original;
        return DOTween.To(() => current, c =>
        {
            current = c;
            mpb.SetColor(colorProp, c);
            rend.SetPropertyBlock(mpb);
        }, flashColor, duration)
               .SetLoops(2, LoopType.Yoyo)
               .SetEase(Ease.InOutSine)
               .SetId(id)
               .SetLink(rend.gameObject)
               .OnKill(() => { mpb.SetColor(colorProp, original); rend.SetPropertyBlock(mpb); })
               .OnComplete(() => { mpb.SetColor(colorProp, original); rend.SetPropertyBlock(mpb); });
    }

}
