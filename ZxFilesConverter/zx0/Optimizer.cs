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
    internal class Optimizer
    {
        #region Constants
        public const int INITIAL_OFFSET = 1;
        public const int MAX_SCALE = 50;
        #endregion

        #region Fields
        private Block[] lastLiteral;
        private Block[] lastMatch;
        private Block[] optimal;
        private int[] matchLength;
        private int[] bestLength;
        #endregion

        #region Public methods
        public Block Optimize(byte[] input, int skip, int offsetLimit)
        {
            // allocate all main data structures at once
            int arraySize = this.OffsetCeiling(input.Length - 1, offsetLimit) + 1;

            this.lastLiteral = new Block[arraySize];
            this.lastMatch = new Block[arraySize];
            this.optimal = new Block[input.Length];
            this.matchLength = new int[arraySize];
            this.bestLength = new int[input.Length];
            
            if (this.bestLength.Length > 2)
            {
                this.bestLength[2] = 2;
            }

            // start with fake block
            this.lastMatch[INITIAL_OFFSET] = new Block(-1, skip - 1, INITIAL_OFFSET, null);

            // process remaining bytes
            for (int index = skip; index < input.Length; index++)
            {
                int maxOffset = this.OffsetCeiling(index, offsetLimit);
                this.optimal[index] = this.ProcessTask(1, maxOffset, index, skip, input);
            }

            return this.optimal[input.Length - 1];
        }
        #endregion

        #region Private methods
        private int EliasGammaBits(int value)
        {
            int bits = 1;

            while (value > 1)
            {
                bits += 2;
                value >>= 1;
            }

            return bits;
        }

        private int OffsetCeiling(int index, int offsetLimit)
        {
            return Math.Min(Math.Max(index, INITIAL_OFFSET), offsetLimit);
        }

        private Block ProcessTask(int initialOffset, int finalOffset, int index, int skip, byte[] input)
        {
            int bestLengthSize = 2;
            Block optimalBlock = null;

            for (int offset = initialOffset; offset <= finalOffset; offset++)
            {
                if (index != skip && index >= offset && input[index] == input[index - offset])
                {
                    // copy from last offset
                    if (this.lastLiteral[offset] != null)
                    {
                        int length = index - this.lastLiteral[offset].Index;
                        int bits = this.lastLiteral[offset].Bits + 1 + this.EliasGammaBits(length);
                        this.lastMatch[offset] = new Block(bits, index, offset, this.lastLiteral[offset]);
                        if (optimalBlock == null || optimalBlock.Bits > bits)
                        {
                            optimalBlock = this.lastMatch[offset];
                        }
                    }
                    // copy from new offset
                    if (++this.matchLength[offset] > 1)
                    {
                        if (bestLengthSize < this.matchLength[offset])
                        {
                            int bits1 = this.optimal[index - this.bestLength[bestLengthSize]].Bits + this.EliasGammaBits(this.bestLength[bestLengthSize] - 1);
                            do
                            {
                                bestLengthSize++;
                                int bits2 = this.optimal[index - bestLengthSize].Bits + this.EliasGammaBits(bestLengthSize - 1);
                                if (bits2 <= bits1)
                                {
                                    this.bestLength[bestLengthSize] = bestLengthSize;
                                    bits1 = bits2;
                                }
                                else
                                {
                                    this.bestLength[bestLengthSize] = this.bestLength[bestLengthSize - 1];
                                }
                            } while (bestLengthSize < this.matchLength[offset]);
                        }

                        int length = this.bestLength[this.matchLength[offset]];
                        int bits = this.optimal[index - length].Bits + 8 + this.EliasGammaBits((offset - 1) / 128 + 1) + this.EliasGammaBits(length - 1);

                        if (this.lastMatch[offset] == null || this.lastMatch[offset].Index != index || this.lastMatch[offset].Bits > bits)
                        {
                            this.lastMatch[offset] = new Block(bits, index, offset, this.optimal[index - length]);

                            if (optimalBlock == null || optimalBlock.Bits > bits)
                            {
                                optimalBlock = this.lastMatch[offset];
                            }
                        }
                    }
                }
                else
                {
                    // copy literals
                    this.matchLength[offset] = 0;
                    if (this.lastMatch[offset] != null)
                    {
                        int length = index - this.lastMatch[offset].Index;
                        int bits = this.lastMatch[offset].Bits + 1 + this.EliasGammaBits(length) + length * 8;

                        this.lastLiteral[offset] = new Block(bits, index, 0, this.lastMatch[offset]);

                        if (optimalBlock == null || optimalBlock.Bits > bits)
                        {
                            optimalBlock = this.lastLiteral[offset];
                        }
                    }
                }
            }
            return optimalBlock;
        }
        #endregion
    }
}
