using System;

namespace PasswordsManager.Cryptography
{

    public sealed class GF256
    {

        #region Fields

        private byte _irreduciblePolynomial;

        #endregion

        #region Constructors

        public GF256(byte irreduciblePolynomial)
        {
            IrreduciblePolynomial = irreduciblePolynomial;
        }

        #endregion

        #region Properties

        private byte IrreduciblePolynomial
        {
            get =>
                _irreduciblePolynomial;

            set
            {
                if (value % 2 == 0)
                {
                    throw new ArgumentException("Polynomial is reducible at 0.", nameof(value));
                }
                var bitsCount = 0;
                var valueCopy = value;
                while (valueCopy != 0)
                {
                    bitsCount += valueCopy % 2;
                    valueCopy >>= 1;
                }
                if (bitsCount % 2 == 1)
                {
                    throw new ArgumentException("Polynomial is reducible at 1.", nameof(value));
                }
                _irreduciblePolynomial = value;
            }
        }

        #endregion

        #region Private methods

        private byte DivisionByIrreduciblePolynomialRemainder(ushort dividend)
        {
            if (dividend == 0)
            {
                return 0;
            }
            if (dividend == 1)
            {
                if (IrreduciblePolynomial == 1)
                {
                    return 0;
                }
                return 1;
            }
            var irreduciblePolynomial = (ushort)(IrreduciblePolynomial | (1 << 8));
            while ((dividend >> 8) != 0)
            {
                var dividendDegree = (int)Math.Floor(Math.Log(dividend, 2));
                dividend ^= (ushort)(irreduciblePolynomial << (dividendDegree - 8));
            }
            return (byte)dividend;
        }

        #endregion

        #region Public methods

        public byte Add(byte firstSummand, byte secondSummand)
        {
            return (byte)(firstSummand ^ secondSummand);
        }

        public byte Multiply(byte firstFactor, byte secondFactor)
        {
            var multiplicationResult = default(ushort);
            for (var i = 0; i < 8; i++)
            {
                if ((firstFactor & 1) == 1)
                {
                    multiplicationResult ^= (ushort)(secondFactor << i);
                }
                firstFactor >>= 1;
            }
            return DivisionByIrreduciblePolynomialRemainder(multiplicationResult);
        }

        public byte Invert(byte element)
        {
            if (element == 0)
            {
                return 0;
            }
            var degree = (byte)254;
            var inversion = (byte)1;
            while (degree != 0)
            {
                if ((degree & 1) == 1)
                {
                    inversion = Multiply(inversion, element);
                }
                inversion = Multiply(inversion, inversion);
                degree >>= 1;
            }
            return inversion;
        }

        #endregion

    }

}