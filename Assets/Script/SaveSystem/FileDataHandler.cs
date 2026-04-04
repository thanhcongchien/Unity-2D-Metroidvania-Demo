using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileDataHandler
{ 
    private string fullPath;
    private bool encrpytData;
    private string codeWord = "89451100";

    public FileDataHandler(string dataDirPath,string dataFileName, bool encrpytData)
    {
        fullPath = Path.Combine(dataDirPath, dataFileName);
        this.encrpytData = encrpytData;
        
    }

    public void SaveData(GameData gameData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToSave = JsonUtility.ToJson(gameData, true);

            if (encrpytData)
                dataToSave = EncryptDecrypt(dataToSave);

            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(dataToSave);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving data: " +fullPath + e.Message);
        }
    }
    public GameData LoadData()
    { 
        GameData loadData = null;

        if(File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream fileStream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if(encrpytData)
                    dataToLoad = EncryptDecrypt(dataToLoad);

                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch(System.Exception e)
            {
                Debug.LogError("Error loading data: " + fullPath + e.Message);
            }
        }
        return loadData;
    }

    public void Delete()
    {
        if(File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string EncryptDecrypt(string data)
    {
        string  modifiedData = "";
        for(int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ codeWord[i % codeWord.Length]);
        }
        return modifiedData;
    }
}
