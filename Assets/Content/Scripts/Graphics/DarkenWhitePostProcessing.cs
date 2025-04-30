using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class DarkenWhitePostProcessing : PostProcessEffectSettings
{
    [Range(0, 1)]
    public FloatParameter threshold = new FloatParameter { value = 0.9f };

    [Range(0, 1)]
    public FloatParameter darkness = new FloatParameter { value = 0.5f };
}

[System.Serializable]
[PostProcess(typeof(DarkenWhiteRenderer), PostProcessEvent.AfterStack, "Custom/DarkenWhite")]
public class DarkenWhite : PostProcessEffectSettings
{
    [Range(0, 1)] public FloatParameter threshold = new FloatParameter { value = 0.9f };
    [Range(0, 1)] public FloatParameter darkness = new FloatParameter { value = 0.5f };
}

public class DarkenWhiteRenderer : PostProcessEffectRenderer<DarkenWhite>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/DarkenWhiteBackground"));
        sheet.properties.SetFloat("_Threshold", settings.threshold);
        sheet.properties.SetFloat("_Darkness", settings.darkness);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}