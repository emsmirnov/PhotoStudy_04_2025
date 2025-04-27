using System.Threading.Tasks;
using UnityEngine;

public static class TextureConverter
{
    /// <summary>
    /// Конвертирует Texture2D в Sprite
    /// </summary>
    /// <param name="texture">Исходная текстура</param>
    /// <param name="pivot">Точка пивота (по умолчанию центр)</param>
    /// <param name="pixelsPerUnit">Пикселей на юнит (100 для 2D)</param>
    /// <param name="meshType">Тип меша (FullRect для простых спрайтов)</param>
    /// <returns>Созданный спрайт</returns>
    public static Sprite ConvertTextureToSprite(Texture2D texture, 
                                             Vector2? pivot = null, 
                                             float pixelsPerUnit = 100.0f, 
                                             SpriteMeshType meshType = SpriteMeshType.FullRect)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null! Cannot convert to sprite.");
            return null;
        }

        // Используем стандартный пивот (центр) если не указан
        Vector2 actualPivot = pivot ?? new Vector2(0.5f, 0.5f);
        
        // Создаем спрайт
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            actualPivot,
            pixelsPerUnit,
            0,  // extrude (не используется в FullRect)
            meshType
        );

        return sprite;
    }

    /// <summary>
    /// Асинхронная версия с обработкой в другом потоке
    /// </summary>
    public static async Task<Sprite> ConvertTextureToSpriteAsync(Texture2D texture)
    {
        return await Task.Run(() => ConvertTextureToSprite(texture));
    }
}