// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using UnityEditor;
using UnityEngine;

namespace CodeSmile.Statemachine
{
	// public class IsBoolVariableEqualCondition : FSM.ICondition
	// {
	// 	private readonly FSM.BoolVariable m_Variable;
	// 	private readonly FSM.BoolVariable m_Comparand;
	//
	// 	private IsBoolVariableEqualCondition() {} // forbidden default ctor
	//
	// 	internal IsBoolVariableEqualCondition(FSM.BoolVariable variable, FSM.BoolVariable comparand)
	// 	{
	// 		m_Variable = variable;
	// 		m_Comparand = comparand;
	// 	}
	//
	// 	public Boolean IsSatisfied(FSM sm) => m_Variable.Equals(m_Comparand);
	//
	// 	public String ToDebugString(FSM sm)
	// 	{
	// 		// var isGlobal = false;
	// 		// var varName = sm.Vars.FindName(m_Variable);
	// 		// if (varName == null)
	// 		// {
	// 		// 	isGlobal = true;
	// 		// 	varName = sm.GlobalVars.FindName(m_Variable);
	// 		// }
	//
	// 		//var scope = isGlobal ? "g" : "m";
	// 		return $"'scope_varName' == {m_Comparand.Value}";
	// 	}
	// }
}
