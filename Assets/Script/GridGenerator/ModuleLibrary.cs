using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/ModuleLibrary")]
public class ModuleLibrary : ScriptableObject
{
    [SerializeField]
    private GameObject importedModules;

    public Dictionary<string, List<Module>> moduleLibrary = new Dictionary<string, List<Module>>();

    private void Awake()
    {
        ImportedModule();
    }

    public void ImportedModule()
    {
        // 初始化 256 个键
        for (int i = 0; i < 256; i++)
        {
            string key = Convert.ToString(i, 2).PadLeft(8, '0');
            Debug.Log($"Initializing key: {key}");
            if (!moduleLibrary.ContainsKey(key))
            {
                moduleLibrary.Add(key, new List<Module>());
            }
            else
            {
                Debug.LogWarning($"Key {key} already exists in moduleLibrary during initialization.");
            }
        }

        // 处理模块
        foreach (Transform child in importedModules.transform)
        {
            Mesh mesh = child.GetComponent<MeshFilter>().sharedMesh;
            string name = child.name.Replace(" ", "");
            if (name.Length < 8)
            {
                Debug.LogError($"Invalid module name: {name}. Expected at least 8 characters.");
                continue;
            }
            string bit = name.Substring(0, 8);
            string sockets = name.Length == 8 ? "aaaaaa" : name.Substring(9, 6);
            Debug.Log($"Processing module: {name}, bit: {bit}, sockets: {sockets}");

            // 添加原始模块
            if (moduleLibrary.ContainsKey(bit))
            {
                moduleLibrary[bit].Add(new Module(name, mesh, 0, false));
            }
            else
            {
                Debug.LogError($"Key {bit} not found in moduleLibrary for module {name}. Adding new key.");
                moduleLibrary.Add(bit, new List<Module>());
                moduleLibrary[bit].Add(new Module(name, mesh, 0, false));
            }

            // 添加旋转模块
            if (!RotationEqualCheck(bit))
            {
                string rotatedKey1 = RotateBit(bit, 1);
                Debug.Log($"Adding rotated key: {rotatedKey1}");
                if (!moduleLibrary.ContainsKey(rotatedKey1))
                {
                    moduleLibrary.Add(rotatedKey1, new List<Module>());
                }
                moduleLibrary[rotatedKey1].Add(new Module(RotateName(bit, sockets, 1), mesh, 1, false));

                if (!RotationTwiceEqualCheck(bit))
                {
                    string rotatedKey2 = RotateBit(bit, 2);
                    Debug.Log($"Adding rotated key: {rotatedKey2}");
                    if (!moduleLibrary.ContainsKey(rotatedKey2))
                    {
                        moduleLibrary.Add(rotatedKey2, new List<Module>());
                    }
                    moduleLibrary[rotatedKey2].Add(new Module(RotateName(bit, sockets, 2), mesh, 2, false));

                    string rotatedKey3 = RotateBit(bit, 3);
                    Debug.Log($"Adding rotated key: {rotatedKey3}");
                    if (!moduleLibrary.ContainsKey(rotatedKey3))
                    {
                        moduleLibrary.Add(rotatedKey3, new List<Module>());
                    }
                    moduleLibrary[rotatedKey3].Add(new Module(RotateName(bit, sockets, 3), mesh, 3, false));

                    if (!FlipRotationEqualCheck(bit))
                    {
                        string flippedKey = FlipBit(bit);
                        Debug.Log($"Adding flipped key: {flippedKey}");
                        if (!moduleLibrary.ContainsKey(flippedKey))
                        {
                            moduleLibrary.Add(flippedKey, new List<Module>());
                        }
                        moduleLibrary[flippedKey].Add(new Module(FlipName(bit, sockets), mesh, 0, true));

                        string flippedRotatedKey1 = RotateBit(flippedKey, 1);
                        Debug.Log($"Adding flipped rotated key: {flippedRotatedKey1}");
                        if (!moduleLibrary.ContainsKey(flippedRotatedKey1))
                        {
                            moduleLibrary.Add(flippedRotatedKey1, new List<Module>());
                        }
                        moduleLibrary[flippedRotatedKey1].Add(new Module(RotateName(flippedKey, FlipName(bit, sockets).Substring(9, 6), 1), mesh, 1, true));

                        string flippedRotatedKey2 = RotateBit(flippedKey, 2);
                        Debug.Log($"Adding flipped rotated key: {flippedRotatedKey2}");
                        if (!moduleLibrary.ContainsKey(flippedRotatedKey2))
                        {
                            moduleLibrary.Add(flippedRotatedKey2, new List<Module>());
                        }
                        moduleLibrary[flippedRotatedKey2].Add(new Module(RotateName(flippedKey, FlipName(bit, sockets).Substring(9, 6), 2), mesh, 2, true));

                        string flippedRotatedKey3 = RotateBit(flippedKey, 3);
                        Debug.Log($"Adding flipped rotated key: {flippedRotatedKey3}");
                        if (!moduleLibrary.ContainsKey(flippedRotatedKey3))
                        {
                            moduleLibrary.Add(flippedRotatedKey3, new List<Module>());
                        }
                        moduleLibrary[flippedRotatedKey3].Add(new Module(RotateName(flippedKey, FlipName(bit, sockets).Substring(9, 6), 3), mesh, 3, true));
                    }
                }
            }
        }
    }

