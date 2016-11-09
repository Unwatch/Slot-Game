﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

public enum ESymbolType
{
	[Description("普通符号")]
	Normal,
	[Description("百搭符号")]
	Wild,
	[Description("分散符号")]
	Scatter
}

public enum EPayType
{
	[Description("正常组合")]
	Normal,
	[Description("免费旋转")]
	FreeSpin,
	[Description("大奖")]
	Bonus
}

public class Symbol : MonoBehaviour {

	public int test;
	public Texture texture;
	public ESymbolType symbolType;
	public EPayType payType;
	public List<int> listPayFold = new List<int>();

}
