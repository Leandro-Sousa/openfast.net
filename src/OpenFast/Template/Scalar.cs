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
                Yuri Astrakhan <FirstName><LastName>@gmail.com
*/
using System;
using System.IO;
using OpenFAST.Error;
using OpenFAST.Template.Operators;
using OpenFAST.Template.Types;
using OpenFAST.Template.Types.Codec;
using OpenFAST.Utility;

namespace OpenFAST.Template
{
    public sealed class Scalar : Field
    {
        private string _dictionary;

        public Scalar(string name, FastType fastType, Operator op, ScalarValue defaultValue,
                      bool optional)
            : this(new QName(name), fastType, op, defaultValue, optional)
        {
        }

        public Scalar(QName name, FastType fastType, Operator op, ScalarValue defaultValue,
                      bool optional)
            : this(name, fastType, op, op.GetCodec(fastType), defaultValue, optional)
        {
        }

        public Scalar(QName name, FastType fastType, OperatorCodec operatorCodec, ScalarValue defaultValue,
                      bool optional)
            : this(name, fastType, operatorCodec.Operator, operatorCodec, defaultValue, optional)
        {
        }

        private Scalar(QName name, FastType fastType, Operator op, OperatorCodec operatorCodec,
                       ScalarValue defaultValue, bool optional)
            : base(name, optional)
        {
            Operator = op;
            OperatorCodec = operatorCodec;
            _dictionary = DictionaryFields.Global;
            DefaultValue = defaultValue ?? ScalarValue.Undefined;
            FastType = fastType;
            TypeCodec = fastType.GetCodec(op, optional);
            BaseValue = (defaultValue == null || defaultValue.IsUndefined) ? FastType.DefaultValue : defaultValue;
            op.Validate(this);
        }

        #region Cloning

        public Scalar(Scalar other)
            : base(other)
        {
            DefaultValue = (ScalarValue)other.DefaultValue.Clone();
            FastType = other.FastType;
            BaseValue = (ScalarValue)other.BaseValue.Clone();
            Operator = other.Operator;
            OperatorCodec = other.OperatorCodec;
            TypeCodec = other.TypeCodec;
            _dictionary = other._dictionary;
        }

        public override Field Clone()
        {
            return new Scalar(this);
        }

        #endregion

        public FastType FastType { get; }

        public Operator Operator { get; }

        public string Dictionary
        {
            get { return _dictionary; }
            set
            {
                ThrowOnReadonly();
                if (value == null) throw new ArgumentNullException("value");
                _dictionary = DictionaryFields.InternDictionaryName(value);
            }
        }

        public ScalarValue DefaultValue { get; }

        public override Type ValueType
        {
            get { return typeof(ScalarValue); }
        }

        public override string TypeName
        {
            get { return "scalar"; }
        }

        public ScalarValue BaseValue { get; }

        public TypeCodec TypeCodec { get; }

        public override bool UsesPresenceMapBit
        {
            get { return OperatorCodec.UsesPresenceMapBit(IsOptional); }
        }

        public OperatorCodec OperatorCodec { get; }

        public override byte[] Encode(IFieldValue fieldValue, Group encodeTemplate, Context context,
                                      BitVectorBuilder presenceMapBuilder)
        {
            IDictionary dict = context.GetDictionary(Dictionary);

            ScalarValue priorValue = context.Lookup(dict, encodeTemplate, Key);
            var value = (ScalarValue)fieldValue;
            if (!OperatorCodec.CanEncode(value, this))
            {
                Global.ErrorHandler.OnError(null, DynError.CantEncodeValue,
                                            "The scalar {0} cannot encode the value {1}", this, value);
            }
            ScalarValue valueToEncode = OperatorCodec.GetValueToEncode(value, priorValue, this,
                                                                        presenceMapBuilder);
            if (Operator.ShouldStoreValue(value))
            {
                context.Store(dict, encodeTemplate, Key, value);
            }
            if (valueToEncode == null)
            {
                return ByteUtil.EmptyByteArray;
            }
            byte[] encoding = TypeCodec.Encode(valueToEncode);
            if (context.TraceEnabled && encoding.Length > 0)
            {
                context.EncodeTrace.Field(this, fieldValue, valueToEncode, encoding, presenceMapBuilder.Index);
            }
            return encoding;
        }

        public override bool IsPresenceMapBitSet(byte[] encoding, IFieldValue fieldValue)
        {
            return OperatorCodec.IsPresenceMapBitSet(encoding, fieldValue);
        }

