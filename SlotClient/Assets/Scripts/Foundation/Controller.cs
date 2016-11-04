﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* 文 件 名：Controller.cs
* 版权所有：	
* 文件编号：
* 创 建 人：Tycho
* 创建日期：2016-11-1
* 修 改 人：
* 修改日期：
* 描	述：业务逻辑类
* 版 本 号：1.0
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 文件名:控制器基类
/// 说明:
/// </summary>
public abstract class Controller
{

	/// <summary>
	/// 得到视图层
	/// </summary>
	/// <value>The view.</value>
	protected ViewPresenters View { get; set; }

	/// <summary>
	/// 得到模型层
	/// </summary>
	/// <value>The model.</value>
	protected Model Model { get; set; }

	/// <summary>
	/// 加载完标记
	/// </summary>
	private bool loaded = false;

	protected int intShowFrame = 0;
	/// <summary>
	///显示标记.
	/// </summary>
	public bool IsOnShow { get; protected set; }
	public bool IsOnExpand = false;
	private bool isFocused = false;
	protected bool isCloseWhenNotClickSelf = false;

	public bool IsFocused
	{
		get
		{
			return isFocused;
		}
		set
		{
			if (isFocused != value)
			{
				isFocused = value;
			}
		}
	}

	protected List<Controller> lstChildDialog = new List<Controller>();

	//窗体点击回调事件
	public DialogClickEvent dlgClickEvent;
	protected DelegateVoid dlgOpenListener = null;
	protected DelegateVoid dlgHideListener = null;
	protected DelegateVoid dlgCloseListener = null;


	/// <summary>
	/// 
	/// </summary>
	/// <param name="active">生成view时是否是激活的</param>
	/// <param name="native">true:the asset is in Resource folder,false:the asset is in other folder </param>
	protected Controller(GameObject view,bool active = true,bool native = false)
	{
		if(null != view)
		{
			loaded = true;
			Init(view, active);
		}
		/*
		string viewPath = string.Format("{0}/{1}", StrDef.VIEWDIR, viewName);
		if (native)
		{
			GameObject view = AssetLoadMgr.Instance.LoadnNativeAsset<GameObject>(viewPath);
			if (null != view)
			{
				Init(view);
			}
			else
			{
				Debug.LogError(string.Format("The view [{0}] is not exists", view.name));
			}
		}
		else
		{
			ModelMgr.Instance.LoadModel(viewPath, new ModelMgr.ModelCallback((string name, GameObject view, object callbackData)=>
			{
				if(null != view)
				{
					Init(view);
				}
				else
				{
					Debug.LogError(string.Format("The view [{0}] is not exists", name));
				}
			}));
		}
		*/
	}

	~Controller()
	{

	}

	/// <summary>
	/// 初始化视图
	/// </summary>
	public void Init(GameObject view,bool active)
	{
		this.Model = CreateModel();
		this.View = CreateView(view);
		this.AddListener();
		this.InitPost();
		this.loaded = true;
		this.ShowView(active);
	}

	/// <summary>
	/// 获取窗口ID
	/// </summary>
	public EViewID GetViewID()
	{
		return this.Model.ViewID;
	}

	/// <summary>
	/// 设置窗口ID
	/// </summary>
	public void SetViewID(EViewID viewID)
	{
		this.Model.ViewID = viewID;
	}

	/// <summary>
	/// 获取窗口实例ID
	/// </summary>
	public int GetViewInstID()
	{
		return this.Model.ViewInstID;
	}

	/// <summary>
	/// 设置窗口实例ID
	/// </summary>
	public void SetViewInstID(int viewInstID)
	{
		this.Model.ViewInstID = viewInstID;
	}

