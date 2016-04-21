﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class UIGenWindow
{
    UIBinder _binder;

    List<UIGenControl> _controls = new List<UIGenControl>();

    public UIGenWindow(UIBinder binder)
    {
        _binder = binder;

        foreach (Transform trans in binder.gameObject.transform)
        {
            var ctrlBinder = trans.GetComponent<UIBinder>();
            if (ctrlBinder == null)
                continue;

            if (ctrlBinder.Type == CodeGenObjectType.Unknown)
            {
                continue;
            }

            if ( !UIBinder.CheckName(trans.gameObject.name) )
            {
                continue;
            }

            _controls.Add(new UIGenControl(this, ctrlBinder));

        }

    }

    public string Name
    {
        get { return _binder.gameObject.name; }
    }

    public GameObject Obj
    {
        get { return _binder.gameObject; }
    }

    // 自动绑定代码
    public string PrintAutoBindCode( )
    {
        var gen = new CodeGenerator();

        gen.PrintLine("// Generated by github.com/davyxu/cellorigin UICodeGen");
        gen.PrintLine("// DO NOT EDIT!");
        gen.PrintLine("using UnityEngine;");
        gen.PrintLine("using UnityEngine.UI;");
        gen.PrintLine();

        gen.PrintLine("public partial class ", Name, " : MonoBehaviour");
        gen.PrintLine("{");
        gen.In();

        // 变量声明
        foreach (UIGenControl ctrl in _controls)
        {
            ctrl.PrintDeclareCode(gen);
        }

        gen.PrintLine();

        // InitUI
        gen.PrintLine("void InitUI()");
        gen.PrintLine("{");
        gen.In();

        if ( _controls.Count > 0 )
        {
            gen.PrintLine("var trans = this.transform;");
        }
        

        // 变量挂接代码
        foreach (UIGenControl ctrl in _controls)
        {
            ctrl.PrintAttachCode(gen);
        }

        gen.PrintLine();

        // 按钮回调
        foreach (UIGenControl ctrl in _controls)
        {
            ctrl.PrintButtonClickRegisterCode(gen);
        }

        gen.Out();
        gen.PrintLine("}");

        // 主逻辑代码存在时, 自动生成这些实现代码, 以保证代码编译的过

        if ( MainLogicFileExists )
        {
            foreach (UIGenControl ctrl in _controls)
            {
                if (ctrl.ObjectType != CodeGenObjectType.GenAsButton)
                {
                    continue;
                }

                // 当主类存在方法, 就不用生成替代的
                if (ctrl.ButtonCallbackExists)
                {
                    continue;
                }

                // 将实现放在自动生成代码, 让代码可以编译通过
                ctrl.PrintButtonClickImplementCode(gen);
            }
        }

        


        gen.Out();
        gen.PrintLine("}");

        return gen.ToString();
    }

    // 主逻辑代码
    public string PrintMainLogicCode( )
    {
        var gen = new CodeGenerator();

        gen.PrintLine("// Generated by github.com/davyxu/cellorigin UICodeGen");
        gen.PrintLine("using UnityEngine;");
        gen.PrintLine("using UnityEngine.UI;");
        gen.PrintLine();

        gen.PrintLine("public partial class ", Name, " : MonoBehaviour");
        gen.PrintLine("{");
        gen.In();


        gen.PrintLine("void Awake( )");
        gen.PrintLine("{");
        gen.In();
        gen.PrintLine("InitUI( );");
        gen.Out();
        gen.PrintLine("}");
        gen.PrintLine();

        // 按钮回调
        foreach (UIGenControl ctrl in _controls)
        {
            ctrl.PrintButtonClickImplementCode(gen);
        }

        gen.Out();
        gen.PrintLine("}");

        return gen.ToString();
    }

    public void PrepareFolder( )
    {
        try
        {
            Directory.CreateDirectory(CodeFolder);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    // 存放绑定UI代码及主逻辑初始代码
    string CodeFolder
    {
        get
        {
            // 代码所在的文件夹是Window名字去掉UI尾缀(如果有的话)
            string folderName;
            if (Name.EndsWith("UI"))
            {
                folderName = Name.Substring(0, Name.Length - "UI".Length);
            }
            else
            {
                folderName = Name;
            }

            return UICodeGen.OutputPath + "/" + folderName;
        }
    }

    public bool MainLogicFileExists
    {
        get
        {
            return File.Exists(CodeFolder + "/" + string.Format("{0}.cs", Name));
        }
    }

    public void WriteFile( string filename, string text )
    {
        var finalname = CodeFolder + "/" + filename;

        try
        {
            System.IO.File.WriteAllText(finalname, text, System.Text.Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
}