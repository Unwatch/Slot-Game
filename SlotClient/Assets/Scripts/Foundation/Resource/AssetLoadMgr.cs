﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* 文 件 名：AssetLoadMgr.cs
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
using System;

/// <summary>
/// 文件名:资源管理器
/// 说明：子管理器
/// </summary>
public class AssetLoadMgr : SingletonWithComponent<AssetLoadMgr>
{
	/// <summary>
	/// 初始化
	/// </summary>
	protected override void InitPre()
	{

	}

	/// <summary>
	/// 后初始化
	/// </summary>
	protected override void InitPost()
	{

	}

	/// <summary>
	/// 清理（多次）
	/// </summary>
	protected override void Clear()
	{

	}

	/// <summary>
	/// 结束（一次）
	/// </summary>
	protected override void Finish()
	{

	}

	/// <summary>
	/// 在本地Recource文件夹
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public T LoadnNativeAsset<T>(string path)
	{
		if (typeof(T) == typeof(GameObject))
		{
			GameObject obj = Instantiate(Resources.Load(path, typeof(GameObject))) as GameObject;
			return (T)Convert.ChangeType(obj, typeof(T));
		}
		return default(T);
	}

	public void LoadAsset(string fullPath, AssetCallback assetCallback, object callbackData)
	{
		StartCoroutine(LoadAssetByWWW(fullPath, new AssetCallback(delegate (string Name, WWW asset, object callbackData0)
		{
			assetCallback(Name, asset, callbackData0);
		}), null));
	}

	private IEnumerator LoadAssetByWWW(string fullPath, AssetCallback assetCallback, object callbackData)
	{
		WWW asset = new WWW(fullPath);
		yield return asset;
		assetCallback(string.Empty,asset,null);
	}

	public delegate void AssetCallback(string Name, WWW asset, object callbackData);

}