    public void AddModuleToLibrary(string name, Mesh mesh)
    {
        if (string.IsNullOrEmpty(name) || name.Length < 8)
        {
            Debug.LogError($"Invalid module name: {name}. Expected at least 8 characters.");
            return;
        }
        string bit = name.Substring(0, 8);
        string sockets = name.Length > 8 ? name.Substring(9, 6) : "aaaaaa";
        Debug.Log($"Adding module to library: {name}, bit: {bit}, sockets: {sockets}");

        if (!moduleLibrary.ContainsKey(bit))
        {
            moduleLibrary.Add(bit, new List<Module>());
        }
        moduleLibrary[bit].Add(new Module(name, mesh, 0, false));

        if (!RotationEqualCheck(bit))
        {
            string newName = RotateName(bit, sockets, 1);
            if (!moduleLibrary.ContainsKey(newName))
            {
                moduleLibrary.Add(newName, new List<Module>());
                moduleLibrary[newName].Add(new Module(newName, mesh, 1, false));
            }

            if (!RotationTwiceEqualCheck(bit))
            {
                string newName2 = RotateBit(bit, 2);
                if (!moduleLibrary.ContainsKey(newName2))
                {
                    moduleLibrary.Add(newName2, new List<Module>());
                    moduleLibrary[newName2].Add(new Module(RotateName(bit, sockets, 2), mesh, 2, false));
                }

                string newName3 = RotateBit(bit, 3);
                if (!moduleLibrary.ContainsKey(newName3))
                {
                    moduleLibrary.Add(newName3, new List<Module>());
                    moduleLibrary[newName3].Add(new Module(RotateName(bit, sockets, 3), mesh, 3, false));
                }
            }

            if (!FlipRotationEqualCheck(bit))
            {
                string flippedName = FlipName(bit, sockets);
                string newBit = flippedName.Substring(0, 8);
                string newSockets = flippedName.Length > 8 ? flippedName.Substring(9, 6) : "aaaaaa";

                if (!moduleLibrary.ContainsKey(newBit))
                {
                    moduleLibrary.Add(newBit, new List<Module>());
                    moduleLibrary[newBit].Add(new Module(flippedName, mesh, 0, true));
                }

                if (!RotationEqualCheck(newBit))
                {
                    string newFlipBit = RotateName(newBit, newSockets, 1);
                    if (!moduleLibrary.ContainsKey(newFlipBit))
                    {
                        moduleLibrary.Add(newFlipBit, new List<Module>());
                        moduleLibrary[newFlipBit].Add(new Module(newFlipBit, mesh, 1, true));
                    }
                }

                if (!RotationTwiceEqualCheck(newBit))
                {
                    string newFlipBit2 = RotateName(newBit, newSockets, 2);
                    if (!moduleLibrary.ContainsKey(newFlipBit2))
                    {
                        moduleLibrary.Add(newFlipBit2, new List<Module>());
                        moduleLibrary[newFlipBit2].Add(new Module(newFlipBit2, mesh, 2, true));
                    }

                    string newFlipBit3 = RotateName(newBit, newSockets, 3);
                    if (!moduleLibrary.ContainsKey(newFlipBit3))
                    {
                        moduleLibrary.Add(newFlipBit3, new List<Module>());
                        moduleLibrary[newFlipBit3].Add(new Module(newFlipBit3, mesh, 3, true));
                    }
                }
            }
        }

        if (bit == "OCG00000" && !moduleLibrary[bit].Exists(m => m.name == "OCG00000"))
        {
            moduleLibrary[bit].Add(new Module("OCG00000", null, 0, false)); // 添加特殊模块
        }
    }

