/*
 * c# code from JAVA: Juan Antonio Rubio García, 2023-12-22.
 * 
 * (c) Copyright 2021 by Einar Saukas. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * The name of its author may not be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace ZxFilesConverter.zx0
{
    public class Compressor
    {
        #region Fields
        private byte[] output;
        private int outputIndex;
        private int inputIndex;
        private int bitIndex;
        private int bitMask;
        private int diff;
        private bool backtrack;
        #endregion

        #region Public methods
        public byte[] Compress(Block optimal, byte[] input, int skip, bool backwardsMode, bool invertMode, int[] delta)
        {
            int lastOffset = Optimizer.INITIAL_OFFSET;

            // calculate and allocate output buffer
            this.output = new byte[(optimal.Bits + 25) / 8];

            // un-reverse optimal sequence
            Block prev = null;

            while (optimal != null)
            {
                Block next = optimal.Chain;
                optimal.Chain = prev;
                prev = optimal;
                optimal = next;
            }

            // initialize data
            this.diff = this.output.Length - input.Length + skip;
            delta[0] = 0;
            this.inputIndex = skip;
            this.outputIndex = 0;
            this.bitMask = 0;
            this.backtrack = true;

            // generate output
            for (optimal = prev.Chain; optimal != null; prev = optimal, optimal = optimal.Chain)
            {
                int length = optimal.Index - prev.Index;
                
                if (optimal.Offset == 0)
                {
                    // copy literals indicator
                    this.WriteBit(0);

                    // copy literals length
                    this.WriteInterlacedEliasGamma(length, backwardsMode, false);

                    // copy literals values
                    for (int i = 0; i < length; i++)
                    {
                        this.WriteByte(input[this.inputIndex]);
                        this.ReadBytes(1, delta);
                    }
                }
                else if (optimal.Offset == lastOffset)
                {
                    // copy from last offset indicator
                    this.WriteBit(0);

                    // copy from last offset length
                    this.WriteInterlacedEliasGamma(length, backwardsMode, false);
                    this.ReadBytes(length, delta);
                }
                else
                {
                    // copy from new offset indicator
                    this.WriteBit(1);

                    // copy from new offset MSB
                    this.WriteInterlacedEliasGamma((optimal.Offset - 1) / 128 + 1, backwardsMode, invertMode);

                    // copy from new offset LSB
                    this.WriteByte(backwardsMode ? ((optimal.Offset - 1) % 128) << 1 : (127 - (optimal.Offset - 1) % 128) << 1);

                    // copy from new offset length
                    this.backtrack = true;
                    this.WriteInterlacedEliasGamma(length - 1, backwardsMode, false);
                    this.ReadBytes(length, delta);

                    lastOffset = optimal.Offset;
                }
            }

            // end marker
            this.WriteBit(1);
            this.WriteInterlacedEliasGamma(256, backwardsMode, invertMode);

            // done!
            return output;
        }
        #endregion

        #region Private methods
        private void ReadBytes(int n, int[] delta)
        {
            this.inputIndex += n;
            this.diff += n;
            if (delta[0] < this.diff)
                delta[0] = this.diff;
        }

        private void WriteByte(int value)
        {
            this.output[outputIndex++] = (byte)(value & 0xff);
            this.diff--;
        }

        private void WriteBit(int value)
        {
            if (this.backtrack)
            {
                if (value > 0)
                {
                    this.output[this.outputIndex - 1] |= 1;
                }

                this.backtrack = false;
            }
            else
            {
                if (this.bitMask == 0)
                {
                    this.bitMask = 128;
                    this.bitIndex = this.outputIndex;
                    this.WriteByte(0);
                }

                if (value > 0)
                {
                    this.output[bitIndex] |= Convert.ToByte(this.bitMask);
                }

                this.bitMask >>= 1;
            }
        }

        private void WriteInterlacedEliasGamma(int value, bool backwardsMode, bool invertMode)
        {
            int i = 2
                ;
            while (i <= value)
            {
                i <<= 1;
            }

            i >>= 1;

            while ((i >>= 1) > 0)
            {
                this.WriteBit(backwardsMode ? 1 : 0);
                this.WriteBit(invertMode == ((value & i) == 0) ? 1 : 0);
            }

            this.WriteBit(!backwardsMode ? 1 : 0);
        }
        #endregion
    }
}
