/*      
	Audio Scout - audio content indexing software      
        Copyright (C) 2010  D. Grant Starkweather & Evan Klinger            
      	Audio Scout is free software: you can redistribute it and/or modify      
 	it under the terms of the GNU General Public License as published by      
	the Free Software Foundation, either version 3 of the License, or      
	(at your option) any later version.        

	This program is distributed in the hope that it will be useful,      
	but WITHOUT ANY WARRANTY; without even the implied warranty of      
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the      
	GNU General Public License for more details.        
	You should have received a copy of the GNU General Public License      
	along with this program.  If not, see <http://www.gnu.org/licenses/>.        

	D. Grant Starkweather - dstarkweather@phash.org      
	Evan Klinger          - eklinger@phash.org  

*/

using System;
using System.Runtime.InteropServices;

namespace PHash
{

  public class PHashAudio 
  {

    /// <summary>
    ///   Calculate audio hash for signal buffer, buf
    /// </summary>
    /// <param name="float[]">signal buffer of which to hash</param>
    /// <param name="Int32">sr, sample rate of signal, e.g. 6000 samples per second</param>
    /// <param name="AudioHashStInfo">hash_st, ref to structure to hold commonly used info across calcs.</param>
    /// <returns>Int32[] array of hash values</returns>

    public static Int32[] audiohash(float[] buf, Int32 sr, Int32 P,
				     ref byte[][] toggles,
				     ref IntPtr hash_st)
    {
      IntPtr hashptr = IntPtr.Zero;
      IntPtr pcoeffs = IntPtr.Zero;
      IntPtr ptoggles = IntPtr.Zero;
      UInt32 nbcoeffs = 0;
      UInt32 nbframes = 0;
      double minB = 0.0;
      double maxB = 0.0;
      int err = audiohash(buf, ref hashptr, ref pcoeffs, ref ptoggles, ref nbcoeffs, ref nbframes, ref minB, 
                          ref maxB, (UInt32)buf.Length, (UInt32)P, sr, ref hash_st);

      Int32[] hash = new Int32[nbframes];
      Marshal.Copy(hashptr, hash, 0, (int)nbframes);

      toggles = new byte[nbframes][];
      for (int i = 0;i < nbframes;i++){
      	  toggles[i] = new byte[P];

	  IntPtr togglerow = (IntPtr)Marshal.PtrToStructure(ptoggles+i, typeof(IntPtr));
	  Marshal.Copy(togglerow, toggles[i], 0, P);

	  ph_free(togglerow);
      }

      ph_free(hashptr);
      ph_free(ptoggles);
      ph_free(pcoeffs);

      return hash;
    }
    
    /// <summary>
    ///   aux. extern function to native library
    /// </summary>
    [DllImport("libpHashAudio.dll", CallingConvention=CallingConvention.Cdecl)]
    private extern static int audiohash(float[] buf, ref IntPtr hash, ref IntPtr coeffs, ref IntPtr toggles,  
                                        ref UInt32 nbcoeffs, ref UInt32 nbframes, ref double minB, ref double maxB, 
                                         UInt32 buflen, UInt32 P, Int32 sr, ref IntPtr hash_st); 

    /// <summary>
    ///   aux free function to free hash array in unmanaged code.
    /// </summary>
    [DllImport("libpHashAudio.dll", CallingConvention=CallingConvention.Cdecl)]
    private extern static void ph_free(IntPtr ptr);


    /// <summary>
    ///   free AudioHashStInfo struct members after done hashing a group of files.
    /// </summary>
    /// <param name="AudioHashStInfo">hash_st, ref to AudioHashStInfo struct</param>

    [DllImport("libpHashAudio.dll", CallingConvention=CallingConvention.Cdecl)]
    public extern static void ph_hashst_free(IntPtr hash_st);

    /// private ctor to keep from being instantiated.  All members are static.
    private PHashAudio(){}

   }
}