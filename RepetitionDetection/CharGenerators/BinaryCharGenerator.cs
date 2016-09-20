﻿using System;
using System.Text;
using RepetitionDetection.Commons;

namespace RepetitionDetection.CharGenerators
{
    public class BinaryCharGenerator : ICharGenerator
    {
        private readonly StringBuilder text;
        private readonly int k;
        private readonly bool[] used;

        public int AlphabetSize { get { return k; } }

        public override string ToString()
        {
            return "BinaryCharGenerator";
        }

        public BinaryCharGenerator(StringBuilder text, int alphabetSize)
        {
            this.text = text;
            k = alphabetSize;
            used = new bool[alphabetSize];
        }

        public char Generate()
        {
            for (var i = 0; i < k; ++i)
                used[i] = false;
            if (text.Length < k - 1)
            {   
                for (var i = 0; i < text.Length; ++i)
                {
                    used[text[text.Length - 1 - i] - 'a'] = true;
                }
                var rand = RandomNumberGenerator.Generate(1, k - text.Length + 1);
                for (var i = 0; i < k; ++i)
                {
                    if (!used[i])
                    {
                        rand--;
                    }
                    if (rand == 0)
                    {
                        return (char) (i + 'a');
                    }
                }
                throw new InvalidProgramStateException("Invalid program state in ByTailGenerator");
            }
            else
            {
                for (var i = 0; i < k - 1; ++i)
                {
                    used[text[text.Length - 1 - i] - 'a'] = true;
                }
                var notUsed = Array.FindIndex(used, b => !b);
                var rand = RandomNumberGenerator.Generate(0, 2);
                if (rand == 0)
                {
                    return text[text.Length - k + 1];
                }
                return (char) (notUsed + 'a');
            }
        }
    }
}
