using UnityEngine;
using ZXing;

public class QRCodeDrawing
{
    //public RawImage QRCode;// Нарисуйте QR -код
    //public Button DrawButton;// генерировать кнопку
    //public string QRCodeText = null;// QR -код содержание, заполните себя 
    BarcodeWriter BarcodeWriter;// QR -код чертеж

    /*private void Start()
    {
        DrawButton.onClick.AddListener(() => DrowQRCode(QRCodeText));
    }*/


    /// <summary>
    /// Конвертируйте информацию о строке в информацию о картинке QR -кода
    /// </summary>
    /// <param name = "formatstr"> Информация о строке для генерации QR -кода </param>
    /// <sem name = "width"> ширина QR -кода </param>
    /// <sem name = "height"> высота QR -кода </param>
    /// <returns></returns>
    Color32[] GeneQRCode(string formatStr, int width, int height)
    {
        ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions();// Настройка перед рисованием QR -кода

        options.CharacterSet = "UTF-8";// Установите кодирование символов, чтобы убедиться, что информация о строке поддерживается правильно

        options.Width = width;// Установить ширину QR -кода
        options.Height = height;// Установить QR -код высокий
        options.Margin = 1;// Установите QR -код, чтобы оставить белый (чем больше значение, тем больше белый, тем меньше QR -код)

        BarcodeWriter = new BarcodeWriter { Format = BarcodeFormat.QR_CODE, Options = options };// Пример рисования строки QR -код инструмент

        return BarcodeWriter.Write(formatStr);
    }


    /// <summary>
    /// Нарисуйте два -размерный код в информацию о строке в соответствии с информацией о QR -коде в указанной области
    /// </summary>
    /// <param name = "str"> string information </param>
    /// <sem name = "width"> ширина QR -кода </param>
    /// <sem name = "height"> высота QR -кода </param>
    /// <returns></returns>
    Texture2D ShowQRCode(string str, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);

        Color32[] colors = GeneQRCode(str, width, height);

        texture.SetPixels32(colors);

        texture.Apply();

        return texture;
    }


    /// <summary>
    /// Нарисуйте QR -код
    /// </summary>
    /// <param name = "formatstr"> Информация о QR -коде </param>
    public Texture2D DrowQRCode(string formatStr)
    {
        Texture2D texture = ShowQRCode(formatStr, 256, 256);// ПРИМЕЧАНИЕ. Не изменяйте эту высоту ширины 256. В противном случае генерируемая информация неверна
                                                            // 256 Это может быть это значение Zxingnet Plug -IN назначенное значение точки пикселя указанного размера
        //QRCode.texture = texture;// показать на картинке интерфейса пользовательского интерфейса
        return texture;
    }
}
