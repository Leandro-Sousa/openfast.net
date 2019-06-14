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
using NUnit.Framework;
using OpenFAST.Error;
using OpenFAST.UnitTests.Test;

namespace OpenFAST.UnitTests
{
    [TestFixture]
    public class DecimalValueTest : OpenFastTestCase
    {
        [Test]
        public void TestMantissaAndExponent()
        {
            var value = new DecimalValue(9427.55);
            AssertEquals(942755, value.Mantissa);
            AssertEquals(-2, value.Exponent);

            value = new DecimalValue(942755, -2);
            AssertEquals(((decimal) 9427.55), value.ToBigDecimal());
        }

        [Test]
        public void TestMaxValue()
        {
            var max = new DecimalValue(int.MaxValue, 10);
            AssertEquals(new Decimal(2147483647e10), max.ToBigDecimal());
        }

        [Test]
        public void TestToBigDecimal()
        {
            AssertEquals(new Decimal(241e5), new DecimalValue(241, 5).ToBigDecimal());
            AssertEquals(new Decimal(15e-4), new DecimalValue(15, -4).ToBigDecimal());
        }

        [Test]
        public void TestToByte()
        {
            AssertEquals(100, Decimal(100.0).ToByte());
        }

        [Test]
        public void TestToByteWithDecimalPart()
        {
            try
            {
                Decimal(100.1).ToByte();
                Assert.Fail();
            }
            catch (RepErrorException e)
            {
                Assert.AreEqual(RepError.DecimalCantConvertToInt, e.Error);
            }
        }

        [Test]
        public void TestToDouble()
        {
            Assert.AreEqual(3.3, new DecimalValue(33, -1).ToDouble(), 0.000000000001);
        }

        [Test]
        public void TestToInt()
        {
            AssertEquals(100, Decimal(100.0).ToInt());
        }

        [Test]
        public void TestToIntWithDecimalPart()
        {
            try
            {
                Decimal(100.1).ToInt();
                Assert.Fail();
            }
            catch (RepErrorException e)
            {
                Assert.AreEqual(RepError.DecimalCantConvertToInt, e.Error);
            }
        }

        [Test]
        public void TestToLong()
        {
            AssertEquals(10000000000000L, Decimal(10000000000000.0).ToLong());
        }

        [Test]
        public void TestToLongWithDecimalPart()
        {
            try
            {
                Decimal(100.1).ToLong();
                Assert.Fail();
            }
            catch (RepErrorException e)
            {
                Assert.AreEqual(RepError.DecimalCantConvertToInt, e.Error);
            }
        }

        [Test]
        public void TestToShort()
        {
            AssertEquals(128, Decimal(128.0).ToShort());
        }

        [Test]
        public void TestToShortWithDecimalPart()
        {
            try
            {
                Decimal(100.1).ToShort();
                Assert.Fail();
            }
            catch (RepErrorException e)
            {
                Assert.AreEqual(RepError.DecimalCantConvertToInt, e.Error);
            }
        }

        [Test]
        public void FromDecimalToDecimalTest()
        {
            decimal[] testValues = new decimal[] { 100.1M, 0M, -20.3M, 123456789.123456M };
            foreach (var value in testValues)
            {

                var val = new OpenFAST.DecimalValue(value);
                var test = val.ToBigDecimal();
                Assert.AreEqual(value, test);
            }
        }
        [Test]
        public void FromDecimalToDoubleTest()
        {
            decimal[] testValues = new decimal[] { 100.1M, 0M, -20.3M, 123456789.123456M };
            foreach (var value in testValues)
            {

                var val = new OpenFAST.DecimalValue(value);
                var test = (decimal)Math.Round(val.ToDouble(), 6);
                Assert.AreEqual(value, test);
            }
        }
        [Test]
        public void FromDoubleToDecimalTest()
        {
            double[] testValues = new double[] { 100.1, 0.0, -20.3, 123456789.123456 };
            foreach (var value in testValues)
            {

                var val = new OpenFAST.DecimalValue(value);
                var test = Math.Round((double)val.ToBigDecimal(), 6);
                Assert.AreEqual(value, test);
            }
        }
        [Test]
        public void FromDoubleToDoubleTest()
        {
            double[] testValues = new double[] { 100.1, 0.0, -20.3, 123456789.123456 };
            foreach (var value in testValues)
            {

                var val = new OpenFAST.DecimalValue(value);
                var test = Math.Round(val.ToDouble(), 6);
                Assert.AreEqual(value, test);
            }
        }
    }
}