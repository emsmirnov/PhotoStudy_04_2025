using UnityEngine;
using ZXing;

public class QRCodeDrawing
{
    //public RawImage QRCode;// ��������� QR -���
    //public Button DrawButton;// ������������ ������
    //public string QRCodeText = null;// QR -��� ����������, ��������� ���� 
    BarcodeWriter BarcodeWriter;// QR -��� ������

    /*private void Start()
    {
        DrawButton.onClick.AddListener(() => DrowQRCode(QRCodeText));
    }*/


    /// <summary>
    /// ������������� ���������� � ������ � ���������� � �������� QR -����
    /// </summary>
    /// <param name = "formatstr"> ���������� � ������ ��� ��������� QR -���� </param>
    /// <sem name = "width"> ������ QR -���� </param>
    /// <sem name = "height"> ������ QR -���� </param>
    /// <returns></returns>
    Color32[] GeneQRCode(string formatStr, int width, int height)
    {
        ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions();// ��������� ����� ���������� QR -����

        options.CharacterSet = "UTF-8";// ���������� ����������� ��������, ����� ���������, ��� ���������� � ������ �������������� ���������

        options.Width = width;// ���������� ������ QR -����
        options.Height = height;// ���������� QR -��� �������
        options.Margin = 1;// ���������� QR -���, ����� �������� ����� (��� ������ ��������, ��� ������ �����, ��� ������ QR -���)

        BarcodeWriter = new BarcodeWriter { Format = BarcodeFormat.QR_CODE, Options = options };// ������ ��������� ������ QR -��� ����������

        return BarcodeWriter.Write(formatStr);
    }


    /// <summary>
    /// ��������� ��� -��������� ��� � ���������� � ������ � ������������ � ����������� � QR -���� � ��������� �������
    /// </summary>
    /// <param name = "str"> string information </param>
    /// <sem name = "width"> ������ QR -���� </param>
    /// <sem name = "height"> ������ QR -���� </param>
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
    /// ��������� QR -���
    /// </summary>
    /// <param name = "formatstr"> ���������� � QR -���� </param>
    public Texture2D DrowQRCode(string formatStr)
    {
        Texture2D texture = ShowQRCode(formatStr, 256, 256);// ����������. �� ��������� ��� ������ ������ 256. � ��������� ������ ������������ ���������� �������
                                                            // 256 ��� ����� ���� ��� �������� Zxingnet Plug -IN ����������� �������� ����� ������� ���������� �������
        //QRCode.texture = texture;// �������� �� �������� ���������� ����������������� ����������
        return texture;
    }
}
