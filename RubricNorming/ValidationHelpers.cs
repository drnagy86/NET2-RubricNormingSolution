﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubricNorming
{
    public static class ValidationHelpers
    {
        public static bool IsValidLength(this string stringToTest, int maxLengthInclusive)
        {
            bool isValid = false;

            if (stringToTest.Length <= maxLengthInclusive && stringToTest != "" && stringToTest != null)
            {
                isValid = true;
            }

            return isValid;
        }

        public static bool HasValidCharactors(this string stringToTest)
        {
            //https://stackoverflow.com/questions/4503542/check-for-special-characters-in-a-string
            return stringToTest.Any(ch => !Char.IsLetterOrDigit(ch));
        }

    }
}
