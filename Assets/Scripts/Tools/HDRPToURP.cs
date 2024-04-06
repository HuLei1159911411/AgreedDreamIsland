using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class HDRP2URPConverter : MonoBehaviour
{
    // 更新文件夹
    public static string root;
    private static List<TextureResource> m_textureResourceList;
    private static List<ShaderResource> m_shaderPairs;

    [MenuItem("转换管线/HDRP转URP")]
    static void HdrpToUrp()
    {
        ManualValidate();
        WalkthroughMaterials(root, ReplaceToUrp, GetURPShader);
    }
    [MenuItem("转换管线/URP转HDRP")]
    static void UrpToHdrp()
    {
        ManualValidate();
        WalkthroughMaterials(root, ReplaceToHDRP, GetHDRPShader);
    }

    private static string[] m_switchPropertiesList = new string[]
    {
        "_MainTex", "_BaseMap", "_BaseColorMap", "_BaseMap", "_NormalMap", "_BumpMap", "_AlphaCutoffEnabled", "_AlphaClip"
    };

    private static string[] m_shaderNameList = new[]
    {
        "HDRP/Lit", "Universal Render Pipeline/Lit", "HDRP/Unlit", "Universal Render Pipeline/Unlit"
    };

    [ContextMenu("初始化工具")]
    private static void ManualValidate()
    {
        // 更换texture
        m_textureResourceList = new List<TextureResource>();
        for (int i = 0; i < m_switchPropertiesList.Length; i += 2)
        {
            m_textureResourceList.Add(new TextureResource()
            {
                source = m_switchPropertiesList[i],
                target = m_switchPropertiesList[i + 1]
            });
        }

        // 更换shader
        m_shaderPairs = new List<ShaderResource>();
        for (int i = 0; i < m_shaderNameList.Length; i += 2)
        {
            var s = m_shaderNameList[i];
            var t = m_shaderNameList[i + 1];
            m_shaderPairs.Add(new ShaderResource()
            {
                source = Shader.Find(s),
                target = Shader.Find(t)
            });
        }
    }

    public static Shader GetURPShader(Shader source)
    {
        foreach (var s in m_shaderPairs)
        {
            if (s.source == source) return s.target;
        }
        return Shader.Find("Universal Render Pipeline/Lit");
    }
    public static Shader GetHDRPShader(Shader target)
    {
        foreach (var s in m_shaderPairs)
        {
            if (s.target == target) return s.source;
        }
        return Shader.Find("HDRP/Lit");
    }
    public static void ReplaceToUrp(Material mat, Shader s)
    {

        foreach (var p in m_textureResourceList)
        {
            if (mat.HasProperty(p.source))
            {
                p.tex = mat.GetTexture(p.source);
            }
        }

        mat.shader = s;
        foreach (var p in m_textureResourceList)
        {
            if (mat.HasProperty(p.target))
            {
                mat.SetTexture(p.target, p.tex);
            }
        }
    }
    public static void ReplaceToHDRP(Material mat, Shader s)
    {
        foreach (var p in m_textureResourceList)
        {
            if (mat.HasProperty(p.target))
            {
                p.tex = mat.GetTexture(p.target);
            }
        }

        mat.shader = s;
        foreach (var p in m_textureResourceList)
        {
            if (mat.HasProperty(p.source))
            {
                mat.SetTexture(p.source, p.tex);
            }
        }
    }
    public static void WalkthroughMaterials(string folder, Action<Material, Shader> replace, Func<Shader, Shader> getShader)
    {
        folder = "Assets/" + folder;
        string[] allFiles = Directory.GetFiles(folder, "*.mat", SearchOption.AllDirectories);

        foreach (string file in allFiles)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(file);
            if (mat != null)
            {
                var s = getShader(mat.shader);
                replace(mat, s);
            }
        }
    }
}

[System.Serializable]
public class TextureResource
{
    public string source;
    public string target;
    public Texture tex;
}
[System.Serializable]
public class ShaderResource
{
    public Shader source;
    public Shader target;
    public Texture tex;
}