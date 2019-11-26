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
using System.IO;
using NUnit.Framework;
using OpenFAST.Codec;
using OpenFAST.Template;
using OpenFAST.Template.Operators;
using OpenFAST.Template.Types;

namespace OpenFAST.UnitTests.Codec
{
    [TestFixture]
    public class FastDecoderTest
    {
        [Test]
        public void TestDecodeEmptyMessage()
        {
            var messageTemplate = new MessageTemplate("", new Field[0]);
            Stream input = ByteUtil.CreateByteStream("11000000 11110001");
            var context = new Context();
            context.RegisterTemplate(113, messageTemplate);

            Message message = new FastDecoder(context, input).ReadMessage();
            Assert.AreEqual(113, message.GetInt(0));
        }

        [Test]
        public void TestDecodeMessageWithAllFieldTypes()
        {
            //   --PMAP-- --TID--- ---#1--- -------#2-------- ------------#3------------ ---#4--- ------------#5------------ ---#6---
            const string msgstr =
                "11111111 11110001 11001000 10000001 11111111 11111101 00001001 10110001 11111111 01100001 01100010 11100011 10000010";
            Stream input = ByteUtil.CreateByteStream(msgstr);

            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.Ascii, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("2", FastType.ByteVector, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("3", FastType.Decimal, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("4", FastType.I32, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("5", FastType.Ascii, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("6", FastType.U32, Operator.Copy, ScalarValue.Undefined, false),
                    });
            var context = new Context();
            context.RegisterTemplate(113, template);

            GroupValue message = new Message(template);
            message.SetString(1, "H");
            message.SetByteVector(2, new[] {(byte) 0xFF});
            message.SetDecimal(3, 1.201);
            message.SetInteger(4, -1);
            message.SetString(5, "abc");
            message.SetInteger(6, 2);
            Assert.AreEqual(message, new FastDecoder(context, input).ReadMessage());
        }

        [Test]
        public void TestDecodeMessageWithSignedIntegerFieldTypesAndAllOperators()
        {
            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.I32, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("2", FastType.I32, Operator.Delta, ScalarValue.Undefined, false),
                        new Scalar("3", FastType.I32, Operator.Increment, new IntegerValue(10), false),
                        new Scalar("4", FastType.I32, Operator.Increment, ScalarValue.Undefined, false),
                        new Scalar("5", FastType.I32, Operator.Constant, new IntegerValue(1), false),
                        /* NON-TRANSFERRABLE */
                        new Scalar("6", FastType.I32, Operator.Default, new IntegerValue(2), false)
                    });

            GroupValue message = new Message(template);
            message.SetInteger(1, 109);
            message.SetInteger(2, 29470);
            message.SetInteger(3, 10);
            message.SetInteger(4, 3);
            message.SetInteger(5, 1);
            message.SetInteger(6, 2);

            //                   --PMAP-- --TID--- --------#1------- ------------#2------------ ---#4---
            const string msg1 = "11101000 11110001 00000000 11101101 00000001 01100110 10011110 10000011";

            //                   --PMAP-- ---#2--- ---#6---
            const string msg2 = "10000100 11111111 10000011";

            //                   --PMAP-- --------#1------- --------#2------- ---#4--- ---#6---
            const string msg3 = "10101100 00000000 11100000 00001000 10000111 10000001 10000011";

            Stream input = ByteUtil.CreateByteStream(msg1 + ' ' + msg2 + ' ' +
                                                     msg3);
            var context = new Context();
            context.RegisterTemplate(113, template);

            var decoder = new FastDecoder(context, input);

            Message readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetInteger(2, 29469);
            message.SetInteger(3, 11);
            message.SetInteger(4, 4);
            message.SetInteger(6, 3);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetInteger(1, 96);
            message.SetInteger(2, 30500);
            message.SetInteger(3, 12);
            message.SetInteger(4, 1);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);
        }