	/// <summary>
	/// 显示
	/// </summary>
	public void ShowView(bool active)
	{
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Click, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Click, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.DoubleClick, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.DoubleClick, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Down, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Down, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.AddMouseEventListener(EMouseEvent.Click, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.AddMouseEventListener(EMouseEvent.Click, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.AddMouseEventListener(EMouseEvent.DoubleClick, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.AddMouseEventListener(EMouseEvent.DoubleClick, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.AddMouseEventListener(EMouseEvent.Down, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.AddMouseEventListener(EMouseEvent.Down, EMouseKey.Right, OnMouseEventHandler);
		KeyBoardEventMgr.Instance.AddKeyBoardEventListener(KeyboardEventHandler);
		if (!IsOnShow)
		{
			intShowFrame = Time.frameCount;
			IsOnShow = true;
			IsFocused = true;
			this.SetActive(true);
			this.OpenDialog(this.GetViewInstID());
			this.View.ViewPanel.SetAsLastSibling();
		}
		if (dlgOpenListener != null)
		{
			dlgOpenListener();
		}
	}

	public Controller OpenDialog(int viewInstID)
	{
		Controller controller = null;

		//如果是已经正在显示，则直接返回
		if (UIMgr.Instance.viewOnShowDic.TryGetValue(viewInstID, out controller))
		{
			return controller;
		}

		//如果是历史窗口，则直接显示
		foreach (KeyValuePair<int, Controller> keyvalue in UIMgr.Instance.listAllViews)
		{
			if (keyvalue.Key == viewInstID)
			{
				controller = keyvalue.Value;
				controller.ShowDialog();
				return controller;
			}
		}
		return null;
	}

	/// <summary>
	/// 关闭窗口 将窗口设为不可见
	/// </summary>
	public virtual void CloseDialog()
	{
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Click, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Click, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.DoubleClick, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.DoubleClick, EMouseKey.Right, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Down, EMouseKey.Left, OnMouseEventHandler);
		MouseEventMgr.Instance.RemoveMouseEventListener(EMouseEvent.Down, EMouseKey.Right, OnMouseEventHandler);
		KeyBoardEventMgr.Instance.RemoveKeyBoardEventListener(KeyboardEventHandler);
		if (IsOnShow)
		{
			for (int i = 0; i < lstChildDialog.Count; ++i)
			{
				if (lstChildDialog[i] != null)
					lstChildDialog[i].CloseDialog();
			}

			IsOnShow = false;
			IsFocused = false;
			this.SetActive(false);
			this.HideDialog(GetViewInstID());
			if (dlgCloseListener != null)
			{
				dlgCloseListener();
			}
		}
	}

	/// <summary>
	/// 隐藏窗口 将窗口从显示列表中移除  内部接口 业务层不允许调用
	/// </summary>
	/// <param name="dlg"></param>
	public void HideDialog(int dialogInstID)
	{
		if (UIMgr.Instance.viewOnShowDic.ContainsKey(dialogInstID))
		{
			UIMgr.Instance.viewOnShowDic.Remove(dialogInstID);
		}
		//隐藏窗体后设置最后一个显示窗体为聚焦状态
		Controller dialog = null;
		foreach (KeyValuePair<int, Controller> keyvalue in UIMgr.Instance.listAllViews)
		{
			if (keyvalue.Key == dialogInstID)
			{
				dialog = keyvalue.Value;
				dialog.IsFocused = false;
			}
		}
		for (int i = UIMgr.Instance.listAllViews.Count - 1; i >= 0; i--)
		{
			if (UIMgr.Instance.listAllViews[i].Value.IsOnShow)
			{
				UIMgr.Instance.listAllViews[i].Value.IsFocused = true;
				break;
			}
		}

		if (dlgHideListener != null)
		{
			dlgHideListener();
		}
	}

	/// <summary>
	/// 显示
	/// </summary>
	public void SetActive(bool active)
	{
		this.View.ViewPanel.gameObject.SetActive(active);
	}

	#region 鼠标事件
	private void OnMouseEventHandler(MouseEventMgr.ResonseInfo eventinfo)
	{
		if (IsOnShow && IsFocused && eventinfo.objTarget != null)
		{
			Transform transDialog = UIMgr.Instance.GetDialogTransformByUI(eventinfo.objTarget.transform);
			if (transDialog != null && transDialog == View.ViewPanel.transform)
			{
				if (!IsFocused)
				{
					IsFocused = true;
					UIMgr.Instance.SetOtherDlgUnFocus(this);
				}
			}

			dlgClickEvent.Invoke();
		}

		if (isCloseWhenNotClickSelf && IsOnShow && IsFocused)
		{
			if (eventinfo.objTarget == null || eventinfo.objTarget.layer != LayerID.LayerUI)
			{
				CloseDialog();
			}
			else
			{
				if (!uiPanel.CheckChild(eventinfo.objTarget))
				{
					CloseDialog();
				}
			}
		}
	}

	#endregion

	#region 键盘事件
	private void KeyboardEventHandler(KeyBoardEventMgr.ResonseInfo eventinfo)
	{
		if (IsOnShow && IsFocused)
		{
			OnKeyboardEventHandler(eventinfo);
		}
	}

	protected virtual void OnKeyboardEventHandler(KeyBoardEventMgr.ResonseInfo eventinfo)
	{

	}
	#endregion

	public class DialogClickEvent : UnityEvent
	{

	}
	/// <summary>
	///创建View
	/// </summary>
	/// <returns>The view.</returns>
	protected abstract ViewPresenters CreateView(GameObject go);

	/// <summary>
	///创建Model
	/// </summary>
	/// <returns>The model.</returns>
	protected abstract Model CreateModel();

	/// <summary>
	/// Adds the listener.
	/// </summary>
	protected abstract void AddListener();

	/// <summary>
	///更新View,初始化逻辑相关数据
	/// </summary>
	protected abstract void InitPost();


}