        public override IFieldValue Decode(Stream inStream, Group decodeTemplate, Context context,
                                           BitVectorReader presenceMapReader)
        {
            try
            {
                ScalarValue priorValue = null;
                IDictionary dict = null;
                QName key = Key;

                ScalarValue value;
                int pmapIndex = presenceMapReader.Index;
                if (IsPresent(presenceMapReader))
                {
                    if (context.TraceEnabled)
                        inStream = new RecordingInputStream(inStream);

                    if (!OperatorCodec.ShouldDecodeType)
                        return OperatorCodec.DecodeValue(null, null, this);

                    if (OperatorCodec.DecodeNewValueNeedsPrevious)
                    {
                        dict = context.GetDictionary(Dictionary);
                        priorValue = context.Lookup(dict, decodeTemplate, key);
                        ValidateDictionaryTypeAgainstFieldType(priorValue, FastType);
                    }

                    ScalarValue decodedValue = TypeCodec.Decode(inStream);
                    value = OperatorCodec.DecodeValue(decodedValue, priorValue, this);

                    if (context.TraceEnabled)
                        context.DecodeTrace.Field(this, value, decodedValue,
                                                  ((RecordingInputStream)inStream).Buffer, pmapIndex);
                }
                else
                {
                    if (OperatorCodec.DecodeEmptyValueNeedsPrevious)
                    {
                        dict = context.GetDictionary(Dictionary);
                        priorValue = context.Lookup(dict, decodeTemplate, key);
                        ValidateDictionaryTypeAgainstFieldType(priorValue, FastType);
                    }

                    value = OperatorCodec.DecodeEmptyValue(priorValue, this);
                }

                ValidateDecodedValueIsCorrectForType(value, FastType);

                if (OperatorCodec.DecodeNewValueNeedsPrevious || OperatorCodec.DecodeEmptyValueNeedsPrevious)
                {
                    context.Store(dict ?? context.GetDictionary(Dictionary), decodeTemplate, key, value);
                }

                return value;
            }
            catch (DynErrorException e)
            {
                throw new DynErrorException(e, e.Error, "Error occurred while decoding {0}", this);
            }
        }

        private static void ValidateDecodedValueIsCorrectForType(ScalarValue value, FastType type)
        {
            if (value == null)
                return;
            type.ValidateValue(value);
        }

        private static void ValidateDictionaryTypeAgainstFieldType(ScalarValue priorValue, FastType type)
        {
            if (priorValue == null || priorValue.IsUndefined)
                return;
            if (!type.IsValueOf(priorValue))
            {
                Global.ErrorHandler.OnError(null, DynError.InvalidType,
                                            "The value '{0}' is not valid for the type {1}", priorValue, type);
            }
        }

        public override string ToString()
        {
            return string.Format("Scalar [name={0}, operator={1}, type={2}, dictionary={3}]", Name, Operator, FastType,
                                 _dictionary);
        }

        public override IFieldValue CreateValue(string value)
        {
            return FastType.GetValue(value);
        }

        [Obsolete("need?")] // BUG? Do we need this?
        public string Serialize(ScalarValue value)
        {
            return FastType.Serialize(value);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, this))
                return true;
            var t = other as Scalar;
            if (t == null)
                return false;
            return Equals(t);
        }

        internal bool Equals(Scalar other)
        {
            bool equals = EqualsPrivate(Name, other.Name);
            equals = equals && EqualsPrivate(FastType, other.FastType);
            equals = equals && EqualsPrivate(TypeCodec, other.TypeCodec);
            equals = equals && EqualsPrivate(Operator, other.Operator);
            equals = equals && EqualsPrivate(OperatorCodec, other.OperatorCodec);
            equals = equals && EqualsPrivate(BaseValue, other.BaseValue);
            equals = equals && EqualsPrivate(_dictionary, other._dictionary);
            equals = equals && EqualsPrivate(Id, other.Id);
            return equals;
        }

        private static bool EqualsPrivate(object o, object o2)
        {
            if (o == null)
            {
                if (o2 == null)
                    return true;
                return false;
            }
            return o.Equals(o2);
        }

        public override int GetHashCode()
        {
            return QName.GetHashCode() + FastType.GetHashCode() + TypeCodec.GetHashCode() + Operator.GetHashCode() +
                   OperatorCodec.GetHashCode() + BaseValue.GetHashCode() + _dictionary.GetHashCode();
        }
    }
}