    public List<Module> GetModuleByBit(string bit)
    {
        List<Module> moduleList;
        if (!moduleLibrary.TryGetValue(bit, out moduleList))
        {
            moduleList = new List<Module>();
            Debug.LogWarning($"No modules found for bit: {bit}");
        }
        return moduleList;
    }

    public string RotateBit(string bit, int time)
    {
        string result = bit;
        for (int i = 0; i < time; i++)
        {
            if (result.Length < 8)
            {
                Debug.LogError($"Invalid bit string length: {result}");
                break;
            }
            result = result[3] + result.Substring(0, 3) + result[7] + result.Substring(4, 3);
        }
        Debug.Log($"RotateBit({bit}, {time}) -> {result}");
        return result;
    }

    public string RotateName(string bit, string sockets, int time)
    {
        string result = sockets;
        for (int i = 0; i < time; i++)
        {
            result = result.Substring(3, 1) + result.Substring(0, 3) + result.Substring(4);
        }
        string rotatedName = RotateBit(bit, time) + "_" + result;
        Debug.Log($"RotateName({bit}, {sockets}, {time}) -> {rotatedName}");
        return rotatedName;
    }

    public string FlipBit(string bit)
    {
        if (bit.Length < 8)
        {
            Debug.LogError($"Invalid bit string length: {bit}");
            return bit;
        }
        string result = bit[3].ToString() + bit[2] + bit[1] + bit[0] + bit[7] + bit[6] + bit[5] + bit[4];
        Debug.Log($"FlipBit({bit}) -> {result}");
        return result;
    }

    private string FlipName(string bit, string sockets)
    {
        string result = sockets;
        result = result.Substring(2, 1) + result.Substring(1, 1) + result.Substring(0, 1) + result.Substring(3, 1) + result.Substring(4);
        string flippedName = FlipBit(bit) + "_" + result;
        Debug.Log($"FlipName({bit}, {sockets}) -> {flippedName}");
        return flippedName;
    }

    public bool RotationEqualCheck(string bit)
    {
        bool result = bit[0] == bit[1] && bit[1] == bit[2] && bit[2] == bit[3] && bit[3] == bit[4] && bit[4] == bit[5] && bit[5] == bit[6] && bit[6] == bit[7];
        Debug.Log($"RotationEqualCheck({bit}) -> {result}");
        return result;
    }

    public bool RotationTwiceEqualCheck(string bit)
    {
        bool result = bit[0] == bit[2] && bit[1] == bit[3] && bit[4] == bit[6] && bit[5] == bit[7];
        Debug.Log($"RotationTwiceEqualCheck({bit}) -> {result}");
        return result;
    }

    public bool FlipRotationEqualCheck(string bit)
    {
        string symetry_vertical = bit[3].ToString() + bit[2] + bit[1] + bit[0] + bit[7] + bit[6] + bit[5] + bit[4];
        string symetry_horizontal = bit[1].ToString() + bit[0] + bit[3] + bit[2] + bit[5] + bit[4] + bit[7] + bit[6];
        string symetry_02 = bit[0].ToString() + bit[3] + bit[2] + bit[1] + bit[4] + bit[7] + bit[6] + bit[5];
        string symetry_03 = bit[2].ToString() + bit[1] + bit[0] + bit[3] + bit[6] + bit[5] + bit[4] + bit[7];

        bool result = bit == symetry_vertical || bit == symetry_horizontal || bit == symetry_02 || bit == symetry_03;
        Debug.Log($"FlipRotationEqualCheck({bit}) -> {result}");
        return result;
    }
}