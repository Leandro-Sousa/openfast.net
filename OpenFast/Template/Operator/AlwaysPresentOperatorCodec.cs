/*

The contents of this file are subject to the Mozilla Public License
Version 1.1 (the "License"); you may not use this file except in
compliance with the License. You may obtain a copy of the License at
http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS"
basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
License for the specific language governing rights and limitations
under the License.

The Original Code is OpenFAST.

The Initial Developer of the Original Code is The LaSalle Technology
Group, LLC.  Portions created by Shariq Muhammad
are Copyright (C) Shariq Muhammad. All Rights Reserved.

Contributor(s): Shariq Muhammad <shariq.muhammad@gmail.com>

*/
using System;
using BitVectorBuilder = OpenFAST.BitVectorBuilder;
using ScalarValue = OpenFAST.ScalarValue;
using Scalar = OpenFAST.Template.Scalar;
using FASTType = OpenFAST.Template.Type.FASTType;

namespace OpenFAST.Template.operator_Renamed
{
	[Serializable]
	public abstract class AlwaysPresentOperatorCodec:OperatorCodec
	{
		protected internal AlwaysPresentOperatorCodec(Operator operator_Renamed, FASTType[] types):base(operator_Renamed, types)
		{
		}
		
		public override bool UsesPresenceMapBit(bool optional)
		{
			return false;
		}
		
		public override ScalarValue GetValueToEncode(ScalarValue value_Renamed, ScalarValue priorValue, Scalar scalar, BitVectorBuilder presenceMapBuilder)
		{
			return GetValueToEncode(value_Renamed, priorValue, scalar);
		}
		
		public  override bool Equals(System.Object obj)
		{
			return obj != null && obj.GetType() == GetType();
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}