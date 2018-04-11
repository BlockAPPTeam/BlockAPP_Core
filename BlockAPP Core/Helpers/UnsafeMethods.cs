using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class UnsafeMethods
    {
        public static unsafe void Copy(Byte[] Src, int SrcIndex, byte[] _Dst, int DstIndex, int Count)
        {
            try
            {
                if (Src == null || SrcIndex < 0 || _Dst == null || DstIndex < 0 || Count < 0)
                {
                    Console.WriteLine("Serious Error in the Copy function 1");
                    // ToDO
                    throw new ArgumentException();
                }
                
                if (Src.Length - SrcIndex < Count || _Dst.Length - DstIndex < Count)
                {
                    Console.WriteLine("Serious Error in the Copy function 2");
                    // ToDo
                    throw new ArgumentException();
                }

                fixed (Byte* _pSrc = Src, _pDst = _Dst)
                {
                    Byte* _PS = _pSrc + SrcIndex;
                    Byte* _PD = _pDst + DstIndex;

                    for (int i = 0; i < Count / 4; i++)
                    {
                        *((int*)_PD) = *((int*)_PS);
                        _PD += 4;
                        _PS += 4;
                    }

                    for (int i = 0; i < Count % 4; i++)
                    {
                        *_PD = *_PS;
                        _PD++;
                        _PS++;
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }
    }
}
