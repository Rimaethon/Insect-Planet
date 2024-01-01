using System;
using Unity.Mathematics;

//This class is used to convert Int16 to Half data type in order to send it to the GPU which doesn`t let me to send Int16 array as data type
public static class ByteConverterCustom
{
    public static byte[] ConvertInt16ToBytes(short value)
    {
        return BitConverter.GetBytes(value);
    }

    public static half ConvertBytesToHalf(byte[] bytes)
    {
        short int16Value = BitConverter.ToInt16(bytes, 0);
        return Int16ToHalf(int16Value);
    }

    public static half ConvertInt16ToHalf(short value)
    {
        byte[] bytes = ConvertInt16ToBytes(value);
        return ConvertBytesToHalf(bytes);
    }

    private static half Int16ToHalf(short value)
    {
        int mantissa = value & 0x03FF;
        int exponent = value & 0x7C00;
        int sign = value & 0x8000;

        if (exponent == 0x7C00)
        {
            exponent = 0x3FC00;
        }
        else if (exponent != 0)
        {
            exponent += 0x1C000;
            if (mantissa == 0 && exponent > 0x1C400)
            {
                return new half(sign == 0 ? float.PositiveInfinity : float.NegativeInfinity);
            }
        }
        else if (mantissa != 0)
        {
            exponent = 0x1C400;
            do
            {
                mantissa <<= 1;
                exponent -= 0x400;
            } while ((mantissa & 0x400) == 0);

            // Adjust for the position of the binary point
            exponent -= 10; // 10 is the number of bits in the mantissa
        }

        int resultValue = ((sign != 0) ? 0x8000 : 0) | ((exponent | mantissa) << 13);
        return new half(BitConverter.ToSingle(BitConverter.GetBytes(resultValue), 0));
    }

}