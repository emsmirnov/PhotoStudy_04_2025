using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RemovableMediaManager : MonoBehaviour
{
    public string GetRemovableDrivePath()
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();

        foreach (DriveInfo drive in allDrives)
        {
            if (drive.DriveType == DriveType.Removable && drive.IsReady)
            {
                Debug.Log($"Найден съёмный носитель: {drive.Name} ({drive.VolumeLabel}, {drive.TotalSize / (1024 * 1024)} MB)");
                return drive.RootDirectory.FullName;
            }
        }

        Debug.LogWarning("Съёмные носители не найдены!");
        return null;
    }

    public void CopyFolderToRemovableDrive(string sourceFolderPath, string destFolderName = null)
    {
        try
        {
            string removableDrivePath = GetRemovableDrivePath();

            if (string.IsNullOrEmpty(removableDrivePath))
            {
                Debug.LogError("Не удалось найти съёмный носитель!");
                return;
            }

            if (!Directory.Exists(sourceFolderPath))
            {
                Debug.LogError($"Исходная папка не существует: {sourceFolderPath}");
                return;
            }
            string folderName = string.IsNullOrEmpty(destFolderName)
                ? Path.GetFileName(sourceFolderPath)
                : destFolderName;

            string destinationPath = Path.Combine(removableDrivePath, folderName);

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (string file in Directory.GetFiles(sourceFolderPath))
            {
                string destFile = Path.Combine(destinationPath, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                Debug.Log($"Скопирован файл: {file} -> {destFile}");
            }

            foreach (string folder in Directory.GetDirectories(sourceFolderPath))
            {
                string destFolder = Path.Combine(destinationPath, Path.GetFileName(folder));
                CopyFolderRecursive(folder, destFolder);
            }

            Debug.Log($"Папка успешно скопирована на съёмный носитель: {destinationPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при копировании папки: {ex.Message}");
        }
    }

    private void CopyFolderRecursive(string sourcePath, string destinationPath)
    {
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        foreach (string file in Directory.GetFiles(sourcePath))
        {
            string destFile = Path.Combine(destinationPath, Path.GetFileName(file));
            File.Copy(file, destFile, true);
            Debug.Log($"Скопирован файл: {file} -> {destFile}");
        }

        foreach (string folder in Directory.GetDirectories(sourcePath))
        {
            string destFolder = Path.Combine(destinationPath, Path.GetFileName(folder));
            CopyFolderRecursive(folder, destFolder);
        }
    }
}