        [Test]
        public void testDecodeMessageWithUnsignedLongFieldTypesAndAllOperators()
        {
            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.U64, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("2", FastType.U64, Operator.Delta, ScalarValue.Undefined, false),
                        new Scalar("3", FastType.U64, Operator.Increment, new IntegerValue(10), false),
                        new Scalar("4", FastType.U64, Operator.Increment, ScalarValue.Undefined, false),
                        new Scalar("5", FastType.U64, Operator.Constant, new IntegerValue(1), false),
                        /* NON-TRANSFERRABLE */
                        new Scalar("6", FastType.U64, Operator.Default, new IntegerValue(2), false)
                    });

            GroupValue message = new Message(template);
            message.SetInteger(1, 109);
            message.SetInteger(2, 29470);
            message.SetInteger(3, 10);
            message.SetInteger(4, 3);
            message.SetInteger(5, 1);
            message.SetInteger(6, 2);

            //                   --PMAP-- --TID--- --------#1------- ------------#2------------ ---#4---
            const string msg1 = "11101000 11110001 11101101 00000001 01100110 10011110 10000011";

            //                   --PMAP-- ---#2--- ---#6---
            const string msg2 = "10000100 11111111 10000011";

            //                   --PMAP-- --------#1------- --------#2------- ---#4--- ---#6---
            const string msg3 = "10101100 11100000 00001000 10000111 10000001 10000011";

            const string msg4 = "10111100 00100101 00100000 00101111 01001000 10000000 00100101 00100000 00101101 01011001 11011100 00100101 00100000 00101111 01001000 10000000 00100101 00100000 00101111 01001000 10000000 00100101 00100000 00101111 01001000 10000000";

            Stream input = ByteUtil.CreateByteStream(msg1 + ' ' + msg2 + ' ' +
                                                     msg3 + ' ' + msg4);
            var context = new Context();
            context.RegisterTemplate(113, template);

            var decoder = new FastDecoder(context, input);

            Message readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetInteger(2, 29469);
            message.SetInteger(3, 11);
            message.SetInteger(4, 4);
            message.SetInteger(6, 3);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetInteger(1, 96);
            message.SetInteger(2, 30500);
            message.SetInteger(3, 12);
            message.SetInteger(4, 1);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);


            message.SetLong(1, 10000000000L);
            message.SetLong(2, 10000000000L);
            message.SetLong(3, 10000000000L);
            message.SetLong(4, 10000000000L);
            message.SetLong(6, 10000000000L);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

        }

        [Test]
        public void testDecodeMessageWithDecimalFieldTypesAndAllOperators()
        {
            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.Decimal, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("2", FastType.Decimal, Operator.Delta, ScalarValue.Undefined, false),
                        new Scalar("3", FastType.Decimal, Operator.Delta, ScalarValue.Undefined, true),
                        new Scalar("4", FastType.Decimal, Operator.Delta, new DecimalValue(12.3M), false),
                        new Scalar("5", FastType.Decimal, Operator.Constant, new DecimalValue(23.4M), false),
                        new Scalar("6", FastType.Decimal, Operator.Default, new DecimalValue(24.5M), false)
                    });

            GroupValue message;

            string msg1 = "11100000 11110001 11111111 10001100 11111111 10010111 10000000 10000000 01111111 10110010";
            string msg2 = "10110000 11111000 00001101 01110000 00101101 01010110 00111010 00111011 00010000 00000000 10000001 11111001 00001101 01110000 00101101 01010110 00111010 00111011 00001111 01111111 11101011 11111000 00001101 01110000 00101101 01010110 00111010 00111011 00010000 00000000 10000011 11111001 00001101 01110000 00101101 01010110 00111010 00111011 00001111 01111111 11010111 11111000 00001101 01110000 00101101 01010110 00111010 00111011 00010000 00000000 10000110";
            string msg3 = "10110000 11111000 00101001 01010001 00001001 00000011 00101111 00110001 00110000 00000000 10000001 10000000 00011011 01100000 01011011 00101100 01110100 01110110 00100000 00000000 10000000 10000000 10000000 00011011 01100000 01011011 00101100 01110100 01110110 00100000 00000000 10000000 11111000 00101001 01010001 00001001 00000011 00101111 00110001 00110000 00000000 10000110";
            string msg4 = "10100000 10000000 10000000 10001000 01010110 00101110 01110110 01111100 01010000 01001110 01001111 01111111 11111110 10000000 10000111 01010110 00101110 01110110 01111100 01010000 01001110 01010000 00000000 11110111";

            Stream input = ByteUtil.CreateByteStream(msg1 + ' ' + msg2 + ' ' + msg3 + ' ' + msg4);

            var context = new Context();
            context.RegisterTemplate(113, template);

            var decoder = new FastDecoder(context, input);

            message = new Message(template);
            message.SetDecimal(1, 1.2M);
            message.SetDecimal(2, 2.3M);
            message.SetDecimal(4, 4.5M);
            message.SetDecimal(5, 23.4M);
            message.SetDecimal(6, 24.5M);

            Message readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message = new Message(template);
            message.SetDecimal(1, 10000000000.00000001M);
            message.SetDecimal(2, 10000000000.00000002M);
            message.SetDecimal(3, 10000000000.00000003M);
            message.SetDecimal(4, 10000000000.00000004M);
            message.SetDecimal(6, 10000000000.00000006M);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message = new Message(template);
            message.SetDecimal(1, 30000000000.00000001M);
            message.SetDecimal(2, 30000000000.00000002M);
            message.SetDecimal(4, 30000000000.00000004M);
            message.SetDecimal(6, 30000000000.00000006M);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message = new Message(template);
            message.SetDecimal(1, 0M);
            message.SetDecimal(2, 0M);
            message.SetDecimal(4, 12.3M);
            message.SetDecimal(6, 24.5M);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

        }
        [Test]
        public void TestDecodeMessageWithStringFieldTypesAndAllOperators()
        {
            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.Ascii, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("2", FastType.Ascii, Operator.Delta, ScalarValue.Undefined, false),
                        new Scalar("3", FastType.Ascii, Operator.Constant, new StringValue("e"), false),
                        /* NON-TRANSFERRABLE */
                        new Scalar("4", FastType.Ascii, Operator.Default, new StringValue("long"), false)
                    });

            var message = new Message(template);
            message.SetString(1, "on");
            message.SetString(2, "DCB32");
            message.SetString(3, "e");
            message.SetString(4, "long");

            //   --PMAP-- --TID--- --------#1------- ---------------------#2---------------------
            const string msg1 =
                "11100000 11110001 01101111 11101110 10000000 01000100 01000011 01000010 00110011 10110010";

            //                   --PMAP-- ------------#2------------ ---------------------#4---------------------
            const string msg2 = "10010000 10000010 00110001 10110110 01110011 01101000 01101111 01110010 11110100";

            Stream input = ByteUtil.CreateByteStream(msg1 + ' ' + msg2);
            var context = new Context();
            context.RegisterTemplate(113, template);

            var decoder = new FastDecoder(context, input);

            Message readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetString(2, "DCB16");
            message.SetString(4, "short");

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);
        }

        [Test]
        public void TestDecodeMessageWithUnsignedIntegerFieldTypesAndAllOperators()
        {
            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.U32, Operator.Copy, ScalarValue.Undefined, false),
                        new Scalar("2", FastType.U32, Operator.Delta, ScalarValue.Undefined, false),
                        new Scalar("3", FastType.U32, Operator.Increment, new IntegerValue(10), false),
                        new Scalar("4", FastType.U32, Operator.Increment, ScalarValue.Undefined, false),
                        new Scalar("5", FastType.U32, Operator.Constant, new IntegerValue(1), false),
                        /* NON-TRANSFERRABLE */
                        new Scalar("6", FastType.U32, Operator.Default, new IntegerValue(2), false)
                    });

            GroupValue message = new Message(template);
            message.SetInteger(1, 109);
            message.SetInteger(2, 29470);
            message.SetInteger(3, 10);
            message.SetInteger(4, 3);
            message.SetInteger(5, 1);
            message.SetInteger(6, 2);

            //                   --PMAP-- --TID--- ---#1--- ------------#2------------ ---#4---
            const string msg1 = "11101000 11110001 11101101 00000001 01100110 10011110 10000011";

            //                   --PMAP-- ---#2--- ---#6---
            const string msg2 = "10000100 11111111 10000011";

            //                   --PMAP-- ---#1--- --------#2------- ---#4--- ---#6---
            const string msg3 = "10101100 11100000 00001000 10000111 10000001 10000011";

            Stream input = ByteUtil.CreateByteStream(msg1 + ' ' + msg2 + ' ' +
                                                     msg3);
            var context = new Context();
            context.RegisterTemplate(113, template);

            var decoder = new FastDecoder(context, input);

            Message readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetInteger(2, 29469);
            message.SetInteger(3, 11);
            message.SetInteger(4, 4);
            message.SetInteger(6, 3);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);

            message.SetInteger(1, 96);
            message.SetInteger(2, 30500);
            message.SetInteger(3, 12);
            message.SetInteger(4, 1);

            readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);
        }

        [Test]
        public void TestDecodeSequentialEmptyMessages()
        {
            var messageTemplate = new MessageTemplate("", new Field[0]);
            Stream input = ByteUtil.CreateByteStream("11000000 11110001 10000000");
            var context = new Context();
            context.RegisterTemplate(113, messageTemplate);

            var decoder = new FastDecoder(context, input);
            GroupValue message = decoder.ReadMessage();
            GroupValue message2 = decoder.ReadMessage();
            Assert.AreEqual(113, message.GetInt(0));
            Assert.AreEqual(113, message2.GetInt(0));
        }

        [Test]
        public void TestDecodeSimpleMessage()
        {
            var template = new MessageTemplate(
                "",
                new Field[]
                    {
                        new Scalar("1", FastType.U32, Operator.Copy, ScalarValue.Undefined, false)
                    });
            Stream input = ByteUtil.CreateByteStream("11100000 11110001 10000001");
            var context = new Context();
            context.RegisterTemplate(113, template);

            var message = new Message(template);
            message.SetInteger(1, 1);

            var decoder = new FastDecoder(context, input);
            GroupValue readMessage = decoder.ReadMessage();
            Assert.AreEqual(message, readMessage);
            Assert.AreEqual(readMessage, message);
        }

        //[Test]
        //public void testDecodeEndOfStream() {
        //    FastDecoder decoder = new FastDecoder(new Context(), new InputStream() {
        //        public int read() throws IOException {
        //            return -1;
        //        }});

        //    Message message = decoder.readMessage();
        //    assertNull(message);
        //}
    }
}
